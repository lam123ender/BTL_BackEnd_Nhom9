using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Models;
using System.Data;

namespace RealEstateWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropertyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Property
        public async Task<IActionResult> Index()
        {
            // Dùng Include để lấy kèm thông tin Tên Danh Mục và Tên Người Đăng
            var properties = _context.Properties
                .Include(p => p.Category)
                .Include(p => p.User)
                .OrderByDescending(p => p.Id);
            return View(await properties.ToListAsync());
        }

        // GET: Admin/Property/Create
        public IActionResult Create()
        {
            // Tạo danh sách thả xuống cho Category và User
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: Admin/Property/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Price,Area,Address,Description,ImageUrl,IsActive,CategoryId,UserId")] Property @property)
        {
            if (ModelState.IsValid)
            {
                @property.CreatedAt = DateTime.Now;
                _context.Add(@property);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm bất động sản thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi, load lại dropdown
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", @property.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", @property.UserId);
            return View(@property);
        }

        // Tạm thời mình dừng ở Index và Create để bạn test trước cho đỡ rối nhé.
        // GET: Admin/Property/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            // Load lại dữ liệu cho Dropdown List, chọn sẵn giá trị hiện tại của BĐS
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", property.UserId);
            return View(property);
        }

        // POST: Admin/Property/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Price,Area,Address,Description,ImageUrl,IsActive,CreatedAt,CategoryId,UserId")] Property property)
        {
            if (id != property.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(property);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật bất động sản thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(property.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại Dropdown
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", property.UserId);
            return View(property);
        }

        // GET: Admin/Property/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // Include để lấy tên Danh mục và Người đăng hiển thị lên View xác nhận xóa
            var property = await _context.Properties
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null) return NotFound();

            return View(property);
        }

        // POST: Admin/Property/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property != null)
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa bất động sản thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
    }
}