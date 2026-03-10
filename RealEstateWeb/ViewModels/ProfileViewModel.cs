using System.ComponentModel.DataAnnotations;

namespace RealEstateWeb.ViewModels
{
    public class ProfileViewModel
    {
        // Email dùng để hiển thị (không cho sửa vì liên quan đến tài khoản đăng nhập)
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }
    }
}