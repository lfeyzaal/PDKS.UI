using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDKS.Data.Contexts;
using PDKS.Entities;

namespace PDKS.UI.Areas.Personnel.Controllers
{
    [Area("Personnel")]
    public class DepartmentController : Controller
    {
        private readonly AppDbContext _context;

        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Include(d => d.Employees)
                .ToListAsync();
            return View(departments);
        }

        // 2. YENİ EKLEME (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. YENİ EKLEME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            ModelState.Remove("Employees");
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // 4. SİLME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}