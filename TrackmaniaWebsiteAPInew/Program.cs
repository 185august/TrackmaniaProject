using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TrackmaniaWebsiteAPInew.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<MapInfoDbContext>(options =>
{
    options.UseMySQL(builder.Configuration.GetConnectionString("MySQL"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
