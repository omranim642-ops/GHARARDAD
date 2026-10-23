using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Gharardad.Data;
namespace Gharardad.Pages;
public class EditModel(AppDb db):PageModel {
 [BindProperty]public Contract Input{get;set;}=new(); [BindProperty]public string Start{get;set;}=Shamsi.Format(DateTime.Today); [BindProperty]public string End{get;set;}=Shamsi.Format(DateTime.Today.AddYears(1));
 public async Task<IActionResult> OnGetAsync(int? id){if(id.HasValue){var c=await db.Contracts.FindAsync(id);if(c==null)return NotFound();Input=c;Start=Shamsi.Format(c.StartDate);End=Shamsi.Format(c.EndDate);}return Page();}
 public async Task<IActionResult> OnPostAsync(int? id){if((id??0)!=Input.Id)return BadRequest();if(!Shamsi.TryParse(Start,out var start))ModelState.AddModelError("Start","تاریخ شروع معتبر نیست؛ قالب سال/ماه/روز.");if(!Shamsi.TryParse(End,out var end))ModelState.AddModelError("End","تاریخ پایان معتبر نیست.");if(end<start)ModelState.AddModelError("End","تاریخ پایان باید پس از شروع باشد.");if(await db.Contracts.AnyAsync(x=>x.Number==Input.Number&&x.Id!=Input.Id))ModelState.AddModelError("Input.Number","شماره قرارداد تکراری است.");if(!ModelState.IsValid)return Page();var c=Input.Id==0?new Contract():await db.Contracts.FindAsync(Input.Id);if(c==null)return NotFound();c.Number=Input.Number;c.Title=Input.Title;c.Party=Input.Party;c.Amount=Input.Amount;c.Status=Input.Status;c.Notes=Input.Notes;c.StartDate=start;c.EndDate=end;if(c.Id==0)db.Contracts.Add(c);try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","ذخیره نشد؛ شماره قرارداد را بررسی کنید و دوباره تلاش کنید.");return Page();}return RedirectToPage("/Details",new{id=c.Id});}
}