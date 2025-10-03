namespace Backend.Model.ReturnModels
{
    public class AuthReturn
    { 
        public bool success { get; set; }

        public string message { get; set; }

        public string? token { get; set; }
    }
}
