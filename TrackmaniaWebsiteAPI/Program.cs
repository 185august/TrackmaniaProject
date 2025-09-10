using System.IO.Abstractions;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TrackmaniaWebsiteAPI.DatabaseQuery;
using TrackmaniaWebsiteAPI.Exceptions;
using TrackmaniaWebsiteAPI.MapInfo;
using TrackmaniaWebsiteAPI.MapTimes;
using TrackmaniaWebsiteAPI.PlayerAccount;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;
using TrackmaniaWebsiteAPI.UserAuth;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

//builder.Services.AddAuthorization();

// builder
//     .Services.AddAuthentication(options =>
//     {
//         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//         options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//     })
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidIssuer = config["Jwt:Issuer"],
//             ValidAudience = config["Jwt:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(
//                 Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)
//             ),
//         };
//     });
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
builder.Services.AddScoped<IApiTokensService, ApiTokensService>();
builder.Services.AddScoped<IMapInfoService, MapInfoService>();
builder.Services.AddScoped<ITimeCalculationService, TimeCalculationService>();
builder.Services.AddScoped<IOAuthService, OAuthService>();
builder.Services.AddScoped<PlayerAccountService>();
builder.Services.AddScoped<JwtHelperService>();
builder.Services.AddScoped<MapRecordsService>();
builder.Services.AddScoped<ApiTokensServiceRefactor>();
builder.Services.AddScoped<IComparisonPlayerService, ComparisonPlayersService>();
builder.Services.AddScoped<ITokenFetcher, ApiTokensServiceRefactor>();
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
