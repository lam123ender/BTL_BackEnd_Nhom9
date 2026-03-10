using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Hubs;
using RealEstateWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. Đăng ký DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Đăng ký Identity (Sử dụng AppUser và IdentityRole)
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    // Tùy chỉnh policy password nếu cần
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Đăng ký MVC
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR(); // Đăng ký SignalR

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// 4. Cấu hình Routing hỗ trợ Area cho Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- BẮT ĐẦU ĐOẠN CODE TẠO ROLE ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // 1. Tạo Role "Administrator" nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync("Administrator"))
    {
        await roleManager.CreateAsync(new IdentityRole("Administrator"));
    }

    // 2. Tạo Role "Customer" (dành cho người dùng bình thường)
    if (!await roleManager.RoleExistsAsync("Customer"))
    {
        await roleManager.CreateAsync(new IdentityRole("Customer"));
    }

    // 3. Tìm tài khoản có UserName là 'admin' (bạn đã tạo bằng SQL) và gán quyền Administrator
    var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Administrator"))
    {
        await userManager.AddToRoleAsync(adminUser, "Administrator");
    }
}
// --- KẾT THÚC ĐOẠN CODE TẠO ROLE ---

// Map endpoint cho SignalR
app.MapHub<NotificationHub>("/notificationHub");

app.Run();