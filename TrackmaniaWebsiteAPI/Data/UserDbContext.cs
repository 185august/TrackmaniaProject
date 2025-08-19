using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}
