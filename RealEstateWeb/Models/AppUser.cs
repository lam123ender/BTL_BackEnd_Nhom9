using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RealEstateWeb.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property: Một user có thể đăng nhiều bất động sản
        public ICollection<Property> Properties { get; set; }
    }
}
