using AutoMapper;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo2.Data;
using DoAnCoSo2.Models;

namespace DoAnCoSo2.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly BookStoreContext _context;
        private readonly IMapper _mapper;

        public BlogRepository(BookStoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
    }
}
