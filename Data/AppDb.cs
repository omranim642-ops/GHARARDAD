using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
namespace Gharardad.Data;
public class Contract {
 public int Id {get;set;}
 [Required, StringLength(100)] public string Number {get;set;}="";
 [Required, StringLength(250)] public string Title {get;set;}="";
 [Required, StringLength(250)] public string Party {get;set;}="";
 [Range(0, 999999999999999)] public decimal Amount {get;set;}
 public DateTime StartDate {get;set;}
 public DateTime EndDate {get;set;}
 [Required, RegularExpression("^(پیش‌نویس|فعال|خاتمه‌یافته|لغوشده)$")] public string Status {get;set;}="پیش‌نویس";
 [StringLength(4000)] public string? Notes {get;set;}
 public List<Attachment> Attachments {get;set;}=[];
}
public class Attachment { public int Id {get;set;} public int ContractId {get;set;} public string Name {get;set;}=""; public byte[] Content {get;set;}=[]; }
public class AdminAccount {
 public int Id {get;set;}
 public string PasswordHash {get;set;}="";
 public bool MustChangePassword {get;set;}=true;
 public string SecurityStamp {get;set;}=Guid.NewGuid().ToString("N");
}
public class AppDb(DbContextOptions<AppDb> options):DbContext(options) {
 public DbSet<AdminAccount> Admins=>Set<AdminAccount>();
 public DbSet<Contract> Contracts=>Set<Contract>(); public DbSet<Attachment> Attachments=>Set<Attachment>();
 protected override void OnModelCreating(ModelBuilder b) { b.Entity<Contract>().HasIndex(x=>x.Number).IsUnique(); b.Entity<Contract>().Property(x=>x.Amount).HasPrecision(18,0); b.Entity<Attachment>().HasOne<Contract>().WithMany(x=>x.Attachments).HasForeignKey(x=>x.ContractId).OnDelete(DeleteBehavior.Cascade); }
}
public static class Shamsi {
 public static string Format(DateTime d) {var p=new PersianCalendar(); return $"{p.GetYear(d):0000}/{p.GetMonth(d):00}/{p.GetDayOfMonth(d):00}";}
 public static bool TryParse(string? s,out DateTime d) {d=default;try { var normalized=string.Concat((s??"").Select(c=>c>='۰'&&c<='۹'?(char)('0'+c-'۰'):c>='٠'&&c<='٩'?(char)('0'+c-'٠'):c)); var a=normalized.Split('/'); if(a.Length!=3)return false; d=new PersianCalendar().ToDateTime(int.Parse(a[0]),int.Parse(a[1]),int.Parse(a[2]),0,0,0,0);return true;}catch{return false;} }
}
