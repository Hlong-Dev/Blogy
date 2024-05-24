using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DoAnCoSo2.Data
{
    [Table("Blog")]
    public class Blog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlogId { get; set; }
        public string Slug { get; set; }
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
        public int CategoryId { get; set; }


        public Category Category { get; set; }

        // Thêm khóa ngoại để liên kết với ApplicationUser
        public string UserId { get; set; }

        // Thêm thuộc tính ApplicationUser để tạo mối quan hệ
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
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
