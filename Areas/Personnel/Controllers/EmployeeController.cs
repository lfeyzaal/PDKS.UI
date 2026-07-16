using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PDKS.Data.Contexts;
using PDKS.Entities;
using Microsoft.EntityFrameworkCore;

namespace PDKS.UI.Areas.Personnel.Controllers
{
    [Area("Personnel")]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .OrderByDescending(e => e.HireDate)
                .ToListAsync();

            return View(employees);
        }

        // 2. YENİ EKLEME (GET)
        public IActionResult Create()
        {
            ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name");
            return View();
        }

        // 3. YENİ EKLEME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            ModelState.Remove("Department");

            if (ModelState.IsValid)
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // 4. PERSONEL DÜZENLEME EKRANINI AÇMA (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // 5. PERSONEL DÜZENLEME İŞLEMİNİ KAYDETME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id) return NotFound();

            ModelState.Remove("Department");

            if (ModelState.IsValid)
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // 6. PERSONEL DETAYI (YENİ EKLENDİ)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Attendances)
                .Include(e => e.LeaveRequests)
                .Include(e => e.OvertimeRequests)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        }
    }
}