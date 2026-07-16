using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDKS.Data.Contexts;
using System.Linq;
using Microsoft.AspNetCore.Authorization; // Güvenlik için gerekli kütüphane

namespace PDKS.UI.Controllers
{
    [Authorize] // ÝÞTE SÝHÝRLÝ KÝLÝT! Giriþ yapmayan kimse bu sayfayý göremez.
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

            // Bugün izinli olanlarý buluyoruz
            ViewBag.OnLeaveEmployee = _context.LeaveRequests.Count(l => l.StartDate <= DateTime.Now && l.EndDate >= DateTime.Now);

            ViewBag.TotalDepartment = _context.Departments.Count();

            // 2. YAKLAÞAN ÝZÝNLERÝ ÇEKÝYORUZ
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