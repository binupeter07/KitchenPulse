using KitchenPulse.Domain.Common;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Entities;

public class Ingredient
{
    private readonly List<IngredientAllergen> _allergens = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public StockUnit Unit { get; private set; }
    public StorageType StorageType { get; private set; }
    public IReadOnlyCollection<Allergen> Allergens => _allergens.Select(a => a.Allergen).ToArray();
    public decimal CurrentStock { get; private set; }
    public decimal ExtraPortion { get; private set; }
    public bool IsStockBelowZero => CurrentStock < 0;

    public Ingredient(string name, StockUnit unit, StorageType storageType)
    {
        ValidateName(name);
        Guard.EnumDefined(unit, nameof(unit));
        Guard.EnumDefined(storageType, nameof(storageType));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Unit = unit;
        StorageType = storageType;
    }

    public void AddAllergen(Allergen allergen)
    {
        Guard.EnumDefined(allergen, nameof(allergen));

        bool alreadyExists = _allergens.Any(a => a.Allergen == allergen);

        if (!alreadyExists)
        {
            var ingredientAllergen = new IngredientAllergen(allergen);
            _allergens.Add(ingredientAllergen);
        }
    }

    public void RemoveAllergen(Allergen allergen)
    {
        Guard.EnumDefined(allergen, nameof(allergen));
        var ingredientAllergen = _allergens.FirstOrDefault(a => a.Allergen == allergen);
        if (ingredientAllergen != null)
        {
            _allergens.Remove(ingredientAllergen);
        }
    }

    public void ApplyMovement(StockMovement movement)
    {
        ArgumentNullException.ThrowIfNull(movement);

        if (movement.Ingredient.Id != Id)
        {
            throw new ArgumentException("Movement belongs to a different ingredient.", nameof(movement));
        }

        CurrentStock += movement.Change;
    }

    public void SetExtraPortion(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Extra portion must be greater than zero.", nameof(amount));
        }

        ExtraPortion = amount;
    }

    public void Rename(string newName)
    {
        ValidateName(newName);

        Name = newName.Trim();
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Ingredient name cannot be null or blank.", nameof(name));
        }
    }
}