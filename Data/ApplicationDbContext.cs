using Contacts.Models;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Contact> Contacts { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Seed some initial data
            builder.Entity<Contact>().HasData(
                new Contact { 
                    Id = 1, 
                    Name = "John Doe", 
                    Email = "john@example.com", 
                    Phone = "555-1234", 
                    Company = "Acme Inc", 
                    Notes= "no notes",
                    CreatedAt = new DateTime(2025,04,28),
                    OwnerId = "demo-user-id" 
                }
            );
        }
    }
}