using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateWeb.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Liên kết với Bất động sản mà khách hàng đang quan tâm
        public int? PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        public Property? Property { get; set; }
    }
}