using Microsoft.AspNetCore.Identity;
using DoAnCoSo2.Models;
using DoAnCoSo2.Data;

namespace DoAnCoSo2.Repositories
{
    public interface IAccountRepository
    {
        public Task<IdentityResult> SignUpAsync(SignUpModel model);
        public Task<string> SignInAsync(SignInModel model);
        public Task<IdentityResult> UpdateUserRoleAsync(string userId, string newRole);
        public Task<IEnumerable<ApplicationUser>> GetUsersAsync();
        public Task<IdentityResult> UpdateUserAsync(string userId, ApplicationUser model);
        public Task<IdentityResult> LockoutUserAsync(string userId);
        public Task<IdentityResult> UnlockUserAsync(string userId);
        public Task<IEnumerable<string>> GetRolesAsync();
        public Task<IdentityResult> UpdateUserAndRoleAsync(string userId, ApplicationUser model, string newRole);
    }
}
