using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Gharardad.Data;
namespace Gharardad.Pages;
public class DetailsModel(AppDb db):PageModel {
 public Contract Item{get;set;}=null!;[BindProperty]public IFormFile? Upload{get;set;}
 public async Task<IActionResult> OnGetAsync(int id){var c=await db.Contracts.AsNoTracking().Include(x=>x.Attachments).FirstOrDefaultAsync(x=>x.Id==id);if(c==null)return NotFound();Item=c;return Page();}
 public async Task<IActionResult> OnPostAsync(int id){var c=await db.Contracts.FindAsync(id);if(c==null)return NotFound();var ext=Path.GetExtension(Upload?.FileName??"").ToLowerInvariant();if(Upload==null||Upload.Length==0||Upload.Length>10*1024*1024||!new[]{".pdf",".png",".jpg",".jpeg",".docx",".xlsx"}.Contains(ext)){ModelState.AddModelError("","نوع فایل مجاز نیست یا اندازه فایل بیش از ۱۰ مگابایت است.");return await OnGetAsync(id);}using var ms=new MemoryStream();await Upload.CopyToAsync(ms);db.Attachments.Add(new Attachment{ContractId=id,Name=Path.GetFileName(Upload.FileName.Replace('\\','/')),Content=ms.ToArray()});await db.SaveChangesAsync();return RedirectToPage(new{id});}
 public async Task<IActionResult> OnGetDownloadAsync(int id,int fileId){var a=await db.Attachments.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==fileId&&x.ContractId==id);return a==null?NotFound():File(a.Content,"application/octet-stream",a.Name);}
 public async Task<IActionResult> OnPostRemoveAsync(int id,int fileId){var a=await db.Attachments.FirstOrDefaultAsync(x=>x.Id==fileId&&x.ContractId==id);if(a==null)return NotFound();db.Attachments.Remove(a);await db.SaveChangesAsync();return RedirectToPage(new{id});}
}