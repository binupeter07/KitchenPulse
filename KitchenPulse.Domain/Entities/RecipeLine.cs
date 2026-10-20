namespace KitchenPulse.Domain.Entities;

public class RecipeLine
{
    public Ingredient Ingredient { get; private set; }
    public decimal Quantity { get; private set; }

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
