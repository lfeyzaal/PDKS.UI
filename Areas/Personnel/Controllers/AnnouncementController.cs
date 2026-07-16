using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDKS.Data.Contexts;
using PDKS.Entities;

namespace PDKS.UI.Areas.Personnel.Controllers
{
    [Area("Personnel")]
    public class AnnouncementController : Controller
    {
        private readonly AppDbContext _context;

        // Veritabanı bağlantımızı (context) başlatıyoruz
        public AnnouncementController(AppDbContext context)
        {
            _context = context;
        }

        // 1. DUYURULARI LİSTELEME EKRANI (Ana Sayfa)
        public async Task<IActionResult> Index()
        {
            // Veritabanından duyuruları en yeniden en eskiye (OrderByDescending) doğru çek
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            // Çekilen verileri View'a (Görünüme) gönder
            return View(announcements);
        }

        // 2. YENİ DUYURU EKLEME SAYFASINI AÇ (Sadece formu gösterir)
        public IActionResult Create()
        {
            return View();
        }

        // 3. FORMDAN GELEN VERİYİ KAYDET (Butona basıldığında çalışır)
        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik önlemi: Dışarıdan sahte form gönderilmesini engeller
        public async Task<IActionResult> Create(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                announcement.CreatedDate = DateTime.Now; // Tarihi sistemden otomatik al
                announcement.IsActive = true; // Varsayılan olarak aktif yap

                _context.Announcements.Add(announcement); // Hafızaya ekle
                await _context.SaveChangesAsync(); // Veritabanına kesin olarak kaydet

                return RedirectToAction(nameof(Index)); // İşlem bitince listeye geri dön
            }
            return View(announcement); // Hata varsa formu tekrar göster
        }

        // 4. SİLME İŞLEMİ
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id); // Silinecek kaydı bul
            if (announcement != null)
            {
                _context.Announcements.Remove(announcement); // Hafızadan sil
                await _context.SaveChangesAsync(); // Değişikliği veritabanına yansıt
            }
            return RedirectToAction(nameof(Index)); // Listeye geri dön
        }
    }
}