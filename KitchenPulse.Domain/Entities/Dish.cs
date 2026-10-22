using KitchenPulse.Domain.Common;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Entities;

public class Dish
{
    private readonly List<RecipeLine> _recipeLines = new();


    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public Station Station { get; private set; }
    public IReadOnlyCollection<RecipeLine> RecipeLines => _recipeLines;
    public IReadOnlyCollection<Allergen> Allergens => _recipeLines
        .SelectMany(rl => rl.Ingredient.Allergens)
        .Distinct()
        .ToList();

    public Dish(string name, string? description, decimal price, Station station)
    {
        

        ValidateName(name);
        ValidatePrice(price);
        Guard.EnumDefined(station, nameof(station));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        Station = station;
    }


    public void AddIngredient(Ingredient ingredient, decimal quantity)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        if (_recipeLines.Any(rl => rl.Ingredient.Id == ingredient.Id))
        {
            throw new InvalidOperationException($"Ingredient '{ingredient.Name}' is already added to the dish.");
        }
        var recipeLine = new RecipeLine(ingredient, quantity);
        _recipeLines.Add(recipeLine);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Dish name cannot be null or blank.", nameof(name));
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new ArgumentException("Dish price must be greater than zero.", nameof(price));
        }
    }
}
