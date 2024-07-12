using DoAnCoSo2.Data;
using DoAnCoSo2.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAnCoSo2.Repositories
{
    public interface IBlogRepository
    {
        Task<IEnumerable<Blog>> GetAllBlogsAsync();
        Task<BlogModel> GetBlogAsync(string slug);
        Task AddUserPostedRelationship(string userId, string blogSlug);
        Task<string> AddBlogAsync(Blog model, string userId, string categorySlug);

        Task UpdateBlogAsync(string slug, BlogModel model);
        Task DeleteBlogAsync(string slug);
        Task<string> UploadImageAsync(IFormFile file);
        Task<List<BlogModel>> GetAllPrivateBlogsByUserAsync(string userId);
        Task<bool> IsSlugExists(string slug);
        Task UnsaveBlogAsync(string userId, string slug);
        Task SaveBlogAsync(string userId, string slug);
        Task<List<BlogModel>> GetSavedBlogsAsync(string userId);
        Task<bool> IsBlogSavedAsync(string userId, string slug);
        Task<IEnumerable<Comment>> GetCommentsForBlogAsync(string slug);
        Task AddCommentToBlogAsync(string userId, string slug, Comment comment);
        Task DeleteCommentAsync(int commentId);
        Task<IEnumerable<Blog>> GetPopularBlogsAsync(int count);
        Task UpdateViewCountAsync(string slug);
        Task<List<Blog>> GetFollowedUsersBlogsAsync(string userId);
        Task<IEnumerable<Blog>> SearchBlogsAsync(string keyword);
    }
}
