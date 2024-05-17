using DoAnCoSo2.Models;

namespace DoAnCoSo2.Repositories
{
    public interface ICategoryRepository
    {
        public Task<List<CategoryModel>> GetAllCategoriesAsync();
        public Task<CategoryModel> GetCategoryAsync(int id);
        public Task<int> AddCategoryAsync(CategoryModel model);
        public Task UpdateCategoryAsync(string slug, CategoryModel model);
        public Task DeleteCategoryAsync(int id);
        public Task<bool> IsSlugExists(string slug);
    }
}
