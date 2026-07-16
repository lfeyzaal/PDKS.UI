using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PDKS.Data.Contexts;
using PDKS.Entities;
using System.Drawing;

namespace PDKS.UI.Areas.Personnel.Controllers
{
    [Area("Personnel")]
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context;
        public AttendanceController(AppDbContext context) { _context = context; }

        // GİRİŞ-ÇIKIŞ LİSTESİ (Dashboard)
        public async Task<IActionResult> Index()
        {
            var records = await _context.Attendances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
            return View(records);
        }

        // YENİ KAYIT SAYFASI
        public IActionResult Create()
        {
            ViewBag.Employees = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FirstName");
            return View();
        }

        // YENİ KAYIT İŞLEMİ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Attendance attendance)
        {
            ModelState.Remove("Employee");
            if (ModelState.IsValid)
            {
                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Employees = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FirstName", attendance.EmployeeId);
            return View(attendance);
        }

        // SİLME İŞLEMİ
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.Attendances.FindAsync(id);
            if (record != null)
            {
                _context.Attendances.Remove(record);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- YENİ EKLENEN EXCEL RAPORU ALMA İŞLEMİ ---
        public async Task<IActionResult> ExportToExcel()
        {
            // 1. Veritabanından kayıtları çek
            var records = await _context.Attendances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            // 2. Yeni bir Excel çalışma kitabı oluştur
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Giriş-Çıkış Raporu");

                // 3. Excel Başlıklarını Yazdır
                worksheet.Cell(1, 1).Value = "Personel Adı Soyadı";
                worksheet.Cell(1, 2).Value = "Tarih";
                worksheet.Cell(1, 3).Value = "Giriş Saati";
                worksheet.Cell(1, 4).Value = "Çıkış Saati";
                worksheet.Cell(1, 5).Value = "Durum Notu";

                // Başlıkları estetik yapalım (Kalın ve Arka plan rengi)
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
                worksheet.Row(1).Style.Font.FontColor = XLColor.White;

                // 4. Verileri Döngüyle Satır Satır Yazdır
                int row = 2; // 2. satırdan başla (1'de başlık var)
                foreach (var item in records)
                {
                    worksheet.Cell(row, 1).Value = $"{item.Employee?.FirstName} {item.Employee?.LastName}";
                    worksheet.Cell(row, 2).Value = item.Date.ToString("dd.MM.yyyy");
                    worksheet.Cell(row, 3).Value = item.CheckInTime?.ToString(@"hh\:mm") ?? "-";
                    worksheet.Cell(row, 4).Value = item.CheckOutTime?.ToString(@"hh\:mm") ?? "-";
                    worksheet.Cell(row, 5).Value = item.Note ?? "-";
                    row++;
                }

                // Sütun genişliklerini yazılara göre otomatik ayarla
                worksheet.Columns().AdjustToContents();

                // 5. Dosyayı İndirmeye Hazırla
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Personel_Giris_Cikis_Raporu.xlsx");
                }
            }
        }
    }
}