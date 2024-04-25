using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo2.Data
{
    public class BookStoreContext : IdentityDbContext<ApplicationUser>
    {
        public BookStoreContext(DbContextOptions<BookStoreContext> opt): base(opt)
        {

        }

        #region DbSet
        public DbSet<Blog>? Blogs { get; set; }
        #endregion
    }
}
