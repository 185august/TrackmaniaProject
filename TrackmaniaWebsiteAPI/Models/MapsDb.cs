using Microsoft.EntityFrameworkCore;

namespace TrackmaniaWebsiteProject.Models;

public class MapsDb : DbContext
{
    public MapsDb(DbContextOptions<MapsDb> options)
        : base(options) { }

    public DbSet<Maps> Maps { get; set; }
}
