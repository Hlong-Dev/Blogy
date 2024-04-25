using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DoAnCoSo2.Models;
using DoAnCoSo2.Repositories;
using DoAnCoSo2.Helpers;
using DoAnCoSo2.Data;

namespace DoAnCoSo2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IBlogRepository _blogRepo;

        public ProductsController(IBlogRepository repo)
        {
            _blogRepo = repo;
        }

        [HttpGet]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetAllBlogs()
        {
            try
            {
                return Ok(await _blogRepo.GetAllBlogsAsync());
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            var blog = await _blogRepo.GetBlogAsync(id);
            return blog == null ? NotFound() : Ok(blog);
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> AddNewBlog(BlogModel model)
        {
            try
            {
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

                // Tạo một đối tượng Blog mới
                var newBlog = new Blog
                {
                    Title = model.Title,
                    Content = model.Content,
                    UserId = model.UserId,
                    CreatedAt = currentDateTime // Thiết lập ngày tạo tự động
                };

                // Thêm blog mới vào cơ sở dữ liệu
                var newBlogId = await _blogRepo.AddBlogAsync(model);

                // Lấy blog mới đã được thêm vào
                var blog = await _blogRepo.GetBlogAsync(newBlogId);

                return blog == null ? NotFound() : Ok(blog);
            }
            catch
            {
                return BadRequest();
            }
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlog(int id, [FromBody] BlogModel model)
        {
            if (id != model.BlogId)
            {
                return NotFound();
            }
            await _blogRepo.UpdateBlogAsync(id, model);
            return Ok();
        }

        [HttpDelete("{id}")]
        //[Authorize]
        public async Task<IActionResult> DeleteBlog([FromRoute] int id)
        {
            await _blogRepo.DeleteBlogAsync(id);
            return Ok();
        }
    }
}
