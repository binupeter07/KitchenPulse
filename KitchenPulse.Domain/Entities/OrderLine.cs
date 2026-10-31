using KitchenPulse.Domain.Common;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Entities;

public class OrderLine
{
    private readonly List<Ingredient> _removed = new();
    private readonly List<Ingredient> _extras = new();
    private readonly List<Allergen> _customerAllergies = new();


    public Dish Dish { get; private set; }
    public int Quantity { get; private set; }
    public Course Course { get; private set; }
    public bool IsDone { get; private set; }
    public bool IsSent { get; private set; }
    public IReadOnlyCollection<Ingredient> Removed => _removed;
    public IReadOnlyCollection<Ingredient> Extras => _extras;
    public IReadOnlyCollection<Allergen> CustomerAllergies => _customerAllergies;
    public bool HasCustomerAllergy => _customerAllergies.Any();
    public IReadOnlyCollection<Allergen> Allergens => IngredientsOnPlate
        .SelectMany(i => i.Allergens)
        .Distinct()
        .ToList();
    public IReadOnlyCollection<Allergen> UnsafeAllergens => Allergens
        .Intersect(CustomerAllergies)
        .ToList();
    public IReadOnlyCollection<Ingredient> UnsafeIngredients => IngredientsOnPlate
        .Where(i => i.Allergens.Any(a => _customerAllergies.Contains(a)))
        .ToList();


    private IEnumerable<Ingredient> IngredientsOnPlate => Dish.RecipeLines
        .Select(rl => rl.Ingredient)
        .Where(i => !_removed.Contains(i))
        .Concat(_extras);


    public OrderLine(Dish dish, int quantity, Course course = Course.Main)
    {
        ArgumentNullException.ThrowIfNull(dish);
        Guard.EnumDefined(course, nameof(course));

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Dish = dish;
        Quantity = quantity;
        Course = course;
    }

    public void RemoveIngredient(Ingredient ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        bool isInDish = Dish.RecipeLines.Any(rl => rl.Ingredient.Id == ingredient.Id);

        if (!isInDish)
        {
            throw new ArgumentException($"There is no {ingredient.Name} in {Dish.Name}.", nameof(ingredient));
        }

        if (_extras.Contains(ingredient))
        {
            throw new InvalidOperationException($"{ingredient.Name} can't be added as an extra and removed at the same time.");
        }

        if (!_removed.Contains(ingredient))
        {
            _removed.Add(ingredient);
        }
    }

    public void AddExtra(Ingredient ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        if (_removed.Contains(ingredient))
        {
            throw new InvalidOperationException($"{ingredient.Name} can't be removed and added as an extra at the same time.");
        }

        if (ingredient.ExtraPortion <= 0)
        {
            throw new InvalidOperationException($"{ingredient.Name} has no extra portion set.");
        }

        if (!_extras.Contains(ingredient))
        {
            _extras.Add(ingredient);
        }
    }

    public void RemoveUnsafeIngredients()
    {
        foreach (var ingredient in UnsafeIngredients)
        {
            if (_extras.Contains(ingredient))
            {
                _extras.Remove(ingredient);
            }
            else
            {
                RemoveIngredient(ingredient);
            }
        }
    }
    public void AddCustomerAllergy(Allergen allergen)
    {
        Guard.EnumDefined(allergen, nameof(allergen));

        if (!_customerAllergies.Contains(allergen))
        {
            _customerAllergies.Add(allergen);
        }
    }

    public void MarkDone()
    {
        IsDone = true;
    }


    public void MarkSent()
    {
        IsSent = true;
    }

    public IReadOnlyCollection<StockMovement> CreateSaleMovements(DateTime occurredAtUtc, string? reference)
    {
        var movements = new List<StockMovement>();

        foreach (var recipeLine in Dish.RecipeLines)
        {
            if (!_removed.Contains(recipeLine.Ingredient))
            {
                var amount = recipeLine.Quantity * Quantity;
                movements.Add(StockMovement.Sale(recipeLine.Ingredient, amount, occurredAtUtc, reference));
            }
        }

        foreach (var extra in _extras)
        {
            var amount = extra.ExtraPortion * Quantity;
            movements.Add(StockMovement.Sale(extra, amount, occurredAtUtc, reference));
        }

        return movements;
    }
}