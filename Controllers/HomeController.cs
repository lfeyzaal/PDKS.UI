using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDKS.Data.Contexts;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;

namespace PDKS.UI.Controllers
{
    [Authorize] // Sadece giriþ yapanlar görebilir
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserEmail = User.Identity.Name;

            // 1. ÝK, MÜDÜR veya ADMIN ÝSE (Yönetici Paneli)
            if (User.IsInRole("IK") || User.IsInRole("Mudur") || User.IsInRole("Admin"))
            {
                ViewBag.IsAdminView = true; // View'da kartlarý göstermek için

                ViewBag.TotalEmployee = await _context.Employees.CountAsync();
                ViewBag.ActiveEmployee = await _context.Employees.CountAsync(e => e.IsActive);
                ViewBag.OnLeaveEmployee = await _context.LeaveRequests.CountAsync(l => l.StartDate <= DateTime.Now && l.EndDate >= DateTime.Now);
                ViewBag.TotalDepartment = await _context.Departments.CountAsync();

                var upcomingLeaves = await _context.LeaveRequests
                    .Include(l => l.Employee)
                    .Where(l => l.StartDate > DateTime.Now)
                    .OrderBy(l => l.StartDate)
                    .Take(5)
                    .ToListAsync();

                return View(upcomingLeaves);
            }
            // 2. PERSONEL ÝSE (Kiþisel Panel)
            else
            {
                ViewBag.IsAdminView = false; // View'da kartlarý gizlemek için

                var currentEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == currentUserEmail);

                if (currentEmployee != null)
                {
                    // Personelin kendi verileri
                    ViewBag.KendiIzinlerim = await _context.LeaveRequests
                        .Where(l => l.EmployeeId == currentEmployee.Id)
                        .OrderByDescending(l => l.StartDate)
                        .Take(5)
                        .ToListAsync();

                    // Mesai taleplerini çek (Eðer Overtime tablon varsa)
                    // ViewBag.Mesailerim = await _context.OvertimeRequests.Where(o => o.EmployeeId == currentEmployee.Id).ToListAsync();
                }

                return View(); // Personel için model göndermiyoruz veya boþ gönderiyoruz
            }
        }
    }
}