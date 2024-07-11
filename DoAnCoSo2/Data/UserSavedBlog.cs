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
        public string Slug { get; set; }
        [ForeignKey(nameof(Slug))]
        public Blog Blog { get; set; }
    }
}
