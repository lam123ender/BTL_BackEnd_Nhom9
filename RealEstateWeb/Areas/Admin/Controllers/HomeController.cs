using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Models;
using System.Diagnostics;

namespace RealEstateWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Thống kê tổng quan (Các con số hiển thị trên thẻ Card)
            ViewBag.TotalProperties = await _context.Properties.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            // 2. Lấy dữ liệu cho biểu đồ: Đếm số lượng BĐS nhóm theo Danh mục
            var propertyCountByCategory = await _context.Categories
                .Select(c => new
                {
                    CategoryName = c.Name,
                    Count = c.Properties.Count
                })
                .ToListAsync();

            // Chuyển đổi dữ liệu sang mảng để Javascript (Chart.js) có thể đọc được
            ViewBag.ChartLabels = propertyCountByCategory.Select(x => x.CategoryName).ToArray();
            ViewBag.ChartData = propertyCountByCategory.Select(x => x.Count).ToArray();

            return View();

        }
    }
}