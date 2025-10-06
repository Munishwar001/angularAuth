//using Backend.Helper;
using Backend.Interfaces;
using Backend.Model.AuthModel;
using Backend.Model.ReturnModels;
using Backend.Model.UserModel;
using Backend.Services;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web.Helpers;
using System.Web.Mvc;


namespace Backend.RepoHelper
{
    public class AuthHelper 
    {
        private string _connectionString;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public readonly EmailService _emailService;

        private readonly IConfiguration _configuration;
        public AuthHelper(IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, EmailService emailService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        public async Task<EmailExistDto?> EmailExistAsync(string email)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser == null)
                {
                    return null;
                }
                return new EmailExistDto {EmailExist = true ,Email = existingUser.Email , PasswordHash = existingUser.PasswordHash};
            }
            catch (Exception ex)
            {
                throw new Exception("Exception while checking email existence: " + ex.Message);
            }
        }

        public async Task<AuthReturn> SignUpHelperAsync(SignUpDto data)
        {
            try
            {
               
                var existingUser = await EmailExistAsync(data.Email);

                if (existingUser != null)
                    return new AuthReturn { success = false, message = "User Already Exist" };

                var user = new ApplicationUser
                {
                    FullName = data.FullName,
                    UserName = data.Email,
                    Email = data.Email
                };

                var result = await _userManager.CreateAsync(user, data.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new AuthReturn { success = false, message = "Unable to create user: " + errors };
                }

                if (!await _roleManager.RoleExistsAsync(data.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(data.Role));
                }

                await _userManager.AddToRoleAsync(user, data.Role);

                return new AuthReturn { success = true, message = "User Registered Successfully" };
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in signup helper: " + ex.Message);
            }
        }

        public async Task<AuthReturn> loginHelperAsync(LoginDto data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data.Email))
                    return new AuthReturn { success = false, message = "Email cannot be empty" };

                if (string.IsNullOrWhiteSpace(data.Password))
                    return new AuthReturn { success = false, message = "Password cannot be empty" };

                var user = await _userManager.FindByEmailAsync(data.Email);
                if (user == null)
                    return new AuthReturn { success = false, message = "User not found" };


                var passwordValid = await _userManager.CheckPasswordAsync(user, data.Password);
                if (!passwordValid)
                    return new AuthReturn { success = false, message = "Invalid password" };

                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user.Email, roles.FirstOrDefault() ?? "User");
                Console.WriteLine(roles);

                return new AuthReturn { success = true, message = "login successfully" , token = token};
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

        public async Task<AuthReturn> Forgotpassword(ForgotPaswordDto data)
        {
            try
            {
                //Console.WriteLine(email);
                if (string.IsNullOrEmpty(data.Email))
                {
                    return  new AuthReturn{ success = false, message = "Email i Required" };
                }
                var user = await _userManager.FindByEmailAsync(data.Email);
                if (user == null)
                    return new AuthReturn { success = false, message = "Email Not Registered" };

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var resetLink = $"http://localhost:52519/resetPassword?email={data.Email}&token={Uri.EscapeDataString(token)}";

                string subject = "Reset Your Password";
                string message = $"Click <a href='{resetLink}'>here</a> to reset your password.";

                await _emailService.SendEmailAsync(data.Email, subject, message);

                return new AuthReturn { success = true, message = "Reset link sent! Check your email." };

            }
            catch (Exception ex)
            {
                throw new Exception("Exception in  forgot helper  Helper " + ex.Message);
            }
        }
        public async Task<AuthReturn> ResetPasswordHelper(ResetPasswordDto dto)
        {
            try
            {   
                if(string.IsNullOrEmpty(dto.NewPassword) || string.IsNullOrEmpty(dto.ConfirmPassword))
                {
                    return new AuthReturn { success = false, message = "please fill the credential correctly" };
                }
                if (!dto.NewPassword.Equals(dto.ConfirmPassword))
                {
                    return new AuthReturn { success = false, message = "Pasword and ConfirmPassword are not same " };
                }
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null) return new  AuthReturn { success = false ,message = "User not found" };

                var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
                if (!result.Succeeded)
                    return new AuthReturn { success = false, message = "Enable to reset Password" };

                return new AuthReturn { success = true , message = "PasswordUpdated" };
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in  ResetPasword   Helper " + ex.Message);
            }
        }
    }
}
