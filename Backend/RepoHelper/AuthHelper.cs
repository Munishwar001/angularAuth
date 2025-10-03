using Backend.Helper;
using Backend.Interfaces;
using Backend.Model.AuthModel;
using Backend.Model.ReturnModels;
using Backend.Model.UserModel;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.RepoHelper
{
    public class AuthHelper : IAuthHelper
    {
        private string _connectionString;

        private readonly IConfiguration _configuration;
        public AuthHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
        }

        public Users emailExist(string email)
        {
            try
            {
                var connection = new SqlConnection(_connectionString);

                Users count = connection.QueryFirstOrDefault<Users>("SELECT * FROM Users WHERE email = @Email", new { Email = email });

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception while checking email exitense " + ex.Message);
            }
        }
        public bool signUpHelper(SignUpDto data)
        {
            try
            {
                data = StringHelper.ToLowerCase(data);
                data.password = BCrypt.Net.BCrypt.HashPassword(data.password);

                var connection = new SqlConnection(_connectionString);
                var user = new Users
                {
                    fullName = data.fullName,
                    email = data.email,
                    password = data.password
                };
                if (emailExist(data.email)!=null)
                {
                    return false;
                }
                var insertedId = connection.Insert(user);

                return insertedId > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in SignUp Helper " + ex.Message);
            }
        }

        public AuthReturn loginHelper(LoginDto data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data.email))
                    return new AuthReturn { success = false, message = "Email cannot be empty" };

                if (string.IsNullOrWhiteSpace(data.password))
                    return new AuthReturn { success = false, message = "Password cannot be empty" };
                 
                var user = emailExist(data.email);
                  if(user == null)
                    return new AuthReturn { success = false, message = "User not found" };

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(data.password, user.password);

                if(!isPasswordValid)
                    return new AuthReturn { success = false, message = "Invalid Pasword" };

                var token = GenerateJwtToken(user.email , user.roles); 

                return new AuthReturn { success = true,message = "login successfully" , token = token };
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in login Helper " + ex.Message);
            }
        }

        public string GenerateJwtToken(string email ,string role)
        {
            var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                //new Claim(ClaimTypes.Role ,role)
                 new Claim("role", role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims : claims ,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
