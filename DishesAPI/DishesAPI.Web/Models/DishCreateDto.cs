using System.ComponentModel.DataAnnotations;

namespace DishesAPI.Web.Models;

public class DishCreateDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public required string Name { get; set; }
}
