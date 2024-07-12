using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSo2.Data
{
    public class Comment
    {
        [Key]
        public int Id { get; set; } // Đặt kiểu dữ liệu cho Id phù hợp với cơ sở dữ liệu của bạn

        public string BlogSlug { get; set; }

        [ForeignKey(nameof(BlogSlug))]
        public Blog Blog { get; set; }

        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string AvatarUrl { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
