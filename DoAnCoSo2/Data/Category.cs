using DoAnCoSo2.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSo2.Data
{
   
    public class Category
    {
       

        [Required]
        public string Name { get; set; }

        //Thêm thuộc tính cho slug nếu cần
        public string Slug { get; set; }
        public List<BlogModel> Blogs { get; set; }
    }
}
