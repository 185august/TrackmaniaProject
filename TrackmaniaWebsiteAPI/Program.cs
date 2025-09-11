using System.IO.Abstractions;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TrackmaniaWebsiteAPI.CampaignMaps;
using TrackmaniaWebsiteAPI.DatabaseQuery;
using TrackmaniaWebsiteAPI.Exceptions;
using TrackmaniaWebsiteAPI.MapTimes;
using TrackmaniaWebsiteAPI.PlayerAccount;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;
using TrackmaniaWebsiteAPI.UserAuth;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionsHandler>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod();
        }
    );
});

builder.Services.AddDbContext<TrackmaniaDbContext>(options =>
    options.UseMySQL(config.GetConnectionString("MySQL")!)
);
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IApiRequestQueue, ApiRequestQueue>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMapInfoService, MapInfoService>();
builder.Services.AddScoped<ITimeCalculationService, TimeCalculationService>();
builder.Services.AddScoped<PlayerAccountService>();
builder.Services.AddScoped<MapTimesService>();
builder.Services.AddScoped<ApiTokensService>();
builder.Services.AddScoped<PlayerAccountService>();
builder.Services.AddScoped<ITokenFetcher, ApiTokensService>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();

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
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

//app.UseAuthentication();

//app.UseAuthorization();

app.UseExceptionHandler();

//app.UseMiddleware<GlobalExceptionsHandler>();

app.Urls.Add("http://localhost:5000");

app.UseSession();

app.MapControllers();

app.Run();
