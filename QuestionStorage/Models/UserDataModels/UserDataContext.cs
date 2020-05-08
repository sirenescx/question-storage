using Microsoft.EntityFrameworkCore;

namespace QuestionStorage.Models.UserDataModels
{
    public sealed class UserDataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public UserDataContext(DbContextOptions<UserDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}