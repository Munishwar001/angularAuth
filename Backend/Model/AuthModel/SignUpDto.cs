﻿namespace Backend.Model.AuthModel
{
    public class SignUpDto
    {     
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}
