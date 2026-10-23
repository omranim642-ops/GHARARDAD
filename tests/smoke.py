"""Functional smoke test. Uses an isolated temporary SQLite database; no third-party Python packages."""
import os, sys, tempfile, subprocess, secrets, time, urllib.request, urllib.parse, urllib.error, http.cookiejar, re, html
from pathlib import Path
root=Path(__file__).resolve().parents[1]
dotnet=os.environ.get('DOTNET','dotnet')
with tempfile.TemporaryDirectory() as temp:
    password=secrets.token_urlsafe(32)
    env=dict(os.environ, ASPNETCORE_ENVIRONMENT='Development', UseSqlite='true', DataProtectionPath=temp+'/keys')
    log=open(temp+'/server.log','w+')
    proc=subprocess.Popen([dotnet,str(root/'publish/Gharardad.dll'),'--urls','http://127.0.0.1:5091','--contentRoot',str(root)],cwd=temp,env=env,stdout=log,stderr=log)
    base='http://127.0.0.1:5091'
    client=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
    def get(path): return client.open(base+path)
    def token(path):
        text=get(path).read().decode()
        return html.unescape(re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',text).group(1))
    def post(path,fields):
        fields=dict(fields)
        fields['__RequestVerificationToken']=token(path.split('?')[0])
        return client.open(base+path,urllib.parse.urlencode(fields).encode())
    try:
        for i in range(100):
            try:
                get('/Login');break
            except (OSError,urllib.error.URLError):
                if proc.poll() is not None: raise RuntimeError('Server stopped')
                time.sleep(.1)
        assert '/Login' in get('/Contracts').url, 'Authentication required'
        assert '/Login' in post('/Login',{'Password':'incorrect'}).url
        first=post('/Login',{'Username':'admin','Password':'Gharardad@12345'})
        assert '/ChangePassword' in first.url
        assert '/ChangePassword' in get('/Contracts').url
        changed=post('/ChangePassword',{'CurrentPassword':'Gharardad@12345','NewPassword':password+'Aa1','ConfirmPassword':password+'Aa1'})
        assert '/ChangePassword' not in changed.url
        post('/Login?handler=Logout',{})
        assert '/Login' in post('/Login',{'Username':'admin','Password':'Gharardad@12345'}).url
        assert '/Login' not in post('/Login',{'Username':'admin','Password':password+'Aa1'}).url
        fields={'Input.Id':'0','Input.Number':'TEST-001','Input.Title':'قرارداد آزمایشی','Input.Party':'شرکت نمونه','Input.Amount':'1250000','Input.Status':'فعال','Input.Notes':'شرح قرارداد','Start':'۱۴۰۵/۰۱/۰۱','End':'1405/12/29'}
        result=post('/Edit',fields); assert '/Details/' in result.url, result.url
        path=urllib.parse.urlsplit(result.url).path
        contract_id=path.rsplit('/',1)[1]
        assert b'TEST-001' in get('/Contracts?Q=TEST-001').read()
        assert '1405/01/01' in get(path).read().decode()
        assert b'TEST-001' in get('/Print/'+contract_id).read()
        duplicate=post('/Edit',dict(fields));assert '/Edit' in duplicate.url
        invalid=dict(fields,Start='1405/13/01',**{'Input.Number':'TEST-002'})
        assert '/Edit' in post('/Edit',invalid).url
        edit=dict(fields,**{'Input.Id':contract_id,'Input.Title':'ویرایش آزمایشی'})
        assert '/Details/' in post('/Edit/'+contract_id,edit).url
        boundary='----smoke'+secrets.token_hex(10)
        payload=b'%PDF-1.4\nsmoke-test\n'
        body=(f'--{boundary}\r\nContent-Disposition: form-data; name="__RequestVerificationToken"\r\n\r\n{token(path)}\r\n--{boundary}\r\nContent-Disposition: form-data; name="Upload"; filename="sample.pdf"\r\nContent-Type: application/pdf\r\n\r\n'.encode()+payload+f'\r\n--{boundary}--\r\n'.encode())
        client.open(urllib.request.Request(base+path,data=body,headers={'Content-Type':'multipart/form-data; boundary='+boundary})).read()
        content=get(path).read().decode();assert 'sample.pdf' in content
        link=html.unescape(re.search(r'href="([^"]+handler=Download[^"]*)"',content).group(1))
        assert get(link).read()==payload
        try:
            client.open(base+'/Edit',urllib.parse.urlencode(fields).encode())
            raise AssertionError('Missing CSRF token accepted')
        except urllib.error.HTTPError as error: assert error.code==400
        proc.terminate(); proc.wait(timeout=10)
        proc=subprocess.Popen([dotnet,str(root/'publish/Gharardad.dll'),'--urls','http://127.0.0.1:5091','--contentRoot',str(root)],cwd=temp,env=env,stdout=log,stderr=log)
        for i in range(100):
            try:
                get('/Login');break
            except (OSError,urllib.error.URLError):
                if proc.poll() is not None: raise RuntimeError('Restart failed')
                time.sleep(.1)
        post('/Login?handler=Logout',{})
        assert '/Login' in post('/Login',{'Username':'admin','Password':'Gharardad@12345'}).url
        assert '/Login' not in post('/Login',{'Username':'admin','Password':password+'Aa1'}).url
        assert b'TEST-001' in get('/Contracts').read()
        print('PASS: restart persistence, default admin, forced password change, new password login, default password revoked, authentication, invalid login, create, Persian dates, search, print, duplicate rejection, invalid date, edit, attachment upload/download, CSRF protection')
    except Exception:
        log.flush();log.seek(0);print(log.read()[-6000:]);raise
    finally:
        proc.terminate();proc.wait(timeout=10);log.close()
