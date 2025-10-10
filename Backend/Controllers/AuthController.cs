using Backend.Interfaces;
using Backend.Model.AuthModel;
using Backend.Model.ReturnModels;
using Backend.Model.UserModel;
using Backend.RepoHelper;

//using BCrypt.Net;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Apis.Auth;


namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private AuthHelper _authHelper;

        public AuthController(AuthHelper authHelper)
        { 
            _authHelper = authHelper;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignUpDto data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data.FullName))
                    return BadRequest(new { success = false, message = "Full Name cannot be empty" });

                if (string.IsNullOrWhiteSpace(data.Email))
                return BadRequest(new { success = false, message = "Email cannot be empty" });

                if (string.IsNullOrWhiteSpace(data.Password))
                    return BadRequest(new { success = false, message = "Password cannot be empty" });
                
                if(!data.Password.Equals(data.ConfirmPassword))
                    return BadRequest(new { success = false, message = "Password and ConfirmPassword are not same" });

                var result = await _authHelper.SignUpHelperAsync(data);

                return Ok(new { success = result.success, message = result.message });

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto data)
        {
            try
            {
                AuthReturn result = await  _authHelper.loginHelperAsync(data);
                if (result.success)
                {
                    return Ok(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> Forgotpasword([FromBody] ForgotPaswordDto data)
        {
            try
            {

                var result =  await _authHelper.Forgotpassword(data);
                if (result.success)
                {
                return Ok(new {success = result.success , message = result.message});
                }
                return BadRequest(new { success = false, message = result.message });

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("validate-token")]
        public IActionResult isLogin()
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {     

            var result = await _authHelper.ResetPasswordHelper(dto);

            if (result.success)
            {
                 return Ok(new { success = true , message = result.message });
            }
            return BadRequest(new { success = false, message = result.message });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenModel tokenModel)
        {   

            var result =  await _authHelper.RefreshToken(tokenModel);

            if (result.success)
            {
                return Ok(result);
            }

            return BadRequest(result);
            
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
        {
            
             var result =  await _authHelper.GoogleLogin(request);
           
            return Ok(result);
            
        }

        [HttpPost("microsoft-login")]
        public async Task<IActionResult> MicrosoftLogin([FromBody] MicrosoftLoginRequestDto request)
        {

            var result = await _authHelper.MicrosoftLogin(request);

            return Ok(result);

        }

        [HttpPut("enable-2fa")]
        [Authorize]
        public async Task<IActionResult> EnableTwoFactor()
        {
           
            var result = await _authHelper.EnableTwoFator(User); 
            if (result.success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            var result = await _authHelper.VerifyOtp(request , User);
            if (result.success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

    }
}
