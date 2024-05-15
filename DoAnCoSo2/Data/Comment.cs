using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSo2.Data
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public int BlogId { get; set; }
        [ForeignKey(nameof(BlogId))]
        public Blog Blog { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
