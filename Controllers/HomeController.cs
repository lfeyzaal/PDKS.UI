using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // .Include ve veritabaný iþlemleri için þart!
using PDKS.Data.Contexts;
using System.Linq;

namespace PDKS.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. ÝSTATÝSTÝKLERÝ ÇEKÝYORUZ
            ViewBag.TotalEmployee = _context.Employees.Count();
            ViewBag.ActiveEmployee = _context.Employees.Count(e => e.IsActive);
            // Bugün izinli olanlarý buluyoruz (Baþlangýç bugünden küçük veya eþit, Bitiþ bugünden büyük veya eþit)
            ViewBag.OnLeaveEmployee = _context.LeaveRequests.Count(l => l.StartDate <= DateTime.Now && l.EndDate >= DateTime.Now);
            ViewBag.TotalDepartment = _context.Departments.Count();

            // 2. YAKLAÞAN ÝZÝNLERÝ ÇEKÝYORUZ (Sadece ileri tarihli olanlar)
            var upcomingLeaves = _context.LeaveRequests
                .Include(l => l.Employee) // Ýzin yapan kiþinin adýný almak için
                .Where(l => l.StartDate > DateTime.Now)
                .OrderBy(l => l.StartDate)
                .Take(5) // En yakýndaki 5 tanesini al
                .ToList();

            return View(upcomingLeaves); // Verileri Ana Sayfa'ya gönderiyoruz!
        }
    }
}