using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;

namespace RealEstateWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Contact
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách liên hệ, kèm theo thông tin Bất động sản khách quan tâm
            var contacts = await _context.Contacts
                .Include(c => c.Property)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(contacts);
        }

        // POST: Admin/Contact/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa liên hệ thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}