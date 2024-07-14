using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo2.Data
{
    public class BookStoreContext : IdentityDbContext<ApplicationUser>
    {
        public BookStoreContext(DbContextOptions<BookStoreContext> opt) : base(opt)
        {

        }

        #region DbSet
        public DbSet<Blog>? Blogs { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<UserSavedBlog> UserSavedBlogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserRelationship> UserRelationships { get; set; }

        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasNoKey();
            modelBuilder.Entity<UserSavedBlog>()
                .HasKey(us => us.Id);

            modelBuilder.Entity<UserRelationship>()
                .HasKey(r => new { r.FollowerId, r.FolloweeId });

            // Thêm đoạn code sau để định nghĩa primary key cho UserRelationship
            modelBuilder.Entity<UserRelationship>()
                .HasKey(r => new { r.FollowerId, r.FolloweeId });

            // Nếu UserRelationship không cần một primary key thực sự, 
            // bạn có thể sử dụng keyless entity type như sau:
            // modelBuilder.Entity<UserRelationship>().HasNoKey();

            // Các cấu hình khác nếu cần
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<Blog>()
        //        .HasOne(b => b.User)
        //        .WithMany(u => u.Blogs)
        //        .HasForeignKey(b => b.UserId);

        //    modelBuilder.Entity<UserSavedBlog>()
        //        .HasKey(us => us.Id);

        //    modelBuilder.Entity<UserRelationship>()
        //     .HasKey(r => new { r.FollowerId, r.FolloweeId });

        //    modelBuilder.Entity<UserRelationship>()
        //        .HasOne<ApplicationUser>(r => r.Follower)
        //        .WithMany(u => u.Following)
        //        .HasForeignKey(r => r.FollowerId)
        //        .OnDelete(DeleteBehavior.Restrict);

        //    modelBuilder.Entity<UserRelationship>()
        //        .HasOne<ApplicationUser>(r => r.Followee)
        //        .WithMany(u => u.Followers)
        //        .HasForeignKey(r => r.FolloweeId)
        //        .OnDelete(DeleteBehavior.Restrict);
        //}

    }
}
