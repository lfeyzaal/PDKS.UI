using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PDKS.Entities;
using PDKS.UI.Models;

namespace PDKS.UI.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // 1. Ekranı Getiren Metot
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 2. Giriş Yap Butonuna Basıldığında Çalışan Metot
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Mail adresine göre kullanıcıyı bul
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Şifresini kontrol et ve sistemi aç
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                    if (result.Succeeded)
                    {
                        // TODO: İleride burada İK ise Dashboard'a, Personel ise profile gönder diyeceğiz.
                        // Şimdilik başarılı olursa herkesi ana sayfaya atıyoruz.
                        return RedirectToAction("Index", "Home");
                    }
                }

                // Şifre veya mail yanlışsa hata ver
                ModelState.AddModelError("", "E-posta adresi veya şifre hatalı!");
            }
            return View(model);
        }

        // 3. Çıkış Yap Metodu
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }

        // 4. Yetkisiz Sayfaya Girmeye Çalışanları Atacağımız Sayfa
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}