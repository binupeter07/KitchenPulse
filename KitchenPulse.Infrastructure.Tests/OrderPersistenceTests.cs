using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KitchenPulse.Infrastructure.Tests;

public class OrderPersistenceTests
{
    private static readonly DateOnly BusinessDate = new(2026, 10, 20);
    private static readonly DateTime PlacedAtUtc = new(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

    private static Dish CreateClassicWithMilkyCheese()
    {
        var beef = new Ingredient("Beef patty", StockUnit.Each, StorageType.Chilled);
        var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
        cheese.AddAllergen(Allergen.Milk);

        var classic = new Dish("The Classic", "Classic beef burger", 11.50m, Station.Grill);
        classic.AddIngredient(beef, 1);
        classic.AddIngredient(cheese, 1);

        return classic;
    }

    private static async Task<(Guid SiteId, Guid DishId)> SeedSiteAndDishAsync(TestDatabase testDatabase, string siteName = "Leeds")
    {
        await using var context = testDatabase.CreateContext();

        var site = new Site(siteName);
        var dish = CreateClassicWithMilkyCheese();
        context.Sites.Add(site);
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        return (site.Id, dish.Id);
    }

    private static async Task<Guid> SeedSiteAsync(TestDatabase testDatabase, string siteName)
    {
        await using var context = testDatabase.CreateContext();

        var site = new Site(siteName);
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        return site.Id;
    }

    private static async Task<Guid> SeedOrderWithLineAsync(TestDatabase testDatabase, Guid siteId, Guid dishId)
    {
        await using var context = testDatabase.CreateContext();

        var dish = await context.Dishes.SingleAsync(d => d.Id == dishId);
        var order = new Order(siteId, 1, "8", PlacedAtUtc, OrderType.EatIn, BusinessDate);
        order.AddDish(dish, 2);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return order.Id;
    }

    [Fact]
    public async Task Order_SaveAndLoad_RoundTripsAllValues()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (siteId, dishId) = await SeedSiteAndDishAsync(testDatabase);

        Guid orderId;
        await using (var context = testDatabase.CreateContext())
        {
            var dish = await context.Dishes.SingleAsync(d => d.Id == dishId);
            var order = new Order(siteId, 1043, " 8 ", PlacedAtUtc, OrderType.EatIn, BusinessDate);
            var starter = order.AddDish(dish, 1, course: Course.Starter);
            starter.MarkDone();
            order.CollectCourse(Course.Starter);
            context.Orders.Add(order);
            context.Entry(order).Property(o => o.IsCollected).CurrentValue = true;
            await context.SaveChangesAsync();
            orderId = order.Id;
        }

        await using var readContext = testDatabase.CreateContext();
        var saved = await readContext.Orders.SingleAsync(o => o.Id == orderId);

        Assert.Equal(siteId, saved.SiteId);
        Assert.Equal(OrderType.EatIn, saved.OrderType);
        Assert.Equal(BusinessDate, saved.BusinessDate);
        Assert.Equal(1043, saved.OrderNumber);
        Assert.Equal("8", saved.Table);
        Assert.Equal(PlacedAtUtc, saved.PlacedAtUtc);
        Assert.True(saved.IsCollected);
        Assert.Equal([Course.Starter], saved.CollectedCourse);
    }

    [Fact]
    public async Task Order_LoadedInNewContext_HasLinesWithDishesAndAllergens()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (siteId, dishId) = await SeedSiteAndDishAsync(testDatabase);
        var orderId = await SeedOrderWithLineAsync(testDatabase, siteId, dishId);

        await using var readContext = testDatabase.CreateContext();
        var saved = await readContext.Orders.SingleAsync(o => o.Id == orderId);

