#requires -Version 5.1
#requires -RunAsAdministrator
[CmdletBinding()]
param(
    [ValidatePattern('^\.(\\[A-Za-z0-9_]+)?$')][string]$SqlInstance = '.',
    [Parameter(Mandatory=$true)][string]$CertificateThumbprint,
    [Parameter(Mandatory=$true)][ValidatePattern('^[a-zA-Z0-9.-]+$')][string]$HostName,
    [ValidateRange(1,65535)][int]$Port = 443
)
$ErrorActionPreference = 'Stop'
Import-Module WebAdministration
Add-Type -AssemblyName System.Data
$root = $PSScriptRoot
$site = 'Gharardad'
$identity = 'IIS APPPOOL\Gharardad'
if (-not (Test-Path (Join-Path $root 'Gharardad.exe'))) {throw 'Extract the full Windows package before running this script.'}
$cert = Get-Item "Cert:\LocalMachine\My\$CertificateThumbprint"
if (-not $cert.HasPrivateKey -or $cert.NotAfter -lt (Get-Date)) {throw 'A current HTTPS certificate with its private key is required.'}
if ((Test-Path "IIS:\Sites\$site") -or (Test-Path "IIS:\AppPools\$site")) {throw 'Gharardad already exists in IIS. Do not rerun a fresh install over an existing site.'}
if (Get-WebBinding | Where-Object { $_.protocol -eq 'https' -and $_.bindingInformation -eq "*:${Port}:${HostName}" }) {throw 'This HTTPS binding already exists.'}
if (-not (Get-WebGlobalModule | Where-Object { $_.Name -eq 'AspNetCoreModuleV2' })) {throw 'Install ASP.NET Core Hosting Bundle 10 after IIS, then restart IIS.'}
# Shared memory (lpc) deliberately restricts this installer to SQL on the same Windows machine.
# It does not use TCP, does not send a SQL password, and needs no SQL TLS certificate.
$master = New-Object System.Data.SqlClient.SqlConnection
$master.ConnectionString = "Server=lpc:$SqlInstance;Database=master;Integrated Security=True;Encrypt=False;Connect Timeout=15"
$master.Open()
function Invoke-Sql([System.Data.SqlClient.SqlConnection]$Connection, [string]$Text) {
    $command = $Connection.CreateCommand()
    $command.CommandText = $Text
    $command.CommandTimeout = 120
    try { [void]$command.ExecuteNonQuery() } finally { $command.Dispose() }
}
try {
    $check = $master.CreateCommand()
    $check.CommandText = "SELECT DB_ID(N'Gharardad')"
    $exists = $check.ExecuteScalar()
    $check.Dispose()
    if ($exists -isnot [DBNull] -and $null -ne $exists) {throw 'Database Gharardad already exists. This fresh installer will not modify it. Use the manual guide.'}
    New-WebAppPool -Name $site | Out-Null
    Set-ItemProperty "IIS:\AppPools\$site" -Name managedRuntimeVersion -Value ''
    Set-ItemProperty "IIS:\AppPools\$site" -Name enable32BitAppOnWin64 -Value $false
    Set-ItemProperty "IIS:\AppPools\$site" -Name processModel.identityType -Value 4
    Invoke-Sql $master 'CREATE DATABASE [Gharardad];'
    $db = New-Object System.Data.SqlClient.SqlConnection
    $db.ConnectionString = "Server=lpc:$SqlInstance;Database=Gharardad;Integrated Security=True;Encrypt=False"
    $db.Open()
    try {
        $schema = [IO.File]::ReadAllText((Join-Path $root 'schema.sql'))
        foreach ($batch in [regex]::Split($schema, '(?im)^\s*GO\s*\r?$')) { if ($batch.Trim()) { Invoke-Sql $db $batch } }
        Invoke-Sql $master "IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name=N'IIS APPPOOL\Gharardad') CREATE LOGIN [IIS APPPOOL\Gharardad] FROM WINDOWS;"
        Invoke-Sql $db 'CREATE USER [IIS APPPOOL\Gharardad] FOR LOGIN [IIS APPPOOL\Gharardad]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\Gharardad]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\Gharardad];'
    } finally { $db.Dispose() }
} finally { $master.Dispose() }
# Only session keys need write access. The executable and configuration remain read-only to the pool.
$keyPath = Join-Path $root 'App_Data\keys'
[void](New-Item -ItemType Directory -Path $keyPath -Force)
& icacls.exe $root /grant "${identity}:(OI)(CI)RX" | Out-Null
if ($LASTEXITCODE -ne 0) {throw 'Failed to grant application read access.'}
& icacls.exe $keyPath /inheritance:r /grant:r "${identity}:(OI)(CI)M" '*S-1-5-18:(OI)(CI)F' '*S-1-5-32-544:(OI)(CI)F' | Out-Null
if ($LASTEXITCODE -ne 0) {throw 'Failed to secure session key storage.'}
$webConfigPath = Join-Path $root 'web.config'
[xml]$web = Get-Content -LiteralPath $webConfigPath -Raw
$asp = $web.configuration.location.'system.webServer'.aspNetCore
$vars = $web.CreateElement('environmentVariables')
foreach ($pair in @{
 'ASPNETCORE_ENVIRONMENT'='Production';
 'ConnectionStrings__Contracts'="Server=lpc:$SqlInstance;Database=Gharardad;Integrated Security=True;Encrypt=False";
 'DataProtectionPath'=$keyPath;
 'AllowedHosts'=$HostName
}.GetEnumerator()) {
    $node = $web.CreateElement('environmentVariable'); $node.SetAttribute('name',$pair.Key); $node.SetAttribute('value',$pair.Value); [void]$vars.AppendChild($node)
}
if ($asp.environmentVariables) { [void]$asp.RemoveChild($asp.environmentVariables) }
[void]$asp.AppendChild($vars)
$web.Save($webConfigPath)
New-Website -Name $site -PhysicalPath $root -ApplicationPool $site -Port $Port -HostHeader $HostName -Ssl | Out-Null
Set-WebBinding -Name $site -BindingInformation "*:${Port}:${HostName}" -PropertyName sslFlags -Value 1
(Get-WebBinding -Name $site -Protocol https).AddSslCertificate($CertificateThumbprint,'My')
Write-Host "Installed. Open https://${HostName}:${Port}/Login"
Write-Host 'Username: admin | Initial password: Gharardad@12345 | Password change is required at first login.'
Write-Host 'Set the DNS name to this server and verify that the HTTPS certificate matches it.'
