using Backend.Interfaces;
using Backend.Model.AuthModel;
using Backend.Model.ReturnModels;
using Backend.Model.UserModel;
using Backend.Services;
using Dapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web.Helpers;
using System.Web.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Backend.RepoHelper
{
    public class AuthHelper
    {
        private string _connectionString;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public readonly EmailService _emailService;
        public readonly GoogleAuthSettings _googleSettings;
        public readonly EncryptionHelper _encryptionHelper;
        private readonly IConfiguration _configuration;
        public AuthHelper(IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, EmailService emailService, IOptions<GoogleAuthSettings> GoogleOptions , EncryptionHelper encryptionHelper)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _googleSettings = GoogleOptions.Value;
            _encryptionHelper = encryptionHelper;
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
                return new EmailExistDto { EmailExist = true, Email = existingUser.Email, PasswordHash = existingUser.PasswordHash };
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
                {
                    await _userManager.AccessFailedAsync(user); 

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        return new AuthReturn { success = false, message = "Account locked. Try again later." };
                    }

                    return new AuthReturn { success = false, message = "Invalid credentials" };
                }

                await _userManager.ResetAccessFailedCountAsync(user);

                if (user.TwoFactorEnabled)
                {
                    await sendOtp(user.Email);
                     string encryEmail = _encryptionHelper.Encrypt(user.Email);
                    return new AuthReturn
                    {
                        success = true,
                        message = "2FA required",
                        isTwoFactorRequired = true,
                        email = encryEmail
                    };
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user.Email, roles.FirstOrDefault() ?? "User");
                //Console.WriteLine(roles);

                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                return new AuthReturn { success = true, message = "login successfully", token = token, refreshToken = refreshToken };
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in login Helper " + ex.Message);
            }
        }

        public string GenerateJwtToken(string email, string role)
        {
            var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                //new Claim(ClaimTypes.Role ,role)
                 new Claim("role", role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<AuthReturn> Forgotpassword(ForgotPaswordDto data)
        {
            try
            {
                //Console.WriteLine(email);
                if (string.IsNullOrEmpty(data.Email))
                {
                    return new AuthReturn { success = false, message = "Email i Required" };
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
                if (string.IsNullOrEmpty(dto.NewPassword) || string.IsNullOrEmpty(dto.ConfirmPassword))
                {
                    return new AuthReturn { success = false, message = "please fill the credential correctly" };
                }
                if (!dto.NewPassword.Equals(dto.ConfirmPassword))
                {
                    return new AuthReturn { success = false, message = "Pasword and ConfirmPassword are not same " };
                }
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null) return new AuthReturn { success = false, message = "User not found" };

                var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
                if (!result.Succeeded)
                    return new AuthReturn { success = false, message = "Enable to reset Password" };

                return new AuthReturn { success = true, message = "PasswordUpdated" };
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in  ResetPasword   Helper " + ex.Message);
            }
        }

        public async Task<AuthReturn> RefreshToken(TokenModel tokenModel)
        {
            try
            {
                if (tokenModel is null)
                    return new AuthReturn { success = false, message = "Unauthorized" };

                var principal = GetPrincipalFromExpiredToken(tokenModel.AccessToken);
                if (principal == null)
                    return new AuthReturn { success = false, message = "Invalid access token" };

                var email = principal.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null ||
                    user.RefreshToken != tokenModel.RefreshToken ||
                    user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return new AuthReturn { success = false, message = "Invalid refresh token" };
                }
                var roles = await _userManager.GetRolesAsync(user);
                var newAccessToken = GenerateJwtToken(user.Email, roles.FirstOrDefault() ?? "User");
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                await _userManager.UpdateAsync(user);

                return new AuthReturn { success = true, message = "Token Refreshed", token = newAccessToken, refreshToken = newRefreshToken };

            }
            catch (Exception ex)
            {
                throw new Exception("Exception in  RefreshToken  Helper " + ex.Message);
            }
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is JwtSecurityToken jwt &&
                    jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public async Task<AuthReturn> GoogleLogin(GoogleLoginRequestDto request)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _googleSettings.ClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                if (payload == null)
                {
                    return new AuthReturn { success = false, message = "Invalid Google token" };
                }

                string email = payload.Email;
                string name = payload.Name;
                Console.WriteLine("The Email Which we get from the payload is => " + email);
                Console.WriteLine(" And the Name is => " + name);

                var user = await _userManager.Users
                 .Where(u => u.Email == email && !u.isDeleted)
                 .FirstOrDefaultAsync();

                if (user == null)
                {
                    return new AuthReturn { success = false, message = "Please Request the Admin to Admit you " };
                }

                if (user.TwoFactorEnabled)
                {
                    // Send OTP
                    await sendOtp(user.Email);

                    string encryEmail = _encryptionHelper.Encrypt(user.Email);

                    return new AuthReturn
                    {
                        success = true,
                        message = "2FA required",
                        isTwoFactorRequired = true,
                        email = encryEmail
                    };
                }


                string token = GenerateJwtToken(email, "User");
                string refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return new AuthReturn { success = false, message = "Enable to Update the User" };
                }
                return new AuthReturn { success = true, message = "Token Refreshed", token = token, refreshToken = refreshToken };
            }
            catch (InvalidJwtException ex)
            {
                Console.WriteLine("Invalid token: " + ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error validating token: " + ex.Message);
                throw;
            }
        }

        public async Task<AuthReturn> MicrosoftLogin(MicrosoftLoginRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return new AuthReturn { success = false, message = "ID token is required" };
                }

                
                string authority = $"https://login.microsoftonline.com/{_configuration["AzureAd:TenantId"]}/v2.0";
                var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{authority}/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever()
                );

                var openIdConfig = await configManager.GetConfigurationAsync(CancellationToken.None);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://login.microsoftonline.com/{_configuration["AzureAd:TenantId"]}/v2.0",
                    ValidateAudience = true,
                    ValidAudience = _configuration["AzureAd:ClientId"],
                    ValidateLifetime = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys
                };

                var handler = new JwtSecurityTokenHandler();
                ClaimsPrincipal principal = handler.ValidateToken(request.IdToken, tokenValidationParameters, out SecurityToken validatedToken);

                if (principal == null)
                {
                    return new AuthReturn { success = false, message = "Invalid Microsoft token" };
                }

                var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("preferred_username")?.Value;
                var name = principal.FindFirst("name")?.Value ?? "MicrosoftUser";

                var user = await _userManager.Users
                    .Where(u => u.Email == email && !u.isDeleted)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return new AuthReturn { success = false, message = "Account Not found Please Register" };
                }

                if (user.TwoFactorEnabled)
                {
                    // Send OTP
                    await sendOtp(user.Email);

                    string encryEmail = _encryptionHelper.Encrypt(user.Email);

                    return new AuthReturn
                    {
                        success = true,
                        message = "2FA required",
                        isTwoFactorRequired = true,
                        email = encryEmail
                    };
                }

                string token = GenerateJwtToken(email, "User");
                string refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return new AuthReturn { success = false, message = "Unable to update user" };
                }

                return new AuthReturn { success = true, message = "Microsoft login successful", token = token, refreshToken = refreshToken };
            }
            catch (SecurityTokenValidationException ex)
            {
                Console.WriteLine("Invalid Microsoft token: " + ex.Message);
                return new AuthReturn { success = false, message = "Invalid Microsoft token" };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error validating Microsoft token: " + ex.Message);
                throw;
            }
        }

        public async Task<AuthReturn> EnableTwoFator(ClaimsPrincipal userPrincipal)
        {
            try
            {
                var email = userPrincipal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    return new AuthReturn { success = false, message = "Email not found in token" };
                }
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new AuthReturn {success = false,message = "User not found" };
                }

                var result = await _userManager.SetTwoFactorEnabledAsync(user, true);

                if (result.Succeeded)
                {
                    return new AuthReturn {success = true, message = "Two-Factor Authentication enabled successfully!"};
                }
                else
                {
                    return new AuthReturn { success = false,message = "Failed to enable 2FA"};
                }
            }
            catch (Exception ex)
            {
                return new AuthReturn {success = false, message = ex.Message };
            }
        }

        public async Task<AuthReturn> sendOtp(string email)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(email))
                    return new AuthReturn { success = false, message = "Login First" };

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return new AuthReturn { success = false, message = "User not found" };

                var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                await _emailService.SendEmailAsync(user.Email, "2FA", $"Your code is {code}");

                return new AuthReturn { success = true, message = "Two-Factor Authentication enabled successfully!" };
            }
            catch(Exception ex)
            {
                return new AuthReturn { success = true, message = "Two-Factor Authentication enabled successfully!" };
            }
        }
        public async Task<AuthReturn> VerifyOtp(VerifyOtpRequestDto request , ClaimsPrincipal userPrincipal)
        {
            try
            {
               var email = userPrincipal.FindFirstValue(ClaimTypes.Email);

                if(string.IsNullOrWhiteSpace(request.Email))
                    return new AuthReturn { success = false, message = "Login First" };

                request.Email = _encryptionHelper.Decrypt(request.Email);
                var user = await _userManager.FindByEmailAsync(request.Email);

                var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", request.Otp);

                if (!isValid)
                    return new AuthReturn { success = false, message = "OTP is not Valid" };
                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user.Email, roles.FirstOrDefault() ?? "User");

                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                return new AuthReturn
                {
                    success = true,
                    message = "OTP verified successfully",
                    token = token,
                    refreshToken = refreshToken
                };
            } 
            catch (Exception ex)
            {
                Console.WriteLine("Error  VerifyOtp: " + ex.Message);
                throw;
            }
        }
    }
}
