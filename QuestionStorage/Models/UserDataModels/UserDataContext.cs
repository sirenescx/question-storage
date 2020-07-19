using Microsoft.EntityFrameworkCore;

namespace QuestionStorage.Models.UserDataModels
{
    public sealed class UserDataContext : DbContext
    {
        public UserDataContext(DbContextOptions<UserDataContext> options) : base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        
    }
}