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


    public Ingredient(string name, StockUnit unit, StorageType storageType)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Ingredient name cannot be null or blank.", nameof(name));
        }
        Id = Guid.NewGuid();
        Name = name.Trim();
        Unit = unit;
        StorageType = storageType;
    }



    public void AddAllergen(Allergen allergen)
    {

        if (!Enum.IsDefined(allergen))
        {
            throw new ArgumentException("Invalid allergen value.", nameof(allergen));
        }

        if (!_allergens.Contains(allergen))
        {
            _allergens.Add(allergen);
        }
    }

    public void RemoveAllergen(Allergen allergen)
    {

        _allergens.Remove(allergen);

    }


}
