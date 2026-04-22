using DishesAPI.Entities;
using DishesAPI.Web.Models;

namespace DishesAPI.Web.Extensions;

public static class MappingExtensions
{
    public static DishDto ToDishDto(this Dish dish)
    {
        return new DishDto
        {
            Id = dish.Id,
            Name = dish.Name
        };
    }

    public static Dish ToDish(this DishCreateDto dishCreateDto)
    {
        return new Dish
        {
            Name = dishCreateDto.Name
        };
    }

    public static IEnumerable<DishDto> ToDishDtoList(this IEnumerable<Dish> dishes) 
    { 
        return dishes.Select(d => d.ToDishDto());
    }

    public static IngredientDto ToIngredientDto(this Ingredient ingredient, Guid dishId)
    {
        return new IngredientDto
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            DishId = dishId
        };
    }

    public static IEnumerable<IngredientDto> ToIngredientDtoList(this IEnumerable<Ingredient> ingredients, Guid dishId)
    {
        return ingredients.Select(i => i.ToIngredientDto(dishId));
    }
}
