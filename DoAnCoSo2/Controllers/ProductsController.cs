using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DoAnCoSo2.Models;
using DoAnCoSo2.Repositories;
using DoAnCoSo2.Helpers;
using DoAnCoSo2.Data;
using System;

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
        [Authorize]
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
                    UserName = model.UserName,
                    CreatedAt = currentDateTime,
                    ImageUrl = model.ImageUrl // Lưu URL của ảnh vào blog
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

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File is empty");

                // Save the uploaded file to a temporary location
                var filePath = Path.GetTempFileName();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Upload the image to Imgur
                var imgUrl = await _blogRepo.UploadImageAsync(file);

                // Return the URL of the uploaded image
                return Ok(new { url = imgUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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