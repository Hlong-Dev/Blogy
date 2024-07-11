using DoAnCoSo2.Data;
using DoAnCoSo2.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAnCoSo2.Repositories
{
    public interface IBlogRepository
    {
        Task<List<BlogModel>> GetAllBlogsAsync();
        Task<BlogModel> GetBlogAsync(string slug);
        Task AddUserPostedRelationship(string userId, string blogSlug);
        Task<string> AddBlogAsync(Blog newBlog, string userId);
        Task UpdateBlogAsync(string slug, BlogModel model);
        Task DeleteBlogAsync(int Id);
        Task<string> UploadImageAsync(IFormFile file);
        Task<List<BlogModel>> GetAllPrivateBlogsByUserAsync(string userId);
        Task<bool> IsSlugExists(string slug);
        Task UnsaveBlogAsync(string userId, int blogId);
        Task SaveBlogAsync(string userId, int blogId);
        Task<List<BlogModel>> GetSavedBlogsAsync(string userId);
        Task<bool> IsBlogSavedAsync(string userId, int blogId);
        Task<IEnumerable<Comment>> GetCommentsForBlogAsync(int blogId);
        Task AddCommentToBlogAsync(string userId, int blogId, Comment comment);
        Task DeleteCommentAsync(int commentId);
        Task<IEnumerable<Blog>> GetPopularBlogsAsync(int count);
        Task UpdateViewCountAsync(int blogId);
        Task<List<Blog>> GetFollowedUsersBlogsAsync(string userId);
        Task<IEnumerable<Blog>> SearchBlogsAsync(string keyword);

    }
}
