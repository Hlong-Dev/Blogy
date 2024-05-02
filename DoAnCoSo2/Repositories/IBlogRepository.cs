using DoAnCoSo2.Data;
using DoAnCoSo2.Models;

namespace DoAnCoSo2.Repositories
{
    public interface IBlogRepository
    {
        public Task<List<BlogModel>> GetAllBlogsAsync();
        public Task<BlogModel> GetBlogAsync(string slug);
        public Task<string> AddBlogAsync(BlogModel model);
        public Task UpdateBlogAsync(string slug, BlogModel model);
        public Task DeleteBlogAsync(int id);
        public Task<string> UploadImageAsync(IFormFile file);
        public Task<List<BlogModel>> GetAllPrivateBlogsByUserAsync(string userId);
        public Task<bool> IsSlugExists(string slug);
    }
}
