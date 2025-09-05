using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.MapTimes;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.UserAuth;

namespace TrackmaniaWebsiteAPI.Data;

public class TrackmaniaDbContext(DbContextOptions<TrackmaniaDbContext> options)
    : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<CampaignMapsInfo> CampaignMaps { get; set; }
    public DbSet<ComparisonPlayers> ComparisonPlayers { get; set; }
    public DbSet<PlayerProfiles> PlayerProfiles { get; set; }
}
