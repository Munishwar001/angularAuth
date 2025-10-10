using Backend.Model.AuthModel;
using Backend.Model.UserModel;

namespace Backend.Model.ReturnModels
{
    public class AuthReturn
    { 
        public bool success { get; set; }

        public string message { get; set; }

        public string token { get; set; }

        public string refreshToken { get; set; }

        public Users user { get; set; }

        public bool? isTwoFactorRequired { get; set; }

        public string? email { get; set; }
    }
}
