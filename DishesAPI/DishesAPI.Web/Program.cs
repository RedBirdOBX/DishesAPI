using DishesAPI.DbContexts;
using DishesAPI.Web.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// routes
app.MapGet("/dishes", async (DishesDbContext dbContext) =>
{
    return (await dbContext.Dishes.ToListAsync()).ToDishDtoList();
});

app.MapGet("/dishes/{dishId:guid}", async (Guid dishId, DishesDbContext dbContext) =>
{
    var dishEntity = await dbContext.Dishes.Where(d => d.Id == dishId).FirstOrDefaultAsync();
    return dishEntity?.ToDishDto();
}).WithName("GetDishById");

app.MapGet("/dishes/{dishName}", async (string dishName, DishesDbContext dbContext) =>
{
    var dishEntity = await dbContext.Dishes.Where(d => d.Name == dishName).FirstOrDefaultAsync();
    return dishEntity?.ToDishDto();
}).WithName("GetDishByName");

app.MapGet("/dishes/{dishId}/ingredients", async (Guid dishId, DishesDbContext dbContext) =>
{
    var dishEntity = await dbContext.Dishes
                        .Include(d => d.Ingredients)
                        .Where(d => d.Id == dishId)
                        .FirstOrDefaultAsync();
    return dishEntity?.Ingredients.ToIngredientDtoList(dishId);
}).WithName("GetDishIngredients");


// recreate & migrate the database on startup
using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.Migrate();
}

app.Run();
