using Gharardad.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.RateLimiting;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["DataProtectionPath"] ?? Path.Combine(builder.Environment.ContentRootPath,".keys")));
var sqlite = builder.Configuration.GetValue<bool>("UseSqlite");
if (sqlite && !builder.Environment.IsDevelopment()) throw new InvalidOperationException("SQLite is for development only.");
builder.Services.AddDbContext<AppDb>(o => { if(sqlite) o.UseSqlite("Data Source=contracts.db"); else o.UseSqlServer(builder.Configuration.GetConnectionString("Contracts") ?? throw new InvalidOperationException("Configure ConnectionStrings__Contracts.")); });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => { o.LoginPath="/Login"; o.Cookie.HttpOnly=true; o.Cookie.SameSite=SameSiteMode.Strict; o.ExpireTimeSpan=TimeSpan.FromHours(8); });
builder.Services.AddAuthorization();
builder.Services.AddRazorPages(o => { o.Conventions.AuthorizeFolder("/"); o.Conventions.AllowAnonymousToPage("/Login"); o.Conventions.AllowAnonymousToPage("/Error"); });
builder.Services.AddRateLimiter(o => o.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit=10, Window=TimeSpan.FromMinutes(1), QueueLimit=0 })));
builder.WebHost.ConfigureKestrel(o=>o.Limits.MaxRequestBodySize=12*1024*1024);
var app=builder.Build();
if(!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); app.UseHttpsRedirection(); }
app.Use(async (ctx,next)=> { ctx.Response.Headers["X-Content-Type-Options"]="nosniff"; ctx.Response.Headers["X-Frame-Options"]="DENY"; ctx.Response.Headers["Content-Security-Policy"]="default-src 'self'; style-src 'self'; form-action 'self'; frame-ancestors 'none'"; await next(); });
app.UseStaticFiles(); app.UseRouting(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization(); app.MapRazorPages();
using(var scope=app.Services.CreateScope()) { var db=scope.ServiceProvider.GetRequiredService<AppDb>(); await db.Database.EnsureCreatedAsync(); }
app.Run();
