using Backend.Helper;
using Backend.Interfaces;
using Backend.Model.AuthModel;
using Backend.Model.ReturnModels;
using Backend.Model.UserModel;
using Backend.RepoHelper;

//using BCrypt.Net;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private IAuthHelper _authHelper;
        public AuthController(IAuthHelper authHelper)
        { 
            _authHelper = authHelper;
        }

        [HttpPost("signup")]
        public IActionResult Signup(SignUpDto data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data.fullName))
                    return BadRequest(new { success = false, message = "Full Name cannot be empty" });

                if (string.IsNullOrWhiteSpace(data.email))
                    return BadRequest(new { success = false, message = "Email cannot be empty" });

                if (string.IsNullOrWhiteSpace(data.password) || string.IsNullOrWhiteSpace(data.confirmPassword))
                    return BadRequest(new { success = false, message = "Password cannot be empty" });
                
                if(!data.password.Equals(data.confirmPassword))
                    return BadRequest(new { success = false, message = "Password and ConfirmPassword are not same" });

                 bool result = _authHelper.signUpHelper(data);

                if (result)
                {
                    return Ok(new { success = true, message = "Signup successful!" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Signup failed. Email may already exist." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto data)
        {
            try
            {
                AuthReturn result = _authHelper.loginHelper(data);
                if (result.success)
                {
                    return Ok(result);
                }
                    
                return Ok(result);
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
    }
}
