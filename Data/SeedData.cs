using Microsoft.AspNetCore.Identity;
using PDKS.Entities;

namespace PDKS.UI.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // 1. ÖNCE ROLLERİ OLUŞTURUYORUZ
            string[] roleNames = { "IK", "Mudur", "Personel" };
            foreach (var roleName in roleNames)
            {
                // Eğer bu rol veritabanında yoksa, yeni oluştur.
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new AppRole { Name = roleName });
                }
            }

            // 2. PATRON (İK) HESABINI OLUŞTURUYORUZ
            string adminEmail = "admin@pdks.com";

            // Eğer bu mailde biri yoksa, ekle.
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                AppUser adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Sistem",
                    LastName = "Yöneticisi"
                };

                // Şifreyi 123 olarak belirliyoruz
                IdentityResult result = await userManager.CreateAsync(adminUser, "123");

                if (result.Succeeded)
                {
                    // Oluşturduğumuz bu kişiye "IK" yetkisini veriyoruz.
                    await userManager.AddToRoleAsync(adminUser, "IK");
                }
            }
        }
    }
}