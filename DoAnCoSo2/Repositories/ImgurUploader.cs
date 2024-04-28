using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace DoAnCoSo2.Repositories
{
    public class ImgurUploader
    {
        private readonly HttpClient _client;
        private const string AccessToken = "ab361f7f8a35fe0a80e8000debbb2f19ef803d55";

        public ImgurUploader()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        }

        public async Task<string> UploadImageAsync(string imagePath)
        {
            try
            {
                byte[] imageData;
                using (var fileStream = new FileStream(imagePath, FileMode.Open))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }
                }

                var content = new MultipartFormDataContent
                {
                    {new ByteArrayContent(imageData), "image", "image.png"}
                };

                var response = await _client.PostAsync("https://api.imgur.com/3/upload", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var imgurResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var imgUrl = imgurResponse.data.link.ToString();
                return imgUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
    }
}
