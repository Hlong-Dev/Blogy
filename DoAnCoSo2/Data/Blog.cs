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

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        // Thêm khóa ngoại để liên kết với ApplicationUser
        public string UserId { get; set; }

        // Thêm thuộc tính ApplicationUser để tạo mối quan hệ
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
        public string? UserName { get; set; }
        public string? ImageUrl { get; set; }

    }
}
