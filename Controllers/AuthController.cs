using Microsoft.AspNetCore.Mvc;
using PDKS.Entities;
using Microsoft.AspNetCore.Identity;
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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        if (await _userManager.IsInRoleAsync(user, "IK") || await _userManager.IsInRoleAsync(user, "Admin"))
                            return RedirectToAction("Index", "Home");
                        else
                            return RedirectToAction("Index", "LeaveRequest", new { area = "Personnel" });
                    }
                }
                ModelState.AddModelError("", "E-posta adresi veya şifre hatalı!");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}