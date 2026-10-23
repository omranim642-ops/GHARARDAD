# سامانه مدیریت قراردادها

ASP.NET Core 10 Razor Pages، SQL Server، رابط فارسی راست‌به‌چپ و تقویم شمسی.

ثبت/ویرایش قرارداد، شماره یکتا، طرف قرارداد، مبلغ ریالی، تاریخ‌های شمسی، وضعیت، جست‌وجو، صفحه‌بندی، پیوست تا ۱۰ مگابایت و نسخه چاپی.

## نصب آماده روی IIS

[راهنمای فارسی نصب و ایجاد بانک](deployment/INSTALL-FA.md)

بسته Windows x64 تمام runtime مورد نیاز برنامه را دارد؛ برای اتصال به IIS نصب Hosting Bundle 10 همچنان لازم است. اسکریپت Install-IIS.ps1 روی Windows PowerShell 5.1 با SQL Server روی همان ویندوز، بانک و جداول، حساب دسترسی و سایت HTTPS را برای نصب تازه ایجاد می‌کند. روش دستی با SSMS هم در راهنما آمده است.

حساب اولیه: `admin` / `Gharardad@12345`. تغییر رمز در اولین ورود اجباری است؛ تا قبل از تغییر رمز سایت را عمومی نکنید. رمزها به شکل هش PasswordHasher در بانک ذخیره می‌شوند. متغیر AdminPassword فقط هنگام ساخت حساب جدید می‌تواند رمز اولیه را جایگزین کند و رمز موجود را بازنشانی نمی‌کند.

## توسعه و آزمون

.NET SDK 10 و Python 3 لازم هستند:

```bash
dotnet publish -c Release -o publish
python3 tests/smoke.py
ASPNETCORE_ENVIRONMENT=Development UseSqlite=true dotnet run --urls http://127.0.0.1:5080
```

آزمون‌ها در بانک SQLite موقت اجرا می‌شوند. SQLite فقط برای توسعه مجاز است. برای SQL Server متغیر ConnectionStrings__Contracts را تنظیم کنید. برای ساخت SQL از مدل: متغیر اتصال غیرمحرمانه آزمایشی SQL را تنظیم و `dotnet publish/Gharardad.dll --write-schema` را اجرا کنید؛ این دستور بدون اتصال به SQL فایل schema.sql تولید می‌کند.

برای بسته ویندوز: `dotnet publish -c Release -r win-x64 --self-contained true -o /path/outside/checkout`. فایل‌های deployment را به ریشه خروجی اضافه کنید. SDK فایل web.config را تولید می‌کند.

## محدودیت‌ها

این بسته مخصوص نصب تازه است. EnsureCreated ارتقای بانک نسخه قبلی را انجام نمی‌دهد؛ قبل از ارتقا باید migration و پشتیبان تهیه شود. نقش‌های چندکاربره، گردش تأیید، گزارش حسابرسی، پرداخت‌ها و اسکن بدافزار پیوست در این نسخه وجود ندارد. بسته ویندوز ساخته شده، اما IIS/SQL Server و نصب‌کننده PowerShell در محیط لینوکسی اجرا نشده‌اند.
