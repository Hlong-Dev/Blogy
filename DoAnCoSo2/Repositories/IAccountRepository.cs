using DoAnCoSo2.Data;
using DoAnCoSo2.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAnCoSo2.Repositories
{
    public interface IAccountRepository
    {
        Task<IdentityResult> SignUpAsync(SignUpModel model);
        Task<string> SignInAsync(SignInModel model);
        Task<IdentityResult> UpdateUserRoleAsync(string userId, string newRole);
        Task<IEnumerable<ApplicationUser>> GetUsersAsync();
        Task<IdentityResult> UpdateUserAsync(string userId, ApplicationUser model);
        Task<IdentityResult> LockoutUserAsync(string userId);
        Task<IdentityResult> UnlockUserAsync(string userId);
        Task<IEnumerable<string>> GetRolesAsync();
        Task<IdentityResult> UpdateUserAndRoleAsync(string userId, ApplicationUser model, string newRole);
        Task<bool> FollowUserAsync(string followerId, string followeeId);
        Task<List<ApplicationUser>> GetFollowingAsync(string userId);
        Task<List<ApplicationUser>> GetFollowersAsync(string userId);
        Task<UserProfileModel> GetUserProfileAsync(string userId);
        Task<bool> UnfollowUserAsync(string followerId, string followeeId);
        Task<bool> IsFollowingAsync(string followerId, string followeeId);
    }
}
