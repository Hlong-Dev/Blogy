using DoAnCoSo2.Data;
using DoAnCoSo2.Models;

namespace DoAnCoSo2.Repositories
{
    public interface IBlogRepository
    {
        public Task<List<BlogModel>> GetAllBlogsAsync();
        public Task<BlogModel> GetBlogAsync(int id);
        public Task<int> AddBlogAsync(BlogModel model);
        public Task UpdateBlogAsync(int id, BlogModel model);
        public Task DeleteBlogAsync(int id);
        public Task<string> UploadImageAsync(IFormFile file);
      
    }
}
