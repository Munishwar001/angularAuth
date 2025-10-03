using Dapper;

namespace Backend.Model.AuthModel
{
    public class LoginDto
    {
        [Required]
        public string email {  get; set; }

        [Required]
        public string password { get; set; }

       
    }
}
