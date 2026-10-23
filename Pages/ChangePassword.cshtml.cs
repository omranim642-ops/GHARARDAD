using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Gharardad.Data;
namespace Gharardad.Pages;
[EnableRateLimiting("login")]
public class ChangePasswordModel(AppDb db,IPasswordHasher<AdminAccount> hasher):PageModel {
 [BindProperty,Required]public string CurrentPassword{get;set;}="";
 [BindProperty,Required,MinLength(12),MaxLength(128)]public string NewPassword{get;set;}="";
 [BindProperty,Compare(nameof(NewPassword))]public string ConfirmPassword{get;set;}="";
 public async Task<IActionResult> OnPostAsync(){
  if(!ModelState.IsValid)return Page();
  var admin=await db.Admins.SingleAsync();
  if(hasher.VerifyHashedPassword(admin,admin.PasswordHash,CurrentPassword)==PasswordVerificationResult.Failed)ModelState.AddModelError("","رمز فعلی صحیح نیست.");
  if(NewPassword==CurrentPassword || NewPassword=="Gharardad@12345" || !NewPassword.Any(char.IsUpper) || !NewPassword.Any(char.IsLower) || !NewPassword.Any(char.IsDigit))ModelState.AddModelError("","رمز جدید باید متفاوت و شامل حرف بزرگ، حرف کوچک و عدد باشد.");
  if(!ModelState.IsValid)return Page();
  admin.PasswordHash=hasher.HashPassword(admin,NewPassword);admin.MustChangePassword=false;admin.SecurityStamp=Guid.NewGuid().ToString("N");await db.SaveChangesAsync();
  await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,"admin"),new Claim("stamp",admin.SecurityStamp)},CookieAuthenticationDefaults.AuthenticationScheme)));
  return RedirectToPage("/Index");
 }
}