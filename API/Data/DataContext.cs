using API.Entities;
using Microsoft.EntityFrameworkCore;
namespace API.Data
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions options):base(options){}

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Like> Likes{get;set;}
        public DbSet<Message> Messages{get;set;}

        protected override void OnModelCreating(ModelBuilder builder){
            base.OnModelCreating(builder);

            builder.Entity<Like>().HasKey(k=>new {k.LikingUserId,k.LikedUserId});
            builder.Entity<Like>().HasOne(x=>x.LikingUser).WithMany(x=>x.LikedUsers).HasForeignKey(x=>x.LikingUserId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Like>()
                .HasOne(x=>x.LikedUser)
                .WithMany(x=>x.LikedByUsers)
                .HasForeignKey(x=>x.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>().HasOne(x=>x.Recipient).WithMany(x=>x.MessagesReceived).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Message>()
                .HasOne(x=>x.Sender)
                .WithMany(x=>x.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);
        } 
    }
}