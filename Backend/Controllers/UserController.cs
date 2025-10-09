using Backend.Model.UserModel;
using Backend.RepoHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
           
namespace Backend.Controllers
{
    [ApiController]

    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private UserHelper _userHelper;
        public UserController(UserHelper userHelper) 
        {  
            _userHelper = userHelper;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Users")]
        public IActionResult getUsers()
        {
            try
            {
                var users = _userHelper.GetUsers();
                return Ok(new {success = true , message = "Users Fetched" , users = users});
            }
            catch (Exception ex)
            { 
                return BadRequest(new { success = false, message = ex.Message});
            }
        }

        [HttpPut("Update/{userId}")]
        public IActionResult updateUser(string userId, [FromBody] UpdateDto data)
        {
            try
            { 
                var users = _userHelper.UpdateData(userId , data);
                return Ok(new { success = users.success, message = users.message});
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{userId}")]
        public IActionResult Delete(string userId)
        {
            try
            {
                var result = _userHelper.DeleteUser(userId);
                return Ok(new { success = result.success, message = result.message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("Profile")]
        public IActionResult Profile(string userEmail)
        {
            try
            {
                var result = _userHelper.GetUser(userEmail);
                return Ok(new { success = result.success, message = result.message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
