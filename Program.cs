using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PDKS.Data.Contexts;
using PDKS.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 1. YENÝ EKLENEN: KÝMLÝK (IDENTITY) VE ROL AYARLARI ---
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    // Testleri kolaylaþtýrmak için þifre kurallarýný esnetiyoruz
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// --- 2. YENÝ EKLENEN: ÇEREZ (COOKIE) VE GÝRÝÞ YÖNLENDÝRME AYARLARI ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login"; // Giriþ yapmayan buraya atýlacak
    options.AccessDeniedPath = "/Auth/AccessDenied"; // Yetkisi yetmeyen buraya atýlacak
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(1); // Kullanýcý 1 gün boyunca sistemde açýk kalsýn
});
// -----------------------------------------------------------

var app = builder.Build();
// --- TOHUMLAMA (SEED) ÝÞLEMÝNÝ ÇAÐIR ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await PDKS.UI.Data.SeedData.Initialize(services);
}
// ---------------------------------------

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- 3. YENÝ EKLENEN: KÝMLÝK DOÐRULAMA (Sýrasý çok önemli, mutlaka Authorization'dan önce olmalý!) ---
app.UseAuthentication();
app.UseAuthorization();

// --- BÜTÜN ALANLARI VE CONTROLLERLARI OTOMATÝK TANIYAN ROUTE ---
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// --- ANA (DEFAULT) ROTA ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();