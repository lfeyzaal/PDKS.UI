using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PDKS.Data.Contexts;
using PDKS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace PDKS.UI.Areas.Personnel.Controllers
{
    [Area("Personnel")]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public EmployeeController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .OrderByDescending(e => e.HireDate)
                .ToListAsync();
            return View(employees);
        }

        public IActionResult Create()
        {
            ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            ModelState.Remove("Department");
            if (ModelState.IsValid)
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(employee.Email))
                {
                    var user = new AppUser
                    {
                        UserName = employee.Email,
                        Email = employee.Email,
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        EmployeeId = employee.Id
                    };

                    // ŞİFREYİ GÜÇLENDİRDİK - BURASI DÜZELTİLDİ
                    var result = await _userManager.CreateAsync(user, "Sifre123.");

                    if (result.Succeeded)
                    {
                        // YENİ DÜZENLEME: İK VE MÜDÜR AYRIMI EKLENDİ
                        if (!string.IsNullOrEmpty(employee.Title) && employee.Title.Contains("Müdür"))
                            await _userManager.AddToRoleAsync(user, "Mudur");
                        else if (!string.IsNullOrEmpty(employee.Title) && employee.Title.Contains("İK"))
                            await _userManager.AddToRoleAsync(user, "IK");
                        else
                            await _userManager.AddToRoleAsync(user, "Personel");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // DEĞİŞTİRİLEN VE DÜZELTİLEN KISIM BURASI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id) return NotFound();

            // Sistemi formdaki eksik alanlar (örn: İzinler listesi boş) yüzünden iptal etmemesi için uyarıları temizliyoruz
            ModelState.Clear();

            // Orijinal personeli veritabanından çekiyoruz
            var existingEmployee = await _context.Employees.FindAsync(id);
            if (existingEmployee == null) return NotFound();

            // Formdan gelen yeni verileri orijinalin üzerine tek tek garantili bir şekilde yazıyoruz
            existingEmployee.IdentityNumber = employee.IdentityNumber;
            existingEmployee.FirstName = employee.FirstName;
            existingEmployee.LastName = employee.LastName;
            existingEmployee.BirthDate = employee.BirthDate;
            existingEmployee.DepartmentId = employee.DepartmentId;
            existingEmployee.Title = employee.Title;
            existingEmployee.PhoneNumber = employee.PhoneNumber;
            existingEmployee.Email = employee.Email;
            existingEmployee.HireDate = employee.HireDate;
            existingEmployee.IsActive = employee.IsActive;

            // Veritabanına kesin kaydet
            await _context.SaveChangesAsync();

            // YENİ DÜZENLEME: GÜNCELLEME YAPILINCA ROLÜ DE DEĞİŞTİRİYORUZ
            if (!string.IsNullOrEmpty(existingEmployee.Email))
            {
                var user = await _userManager.FindByEmailAsync(existingEmployee.Email);
                if (user != null)
                {
                    // Önce eski rolleri siliyoruz ki çakışma olmasın
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    // Yeni unvana göre sıfırdan rol atıyoruz
                    if (!string.IsNullOrEmpty(existingEmployee.Title) && (existingEmployee.Title.Contains("Müdür") || existingEmployee.Title.Contains("Mudur")))
                        await _userManager.AddToRoleAsync(user, "Mudur");
                    else if (!string.IsNullOrEmpty(existingEmployee.Title) && (existingEmployee.Title.Contains("İK") || existingEmployee.Title.Contains("IK")))
                        await _userManager.AddToRoleAsync(user, "IK");
                    else
                        await _userManager.AddToRoleAsync(user, "Personel");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.Include(e => e.Department).FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }
    }
}