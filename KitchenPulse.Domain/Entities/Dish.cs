namespace KitchenPulse.Domain.Entities;

public class Dish
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }

    public Dish(string name, string? description, decimal price)
    {
        ValidateName(name);
        ValidatePrice(price);

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
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
