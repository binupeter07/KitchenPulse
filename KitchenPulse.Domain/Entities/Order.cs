namespace KitchenPulse.Domain.Entities;

public class Order
{

    private readonly List<OrderLine> _lines = new();
    public Guid Id { get; private set; }
    public int OrderNumber { get; private set; }
    public string Table { get; private set; }
    public DateTime PlacedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines;

    public Order(int orderNumber, string table, DateTime placedAtUtc)
    {

        if (string.IsNullOrWhiteSpace(table))
        {
            throw new ArgumentException("Table cannot be null or blank.", nameof(table));
        }
        if (orderNumber <= 0)
        {
            throw new ArgumentException("Order number must be greater than zero.", nameof(orderNumber));
        }

        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        Table = table.Trim();
        PlacedAtUtc = placedAtUtc;
    }


    public OrderLine AddDish(Dish dish, int quantity, IEnumerable<Ingredient>? removeIngredients = null)
    {
        var line = new OrderLine(dish, quantity);

        foreach (var ingredient in removeIngredients ?? [])
        {
            line.RemoveIngredient(ingredient);
        }

        _lines.Add(line);

        return line;
    }

   
}
