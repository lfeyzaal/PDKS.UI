using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PDKS.Data.Contexts;
using PDKS.Entities;
using Microsoft.AspNetCore.Authorization;

[Area("Personnel")]
[Authorize] // Controller'a giren herkes giriş yapmış olmalı
public class LeaveRequestController : Controller
{
    private readonly AppDbContext _context;

    public LeaveRequestController(AppDbContext context)
    {
        _context = context;
    }

    // LİSTELEME
    public async Task<IActionResult> Index()
    {
        var query = _context.LeaveRequests
            .Include(l => l.Employee).ThenInclude(e => e.Department)
            .AsQueryable();

        var currentUserEmail = User.Identity.Name;

        // 1. ÖNCELİK: İK veya Admin ise her şeyi görsün
        if (User.IsInRole("IK") || User.IsInRole("Admin"))
        {
            // Filtre yok, hepsini getir
        }
        // 2. ÖNCELİK: Müdür ise kendi departmanındakileri görsün
        else if (User.IsInRole("Mudur"))
        {
            var currentManager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == currentUserEmail);
            if (currentManager != null)
            {
                query = query.Where(l => l.Employee.DepartmentId == currentManager.DepartmentId);
            }
        }
        // 3. ÖNCELİK: Personel ise sadece kendi verisini görsün
        else
        {
            var currentEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == currentUserEmail);
            if (currentEmployee != null)
            {
                query = query.Where(l => l.EmployeeId == currentEmployee.Id);
            }
        }

        var leaves = await query.OrderByDescending(l => l.StartDate).ToListAsync();
        return View(leaves);
    }

    // EKLEME (GET)
    public IActionResult Create()
    {
        ViewBag.Employees = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FirstName");
        ViewBag.LeaveTypes = new SelectList(new List<string> { "Yıllık İzin", "Ücretsiz İzin", "Mazeret İzni", "Raporlu İzin" });
        return View();
    }

    // EKLEME (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveRequest leave)
    {
        ModelState.Remove("Employee");
        if (ModelState.IsValid)
        {
            _context.LeaveRequests.Add(leave);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Employees = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FirstName", leave.EmployeeId);
        ViewBag.LeaveTypes = new SelectList(new List<string> { "Yıllık İzin", "Ücretsiz İzin", "Mazeret İzni", "Raporlu İzin" }, leave.LeaveType);
        return View(leave);
    }

    // ONAYLAMA
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Mudur,IK")]
    public async Task<IActionResult> Approve(int id)
    {
        var leave = await _context.LeaveRequests.FindAsync(id);
        if (leave != null)
        {
            leave.Status = "Onaylandı";
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // REDDETME
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Mudur,IK")]
    public async Task<IActionResult> Reject(int id)
    {
        var leave = await _context.LeaveRequests.FindAsync(id);
        if (leave != null)
        {
            leave.Status = "Reddedildi";
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // SİLME
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Mudur,IK")] // Personel kendi kendine silmesin diye güvenlik ekledik
    public async Task<IActionResult> Delete(int id)
    {
        var leave = await _context.LeaveRequests.FindAsync(id);
        if (leave != null)
        {
            _context.LeaveRequests.Remove(leave);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}