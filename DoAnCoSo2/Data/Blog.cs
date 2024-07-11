namespace DoAnCoSo2.Data
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoryId { get; set; }
        public string Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public string Description { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public int ViewCount { get; set; }
    }
}