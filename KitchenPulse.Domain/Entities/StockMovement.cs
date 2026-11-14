using KitchenPulse.Domain.Common;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Entities;

public class StockMovement
{
    public Guid Id { get; private set; }
    public Guid SiteId { get; private set; }
    public Ingredient Ingredient { get; private set; }
    public StockMovementType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public string? Reference { get; private set; }
    public decimal Change => Type switch
    {
        StockMovementType.Sale => -Quantity,
        StockMovementType.Delivery => Quantity,
        StockMovementType.Waste => -Quantity,
        StockMovementType.StaffMeal => -Quantity,
        StockMovementType.StocktakeLoss => -Quantity,
        StockMovementType.StocktakeGain => Quantity,
        _ => throw new InvalidOperationException("Unknown type.")
    };

    private StockMovement(Guid siteId, Ingredient ingredient, StockMovementType type, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        ArgumentNullException.ThrowIfNull(ingredient);
        Guard.EnumDefined(type, nameof(type));
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        }

        Id = Guid.NewGuid();
        SiteId = siteId;
        Ingredient = ingredient;
        Type = type;
        Quantity = quantity;
        OccurredAtUtc = occurredAtUtc;
        Reference = reference?.Trim();
    }

    public static StockMovement Sale(Guid siteId, Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(siteId, ingredient, StockMovementType.Sale, quantity, occurredAtUtc, reference);
    }

    public static StockMovement Waste(Guid siteId, Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(siteId, ingredient, StockMovementType.Waste, quantity, occurredAtUtc, reference);
    }

    public static StockMovement StaffMeal(Guid siteId, Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(siteId, ingredient, StockMovementType.StaffMeal, quantity, occurredAtUtc, reference);
    }

    public static StockMovement StocktakeGain(Guid siteId, Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(siteId, ingredient, StockMovementType.StocktakeGain, quantity, occurredAtUtc, reference);
    }

    public static StockMovement StocktakeLoss(Guid siteId, Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(siteId, ingredient, StockMovementType.StocktakeLoss, quantity, occurredAtUtc, reference);
    }

    public static StockMovement Delivery(Guid siteId, Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(siteId, ingredient, StockMovementType.Delivery, quantity, occurredAtUtc, reference);
    }
}
