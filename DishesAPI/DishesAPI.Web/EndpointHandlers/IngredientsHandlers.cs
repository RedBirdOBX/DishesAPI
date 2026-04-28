using DishesAPI.DbContexts;
using DishesAPI.Web.Extensions;
using DishesAPI.Web.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DishesAPI.Web.EndpointHandlers;

public static class IngredientsHandlers
{
    /// <summary>
    /// Gets the ingredients for a specific dish. If the dish does not exist, returns a 404 Not Found response.
    /// Otherwise, returns a 200 OK response with the list of ingredients for the dish.
    /// </summary>
    /// <param name="dishId"></param>
    /// <param name="dbContext"></param>
    /// <returns>collection of IngredientDto</returns>
    public static async Task<Results<NotFound, Ok<IEnumerable<IngredientDto>>>> GetIngredientsForDishAsync(Guid dishId, DishesDbContext dbContext)
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
    }
}
