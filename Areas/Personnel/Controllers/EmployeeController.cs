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
                    var result = await _userManager.CreateAsync(user, "123");
                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(employee.Title) && employee.Title.Contains("Müdür"))
                            await _userManager.AddToRoleAsync(user, "Mudur");
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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.Include(e => e.Department).FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }
    }
}