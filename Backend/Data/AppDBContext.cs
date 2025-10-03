using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Backend.Model.UserModel;

namespace Backend.Data
{
    public class AppDBContext: IdentityDbContext<IdentityUser>
    { 
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<Users> Users { get; set; }
    }
}
