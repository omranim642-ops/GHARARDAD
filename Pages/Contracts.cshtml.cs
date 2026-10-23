using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Gharardad.Data;
namespace Gharardad.Pages;
public class ContractsModel(AppDb db):PageModel { [BindProperty(SupportsGet=true)]public string? Q{get;set;} [BindProperty(SupportsGet=true)]public string? Status{get;set;} [BindProperty(SupportsGet=true)]public int P{get;set;}=1; public int Total;public List<Contract> Items=[]; public async Task OnGetAsync(){P=Math.Clamp(P,1,100000);var q=db.Contracts.AsNoTracking();if(!string.IsNullOrWhiteSpace(Q))q=q.Where(x=>x.Title.Contains(Q)||x.Number.Contains(Q)||x.Party.Contains(Q));if(!string.IsNullOrEmpty(Status))q=q.Where(x=>x.Status==Status);Total=await q.CountAsync();Items=await q.OrderByDescending(x=>x.Id).Skip((P-1)*25).Take(25).ToListAsync();} }
