using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWeb.Models
{
    public class Property
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Giá trị không được để trống")]
        public decimal Price { get; set; }

        public double Area { get; set; } // Diện tích (m2)

        [Required]
        public string Address { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true; // Trạng thái hiển thị

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- Khóa ngoại liên kết với Category ---
        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; } // Thêm dấu ?

        // --- Khóa ngoại liên kết với AppUser (Người đăng) ---
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; } // Thêm dấu ?


    }
}
