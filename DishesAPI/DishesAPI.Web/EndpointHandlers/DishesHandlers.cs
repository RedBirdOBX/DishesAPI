using DishesAPI.DbContexts;
using DishesAPI.Web.Extensions;
using DishesAPI.Web.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DishesAPI.Web.EndpointHandlers;

public static class DishesHandlers
{
    /// <summary>
    /// Get Dishes with optional name filter. If the name query parameter is provided,
    /// it will return dishes that contain the specified name. If no name is provided,
    /// it will return all dishes.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="user"></param>
    /// <param name="name"></param>
    /// <returns>collection of DishDto</returns>
    public static async Task<Ok<IEnumerable<DishDto>>>GetDishesAsync(DishesDbContext dbContext, ILogger<DishDto> logger, ClaimsPrincipal user, [FromQuery(Name = "name")] string? name)
    {
        Console.WriteLine($"User: {user.Identity?.Name ?? "Anonymous"}, Name filter: {name}");
        logger.LogInformation("Getting dishes with name filter: {NameFilter}. Is authenticated: {IsAuthenticated}", name, user.Identity?.IsAuthenticated ?? false);

        var dishes = (await dbContext.Dishes.Where(d => string.IsNullOrEmpty(name) || d.Name.Contains(name))
                                        .ToListAsync()).ToDishDtoList();
        return TypedResults.Ok(dishes);
    }

    /// <summary>
    /// Get a dish by its unique identifier (dishId). If a dish with the specified ID exists,
    /// it will return the dish. If no dish is found, it will return a NotFound result.
    /// </summary>
    /// <param name="dishId"></param>
    /// <param name="dbContext"></param>
    /// <returns>DishDto</returns>
    public static async Task<Results<NotFound, Ok<DishDto>>> GetDishByIdAsync([FromRoute] Guid dishId, DishesDbContext dbContext)
    {
        var dishEntity = await dbContext.Dishes.Where(d => d.Id == dishId).FirstOrDefaultAsync();
        if (dishEntity is null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(dishEntity.ToDishDto());
    }

    /// <summary>
    /// Get Dish by name. If a dish with the specified name exists, it will return the dish.
    /// </summary>
    /// <param name="dishName"></param>
    /// <param name="dbContext"></param>
    /// <returns>DishDto</returns>
    public static async Task<Results<NotFound, Ok<DishDto>>> GetDIshByNameAsync(string dishName, DishesDbContext dbContext)
    {
        var dishEntity = await dbContext.Dishes.Where(d => d.Name == dishName).FirstOrDefaultAsync();
        if (dishEntity is null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(dishEntity?.ToDishDto());
    }

    /// <summary>
    /// Create a new dish using the provided DishCreateDto.
    /// The method will convert the DTO to a Dish entity,
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="dishCreateDto"></param>
    /// <returns>DishDto</returns>
    public static async Task<CreatedAtRoute<DishDto>> CreateDishAsync(DishesDbContext dbContext, [FromBody] DishCreateDto dishCreateDto)
    {
        var dishEntity = dishCreateDto.ToDish();
        dbContext.Dishes.Add(dishEntity);
        await dbContext.SaveChangesAsync();
        var newDish = dishEntity.ToDishDto();
        return TypedResults.CreatedAtRoute(newDish, "GetDishById", new { dishId = newDish.Id });
    }

    /// <summary>
    /// Updates dish with the specified dishId using the provided DishUpdateDto.
    /// If a dish with the specified ID exists,
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="dishId"></param>
    /// <param name="dishUpdateDto"></param>
    /// <returns>DishUpdateDto</returns>
    public static async Task<Results<NotFound, Ok<DishUpdateDto>>> UpdateDishAsync(DishesDbContext dbContext, Guid dishId, DishUpdateDto dishUpdateDto)
    {
        var dishEntity = await dbContext.Dishes.Where(d => d.Id == dishId).FirstOrDefaultAsync();
        if (dishEntity is null)
        {
            return TypedResults.NotFound();
        }

        dishEntity.UpdateDishFromDto(dishUpdateDto);
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(dishUpdateDto);
    }

    /// <summary>
    /// Deletes dish from the database based on the provided dishId.
    /// If a dish with the specified ID exists, it will be removed from the database and a NoContent result will be returned. If no dish is found, a NotFound result will be returned.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="dishId"></param>
    /// <returns>void</returns>
    public static async Task<Results<NotFound, NoContent>> DeleteDishAsync(DishesDbContext dbContext, Guid dishId)
    {
        var dishEntity = await dbContext.Dishes.Where(d => d.Id == dishId).FirstOrDefaultAsync();
        if (dishEntity is null)
        {
            return TypedResults.NotFound();
        }

        dbContext.Dishes.Remove(dishEntity);
            await dbContext.SaveChangesAsync();
            return TypedResults.NoContent();
    }
}
