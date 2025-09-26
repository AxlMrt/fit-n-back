using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FitnessApp.Modules.Users.Infrastructure.Persistence;

public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        
        // Use a default connection string for design time
        optionsBuilder.UseNpgsql("Host=localhost;Database=fitnessapp_dev;Username=postgres;Password=password");
        
        return new UsersDbContext(optionsBuilder.Options);
    }
}
