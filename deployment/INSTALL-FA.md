# نصب بسته آماده قرارداد روی IIS و SQL Server

این ZIP خروجی Windows x64 است، کد منبع نیست. تمام محتویات را استخراج کنید؛ کپی یک فایل DLL کافی نیست. برنامه و runtime دات‌نت داخل بسته هستند. IIS همچنان به ASP.NET Core Hosting Bundle 10 برای ماژول اتصال IIS نیاز دارد.

## ورود اولیه

- نام کاربری: `admin`
- رمز اولیه: `Gharardad@12345`
- در اولین ورود، برنامه شما را به صفحه تغییر رمز هدایت می‌کند. تا رمز عوض نشود، قراردادها باز نمی‌شوند. رمز جدید حداقل ۱۲ نویسه، شامل حرف بزرگ انگلیسی، حرف کوچک و عدد باشد. رمز اولیه پس از تغییر دیگر کار نمی‌کند.
- برنامه را تا قبل از تغییر این رمز در اینترنت عمومی در دسترس نگذارید. این حساب به صورت پیش‌فرض فقط هنگام خالی بودن جدول مدیران ساخته می‌شود؛ اجرای مجدد یا ری‌استارت رمز را بازنشانی نمی‌کند.

## روش آسان: ساخت خودکار بانک، جداول و سایت

این روش مخصوص نصب تازه است؛ SQL Server و IIS باید روی **همان ویندوز** نصب باشند. نیازی به فعال‌کردن Mixed Mode، رمز sa یا بازکردن پورت SQL نیست. اتصال بانک از نوع محلی Shared Memory و Windows Authentication است.

۱. IIS را همراه IIS Management Scripts and Tools نصب کنید. سپس ASP.NET Core Hosting Bundle 10 x64 را از سایت رسمی Microsoft نصب کنید و IIS را ری‌استارت کنید. اگر IIS بعد از Hosting Bundle نصب شده، Hosting Bundle را Repair کنید.

۲. SQL Server (مثلاً Express) و SSMS را نصب کنید. سرویس SQL باید روشن باشد. در SSMS با Windows Authentication مطمئن شوید حساب ویندوزی شما به SQL دسترسی مدیریتی دارد. برای نمونه پیش‌فرض نام اتصال `.` و برای SQL Express معمولاً `.\SQLEXPRESS` است.

۳. ZIP را مثلاً در `C:\Sites\Gharardad` استخراج کنید. این مسیر محل سایت خواهد بود؛ بعد از نصب آن را جابه‌جا نکنید.

۴. گواهی HTTPS معتبر همراه private key را در Certificates → Local Computer → Personal نصب کنید. نام سایت باید در گواهی باشد و در DNS به این ویندوز اشاره کند. Thumbprint گواهی را از IIS → Server Certificates یا PowerShell زیر پیدا کنید:

```powershell
Get-ChildItem Cert:\LocalMachine\My | Select Subject, Thumbprint, NotAfter
```

۵. **Windows PowerShell 5.1 را با Run as administrator** و با همان حسابی که در SQL مدیر است باز کنید. فایل دانلودی را Unblock کنید. دستور زیر را اجرا کنید؛ مقادیر داخل کوتیشن را با مشخصات خودتان جایگزین کنید:

```powershell
Set-Location C:\Sites\Gharardad
Get-ChildItem -Recurse -File | Unblock-File
.\Install-IIS.ps1 -SqlInstance '.\SQLEXPRESS' -HostName 'contracts.example.com' -CertificateThumbprint 'THUMBPRINT-FROM-YOUR-CERTIFICATE'
```

اگر SQL شما نمونه پیش‌فرض است، به جای `.\SQLEXPRESS` از `.` استفاده کنید. اگر سیستم به دلیل سیاست اجرای PowerShell اجازه اجرا نمی‌دهد، مطابق سیاست سازمان فایل را امضا یا اسکریپت و مراحلش را در PowerShell مدیریتی بررسی و اجرا کنید؛ نیازی به غیرفعال‌کردن سیاست امنیتی سیستم نیست.

اسکریپت بانک `Gharardad`، جداول، Application Pool اختصاصی، دسترسی خواندن/نوشتن بانک، مسیر کلیدهای نشست و سایت HTTPS را ایجاد می‌کند. حساب برنامه اجازه ساخت یا حذف بانک ندارد. دسترسی مدیریتی فقط برای نصب استفاده می‌شود.

۶. آدرس `https://contracts.example.com/Login` را باز کنید؛ با حساب اولیه وارد شوید و رمز را عوض کنید. یک قرارداد ثبت، فایل پیوست و دانلود کنید.

