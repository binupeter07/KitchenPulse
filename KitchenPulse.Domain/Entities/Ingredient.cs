using KitchenPulse.Domain.Common;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Entities;

public class Ingredient
{
    private readonly List<Allergen> _allergens = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public StockUnit Unit { get; private set; }
    public StorageType StorageType { get; private set; }
    public IReadOnlyCollection<Allergen> Allergens => _allergens;
    public decimal CurrentStock { get; private set; }
    public decimal ExtraPortion { get; private set; }

    public bool IsStockBelowZero => CurrentStock < 0;
    public Ingredient(string name, StockUnit unit, StorageType storageType)
    {

        ValidateName(name);
        Guard.EnumDefined(storageType, nameof(storageType));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Unit = unit;
        StorageType = storageType;
    }

    public void AddAllergen(Allergen allergen)
    {

        Guard.EnumDefined(allergen, nameof(allergen));

        if (!_allergens.Contains(allergen))
        {
            _allergens.Add(allergen);
        }
    }

    public void RemoveAllergen(Allergen allergen)
    {

        _allergens.Remove(allergen);

    }

    public void ApplyMovement(StockMovement movement)
    {
        ArgumentNullException.ThrowIfNull(movement);

        if (movement.Ingredient.Id != Id)
        {
            throw new ArgumentException("Ingredient is not matching", nameof(movement));
        }
        CurrentStock += movement.Change;
    }

    public void SetExtraPortion(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Extra should be greater than zero", nameof(amount));
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
