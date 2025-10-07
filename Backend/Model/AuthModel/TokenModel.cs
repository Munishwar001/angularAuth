namespace Backend.Model.AuthModel
{
    public class TokenModel
    { 
        public required string AccessToken { get; set; }

        public required string RefreshToken { get; set; }


    }
}
