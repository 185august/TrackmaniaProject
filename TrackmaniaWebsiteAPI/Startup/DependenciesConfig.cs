using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteProject.Models;

namespace TrackmaniaWebsiteProject.Startup;

public static class DependenciesConfig
{
    public static void AddDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApiServices();
        var mySqlConnectionString = builder.Configuration.GetConnectionString("MySQL");
        builder.Services.AddDbContext<UsersDb>(options => options.UseMySQL(mySqlConnectionString));
        builder.Services.AddDbContext<MapsDb>(options => options.UseMySQL(mySqlConnectionString));
    }
}
