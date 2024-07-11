using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DoAnCoSo2.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string AvatarUrl { get; set; } = null!;
        public ICollection<Blog> Blogs { get; set; }

        // Các thuộc tính liên quan đến quan hệ theo dõi trong Neo4j sẽ được xử lý qua repository
        // Bạn không cần định nghĩa các ICollection<Blog> và ICollection<UserRelationship> vì Neo4j sẽ xử lý các mối quan hệ này

        public ApplicationUser()
        {
            Blogs = new HashSet<Blog>();
        }
    }
}
