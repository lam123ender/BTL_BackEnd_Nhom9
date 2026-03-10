using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Models;

namespace RealEstateWeb.Data;
public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Khai báo các bảng
    public DbSet<Category> Categories { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Bắt buộc phải có dòng base.OnModelCreating(builder) khi dùng Identity

        // Bạn có thể đổi tên các bảng Identity mặc định ở đây nếu muốn (tùy chọn)
        // builder.Entity<AppUser>().ToTable("Users");
        // builder.Entity<IdentityRole>().ToTable("Roles");
    }
}