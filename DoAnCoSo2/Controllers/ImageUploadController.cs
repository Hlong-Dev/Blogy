using DoAnCoSo2.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DoAnCoSo2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageUploadController : ControllerBase
    {
        private readonly ImgurUploader _imgurUploader;

        public ImageUploadController(ImgurUploader imgurUploader)
        {
            _imgurUploader = imgurUploader;
        }

        [HttpPost]
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
                var imgUrl = await _imgurUploader.UploadImageAsync(filePath);

                // Return the URL of the uploaded image
                return Ok(new { url = imgUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
