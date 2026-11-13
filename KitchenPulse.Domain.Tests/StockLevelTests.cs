using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests;

public class StockLevelTests
{
    private static readonly Guid SiteId = Guid.CreateVersion7();
    private static readonly DateTime OccurredAtUtc = new DateTime(2026, 10, 26, 10, 0, 0, DateTimeKind.Utc);

    private static Ingredient CreateCheese()
    {
        return new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
    }

    private static StockLevel CreateLevelFor(Ingredient ingredient)
    {
        return new StockLevel(SiteId, ingredient.Id);
    }

    [Fact]
    public void Constructor_ValidIds_SetsIdsAndStartsAtZero()
    {
        var ingredientId = Guid.CreateVersion7();

        var level = new StockLevel(SiteId, ingredientId);

        Assert.Equal(SiteId, level.SiteId);
        Assert.Equal(ingredientId, level.IngredientId);
        Assert.Equal(0m, level.Quantity);
    }

    [Fact]
    public void Constructor_EmptySiteId_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => new StockLevel(Guid.Empty, Guid.CreateVersion7()));

        Assert.Equal("siteId", ex.ParamName);
    }

    [Fact]
    public void Constructor_EmptyIngredientId_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => new StockLevel(SiteId, Guid.Empty));

        Assert.Equal("ingredientId", ex.ParamName);
    }

    [Fact]
    public void Apply_Delivery_IncreasesQuantity()
    {
        var cheese = CreateCheese();
        var level = CreateLevelFor(cheese);

        level.Apply(StockMovement.Delivery(SiteId, cheese, 40m, OccurredAtUtc, "D071"));

        Assert.Equal(40m, level.Quantity);
    }

    [Fact]
    public void Apply_Sale_DecreasesQuantity()
    {
        var cheese = CreateCheese();
        var level = CreateLevelFor(cheese);
        level.Apply(StockMovement.Delivery(SiteId, cheese, 40m, OccurredAtUtc, "D071"));

        level.Apply(StockMovement.Sale(SiteId, cheese, 1m, OccurredAtUtc, "S071"));

        Assert.Equal(39m, level.Quantity);
    }

    [Fact]
    public void Apply_SaleMoreThanStock_SetsIsStockBelowZero()
    {
        var cheese = CreateCheese();
        var level = CreateLevelFor(cheese);
        level.Apply(StockMovement.Delivery(SiteId, cheese, 5m, OccurredAtUtc, "D071"));

        level.Apply(StockMovement.Sale(SiteId, cheese, 8m, OccurredAtUtc, "S071"));

        Assert.Equal(-3m, level.Quantity);
        Assert.True(level.IsStockBelowZero);
    }

    [Fact]
    public void Apply_SaleToExactlyZero_IsNotBelowZero()
    {
        var cheese = CreateCheese();
        var level = CreateLevelFor(cheese);
        level.Apply(StockMovement.Delivery(SiteId, cheese, 5m, OccurredAtUtc, "D071"));
        Assert.Equal(5m, level.Quantity);

        level.Apply(StockMovement.Sale(SiteId, cheese, 5m, OccurredAtUtc, "S071"));

        Assert.Equal(0m, level.Quantity);
        Assert.False(level.IsStockBelowZero);
    }

    [Fact]
    public void Apply_DifferentSite_ThrowsAndQuantityUnchanged()
    {
        var cheese = CreateCheese();
        var level = CreateLevelFor(cheese);
        level.Apply(StockMovement.Delivery(SiteId, cheese, 10m, OccurredAtUtc, "D071"));
        var otherSiteDelivery = StockMovement.Delivery(Guid.CreateVersion7(), cheese, 25m, OccurredAtUtc, "D072");

        var ex = Assert.Throws<ArgumentException>(() => level.Apply(otherSiteDelivery));

        Assert.Equal("movement", ex.ParamName);
        Assert.Equal(10m, level.Quantity);
    }

    [Fact]
    public void Apply_DifferentIngredient_ThrowsAndQuantityUnchanged()
    {
        var cheese = CreateCheese();
        var beef = new Ingredient("Beef patty", StockUnit.Each, StorageType.Chilled);
        var level = CreateLevelFor(cheese);
        level.Apply(StockMovement.Delivery(SiteId, cheese, 10m, OccurredAtUtc, "D071"));
        var beefDelivery = StockMovement.Delivery(SiteId, beef, 25m, OccurredAtUtc, "D072");

        var ex = Assert.Throws<ArgumentException>(() => level.Apply(beefDelivery));

        Assert.Equal("movement", ex.ParamName);
        Assert.Equal(10m, level.Quantity);
    }

    [Fact]
    public void Apply_NullMovement_ThrowsArgumentNullException()
    {
        var level = CreateLevelFor(CreateCheese());

        var ex = Assert.Throws<ArgumentNullException>(() => level.Apply(null!));

        Assert.Equal("movement", ex.ParamName);
    }
}
