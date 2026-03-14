using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using RealEstateWeb.Hubs;

namespace RealEstateWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public HomeController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Bổ sung tham số int page = 1 vào đây
        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice, int page = 1)
        {
            int pageSize = 6; // Số lượng BĐS trên 1 trang

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

            // --- LOGIC PHÂN TRANG  ---
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Sử dụng Skip và Take để cắt dữ liệu theo trang
            var result = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Gửi danh sách danh mục sang View để hiển thị trong Dropdown tìm kiếm
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", categoryId);

            // Giữ lại các giá trị đã lọc và số trang để hiển thị trên View
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentMinPrice"] = minPrice;
            ViewData["CurrentMaxPrice"] = maxPrice;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

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

                // ===== BÁO CÁO CHO ADMIN QUA SIGNALR =====
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"📞 Có liên hệ mới từ: {contact.FullName} ({contact.PhoneNumber})");
                // ==============================================

                TempData["SuccessMessage"] = "Gửi thông tin thành công! Chúng tôi sẽ liên hệ lại với bạn sớm nhất.";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ họ tên và số điện thoại.";
            }

            return RedirectToAction("Details", new { id = contact.PropertyId });
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}