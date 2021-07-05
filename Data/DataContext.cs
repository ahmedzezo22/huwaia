using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZawagProject.Models;
namespace ZawagProject.Data
{
    public class DataContext:IdentityDbContext<User, Role, int,IdentityUserClaim<int>,UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>> //make id int
    {
      public DataContext(DbContextOptions<DataContext> options):base(options){ }
       // public DbSet<User> Users{ get; set; }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Payment> Payments { get; set; }
        
        
        

        protected override void OnModelCreating(ModelBuilder modelBuilder){

            //identity
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new {ur.RoleId,ur.UserId });
                userRole.HasOne(ur => ur.Role).WithMany(ur => ur.userRole).HasForeignKey(ur => ur.RoleId).IsRequired();
                
                userRole.HasOne(ur => ur.User).WithMany(ur => ur.userRole).HasForeignKey(ur => ur.UserId).IsRequired();
            });

          //Like relations
          modelBuilder.Entity<Like>().HasKey(k=>new{k.LikeeId,k.LikerId});
          modelBuilder.Entity<Like>().HasOne(l=>l.Likee).WithMany(u=>u.Likers).HasForeignKey(l=>l.LikeeId)
          .OnDelete(DeleteBehavior.Restrict);
         
          modelBuilder.Entity<Like>().HasKey(k=>new{k.LikeeId,k.LikerId});
         modelBuilder.Entity<Like>().HasOne(l=>l.Liker).WithMany(u=>u.Likees).HasForeignKey(l=>l.LikerId)
          .OnDelete(DeleteBehavior.Restrict);

          //Message Relations
          modelBuilder.Entity<Message>().HasOne(s=>s.Sender).WithMany(u=>u.MessagesSent).OnDelete(DeleteBehavior.Restrict);
          modelBuilder.Entity<Message>().HasOne(s=>s.Recepient).WithMany(u=>u.MessagesRecieved).OnDelete(DeleteBehavior.Restrict);
        //filtet of photo global
        modelBuilder.Entity<Photo>().HasQueryFilter(p=>p.IsApproved==true);
        }
    }
    
}
