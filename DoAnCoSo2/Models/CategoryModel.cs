using System.ComponentModel.DataAnnotations;

namespace DoAnCoSo2.Models
{
    public class CategoryModel
    {
      

        [Required]
        public string Name { get; set; }

        //Thêm thuộc tính cho slug nếu cần
        public string Slug { get; set; }
        public List<BlogModel> Blogs { get; set; }
    }
}