اسکریپت فقط برای نصب تازه است؛ اگر بانک یا سایت Gharardad از قبل موجود باشد، متوقف می‌شود و آن را پاک نمی‌کند. اگر نصب در میانه شکست خورد، ابتدا وضعیت موجود و متن خطا را بررسی کنید؛ برای تلاش مجدد بانک یا فایل‌ها را بدون بررسی پاک نکنید.

## روش دستی با SSMS و IIS

۱. در IIS یک Application Pool به نام دقیق `Gharardad`، حالت No Managed Code، هویت ApplicationPoolIdentity و 32-bit Disabled بسازید.
۲. در SSMS با حساب مدیر، `Create-Database.sql` را اجرا کنید. این فایل بانک و حساب Windows برنامه را ایجاد می‌کند. سپس بانک `Gharardad` را از لیست بالای SSMS انتخاب کنید و `schema.sql` را یک بار اجرا کنید. schema روی بانک دارای جدول‌های قبلی اجرا نشود.
۳. در پوشه سایت مسیر `App_Data\keys` بسازید. به `IIS APPPOOL\Gharardad` روی پوشه سایت Read & Execute و فقط روی پوشه keys دسترسی Modify بدهید؛ کلیدها عمومی نباشند.
۴. در web.config، داخل تگ aspNetCore، تنظیمات زیر را بگذارید؛ این مثال مخصوص SQL محلی Express است:

```xml
<environmentVariables>
  <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  <environmentVariable name="ConnectionStrings__Contracts" value="Server=lpc:.\SQLEXPRESS;Database=Gharardad;Integrated Security=True;Encrypt=False" />
  <environmentVariable name="DataProtectionPath" value="C:\Sites\Gharardad\App_Data\keys" />
  <environmentVariable name="AllowedHosts" value="contracts.example.com" />
</environmentVariables>
```

`Encrypt=False` در این مثال فقط برای **Shared Memory محلی با پیشوند lpc:** است و به اتصال شبکه‌ای SQL تعمیم داده نشود. برای SQL روی سرور دیگر، اتصال TCP، گواهی TLS معتبر، `Encrypt=True;TrustServerCertificate=False` و حساب اختصاصی مجاز تنظیم کنید. هویت محلی IIS APPPOOL به سرور SQL دیگری منتقل نمی‌شود؛ از حساب دامنه یا SQL login مجزا استفاده کنید.

۵. سایت IIS را روی پوشه استخراج‌شده و Application Pool بالا بسازید. HTTPS binding با گواهی معتبر تنظیم کنید و سایت را باز کنید. اگر بانک وجود نداشته باشد و حساب اتصال مجوز ساخت بانک و جدول داشته باشد، خود برنامه آن را می‌سازد؛ روش SSMS/اسکریپت ترجیح دارد چون به حساب برنامه مجوز مدیریتی سرور نمی‌دهد.

## خطاهای رایج

- 500.19 یا ماژول aspNetCore ناشناخته: Hosting Bundle را بعد از IIS نصب/Repair و IIS را ری‌استارت کنید.
- Login failed یا Cannot open database: نام SQL، فعال‌بودن سرویس، نام Application Pool و اجرای دستورات دسترسی را بررسی کنید.
- نام جدول Admins نامعتبر: بانک متعلق به نسخه قدیمی است؛ این بسته برای نصب تازه است. روی بانک دارای اطلاعات بدون طرح ارتقا اجرا نشود.
- خطای گواهی: نام دامنه، اعتبار گواهی، private key و دسترسی مرورگر به زنجیره اعتماد را بررسی کنید.
- 503: وضعیت Application Pool و Event Viewer → Windows Logs → Application را بررسی کنید.

پشتیبان‌گیری منظم از کل بانک لازم است؛ پیوست‌ها و هش رمز هم داخل بانک هستند. کلیدهای App_Data\keys را در جای امن نگه دارید. فایل‌های نصب و SQL بعد از نصب می‌توانند خارج از پوشه سایت نگهداری شوند. هیچ‌کدام از فایل‌های SQL، راهنما یا پیکربندی نباید از وب سرو شوند.

## وضعیت اعتبارسنجی

ساخت بسته Windows x64 و آزمون‌های عملی برنامه در Linux/SQLite انجام شده‌اند؛ نصب‌کننده PowerShell، IIS و اتصال واقعی SQL Server در این محیط اجرا نشده‌اند و باید روی ویندوز شما تأیید شوند.
