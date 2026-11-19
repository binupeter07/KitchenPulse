namespace KitchenPulse.Domain.Entities;

public class RecipeLine
{
    public Ingredient Ingredient { get; private set; }
    public decimal Quantity { get; private set; }

    // Only for EF Core.
    private RecipeLine()
    {
        Ingredient = null!; // EF Core sets this when it loads the entity.
    }

    public RecipeLine(Ingredient ingredient, decimal quantity)
    {
        if (ingredient is null)
        {
            throw new ArgumentNullException(nameof(ingredient), "Ingredient cannot be null.");
        }
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }
        Ingredient = ingredient;
        Quantity = quantity;
    }
}
