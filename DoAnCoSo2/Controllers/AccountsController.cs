using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DoAnCoSo2.Models;
using DoAnCoSo2.Repositories;
using DoAnCoSo2.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DoAnCoSo2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository accountRepo;

        public AccountsController(IAccountRepository repo)
        {
            accountRepo = repo;
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(SignUpModel signUpModel)
        {
            var result = await accountRepo.SignUpAsync(signUpModel);
            if (result.Succeeded)
            {
                return Ok(result.Succeeded);
            }

            return StatusCode(500);
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            var result = await accountRepo.SignInAsync(signInModel);

            if (string.IsNullOrEmpty(result))
            {
                return Unauthorized();
            }

            return Ok(result);
        }
        [HttpPut("UpdateUserRole/{userId}")]
        public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] string newRole)
        {
            var result = await accountRepo.UpdateUserRoleAsync(userId, newRole);
            if (result.Succeeded)
            {
                return Ok(result);
            }

            return StatusCode(500, result.Errors);
        }
        [HttpGet("GetUsers")]
        public async Task<IEnumerable<ApplicationUser>> GetUsers()
        {
            return await accountRepo.GetUsersAsync();
        }
        //[HttpPut("UpdateUser/{userId}")]
        //[Authorize(Roles = "Administrator")] // Giả sử chỉ admin mới có quyền cập nhật thông tin người dùng
        //public async Task<IActionResult> UpdateUser(string userId, ApplicationUser model)
        //{
        //    var result = await accountRepo.UpdateUserAsync(userId, model);
        //    if (result.Succeeded)
        //    {
        //        return Ok(model);
        //    }
        //    return BadRequest(result.Errors);
        //}
        [HttpPut("LockoutUser/{userId}")]
        [Authorize(Roles = "Administrator")] // Giả sử chỉ admin mới có quyền lockout tài khoản
        public async Task<IActionResult> LockoutUser(string userId)
        {
            var result = await accountRepo.LockoutUserAsync(userId);
            if (result.Succeeded)
            {
                return Ok("User locked out successfully");
            }
            return BadRequest(result.Errors);
        }
        [HttpPut("UnLockoutUser/{userId}")]
    
        public async Task<IActionResult> UnLockoutUser(string userId)
        {
            var result = await accountRepo.UnlockUserAsync(userId);
            if (result.Succeeded)
            {
                return Ok("User unlocked out successfully");
            }
            return BadRequest(result.Errors);
        }
        [HttpGet("Roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await accountRepo.GetRolesAsync();
            return Ok(roles);
        }
        [HttpPut("UpdateUserAndRole/{userId}")]
        
        public async Task<IActionResult> UpdateUserAndRole(string userId, ApplicationUser model, string newRole)
        {
            var result = await accountRepo.UpdateUserAndRoleAsync(userId, model, newRole);
            if (result.Succeeded)
            {
                return Ok(model);
            }

            return StatusCode(500, result.Errors);
        }
        [HttpPost("SignOut")]
        [Authorize]
        public async Task<IActionResult> SignOut()
        {
            // Handle sign out logic here (invalidate token)
            // For example: Clear authentication cookie or invalidate JWT token
            return Ok("Sign out successful");
        }

        [HttpGet("CheckLoggedIn")]
        [Authorize]
        public IActionResult CheckLoggedIn()
        {
           
            return Ok("User is logged in");
        }
        [HttpGet("UserInfo")]
        [Authorize]
        public IActionResult UserInfo()
        {
            // Lấy thông tin người dùng từ JWT token
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var userAvatarUrl = User.FindFirstValue("avatarUrl"); // Lấy thông tin avatarUrl từ claim
            var userGivenName = User.FindFirstValue(ClaimTypes.GivenName); // Lấy thông tin givenname từ claim
            

            // Tạo đối tượng chứa thông tin người dùng
            var userInfo = new
            {
                Email = userEmail,
                Roles = userRoles,
                AvatarUrl = userAvatarUrl,
                GivenName = userGivenName
            };

            // Trả về thông tin người dùng
            return Ok(userInfo);
        }

        //[HttpPost("ResetPassword")]
        //public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        //{
        //    var result = await accountRepo.ResetPasswordAsync(resetPasswordModel.Email);
        //    if (result.Succeeded)
        //    {
        //        // Gửi mật khẩu mới đến email của người dùng (không được triển khai trong ví dụ này)
        //        return Ok("Password reset successful");
        //    }

        //    return StatusCode(500, result.Errors);
        //}

    }
}
