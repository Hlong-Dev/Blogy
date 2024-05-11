using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSo2.Data
{
    public class UserSavedBlog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int BlogId { get; set; }
        [ForeignKey(nameof(BlogId))]
        public Blog Blog { get; set; }
    }
}
