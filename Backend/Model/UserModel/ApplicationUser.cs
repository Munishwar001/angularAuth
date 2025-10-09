using Microsoft.AspNetCore.Identity;

namespace Backend.Model.UserModel
{
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public string? RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryTime { get; set; } 

        public bool isDeleted { get; set; }

        public DateTime DeletedAt { get; set; }
    }
}
