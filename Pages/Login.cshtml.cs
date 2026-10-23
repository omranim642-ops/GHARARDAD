using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Gharardad.Data;
namespace Gharardad.Pages;
[EnableRateLimiting("login")]
public class LoginModel(AppDb db,IPasswordHasher<AdminAccount> hasher):PageModel {
 [BindProperty] public string Username {get;set;}="admin";
 [BindProperty] public string Password {get;set;}="";
 public async Task<IActionResult> OnPostAsync() {
  var admin=await db.Admins.SingleAsync();
  var verified=hasher.VerifyHashedPassword(admin,admin.PasswordHash,Password);
  if(Username!="admin" || verified==PasswordVerificationResult.Failed) {ModelState.AddModelError("","نام کاربری یا رمز عبور صحیح نیست.");return Page();}
  if(verified==PasswordVerificationResult.SuccessRehashNeeded){admin.PasswordHash=hasher.HashPassword(admin,Password);await db.SaveChangesAsync();}
  await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,"admin"),new Claim("stamp",admin.SecurityStamp)},CookieAuthenticationDefaults.AuthenticationScheme)));
  return RedirectToPage(admin.MustChangePassword?"/ChangePassword":"/Index");
 }
 public async Task<IActionResult> OnPostLogoutAsync(){await HttpContext.SignOutAsync();return RedirectToPage();}
}