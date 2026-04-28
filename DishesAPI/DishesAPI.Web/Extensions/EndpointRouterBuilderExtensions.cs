using DishesAPI.Web.EndpointHandlers;

namespace DishesAPI.Web.Extensions;

public static class EndpointRouterBuilderExtensions
{
    public static void RegisterDishesEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Dishes Routes
        var dishEndpoints = endpointRouteBuilder.MapGroup("/dishes");

        // dishes - uses the route pattern and the method **delegate**
        dishEndpoints.MapGet("", DishesHandlers.GetDishesAsync).WithName("GetDishes");

        // dish by Id
        dishEndpoints.MapGet("/{dishId:guid}", DishesHandlers.GetDishByIdAsync).WithName("GetDishById");

        // dish by name
        dishEndpoints.MapGet("/{dishName}", DishesHandlers.GetDIshByNameAsync).WithName("GetDishByName");

        // dish create
        dishEndpoints.MapPost("", DishesHandlers.CreateDishAsync).WithName("CreateDish");

        // dish update
        dishEndpoints.MapPut("/{dishId}", DishesHandlers.UpdateDishAsync).WithName("UpdateDish");

        // dish delete
        dishEndpoints.MapDelete("/{dishId}", DishesHandlers.DeleteDishAsync).WithName("DeleteDish");

    }

    public static void RegisterIngredientsEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Ingredient routes
        var ingredientEndpoints = endpointRouteBuilder.MapGroup("/dishes/{dishId}/ingredients");

        // ingredients for a dish
        ingredientEndpoints.MapGet("", IngredientsHandlers.GetIngredientsForDishAsync).WithName("GetDishIngredients");
    }
}
