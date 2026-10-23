using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Gharardad.Data;
namespace Gharardad.Pages;
public class IndexModel(AppDb db):PageModel {public int Total,Active,Expiring; public async Task OnGetAsync(){Total=await db.Contracts.CountAsync();Active=await db.Contracts.CountAsync(x=>x.Status=="فعال");var today=DateTime.Today;var end=today.AddDays(30);Expiring=await db.Contracts.CountAsync(x=>x.Status=="فعال"&&x.EndDate>=today&&x.EndDate<=end);} }
