using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PDKS.Data.Contexts;
using PDKS.Entities;

namespace PDKS.UI.Areas.Personnel.Controllers
{
    [Area("Personnel")]
    public class OvertimeRequestController : Controller
    {
        private readonly AppDbContext _context;

        public OvertimeRequestController(AppDbContext context)
        {
            _context = context;
        }

        // 1. MESAİ LİSTESİ (Dashboard)
        public async Task<IActionResult> Index()
        {
            var requests = await _context.OvertimeRequests
                .Include(o => o.Employee)
                .OrderByDescending(o => o.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // 2. YENİ MESAİ TALEBİ SAYFASI (Açılış)
        public IActionResult Create()
        {
            ViewBag.Employees = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FirstName");
            return View();
        }

        // 3. YENİ MESAİ TALEBİ KAYDETME (Form Gönderildiğinde)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OvertimeRequest overtime)
        {
            ModelState.Remove("Employee"); // Employee nesnesi doğrulamadan çıkarılır, Id yeterli

            if (ModelState.IsValid)
            {
                overtime.Status = "Bekliyor"; // Varsayılan durum
                _context.OvertimeRequests.Add(overtime);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employees = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FirstName", overtime.EmployeeId);
            return View(overtime);
        }

        // 4. DURUM GÜNCELLEME (Onayla / Reddet Butonları İçin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var request = await _context.OvertimeRequests.FindAsync(id);
            if (request != null)
            {
                request.Status = status; // "Onaylandı" veya "Reddedildi"
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 5. SİLME İŞLEMİ (Sil Butonu İçin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.OvertimeRequests.FindAsync(id);
            if (request != null)
            {
                _context.OvertimeRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}