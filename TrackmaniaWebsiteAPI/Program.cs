using Scalar.AspNetCore;
using TrackmaniaWebsiteAPI.Startup;
using TrackmaniaWebsiteProject.Startup;
using TrackmaniaWebsiteProject.Startup.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddDependencies();

var app = builder.Build();

app.UseOpenApi();

app.UseHttpsRedirection();

app.AddRootEndpoints();

app.Run();
