namespace KitchenPulse.Domain.Entities;

public class OrderLine
{
    public Dish Dish { get; private set; }
    public int Quantity { get; private set; }

    public OrderLine(Dish dish, int quantity)
    {

        ArgumentNullException.ThrowIfNull(dish);

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        Dish = dish;
        Quantity = quantity;
    }
}
