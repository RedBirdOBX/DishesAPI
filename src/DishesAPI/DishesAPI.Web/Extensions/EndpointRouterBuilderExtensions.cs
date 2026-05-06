using DishesAPI.Web.EndpointHandlers;

namespace DishesAPI.Web.Extensions;

public static class EndpointRouterBuilderExtensions
{
    public static void RegisterDishesEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Dishes Routes
        var dishEndpoints = endpointRouteBuilder.MapGroup("/dishes")
                            .WithTags("Dishes");

        // dishes - uses the route pattern and the method **delegate**
        dishEndpoints.MapGet("", DishesHandlers.GetDishesAsync)
                        .RequireAuthorization()
                        .WithName("GetDishes")
                        .WithSummary("Gets all dishes")
                        .WithDescription("Retrieves a list of all available dishes.");

        // dish by Id
        dishEndpoints.MapGet("/{dishId:guid}", DishesHandlers.GetDishByIdAsync)
                        .AllowAnonymous()
                        .WithName("GetDishById")
                        .WithSummary("Gets a dish by its ID")
                        .WithDescription("Retrieves the details of a specific dish using its unique identifier.");

        // dish by name
        dishEndpoints.MapGet("/{dishName}", DishesHandlers.GetDIshByNameAsync)
                        .WithName("GetDishByName")
                        .WithSummary("Gets a dish by its name")
                        .WithDescription("Retrieves the details of a specific dish using its name.");

        // dish create
        dishEndpoints.MapPost("", DishesHandlers.CreateDishAsync)
                        .RequireAuthorization("MustBeAdmin")
                        .WithName("CreateDish")
                        .WithSummary("Creates a new dish")
                        .WithDescription("Adds a new dish to the collection.")
                        .ProducesValidationProblem(400);

        // dish update
        dishEndpoints.MapPut("/{dishId}", DishesHandlers.UpdateDishAsync)
                        .WithName("UpdateDish")
                        .WithSummary("Updates an existing dish")
                        .WithDescription("Modifies the details of a specific dish using its unique identifier.");

        // dish delete
        dishEndpoints.MapDelete("/{dishId}", DishesHandlers.DeleteDishAsync)
                        .WithName("DeleteDish")
                        .WithSummary("Deletes a dish")
                        .WithDescription("Removes a specific dish using its unique identifier.");
    }

    public static void RegisterIngredientsEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Ingredient routes
        var ingredientEndpoints = endpointRouteBuilder.MapGroup("/dishes/{dishId}/ingredients")
                                    .WithTags("Ingredients");

        // ingredients for a dish
        ingredientEndpoints.MapGet("", IngredientsHandlers.GetIngredientsForDishAsync)
                            .WithName("GetDishIngredients")
                            .WithSummary("Gets ingredients for a dish")
                            .WithDescription("Retrieves the list of ingredients for a specific dish using its unique identifier.");
    }
}
