using AutoMapper;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo2.Data;
using DoAnCoSo2.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

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

        public async Task<int> AddBlogAsync(BlogModel model)
        {
            var newBlog = _mapper.Map<Blog>(model);
            _context.Blogs!.Add(newBlog);
            await _context.SaveChangesAsync();

            return newBlog.BlogId;
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
            var blogs = await _context.Blogs!.ToListAsync();
            return _mapper.Map<List<BlogModel>>(blogs);
        }

        public async Task<BlogModel> GetBlogAsync(int id)
        {
            var blog = await _context.Blogs!.FindAsync(id);
            return _mapper.Map<BlogModel>(blog);
        }

        public async Task UpdateBlogAsync(int id, BlogModel model)
        {
            if (id == model.BlogId)
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

    }
}
