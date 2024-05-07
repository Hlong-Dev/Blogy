using DoAnCoSo2.Data;
using DoAnCoSo2.Models;
using DoAnCoSo2.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DoAnCoSo2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepo;


        public CategoryController(ICategoryRepository repo)
        {
            _categoryRepo = repo;

        }
        [HttpPost]
        public async Task<IActionResult> AddNewCategory(CategoryModel model)
        {
            try
            {
                // Kiểm tra xem slug đã tồn tại trong cơ sở dữ liệu hay chưa
                var slugExists = await _categoryRepo.IsSlugExists(model.Slug);
                if (slugExists)
                {
                    // Nếu slug đã tồn tại, thực hiện một biện pháp để tạo ra một slug mới
                    model.Slug = GenerateUniqueSlug(model.Slug);
                }

                // Tạo một đối tượng Category mới
                var newCategory = new Category
                {
                    Name = model.Name,
                    Slug = model.Slug // Lưu URL của ảnh vào blog
                };

                // Thêm category mới vào cơ sở dữ liệu
                var newSlug = await _categoryRepo.AddCategoryAsync(model);

                // Lấy category mới đã được thêm vào
                var category = await _categoryRepo.GetCategoryAsync(newSlug);

                return category == null ? NotFound() : Ok(category);
            }
            catch
            {
                return BadRequest();
            }
        }

        private string GenerateUniqueSlug(string slug)
        {
            // Tạo một slug mới không trùng lặp, ví dụ: thêm số vào cuối slug
            var uniqueSlug = $"{slug}-{DateTime.Now.Ticks}";

            return uniqueSlug;
        }
        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBlogById(string slug)
        {
            var blog = await _categoryRepo.GetCategoryAsync(slug);
            return blog == null ? NotFound() : Ok(blog);
        }
    }
}
