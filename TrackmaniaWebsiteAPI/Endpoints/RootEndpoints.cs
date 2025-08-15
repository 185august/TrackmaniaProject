using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteProject.Models;

namespace TrackmaniaWebsiteProject.Startup.Endpoints;

public static class RootEndpoints
{
    public static void AddRootEndpoints(this WebApplication app)
    {
        app.MapGet("/GetUsers", async (UsersDb db) => db.Users.ToListAsync());
        app.MapPost(
            "/CreateUser",
            async (UsersDb db, Users user) =>
            {
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
                return Results.Created("CreateUser", user);
            }
        );
        app.MapPost(
            "/UploadMap",
            async (MapsDb db, Maps map) =>
            {
                await db.Maps.AddAsync(map);
                await db.SaveChangesAsync();
                return Results.Created("UploadMap", map);
            }
        );
    }
}
