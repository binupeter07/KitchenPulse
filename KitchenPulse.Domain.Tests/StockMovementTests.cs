using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests;

public class StockMovementTests
{
    [Fact]
    public void Sale_Change_IsNegative()
    {
        var sale = CreateMovement(StockMovement.Sale);

        Assert.Equal(-60m, sale.Change);
    }

    [Fact]
    public void Waste_Change_IsNegative()
    {
        var waste = CreateMovement(StockMovement.Waste);

        Assert.Equal(-60m, waste.Change);
    }

    [Fact]
    public void StaffMeal_Change_IsNegative()
    {
        var staffMeal = CreateMovement(StockMovement.StaffMeal);

        Assert.Equal(-60m, staffMeal.Change);
    }

    [Fact]
    public void StocktakeLoss_Change_IsNegative()
    {
        var stocktakeLoss = CreateMovement(StockMovement.StocktakeLoss);

        Assert.Equal(-60m, stocktakeLoss.Change);
    }

    [Fact]
    public void StocktakeGain_Change_IsPositive()
    {
        var stocktakeGain = CreateMovement(StockMovement.StocktakeGain);

        Assert.Equal(60m, stocktakeGain.Change);
    }

    [Fact]
    public void Delivery_Change_IsPositive()
    {
        var delivery = CreateMovement(StockMovement.Delivery);

        Assert.Equal(60m, delivery.Change);
    }

    [Fact]
    public void Create_ZeroQuantity_Throws()
    {
        var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
        var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);

        var exception = Assert.Throws<ArgumentException>(
            () => StockMovement.Waste(cheese, 0m, occurredAtUtc, "REF-001"));

        Assert.Equal("quantity", exception.ParamName);
    }

    private static StockMovement CreateMovement(Func<Ingredient, decimal, DateTime, string?, StockMovement> factory)
    {
        var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
        var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);

        return factory(cheese, 60m, occurredAtUtc, "REF-001");
    }
}