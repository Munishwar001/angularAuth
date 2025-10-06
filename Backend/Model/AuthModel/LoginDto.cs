using Dapper;

namespace Backend.Model.AuthModel
{
    public class LoginDto
    {
        [Required]
        public string Email {  get; set; }

        [Required]
        public string Password { get; set; }

       
    }
}
