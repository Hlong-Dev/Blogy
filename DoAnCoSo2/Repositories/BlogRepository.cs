using AutoMapper;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo2.Data;
using DoAnCoSo2.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Linq;

namespace DoAnCoSo2.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly BookStoreContext _context;
        private readonly IMapper _mapper;
        private readonly HttpClient _client;
        private const string AccessToken = "ab361f7f8a35fe0a80e8000debbb2f19ef803d55";

        public BlogRepository(BookStoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        }

        public async Task<string> AddBlogAsync(BlogModel model)
        {
            var newBlog = _mapper.Map<Blog>(model);
            newBlog.IsPublic = model.IsPublic;
            _context.Blogs!.Add(newBlog);
            await _context.SaveChangesAsync();

            return newBlog.Slug;
        } 
        public async Task<bool> IsSlugExists(string slug)
        {
            // Kiểm tra xem có blog nào có slug giống với slug đã cho không
            return await _context.Blogs.AnyAsync(b => b.Slug == slug);
        }
        public async Task DeleteBlogAsync(int id)
        {
            var deleteBlog = _context.Blogs!.SingleOrDefault(b => b.BlogId == id);
            if (deleteBlog != null)
            {
                _context.Blogs!.Remove(deleteBlog);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<BlogModel>> GetAllBlogsAsync()
        {
            var blogs = await _context.Blogs!.Where(b => b.IsPublic)
           .ToListAsync();
            return _mapper.Map<List<BlogModel>>(blogs);
        }

        public async Task<BlogModel> GetBlogAsync(string slug)
        {
            var blog = await _context.Blogs!.FirstOrDefaultAsync(b => b.Slug == slug);

            return _mapper.Map<BlogModel>(blog);
        }

        public async Task UpdateBlogAsync(string slug, BlogModel model)
        {
            if (slug == model.Slug)
            {
                var updateBlog = _mapper.Map<Blog>(model);
                _context.Blogs!.Update(updateBlog);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentNullException(nameof(file), "File is empty");

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var imageData = memoryStream.ToArray();

                    var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(imageData), "image", file.FileName }
            };

                    var response = await _client.PostAsync("https://api.imgur.com/3/upload", content);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var imgurResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var imgUrl = imgurResponse.data.link.ToString();
                    return imgUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
        public async Task<List<BlogModel>> GetAllPrivateBlogsByUserAsync(string userId)
        {
            var blogs = await _context.Blogs!
                .Where(b => !b.IsPublic && b.UserId == userId)
                .ToListAsync();
            return _mapper.Map<List<BlogModel>>(blogs);
        }
        public async Task SaveBlogAsync(string userId, int blogId)
        {
            // Kiểm tra xem bài viết đã được lưu bởi người dùng hay chưa
            if (!await IsBlogSavedAsync(userId, blogId))
            {
                // Nếu bài viết chưa được lưu, thêm một bản ghi mới vào cơ sở dữ liệu
                var userSavedBlog = new UserSavedBlog { UserId = userId, BlogId = blogId };
                _context.UserSavedBlogs.Add(userSavedBlog);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnsaveBlogAsync(string userId, int blogId)
        {
            // Tìm và xóa bản ghi lưu bài viết khỏi cơ sở dữ liệu
            var savedBlog = await _context.UserSavedBlogs.FirstOrDefaultAsync(us => us.UserId == userId && us.BlogId == blogId);
            if (savedBlog != null)
            {
                _context.UserSavedBlogs.Remove(savedBlog);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<BlogModel>> GetSavedBlogsAsync(string userId)
        {
            var savedBlogIds = await _context.UserSavedBlogs
                .Where(us => us.UserId == userId)
                .Select(us => us.BlogId)
                .ToListAsync();

            var savedBlogs = await _context.Blogs
                .Where(b => savedBlogIds.Contains(b.BlogId))
                .ToListAsync();

            return _mapper.Map<List<BlogModel>>(savedBlogs);
        }
        public async Task<bool> IsBlogSavedAsync(string userId, int blogId)
        {
            // Kiểm tra xem bài viết đã được lưu bởi người dùng hay chưa
            return await _context.UserSavedBlogs.AnyAsync(us => us.UserId == userId && us.BlogId == blogId);
        }
    }
}
