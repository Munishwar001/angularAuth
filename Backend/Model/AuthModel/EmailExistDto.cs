namespace Backend.Model.AuthModel
{
    public class EmailExistDto
    {  

        public bool EmailExist { get; set; }
       
        public string? Email { get; set; }

        public string? PasswordHash { get; set; }

    }
}
