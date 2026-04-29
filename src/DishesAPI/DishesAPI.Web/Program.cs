using DishesAPI.DbContexts;
using DishesAPI.Web.Extensions;
using DishesAPI.Web.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));

// To get some structure into the response, we want to use the problem details format.
// To enable that, we call to builder.Services.AddProblemDetails.
// This will ensure that the responses are formatted according to the
// problem details standard. https://datatracker.ietf.org/doc/html/rfc9457
builder.Services.AddProblemDetails();
builder.Services.AddValidation();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}
else
{
    // this is enabled by **default** in development environment and not really needed here
    app.UseDeveloperExceptionPage();
}

// Error routes
app.MapGet("/error", () => {throw new NotImplementedException();});

// registering the endpoints
app.RegisterDishesEndpoints();
app.RegisterIngredientsEndpoints();

// recreate & migrate the database on startup
using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.Migrate();
}

app.Run();
