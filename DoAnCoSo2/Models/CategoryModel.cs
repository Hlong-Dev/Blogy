using System.ComponentModel.DataAnnotations;

namespace DoAnCoSo2.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        //Thêm thuộc tính cho slug nếu cần
        public string Slug { get; set; }
    }
}
