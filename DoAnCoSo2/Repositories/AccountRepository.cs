using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using DoAnCoSo2.Data;
using DoAnCoSo2.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DoAnCoSo2.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo2.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.roleManager = roleManager;
        }

        public async Task<string> SignInAsync(SignInModel model)
        {
         
            var user = await userManager.FindByEmailAsync(model.Email);
            var passwordValid = await userManager.CheckPasswordAsync(user, model.Password);
            
            if (user == null || !passwordValid) {
                return string.Empty;
            }
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (!result.Succeeded)
            {
                return string.Empty;
            }

            var authClaims = new List<Claim>
            {
                
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("AvatarUrl", user.AvatarUrl)
               
            };
            var userRoles = await userManager.GetRolesAsync(user);
            foreach(var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMonths(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<IdentityResult> SignUpAsync(SignUpModel model)
        {
            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                AvatarUrl = "https://i.imgur.com/BvIv2iv.png"
            };

            var resuls= await userManager.CreateAsync(user, model.Password);
            if (resuls.Succeeded)
            {
                //kiemtrarole cus
                if(!await roleManager.RoleExistsAsync(AppRole.Customer))
                {
                    await roleManager.CreateAsync(new IdentityRole(AppRole.Customer));
                }
                await userManager.AddToRoleAsync(user, AppRole.Customer);
            }
            return resuls;
        }
        public async Task<IdentityResult> UpdateUserRoleAsync(string userId, string newRole)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, userRoles);
            if (!result.Succeeded)
            {
                return result;
            }

            result = await userManager.AddToRoleAsync(user, newRole);
            return result;
        }
        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
        {
            return await userManager.Users.ToListAsync();
        }
        public async Task<IdentityResult> UpdateUserAsync(string userId, ApplicationUser model)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;

            return await userManager.UpdateAsync(user);
        }
        public async Task<IdentityResult> LockoutUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"User with ID {userId} not found");
            }

            return await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(1)); // Lockout tài khoản trong 100 năm
        }
        public async Task<IdentityResult> UnlockUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"User with ID {userId} not found");
            }

            return await userManager.SetLockoutEndDateAsync(user, null); // Xóa thời gian khóa
        }
        public async Task<IEnumerable<string>> GetRolesAsync()
        {
            var roles = await roleManager.Roles.ToListAsync();
            return roles.Select(r => r.Name); // Chỉ trả về tên của các vai trò
        }
        public async Task<IdentityResult> UpdateUserAndRoleAsync(string userId, ApplicationUser model, string newRole)
        {
            // Kiểm tra xem vai trò mới có tồn tại không
            if (!await roleManager.RoleExistsAsync(newRole))
            {
                // Nếu không tồn tại, có thể xử lý tùy thuộc vào yêu cầu của bạn
                return IdentityResult.Failed(new IdentityError { Description = "Role does not exist" });
            }

            // Lấy người dùng từ ID
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            // Cập nhật thông tin người dùng
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;

            var userResult = await userManager.UpdateAsync(user);
            if (!userResult.Succeeded)
            {
                return userResult;
            }

            // Xóa tất cả các vai trò hiện tại của người dùng
            var currentRoles = await userManager.GetRolesAsync(user);
            var removeRoleResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRoleResult.Succeeded)
            {
                return removeRoleResult;
            }

            // Thêm vai trò mới cho người dùng
            var addRoleResult = await userManager.AddToRoleAsync(user, newRole);
            return addRoleResult;
        }
    }
}
