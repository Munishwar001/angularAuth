using Microsoft.AspNetCore.Identity;

namespace Backend.Model.UserModel
{
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
