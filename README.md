# سامانه مدیریت قراردادها

ASP.NET Core 10 Razor Pages، رابط فارسی راست‌به‌چپ، SQL Server و تاریخ شمسی.

## امکانات نسخه اول
ورود مدیر، داشبورد، ثبت و ویرایش قرارداد، شماره یکتا، طرف قرارداد، مبلغ ریالی، تاریخ شروع و پایان شمسی با اعتبارسنجی، وضعیت، توضیحات، جست‌وجو و صفحه‌بندی، بارگذاری/دانلود/حذف پیوست و نسخه چاپی. تاریخ‌ها در بانک میلادی و در رابط شمسی هستند؛ ورودی تاریخ با قالب سال/ماه/روز است (از جمله ارقام فارسی). انتخابگر تقویم شمسی نیز در فرم در دسترس است.

## توسعه
.NET SDK 10 لازم است. در پوشه پروژه:
```bash
dotnet restore
dotnet build
# رمز را در محیط خود به‌صورت امن تنظیم کنید؛ داخل مخزن ننویسید.
export AdminPassword='YOUR-STRONG-PASSWORD'
ASPNETCORE_ENVIRONMENT=Development UseSqlite=true dotnet run --urls http://127.0.0.1:5080
```
SQLite فقط برای توسعه است. برای توسعه با SQL Server، UseSqlite را حذف و ConnectionStrings__Contracts را تنظیم کنید.

## استقرار روی IIS و SQL Server
1. Windows Server، IIS و ASP.NET Core Hosting Bundle نسخه 10 را نصب کنید. اگر IIS بعداً نصب شد Hosting Bundle را repair کنید.
2. یک بانک خالی SQL Server و حساب اختصاصی برنامه با دسترسی لازم برای ساخت جداول اولیه فراهم کنید. از حساب sa استفاده نکنید. اتصال TLS با گواهی معتبر باشد؛ TrustServerCertificate=False.
3. `dotnet publish -c Release -o publish` را اجرا کنید و محتویات publish را روی سرور کپی کنید. web.config توسط SDK تولید می‌شود.
4. Application Pool اختصاصی با No Managed Code بسازید و دسترسی خواندن/اجرا به پوشه برنامه بدهید.
5. متغیرهای محیطی سایت/پردازش IIS را امن تنظیم کنید: ASPNETCORE_ENVIRONMENT=Production، AdminPassword (رمز قوی مدیر)، ConnectionStrings__Contracts. نمونه غیرمحرمانه اتصال: Server=SQL_HOST;Database=Gharardad;User Id=APP_USER;Password=YOUR_SECRET;Encrypt=True;TrustServerCertificate=False. UseSqlite در تولید نباید فعال باشد. رمزها را commit نکنید.
6. HTTPS binding و گواهی معتبر تنظیم و App Pool را بازیابی کنید. ورود، ثبت قرارداد، پیوست و دانلود را آزمایش کنید.

جداول در اولین اجرا با EnsureCreated ساخته می‌شوند؛ این روش برای بانک خالی نسخه اول است و ارتقای schema را مدیریت نمی‌کند. برای تغییرات آینده باید migration و طرح ارتقای بانک تهیه شود. پیوست‌ها داخل بانک ذخیره می‌شوند؛ از کل بانک پشتیبان بگیرید. محدودیت هر فایل ۱۰ مگابایت و پسوندهای مجاز PDF، PNG، JPG، JPEG، DOCX، XLSX است؛ فایل‌ها اجرا نمی‌شوند و به صورت دانلود عرضه می‌شوند. اسکن بدافزار، نقش‌های چندکاربره، گردش تأیید، گزارش حسابرسی و پیگیری پرداخت‌ها در این نسخه وجود ندارد. پیش از استقرار سازمانی این نیازها را بررسی کنید. این سامانه ثبت اطلاعات قرارداد است و متن حقوقی قرارداد تولید نمی‌کند.

کلیدهای نشست در `.keys` یا مسیر `DataProtectionPath` ذخیره می‌شوند. در IIS به هویت Application Pool روی این مسیر دسترسی نوشتن بدهید و دسترسی سایر کاربران را محدود کنید. کلیدها را خارج از مسیر عمومی نگه دارید، در مخزن قرار ندهید و همراه عملیات استقرار حفظ کنید.
