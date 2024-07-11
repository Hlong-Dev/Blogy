using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
namespace DoAnCoSo2.Data
{
   

    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }

        public List<Blog> Blogs { get; set; }
    }

}
