using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TrackmaniaWebsiteAPI.Data;
using TrackmaniaWebsiteAPI.Services;
using TrackmaniaWebsiteAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<TrackmaniaDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("MySQL")!)
);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INadeoTokenService, NadeoTokenService>();
builder.Services.AddScoped<IApiTokensService, ApiTokensService>();

builder.Services.AddDataProtection();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".TrackmaniaWebsiteAPI.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSession();

app.MapControllers();

app.Run();
