using AutoMapper;
using Neo4jClient;
using DoAnCoSo2.Data;
using DoAnCoSo2.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoAnCoSo2.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly IGraphClient _client;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private const string AccessToken = "ab361f7f8a35fe0a80e8000debbb2f19ef803d55";

        public BlogRepository(IGraphClient client, IMapper mapper)
        {
            _client = client;
            _mapper = mapper;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
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

        public async Task<string> AddBlogAsync(Blog model, string userId)
        {
            var newBlog = _mapper.Map<Blog>(model);
            newBlog.IsPublic = model.IsPublic;

            await _client.Cypher
                .Create("(blog:Blog $newBlog)")
                .WithParam("newBlog", newBlog)
                .ExecuteWithoutResultsAsync();

            await AddUserPostedRelationship(userId, newBlog.Slug);

            return newBlog.Slug;
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

        public async Task DeleteBlogAsync(int id)
        {
            await _client.Cypher
                .Match("(blog:Blog)")
                .Where((Blog blog) => blog.BlogId == id)
                .Delete("blog")
                .ExecuteWithoutResultsAsync();
        }

        public async Task<List<BlogModel>> GetAllBlogsAsync()
        {
            try
            {
                var blogs = await _client.Cypher
                    .Match("(blog:Blog)")
                    .Where((Blog blog) => blog.IsPublic)
                    .Return(blog => blog.As<Blog>())
                    .ResultsAsync;

                if (blogs == null)
                {
                    Console.WriteLine("No blogs found.");
                    return new List<BlogModel>();
                }

                Console.WriteLine($"Found {blogs.Count()} blogs.");

                var blogModels = _mapper.Map<List<BlogModel>>(blogs);

                return blogModels;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in GetAllBlogsAsync: {ex.Message}");
                throw;
            }
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
                .Where((Blog blog) => blog.Id == userId && blog.IsPublic == false)
                .Return(blog => blog.As<Blog>())
                .ResultsAsync;

            return _mapper.Map<List<BlogModel>>(blogs);
        }

        public async Task SaveBlogAsync(string userId, int blogId)
        {
            await _client.Cypher
                .Match("(user:User)", "(blog:Blog)")
                .Where((User user) => user.Id == userId)
                .AndWhere((Blog blog) => blog.BlogId == blogId)
                .Create("(user)-[:SAVED]->(blog)")
                .ExecuteWithoutResultsAsync();
        }

        public async Task UnsaveBlogAsync(string userId, int blogId)
        {
            await _client.Cypher
                .Match("(user:User)-[r:SAVED]->(blog:Blog)")
                .Where((User user) => user.Id == userId)
                .AndWhere((Blog blog) => blog.BlogId == blogId)
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

        public async Task<bool> IsBlogSavedAsync(string userId, int blogId)
        {
            var result = await _client.Cypher
                .Match("(user:User)-[:SAVED]->(blog:Blog)")
                .Where((User user) => user.Id == userId)
                .AndWhere((Blog blog) => blog.BlogId == blogId)
                .Return(blog => blog.As<Blog>())
                .ResultsAsync;

            return result.Any();
        }

        public async Task<IEnumerable<Comment>> GetCommentsForBlogAsync(int blogId)
        {
            var comments = await _client.Cypher
                .Match("(comment:Comment)-[:ON]->(blog:Blog)")
                .Where((Blog blog) => blog.BlogId == blogId)
                .Return(comment => comment.As<Comment>())
                .ResultsAsync;

            return comments;
        }

        public async Task AddCommentToBlogAsync(string userId, int blogId, Comment comment)
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
                    .AndWhere((Blog blog) => blog.BlogId == blogId)
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

        public async Task UpdateViewCountAsync(int blogId)
        {
            await _client.Cypher
                .Match("(blog:Blog)")
                .Where((Blog blog) => blog.BlogId == blogId)
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
                .Where("blog.Title CONTAINS $keyword OR blog.Content CONTAINS $keyword OR blog.Description CONTAINS $keyword")
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