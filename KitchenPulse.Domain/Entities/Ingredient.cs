using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Entities;

public class Ingredient
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public StockUnit Unit { get; private set; }
    public StorageType StorageType { get; private set; }

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

}
