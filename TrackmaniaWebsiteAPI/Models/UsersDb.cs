using Microsoft.EntityFrameworkCore;

namespace TrackmaniaWebsiteProject.Models;

public class UsersDb : DbContext
{
    public UsersDb(DbContextOptions<UsersDb> options)
        : base(options) { }

    public DbSet<Users> Users { get; set; }
}
