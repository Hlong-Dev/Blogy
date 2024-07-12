using DoAnCoSo2.Models;

namespace DoAnCoSo2.Repositories
{
    public interface ICategoryRepository
    {
        public Task<List<CategoryModel>> GetAllCategoriesAsync();
        public Task<CategoryModel> GetCategoryAsync(string slug);
        public Task<string> AddCategoryAsync(CategoryModel model);
        public Task UpdateCategoryAsync(string slug, CategoryModel model);
        public Task DeleteCategoryAsync(string slug);
        public Task<bool> IsSlugExists(string slug);
    }
}
