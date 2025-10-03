using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Model.UserModel
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id {  get; set; }

        [Required]
        [MaxLength(100)]
        public string fullName { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string email { get; set; }

        [Required] 
        [MaxLength(255)]
        public string password { get; set; }

        public string roles { get; set; }
    }
}
