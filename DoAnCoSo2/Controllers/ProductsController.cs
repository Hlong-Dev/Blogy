using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DoAnCoSo2.Models;
using DoAnCoSo2.Repositories;
using DoAnCoSo2.Helpers;
using DoAnCoSo2.Data;
using System;
using System.Security.Claims;

namespace DoAnCoSo2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IBlogRepository _blogRepo;

        public class SaveBlogRequest
        {
            public int BlogId { get; set; }
        }
        public class UnsaveBlogRequest
        {
            public int BlogId { get; set; }
        }

        public ProductsController(IBlogRepository repo)
        {
            _blogRepo = repo;

        }

        [HttpGet]
      
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
        [HttpGet("private/{userId}")]
        public async Task<IActionResult> GetPrivateBlogs(string userId)
        {
            try
            {
                return Ok(await _blogRepo.GetAllPrivateBlogsByUserAsync(userId));
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBlogById(string slug)
        {
            var blog = await _blogRepo.GetBlogAsync(slug);
            return blog == null ? NotFound() : Ok(blog);
        }
        [HttpPost]
        public async Task<IActionResult> AddNewBlog(BlogModel model)
        {
            try
            {
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

                // Kiểm tra xem slug đã tồn tại trong cơ sở dữ liệu hay chưa
                var slugExists = await _blogRepo.IsSlugExists(model.Slug);
                if (slugExists)
                {
                    // Nếu slug đã tồn tại, thực hiện một biện pháp để tạo ra một slug mới
                    model.Slug = GenerateUniqueSlug(model.Slug);
                }

                // Tạo một đối tượng Blog mới
                var newBlog = new Blog
                {
                    Title = model.Title,
                    Content = model.Content,
                    UserId = model.UserId,
                    UserName = model.UserName,
                    CreatedAt = currentDateTime,
                    ImageUrl = model.ImageUrl,
                    Slug = model.Slug,// Lưu URL của ảnh vào blog
                    Description = model.Description,
                    AvatarUrl = model.AvatarUrl,
                    FirstName = model.FirstName,
                    CategoryId = model.CategoryId
                };

                // Thêm blog mới vào cơ sở dữ liệu
                var newSlug = await _blogRepo.AddBlogAsync(model);

                // Lấy blog mới đã được thêm vào
                var blog = await _blogRepo.GetBlogAsync(newSlug);

                return blog == null ? NotFound() : Ok(blog);
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

        [HttpPut("{slug}")]
        public async Task<IActionResult> UpdateBlog(string slug, [FromBody] BlogModel model)
        {
            if (slug != model.Slug)
            {
                return NotFound();
            }
            await _blogRepo.UpdateBlogAsync(slug, model);
            return Ok();
        }

        [HttpDelete("{id}")]
        //[Authorize]
        public async Task<IActionResult> DeleteBlog([FromRoute] int id)
        {
            await _blogRepo.DeleteBlogAsync(id);
            return Ok();
        }
        [HttpPost("saved")]
        public async Task<IActionResult> SaveOrUnsaveBlog([FromBody] SaveBlogRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isBlogSaved = await _blogRepo.IsBlogSavedAsync(userId, request.BlogId);

            if (isBlogSaved)
            {
                // Nếu bài viết đã được lưu, thực hiện hành động bỏ lưu
                await _blogRepo.UnsaveBlogAsync(userId, request.BlogId);
                return Ok("Blog unsaved successfully!");
            }
            else
            {
                // Nếu bài viết chưa được lưu, thực hiện hành động lưu
                await _blogRepo.SaveBlogAsync(userId, request.BlogId);
                return Ok("Blog saved successfully!");
            }
        }


    }
}