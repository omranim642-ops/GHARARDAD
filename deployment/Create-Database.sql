-- Optional manual installation using SSMS, on the same Windows server as IIS.
-- Run as a SQL administrator. Create IIS application pool Gharardad first.
-- This script is for a NEW database only; it never drops existing data.
USE [master];
GO
IF DB_ID(N'Gharardad') IS NULL CREATE DATABASE [Gharardad];
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name=N'IIS APPPOOL\Gharardad')
 CREATE LOGIN [IIS APPPOOL\Gharardad] FROM WINDOWS;
GO
USE [Gharardad];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name=N'IIS APPPOOL\Gharardad')
 CREATE USER [IIS APPPOOL\Gharardad] FOR LOGIN [IIS APPPOOL\Gharardad];
GO
-- Open schema.sql in SSMS, select Gharardad as the database, and run it ONCE.
-- Then run the two grants below. Do not execute schema.sql on an existing installation.
ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\Gharardad];
ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\Gharardad];
GO
