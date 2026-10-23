using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace Gharardad.Pages;
[EnableRateLimiting("login")]
public class LoginModel(IConfiguration config):PageModel {
 [BindProperty] public string Password {get;set;}="";
 public async Task<IActionResult> OnPostAsync() {var expected=config["AdminPassword"]; if(string.IsNullOrEmpty(expected)){ModelState.AddModelError("","رمز مدیر در تنظیمات سرور تعریف نشده است.");return Page();} if(!CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(Password)),SHA256.HashData(Encoding.UTF8.GetBytes(expected)))) {ModelState.AddModelError("","رمز عبور صحیح نیست.");return Page();} await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,"مدیر")},CookieAuthenticationDefaults.AuthenticationScheme)));return RedirectToPage("/Index"); }
 public async Task<IActionResult> OnPostLogoutAsync(){await HttpContext.SignOutAsync();return RedirectToPage();}
}