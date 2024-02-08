using julianapi.Models;
using Microsoft.EntityFrameworkCore;

namespace julianapi.Data
{
    public class DataContextEF : DbContext
    {

     private readonly IConfiguration _config;

     public DataContextEF(IConfiguration config){
        _config = config;
     }
    //  this is a property on this class
     public virtual DbSet<User> Users {get; set;}
     public virtual DbSet<UserSalary> UserSalary {get; set;}
     public virtual DbSet<UserJobInfo> UserJobInfo {get; set;}
     
     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
     {
        if(!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_config.GetConnectionString("DefaultConnection"),
            // incase of a network flicker it will retry
            optionsBuilder => optionsBuilder.EnableRetryOnFailure()
            );
        }
     }
      protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //  bring in entity framework relational package to be able to use hasdefaultschema
        modelBuilder.HasDefaultSchema("TutorialAppSchema");
        modelBuilder.Entity<User>()
        // tables in our tutorial app schema
        .ToTable("Users", "TutorialAppSchema")
        .HasKey(u => u.UserId);
         modelBuilder.Entity<UserSalary>()
        
        .HasKey(u => u.UserId);
         modelBuilder.Entity<UserJobInfo>()
        
        .HasKey(u => u.UserId);
    }
    }

   
}
