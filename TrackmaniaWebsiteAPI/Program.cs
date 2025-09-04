using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TrackmaniaWebsiteAPI.Data;
using TrackmaniaWebsiteAPI.MapTimes;
using TrackmaniaWebsiteAPI.Services;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.Services.AddAuthorization();

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)
            ),
        };
    });

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
builder.Services.AddScoped<ApiRequestQueue>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApiTokensService, ApiTokensService>();
builder.Services.AddScoped<IMapInfoService, MapInfoService>();
builder.Services.AddScoped<ITimeCalculationService, TimeCalculationService>();
builder.Services.AddScoped<IOAuthService, OAuthService>();
builder.Services.AddScoped<JwtHelperService>();
builder.Services.AddScoped<MapRecordsService>();

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

app.UseSession();

app.MapControllers();

app.Run();
