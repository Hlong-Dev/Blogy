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
        public async Task<int> AddCategoryAsync(CategoryModel model)
        {
            var newCategory = _mapper.Map<Category>(model);
          
            _context.Categories!.Add(newCategory);
            await _context.SaveChangesAsync();

            return newCategory.Id;
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

        public async Task<List<CategoryModel>> GetAllCategoriesAsync()
        {
            var blogs = await _context.Categories!
           .ToListAsync();
            return _mapper.Map<List<CategoryModel>>(blogs);
        }

        public async Task<CategoryModel> GetCategoryAsync(int id)
        {
            // Tìm kiếm category trong cơ sở dữ liệu dựa trên id
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            // Kiểm tra xem category có tồn tại không
            if (category == null)
            {
                // Nếu không tìm thấy, trả về null hoặc xử lý theo nhu cầu của bạn
                return null;
            }

            // Ánh xạ category thành một CategoryModel và trả về
            return _mapper.Map<CategoryModel>(category);
        }


        public Task UpdateCategoryAsync(string slug, CategoryModel model)
        {
            throw new NotImplementedException();
        }
    }
}
