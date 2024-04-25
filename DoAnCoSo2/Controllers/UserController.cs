using DoAnCoSo2.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoAnCoSo2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IEnumerable<ApplicationUser> GetUsers()
        {
            return _userManager.Users.ToList();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(ApplicationUser model)
        {
            var result = await _userManager.CreateAsync(model);
            if (result.Succeeded)
            {
                return Ok(model);
            }
            return BadRequest(result.Errors);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, ApplicationUser model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;
           
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(user);
            }
            return BadRequest(result.Errors);
        }
        // Thêm các phương thức khác như cập nhật, xóa người dùng, vv.
    }

}
