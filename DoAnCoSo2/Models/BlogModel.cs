using System.ComponentModel.DataAnnotations;

namespace DoAnCoSo2.Models
{
    public class BlogModel
    {
        public int BlogId { get; set; }
        public string Slug { get; set; }
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        
        public string? UserId { get; set; }
        public string? UserName { get; set;}
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public string Description { get; set; }
        public string? AvatarUrl { get; set; }
        public string? FirstName { get; set; }
        // Sử dụng kiểu string cho UserId
    }

}
