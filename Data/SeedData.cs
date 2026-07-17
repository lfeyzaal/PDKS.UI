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
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new AppRole { Name = roleName });
                }
            }

            // 2. PATRON (İK) HESABINI KONTROL ET
            string adminEmail = "admin@pdks.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            // Eğer admin yoksa, oluştur
            if (adminUser == null)
            {
                AppUser newAdmin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Sistem",
                    LastName = "Yöneticisi"
                };
                // 3. TEST PERSONELİ HESABINI OLUŞTUR
                string personelEmail = "personel@pdks.com";
                var personelUser = await userManager.FindByEmailAsync(personelEmail);

                if (personelUser == null)
                {
                    AppUser newPersonel = new AppUser
                    {
                        UserName = personelEmail,
                        Email = personelEmail,
                        FirstName = "Deneme",
                        LastName = "Personel"
                    };

                    // Personel için de aynı şifreyi veriyoruz
                    await userManager.CreateAsync(newPersonel, "Sifre123.");
                    await userManager.AddToRoleAsync(newPersonel, "Personel");
                }

                // İlk kurulum şifresi
                await userManager.CreateAsync(newAdmin, "Sifre123.");
                await userManager.AddToRoleAsync(newAdmin, "IK");
            }
        }
    }
}