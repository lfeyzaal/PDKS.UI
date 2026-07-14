using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PDKS.Data.Contexts;
using PDKS.Entities;

[Area("Personnel")]
public class LeaveRequestController : Controller
{
    private readonly AppDbContext _context;
    public LeaveRequestController(AppDbContext context) { _context = context; }

    // LİSTELEME
    public async Task<IActionResult> Index()
    {
        var leaves = await _context.LeaveRequests
            .Include(l => l.Employee).ThenInclude(e => e.Department)
            .OrderByDescending(l => l.StartDate).ToListAsync();
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

    // ONAYLAMA (YENİ EKLENDİ)
    [HttpPost]
    [ValidateAntiForgeryToken]
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

    // REDDETME (YENİ EKLENDİ)
    [HttpPost]
    [ValidateAntiForgeryToken]
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
    public async Task<IActionResult> Delete(int id)
    {
        var leave = await _context.LeaveRequests.FindAsync(id);
        if (leave != null) { _context.LeaveRequests.Remove(leave); await _context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}