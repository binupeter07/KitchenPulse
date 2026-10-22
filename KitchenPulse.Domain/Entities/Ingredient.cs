using KitchenPulse.Domain.Common;
using KitchenPulse.Domain.Enums;
using static System.Collections.Specialized.BitVector32;

namespace KitchenPulse.Domain.Entities;

public class Ingredient
{
    private readonly List<Allergen> _allergens = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public StockUnit Unit { get; private set; }
    public StorageType StorageType { get; private set; }
    public IReadOnlyCollection<Allergen> Allergens => _allergens;

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
