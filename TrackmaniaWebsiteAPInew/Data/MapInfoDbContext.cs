using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using TrackmaniaWebsiteAPInew.Models;

namespace TrackmaniaWebsiteAPInew.Data;

public class MapInfoDbContext(DbContextOptions<MapInfoDbContext> options) : DbContext(options)
{
    public DbSet<MapInfo> AllMapsInfo => Set<MapInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<MapInfo>()
            .HasData(
                new MapInfo { Id = 1, Title = "First map " },
                new MapInfo { Id = 2, Title = "Second map " },
                new MapInfo { Id = 3, Title = "Third map " }
            );
    }
}
