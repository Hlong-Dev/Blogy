using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using DoAnCoSo2.Models;
using Neo4jClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using DoAnCoSo2.Data;
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
        private readonly IGraphClient _client;

        public AccountRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, IGraphClient client)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.roleManager = roleManager;
            _client = client;
        }

        public async Task<string> SignInAsync(SignInModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            var passwordValid = await userManager.CheckPasswordAsync(user, model.Password);

            if (user == null || !passwordValid)
            {
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
            foreach (var role in userRoles)
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

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await roleManager.RoleExistsAsync(AppRole.Customer))
                {
                    await roleManager.CreateAsync(new IdentityRole(AppRole.Customer));
                }
                await userManager.AddToRoleAsync(user, AppRole.Customer);

                await _client.Cypher
                    .Create("(u:User { Id: $userId, UserName: $userName, Email: $email, AvatarUrl: $avatarUrl })")
                    .WithParams(new { userId = user.Id, userName = user.UserName, email = user.Email, avatarUrl = user.AvatarUrl })
                    .ExecuteWithoutResultsAsync();
            }

            return result;
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

            if (result.Succeeded)
            {
                // Cập nhật role trong Neo4j
                await _client.Cypher
                    .Match("(u:User {Id: $userId})")
                    .Set("u.Role = $newRole")
                    .WithParams(new { userId, newRole })
                    .ExecuteWithoutResultsAsync();
            }

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

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Cập nhật thông tin trong Neo4j
                await _client.Cypher
                    .Match("(u:User {Id: $userId})")
                    .Set("u.UserName = $userName, u.PhoneNumber = $phoneNumber")
                    .WithParams(new { userId, userName = model.UserName, phoneNumber = model.PhoneNumber })
                    .ExecuteWithoutResultsAsync();
            }

            return result;
        }

        public async Task<IdentityResult> LockoutUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"User with ID {userId} not found");
            }

            var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(1)); // Lockout tài khoản trong 1 năm

            if (result.Succeeded)
            {
                // Cập nhật lockout trong Neo4j
                await _client.Cypher
                    .Match("(u:User {Id: $userId})")
                    .Set("u.LockoutEndDate = $lockoutEndDate")
                    .WithParams(new { userId, lockoutEndDate = DateTimeOffset.UtcNow.AddYears(1) })
                    .ExecuteWithoutResultsAsync();
            }

            return result;
        }

        public async Task<IdentityResult> UnlockUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"User with ID {userId} not found");
            }

            var result = await userManager.SetLockoutEndDateAsync(user, null); // Xóa thời gian khóa

            if (result.Succeeded)
            {
                // Cập nhật lockout trong Neo4j
                await _client.Cypher
                    .Match("(u:User {Id: $userId})")
                    .Set("u.LockoutEndDate = null")
                    .WithParam("userId", userId)
                    .ExecuteWithoutResultsAsync();
            }

            return result;
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
            if (addRoleResult.Succeeded)
            {
                // Cập nhật trong Neo4j
                await _client.Cypher
                    .Match("(u:User {Id: $userId})")
                    .Set("u.UserName = $userName, u.PhoneNumber = $phoneNumber, u.Role = $newRole")
                    .WithParams(new { userId, userName = model.UserName, phoneNumber = model.PhoneNumber, newRole })
                    .ExecuteWithoutResultsAsync();
            }

            return addRoleResult;
        }

        public async Task<bool> FollowUserAsync(string followerId, string followeeId)
        {
            var relationshipExists = await _client.Cypher
                .Match("(follower:User {Id: $followerId})-[r:FOLLOWS]->(followee:User {Id: $followeeId})")
                .WithParams(new { followerId, followeeId })
                .Return(r => r.As<object>())
                .ResultsAsync;

            if (relationshipExists.Any())
            {
                return false; // Relationship already exists
            }

            await _client.Cypher
                .Match("(follower:User {Id: $followerId}), (followee:User {Id: $followeeId})")
                .Create("(follower)-[:FOLLOWS]->(followee)")
                .WithParams(new { followerId, followeeId })
                .ExecuteWithoutResultsAsync();

            return true;
        }

        public async Task<List<ApplicationUser>> GetFollowingAsync(string userId)
        {
            var result = await _client.Cypher
                .Match("(user:User {Id: $userId})-[:FOLLOWS]->(followee:User)")
                .WithParam("userId", userId)
                .Return(followee => followee.As<ApplicationUser>())
                .ResultsAsync;

            return result.ToList();
        }

        public async Task<List<ApplicationUser>> GetFollowersAsync(string userId)
        {
            var result = await _client.Cypher
                .Match("(user:User {Id: $userId})<-[:FOLLOWS]-(follower:User)")
                .WithParam("userId", userId)
                .Return(follower => follower.As<ApplicationUser>())
                .ResultsAsync;

            return result.ToList();
        }

        public async Task<bool> UnfollowUserAsync(string followerId, string followeeId)
        {
            var relationship = await _client.Cypher
                .Match("(follower:User {Id: $followerId})-[r:FOLLOWS]->(followee:User {Id: $followeeId})")
                .WithParams(new { followerId, followeeId })
                .Return(r => r.As<object>())
                .ResultsAsync;

            if (!relationship.Any())
            {
                return false; // Relationship does not exist
            }

            await _client.Cypher
                .Match("(follower:User {Id: $followerId})-[r:FOLLOWS]->(followee:User {Id: $followeeId})")
                .WithParams(new { followerId, followeeId })
                .Delete("r")
                .ExecuteWithoutResultsAsync();

            return true;
        }

        public async Task<UserProfileModel> GetUserProfileAsync(string userId)
        {
            var user = await _client.Cypher
                .Match("(user:User {Id: $userId})")
                .WithParam("userId", userId)
                .Return(user => user.As<ApplicationUser>())
                .ResultsAsync;

            if (user == null)
            {
                return null;
            }

            var userProfile = new UserProfileModel
            {
                UserId = user.First().Id,
                UserName = user.First().UserName,
                FullName = $"{user.First().FirstName} {user.First().LastName}",
                Email = user.First().Email,
                AvatarUrl = user.First().AvatarUrl
            };

            return userProfile;
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followeeId)
        {
            var relationship = await _client.Cypher
                .Match("(follower:User {Id: $followerId})-[r:FOLLOWS]->(followee:User {Id: $followeeId})")
                .WithParams(new { followerId, followeeId })
                .Return(r => r.As<object>())
                .ResultsAsync;

            return relationship.Any();
        }
    }
}
