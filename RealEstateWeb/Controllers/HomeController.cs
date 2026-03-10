using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Models;
using System.Diagnostics;

namespace RealEstateWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            // 1. Khởi tạo query cơ bản
            var query = _context.Properties
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            // 2. Lọc theo từ khóa (Tiêu đề hoặc Địa chỉ)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm) || p.Address.Contains(searchTerm));
            }

            // 3. Lọc theo Danh mục
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // 4. Lọc theo khoảng giá
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Gửi danh sách danh mục sang View để hiển thị trong Dropdown tìm kiếm
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", categoryId);

            // Giữ lại các giá trị đã lọc để hiển thị trên Form
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentMinPrice"] = minPrice;
            ViewData["CurrentMaxPrice"] = maxPrice;

            var result = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(result);
        }
        // GET: /Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Lấy chi tiết BĐS kèm theo Danh mục và thông tin người đăng (User)
            var property = await _context.Properties
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null || !property.IsActive)
            {
                return NotFound();
            }

            return View(property);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitContact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.CreatedAt = DateTime.Now;
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Gửi thông tin thành công! Chúng tôi sẽ liên hệ lại với bạn sớm nhất.";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ họ tên và số điện thoại.";
            }

            // Quay lại đúng trang chi tiết của căn nhà đó
            return RedirectToAction("Details", new { id = contact.PropertyId });
        }
    }
}