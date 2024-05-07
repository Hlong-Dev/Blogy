using AutoMapper;
using DoAnCoSo2.Data;
using DoAnCoSo2.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace DoAnCoSo2.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly BookStoreContext _context;
        private readonly IMapper _mapper;
        public CategoryRepository(BookStoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<string> AddCategoryAsync(CategoryModel model)
        {
            var newCategory = _mapper.Map<Category>(model);
          
            _context.Categories!.Add(newCategory);
            await _context.SaveChangesAsync();

            return newCategory.Slug;
        }
        public async Task<bool> IsSlugExists(string slug)
        {
            // Kiểm tra xem có blog nào có slug giống với slug đã cho không
            return await _context.Categories.AnyAsync(b => b.Slug == slug);
        }
        public Task DeleteCategoryAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<CategoryModel>> GetAllCategoriesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<CategoryModel> GetCategoryAsync(string slug)
        {
            var blog = await _context.Categories!.FirstOrDefaultAsync(b => b.Slug == slug);

            return _mapper.Map<CategoryModel>(blog);
        }

        public Task UpdateCategoryAsync(string slug, CategoryModel model)
        {
            throw new NotImplementedException();
        }
    }
}