        var line = Assert.Single(saved.Lines);
        Assert.NotEqual(Guid.Empty, line.Id);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(Course.Main, line.Course);
        Assert.Equal("The Classic", line.Dish.Name);
        Assert.Equal(2, line.Dish.RecipeLines.Count);
        Assert.Equal([Allergen.Milk], line.Allergens);
    }

    [Fact]
    public async Task Order_DuplicateSiteDayTypeAndNumber_IsRejected()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var siteId = await SeedSiteAsync(testDatabase, "Leeds");

        await using (var context = testDatabase.CreateContext())
        {
            context.Orders.Add(new Order(siteId, 1, "8", PlacedAtUtc, OrderType.EatIn, BusinessDate));
            await context.SaveChangesAsync();
        }

        await using var duplicateContext = testDatabase.CreateContext();
        duplicateContext.Orders.Add(new Order(siteId, 1, "9", PlacedAtUtc, OrderType.EatIn, BusinessDate));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => duplicateContext.SaveChangesAsync());

        Assert.Contains("IX_Orders_SiteId_BusinessDate_OrderType_OrderNumber", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("Type")]
    [InlineData("Day")]
    [InlineData("Site")]
    public async Task Order_SameNumberWhenOneValueDiffers_IsAccepted(string differentValue)
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var siteId = await SeedSiteAsync(testDatabase, "Leeds");
        var otherSiteId = await SeedSiteAsync(testDatabase, "York");

        await using (var context = testDatabase.CreateContext())
        {
            context.Orders.Add(new Order(siteId, 1, "8", PlacedAtUtc, OrderType.EatIn, BusinessDate));

            var second = differentValue switch
            {
                "Type" => new Order(siteId, 1, null, PlacedAtUtc, OrderType.Takeaway, BusinessDate),
                "Day" => new Order(siteId, 1, "8", PlacedAtUtc, OrderType.EatIn, BusinessDate.AddDays(1)),
                _ => new Order(otherSiteId, 1, "8", PlacedAtUtc, OrderType.EatIn, BusinessDate)
            };
            context.Orders.Add(second);

            await context.SaveChangesAsync();
        }

        await using var readContext = testDatabase.CreateContext();
        Assert.Equal(2, await readContext.Orders.CountAsync());
    }

    [Fact]
    public async Task Site_DeleteWithOrders_IsRejected()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (siteId, dishId) = await SeedSiteAndDishAsync(testDatabase);
        await SeedOrderWithLineAsync(testDatabase, siteId, dishId);

        await using var context = testDatabase.CreateContext();
        var site = await context.Sites.SingleAsync(s => s.Id == siteId);
        context.Sites.Remove(site);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains("FK_Orders_Sites_SiteId", exception.InnerException!.Message);
    }

    [Fact]
    public async Task Dish_DeleteWhileOrdered_IsRejected()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (siteId, dishId) = await SeedSiteAndDishAsync(testDatabase);
        await SeedOrderWithLineAsync(testDatabase, siteId, dishId);

        await using var context = testDatabase.CreateContext();
        var dish = await context.Dishes.SingleAsync(d => d.Id == dishId);
        context.Dishes.Remove(dish);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains("FK_OrderLines_Dishes_DishId", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("OrderType", "CK_Orders_OrderType")]
    [InlineData("OrderNumber", "CK_Orders_OrderNumber")]
    [InlineData("Quantity", "CK_OrderLines_Quantity")]
    [InlineData("Course", "CK_OrderLines_Course")]
    public async Task Order_CheckConstraints_AreEnforced(string invalidColumn, string expectedConstraint)
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (siteId, dishId) = await SeedSiteAndDishAsync(testDatabase);
        var orderId = await SeedOrderWithLineAsync(testDatabase, siteId, dishId);

        await using var context = testDatabase.CreateContext();
        var newId = Guid.CreateVersion7();
        var orderType = invalidColumn == "OrderType" ? 99 : 2;
        var orderNumber = invalidColumn == "OrderNumber" ? 0 : 5;
        var quantity = invalidColumn == "Quantity" ? 0 : 1;
        var course = invalidColumn == "Course" ? 99 : 2;

        var exception = await Assert.ThrowsAsync<SqlException>(() =>
            invalidColumn is "OrderType" or "OrderNumber"
                ? context.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO Orders (Id, SiteId, OrderNumber, OrderType, BusinessDate, PlacedAtUtc, IsCollected, CollectedCourses) VALUES ({newId}, {siteId}, {orderNumber}, {orderType}, {BusinessDate}, {PlacedAtUtc}, 0, '[]')")
                : context.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO OrderLines (Id, OrderId, DishId, Quantity, Course, IsDone, IsSent) VALUES ({newId}, {orderId}, {dishId}, {quantity}, {course}, 0, 0)"));

        Assert.Contains(expectedConstraint, exception.Message);
    }
}
