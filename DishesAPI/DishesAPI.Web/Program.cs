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
builder.Services.AddProblemDetails();

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


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// GET routes
app.MapGet("/dishes", async Task<Ok<IEnumerable<DishDto>>>(DishesDbContext dbContext, ClaimsPrincipal user, [FromQuery(Name = "name")] string? name) =>
{
    Console.WriteLine($"User: {user.Identity?.Name ?? "Anonymous"}, Name filter: {name}");
    var dishes = (await dbContext.Dishes.Where(d => string.IsNullOrEmpty(name) || d.Name.Contains(name))
                                    .ToListAsync()).ToDishDtoList();
    return TypedResults.Ok(dishes);
}).WithName("GetDishes");

app.MapGet("/dishes/{dishId:guid}", async Task<Results<NotFound, Ok<DishDto>>> (Guid dishId, DishesDbContext dbContext) =>
{
    var dishEntity = await dbContext.Dishes.Where(d => d.Id == dishId).FirstOrDefaultAsync();
    if (dishEntity is null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(dishEntity.ToDishDto());
}).WithName("GetDishById");

app.MapGet("/dishes/{dishName}", async Task<Results<NotFound, Ok<DishDto>>> (string dishName, DishesDbContext dbContext) =>
{
    var dishEntity = await dbContext.Dishes.Where(d => d.Name == dishName).FirstOrDefaultAsync();
    if (dishEntity is null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(dishEntity?.ToDishDto());
}).WithName("GetDishByName");

app.MapGet("/dishes/{dishId}/ingredients", async Task<Results<NotFound, Ok<IEnumerable<IngredientDto>>>>(Guid dishId, DishesDbContext dbContext) =>
{
    var dishEntity = await dbContext.Dishes
                        .Include(d => d.Ingredients)
                        .Where(d => d.Id == dishId)
                        .FirstOrDefaultAsync();
    if (dishEntity is null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(dishEntity.Ingredients.ToIngredientDtoList(dishId));

}).WithName("GetDishIngredients");

app.MapGet("/error", () => {throw new NotImplementedException();});

// POST routes
app.MapPost("/dishes", async Task<CreatedAtRoute<DishDto>> (DishesDbContext dbContext, [FromBody] DishCreateDto dishCreateDto) =>
{
    var dishEntity = dishCreateDto.ToDish();
    dbContext.Dishes.Add(dishEntity);
    await dbContext.SaveChangesAsync();
    var newDish = dishEntity.ToDishDto();
    return TypedResults.CreatedAtRoute(newDish, "GetDishById", new { dishId = newDish.Id });
});

// recreate & migrate the database on startup
using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.Migrate();
}

app.Run();
