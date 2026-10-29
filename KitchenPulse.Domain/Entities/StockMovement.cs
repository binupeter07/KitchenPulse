using KitchenPulse.Domain.Common;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Entities;

public class StockMovement
{
    public Guid Id { get; private set; }
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

    private StockMovement(Ingredient ingredient, StockMovementType type, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        ArgumentNullException.ThrowIfNull(ingredient);
        Guard.EnumDefined(type, nameof(type));
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Id = Guid.NewGuid();
        Ingredient = ingredient;
        Type = type;
        Quantity = quantity;
        OccurredAtUtc = occurredAtUtc;
        Reference = reference?.Trim();
    }

    public static StockMovement Sale(Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(ingredient, StockMovementType.Sale, quantity, occurredAtUtc, reference);
    }

    public static StockMovement Waste(Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(ingredient, StockMovementType.Waste, quantity, occurredAtUtc, reference);
    }

    public static StockMovement StaffMeal(Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(ingredient, StockMovementType.StaffMeal, quantity, occurredAtUtc, reference);
    }

    public static StockMovement StocktakeGain(Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(ingredient, StockMovementType.StocktakeGain, quantity, occurredAtUtc, reference);
    }

    public static StockMovement StocktakeLoss(Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(ingredient, StockMovementType.StocktakeLoss, quantity, occurredAtUtc, reference);
    }

    public static StockMovement Delivery(Ingredient ingredient, decimal quantity, DateTime occurredAtUtc, string? reference)
    {
        return new StockMovement(ingredient, StockMovementType.Delivery, quantity, occurredAtUtc, reference);
    }
}
