namespace KitchenPulse.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public int OrderNumber { get; private set; }
    public string Table { get; private set; }
    public DateTime PlacedAtUtc { get; private set; }

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

  

}
