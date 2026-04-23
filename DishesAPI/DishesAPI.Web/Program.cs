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

// Dishes Routes
var dishEndpoints = app.MapGroup("/dishes");

// dishes
dishEndpoints.MapGet("", async Task<Ok<IEnumerable<DishDto>>>(DishesDbContext dbContext, ClaimsPrincipal user, [FromQuery(Name = "name")] string? name) =>
{
    Console.WriteLine($"User: {user.Identity?.Name ?? "Anonymous"}, Name filter: {name}");
    var dishes = (await dbContext.Dishes.Where(d => string.IsNullOrEmpty(name) || d.Name.Contains(name))
                                    .ToListAsync()).ToDishDtoList();
    return TypedResults.Ok(dishes);
}).WithName("GetDishes");

// dish by Id
dishEndpoints.MapGet("/{dishId:guid}", async Task<Results<NotFound, Ok<DishDto>>> (Guid dishId, DishesDbContext dbContext) =>
{
    var dishEntity = await dbContext.Dishes.Where(d => d.Id == dishId).FirstOrDefaultAsync();
    if (dishEntity is null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(dishEntity.ToDishDto());
}).WithName("GetDishById");

// dish by name
dishEndpoints.MapGet("/{dishName}", async Task<Results<NotFound, Ok<DishDto>>> (string dishName, DishesDbContext dbContext) =>
{
    var dishEntity = await dbContext.Dishes.Where(d => d.Name == dishName).FirstOrDefaultAsync();
    if (dishEntity is null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(dishEntity?.ToDishDto());
}).WithName("GetDishByName");

// dish create
dishEndpoints.MapPost("", async Task<CreatedAtRoute<DishDto>> (DishesDbContext dbContext, [FromBody] DishCreateDto dishCreateDto) =>
{
    var dishEntity = dishCreateDto.ToDish();
    dbContext.Dishes.Add(dishEntity);
    await dbContext.SaveChangesAsync();
    var newDish = dishEntity.ToDishDto();
    return TypedResults.CreatedAtRoute(newDish, "GetDishById", new { dishId = newDish.Id });
});

// dish update
dishEndpoints.MapPut("/{dishId}", async Task<Results<NotFound, Ok<DishUpdateDto>>> (DishesDbContext dbContext, Guid dishId, DishUpdateDto dishUpdateDto) =>
{
    var dishEntity = await dbContext.Dishes.Where(d => d.Id == dishId).FirstOrDefaultAsync();
    if (dishEntity is null)
    {
        return TypedResults.NotFound();
    }

    dishEntity.UpdateDishFromDto(dishUpdateDto);
    await dbContext.SaveChangesAsync();
    return TypedResults.Ok(dishUpdateDto);
});

// dish delete
dishEndpoints.MapDelete("/{dishId}", async Task<Results<NotFound, NoContent>> (DishesDbContext dbContext, Guid dishId) =>
{
    var dishEntity = await dbContext.Dishes.Where(d => d.Id == dishId).FirstOrDefaultAsync();
    if (dishEntity is null)
    {
        return TypedResults.NotFound();
    }
    dbContext.Dishes.Remove(dishEntity);
    await dbContext.SaveChangesAsync();
    return TypedResults.NoContent();
});

// Ingredient routes
var ingredientEndpoints = dishEndpoints.MapGroup("/{dishId}/ingredients");

// ingredients for a dish
ingredientEndpoints.MapGet("", async Task<Results<NotFound, Ok<IEnumerable<IngredientDto>>>>(Guid dishId, DishesDbContext dbContext) =>
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

// Error routes
app.MapGet("/error", () => {throw new NotImplementedException();});


// recreate & migrate the database on startup
using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.Migrate();
}

app.Run();
