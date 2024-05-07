using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSo2.Data
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        //Thêm thuộc tính cho slug nếu cần
        public string Slug { get; set; }
    }
}
