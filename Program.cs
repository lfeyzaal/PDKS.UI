using Microsoft.EntityFrameworkCore;
using PDKS.Data.Contexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// AREA ROTASI - Tek ve Doðru Rota
app.MapAreaControllerRoute(
    name: "Personnel",
    areaName: "Personnel",
    pattern: "Personnel/{controller=Employee}/{action=Index}/{id?}");

// DEFAULT ROTA
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();