using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProiectASP.Models;

namespace ProiectASP.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // relatia many(groups) - many(users)

            // definire primary key compus
            modelBuilder.Entity<UserGroup>()
            .HasKey(uc => new { uc.UserId, uc.GroupId });

            // definire relatii cu modelele Category si Article (FK)
            modelBuilder.Entity<UserGroup>()
            .HasOne(uc => uc.User)
            .WithMany(uc => uc.UserGroups)
            .HasForeignKey(uc => uc.UserId);
            modelBuilder.Entity<UserGroup>()
            .HasOne(uc => uc.Group)
            .WithMany(uc => uc.UserGroups)
            .HasForeignKey(uc => uc.GroupId);

            //many(users) - many(users) prin follow
            // un user are mai multi urmaritori
            // un user poate urmari mai multe persoane
            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.UserId, f.FollowerId });
            modelBuilder.Entity<Follow>()
                 .HasOne(f=>f.User)
                 .WithMany(f=>f.Followers)
                 .HasForeignKey(f => f.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            // nu lasam on delete cascade la ambele relatii pentru ca intoarce eroare ca am putea avea "cycles" sau "multiple cascade paths"
            // eroarea provine din faptul ca ambele relatii contin ApplicationUser

            modelBuilder.Entity<Follow>()
                .HasOne(f=>f.Follower)
                .WithMany(f=>f.Follows)
                .HasForeignKey(f => f.FollowerId);

        }
    }
}

