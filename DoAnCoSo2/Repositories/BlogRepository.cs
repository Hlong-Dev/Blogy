using AutoMapper;
using Neo4jClient;
using DoAnCoSo2.Data;
using DoAnCoSo2.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DoAnCoSo2.Services;

namespace DoAnCoSo2.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly IGraphClient _client;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private const string AccessToken = "ab361f7f8a35fe0a80e8000debbb2f19ef803d55";
        private readonly WebSocketConnectionManager _webSocketConnectionManager;

        public BlogRepository(IGraphClient client, IMapper mapper, WebSocketConnectionManager webSocketConnectionManager)
        {
            _client = client;
            _mapper = mapper;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            _webSocketConnectionManager = webSocketConnectionManager;
        }

        public async Task AddUserPostedRelationship(string userId, string blogSlug)
        {
            try
            {
                var result = await _client.Cypher
                    .Match("(blog:Blog)")
                    .Where((Blog blog) => blog.Slug == blogSlug)
                    .Return(blog => blog.As<Blog>())
                    .ResultsAsync;

                var blogNode = result.FirstOrDefault();

                if (blogNode != null)
                {
                    await _client.Cypher
                        .Match("(user:User)", "(blog:Blog)")
                        .Where((User user) => user.Id == userId)
                        .AndWhere((Blog blog) => blog.Slug == blogSlug)
                        .Create("(user)-[:POSTED]->(blog)")
                        .ExecuteWithoutResultsAsync();
                }
                else
                {
                    throw new Exception($"Blog with slug '{blogSlug}' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user-posted relationship: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AddBlogAsync(Blog model, string userId, string categorySlug)
        {
            var newBlog = _mapper.Map<Blog>(model);
            newBlog.IsPublic = model.IsPublic;

            await _client.Cypher
                .Create("(blog:Blog $newBlog)")
                .WithParam("newBlog", newBlog)
                .ExecuteWithoutResultsAsync();

            await AddUserPostedRelationship(userId, newBlog.Slug);
            await AddCategoryBlogRelationship(categorySlug, newBlog.Slug);

            // Gửi tin nhắn WebSocket đến những người theo dõi
            var followers = await _client.Cypher
                .Match("(follower:User)-[:FOLLOWS]->(user:User {Id: $userId})")
                .WithParam("userId", userId)
                .Return(follower => follower.As<User>())
                .ResultsAsync;

            var message = new
            {
                Text = $"User {userId} vừa đăng bài viết mới với tiêu đề: {newBlog.Title}",
                Slug = newBlog.Slug,
                AvatarUrl = newBlog.AvatarUrl // Include AvatarUrl in the message
            };
            var messageJson = JsonConvert.SerializeObject(message);

            foreach (var follower in followers)
            {
                await _webSocketConnectionManager.SendMessageAsync(follower.Id, messageJson);
                await AddNotificationAsync(follower.Id, message.Text, newBlog.Slug, message.AvatarUrl);
            }

            return newBlog.Slug;
        }


        public async Task AddNotificationAsync(string userId, string message, string slug, string avatarUrl)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                Message = message,
                Slug = slug,
                CreatedAt = DateTime.UtcNow,
                AvatarUrl = avatarUrl // Set AvatarUrl
            };

            await _client.Cypher
                .Create("(notification:Notification {Id: $id, Message: $message, Slug: $slug, CreatedAt: $createdAt, AvatarUrl: $avatarUrl})")
                .WithParams(new
                {
                    id = notification.Id,
                    message = notification.Message,
                    slug = notification.Slug,
                    createdAt = notification.CreatedAt,
                    avatarUrl = notification.AvatarUrl // Add AvatarUrl param
                })
                .ExecuteWithoutResultsAsync();

            await _client.Cypher
                .Match("(user:User)")
                .Where((User user) => user.Id == userId)
                .Match("(notification:Notification {Id: $id})")
                .WithParam("id", notification.Id)
                .Create("(user)-[:HAS_NOTIFICATION]->(notification)")
                .ExecuteWithoutResultsAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsForUserAsync(string userId)
        {
            var notifications = await _client.Cypher
                .Match("(user:User)-[:HAS_NOTIFICATION]->(notification:Notification)")
                .Where((User user) => user.Id == userId)
                .Return(notification => notification.As<Notification>())
                .ResultsAsync;

            return notifications;
        }
        private async Task AddCategoryBlogRelationship(string categorySlug, string blogSlug)
        {
            await _client.Cypher
                .Match("(category:Category)", "(blog:Blog)")
                .Where((Category category) => category.Slug == categorySlug)
                .AndWhere((Blog blog) => blog.Slug == blogSlug)
                .Create("(blog)-[:BELONGS_TO]->(category)")
                .ExecuteWithoutResultsAsync();
        }


        public async Task<bool> IsSlugExists(string slug)
        {
            var result = await _client.Cypher
                .Match("(blog:Blog)")
                .Where((Blog blog) => blog.Slug == slug)
                .Return(blog => blog.As<Blog>())
                .ResultsAsync;

            return result.Any();
        }

        public async Task DeleteBlogAsync(string blogSlug)
        {
            try
            {
                // Xóa tất cả các comment của bài viết
                await _client.Cypher
                    .Match("(blog:Blog { Slug: $blogSlug })<-[:HAS_COMMENT]-(comment:Comment)")
                    .WithParam("blogSlug", blogSlug)
                    .DetachDelete("comment")
                    .ExecuteWithoutResultsAsync();

                // Xóa tất cả các saved nodes của bài viết
                await _client.Cypher
                    .Match("(blog:Blog { Slug: $blogSlug })<-[:SAVED]-(saved:Saved)")
                    .WithParam("blogSlug", blogSlug)
                    .DetachDelete("saved")
                    .ExecuteWithoutResultsAsync();

                // Xóa tất cả các quan hệ liên kết khác của bài viết
                await _client.Cypher
                    .Match("(blog:Blog { Slug: $blogSlug })-[r]-()")
                    .WithParam("blogSlug", blogSlug)
                    .Delete("r")
                    .ExecuteWithoutResultsAsync();

                // Sau đó mới xóa bài viết chính thức
                await _client.Cypher
                    .Match("(blog:Blog)")
                    .Where((Blog blog) => blog.Slug == blogSlug)
                    .Delete("blog")
                    .ExecuteWithoutResultsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting blog: {ex.Message}");
                throw;
            }
        }



        public async Task<IEnumerable<Blog>> GetAllBlogsAsync()
        {
            var blogs = await _client.Cypher
                .Match("(blog:Blog)")
                .Where("blog.IsPublic = true")
                .Return(blog => blog.As<Blog>())
                .ResultsAsync;

            return blogs;
        }

        public async Task<BlogModel> GetBlogAsync(string slug)
        {
            var blog = await _client.Cypher
                .Match("(blog:Blog)")
                .Where((Blog blog) => blog.Slug == slug)
                .Return(blog => blog.As<Blog>())
                .ResultsAsync;

            return _mapper.Map<BlogModel>(blog.FirstOrDefault());
        }

        public async Task UpdateBlogAsync(string slug, BlogModel model)
        {
            var updateBlog = _mapper.Map<Blog>(model);
            await _client.Cypher
                .Match("(blog:Blog)")
                .Where((Blog blog) => blog.Slug == slug)
                .Set("blog = $updateBlog")
                .WithParam("updateBlog", updateBlog)
                .ExecuteWithoutResultsAsync();
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentNullException(nameof(file), "File is empty");

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var imageData = memoryStream.ToArray();

                    var content = new MultipartFormDataContent
                    {
                        { new ByteArrayContent(imageData), "image", file.FileName }
                    };

                    var response = await _httpClient.PostAsync("https://api.imgur.com/3/upload", content);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var imgurResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var imgUrl = imgurResponse.data.link.ToString();
                    return imgUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<List<BlogModel>> GetAllPrivateBlogsByUserAsync(string userId)
        {
            var blogs = await _client.Cypher
                .Match("(blog:Blog)")
                .Where((Blog blog) => blog.Id == userId)
                .Return(blog => blog.As<Blog>())
                .ResultsAsync;

            return _mapper.Map<List<BlogModel>>(blogs);
        }

        public async Task SaveBlogAsync(string userId, string slug)
        {
            await _client.Cypher
                .Match("(user:User)", "(blog:Blog)")
                .Where((User user) => user.Id == userId)
                .AndWhere((Blog blog) => blog.Slug == slug)
                .Create("(user)-[:SAVED]->(blog)")
                .ExecuteWithoutResultsAsync();
        }

        public async Task UnsaveBlogAsync(string userId, string slug)
        {
            await _client.Cypher
                .Match("(user:User)-[r:SAVED]->(blog:Blog)")
                .Where((User user) => user.Id == userId)
                .AndWhere((Blog blog) => blog.Slug == slug)
                .Delete("r")
                .ExecuteWithoutResultsAsync();
        }

        public async Task<List<BlogModel>> GetSavedBlogsAsync(string userId)
        {
            var blogs = await _client.Cypher
                .Match("(user:User)-[:SAVED]->(blog:Blog)")
                .Where((User user) => user.Id == userId)
                .Return(blog => blog.As<Blog>())
                .ResultsAsync;

            return _mapper.Map<List<BlogModel>>(blogs);
        }

        public async Task<bool> IsBlogSavedAsync(string userId, string slug)
        {
            var result = await _client.Cypher
                .Match("(user:User)-[:SAVED]->(blog:Blog)")
                .Where((User user) => user.Id == userId)
                .AndWhere((Blog blog) => blog.Slug == slug)
                .Return(blog => blog.As<Blog>())
                .ResultsAsync;

            return result.Any();
        }

        public async Task<IEnumerable<Comment>> GetCommentsForBlogAsync(string slug)
        {
            var comments = await _client.Cypher
                .Match("(comment:Comment)-[:ON]->(blog:Blog)")
                .Where((Blog blog) => blog.Slug == slug)
                .Return(comment => comment.As<Comment>())
                .ResultsAsync;

            return comments;
        }

        public async Task AddCommentToBlogAsync(string userId, string slug, Comment comment)
        {
            try
            {
                await _client.Cypher
                    .Create("(comment:Comment $newComment)")
                    .WithParam("newComment", comment)
                    .ExecuteWithoutResultsAsync();

                await _client.Cypher
                    .Match("(user:User)", "(comment:Comment)")
                    .Where((User user) => user.Id == userId)
                    .AndWhere((Comment cmt) => cmt.Id == comment.Id)
                    .Create("(user)-[:COMMENTED]->(comment)")
                    .ExecuteWithoutResultsAsync();

                await _client.Cypher
                    .Match("(comment:Comment)", "(blog:Blog)")
                    .Where((Comment cmt) => cmt.Id == comment.Id)
                    .AndWhere((Blog blog) => blog.Slug == slug)
                    .Create("(comment)-[:ON]->(blog)")
                    .ExecuteWithoutResultsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding comment: {ex.Message}");
                throw;
            }
        }

        //public async Task DeleteCommentAsync(int commentId)
        //{
        //    await _client.Cypher
        //        .Match("(comment:Comment)")
        //        .Where((Comment comment) => comment.Id == commentId)
        //        .Delete("comment")
        //        .ExecuteWithoutResultsAsync();
        //}

        public async Task<IEnumerable<Blog>> GetPopularBlogsAsync(int count)
        {
            var blogs = await _client.Cypher
                .Match("(blog:Blog)")
                .Return(blog => blog.As<Blog>())
                .OrderByDescending("blog.ViewCount")
                .Limit(count)
                .ResultsAsync;

            return blogs;
        }

        public async Task UpdateViewCountAsync(string slug)
        {
            await _client.Cypher
                .Match("(blog:Blog)")
                .Where((Blog blog) => blog.Slug == slug)
                .Set("blog.ViewCount = blog.ViewCount + 1")
                .ExecuteWithoutResultsAsync();
        }

        public async Task<List<Blog>> GetFollowedUsersBlogsAsync(string userId)
        {
            var blogs = await _client.Cypher
                .Match("(user:User)-[:FOLLOWS]->(followee:User)-[:POSTED]->(blog:Blog)")
                .Where((User user) => user.Id == userId)
                .Return(blog => blog.As<Blog>())
                .ResultsAsync;

            return blogs.ToList();
        }

        public async Task<IEnumerable<Blog>> SearchBlogsAsync(string keyword)
        {
            var blogs = await _client.Cypher
                .Match("(blog:Blog)")
                .Where("blog.Title CONTAINS $keyword")
                .WithParam("keyword", keyword)
                .Return(blog => blog.As<Blog>())
                .ResultsAsync;

            return blogs;
        }


        public Task DeleteCommentAsync(int commentId)
        {
            throw new NotImplementedException();
        }
    }
}