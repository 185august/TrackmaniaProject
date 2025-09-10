using Microsoft.EntityFrameworkCore;

namespace TrackmaniaWebsiteAPI.DatabaseQuery;

public class TrackmaniaDbContext(DbContextOptions<TrackmaniaDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<CampaignMapsInfo> CampaignMaps { get; set; }
    public DbSet<PlayerProfiles> PlayerProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<User>()
            .HasOne(u => u.PlayerProfile)
            .WithMany()
            .HasForeignKey(u => u.PlayerProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        base.OnModelCreating(modelBuilder);
    }
}
