using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KitchenPulse.Infrastructure.Tests;

public class StockLevelPersistenceTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static async Task<(Site Site, Ingredient Ingredient)> SeedSiteAndIngredientAsync(TestDatabase testDatabase)
    {
        await using var context = testDatabase.CreateContext();

        var site = new Site("Leeds");
        var flour = new Ingredient("Flour", StockUnit.Each, StorageType.Ambient);
        context.Sites.Add(site);
        context.Ingredients.Add(flour);
        await context.SaveChangesAsync();

        return (site, flour);
    }

    [Fact]
    public async Task StockLevel_SaveAndLoad_RoundTripsQuantityAfterApply()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (site, flour) = await SeedSiteAndIngredientAsync(testDatabase);

        var stockLevel = new StockLevel(site.Id, flour.Id);
        stockLevel.Apply(StockMovement.Delivery(site.Id, flour, 10.125m, Now, null));
        stockLevel.Apply(StockMovement.Sale(site.Id, flour, 2.5m, Now, null));

        await using (var context = testDatabase.CreateContext())
        {
            context.StockLevels.Add(stockLevel);
            await context.SaveChangesAsync();
        }

        await using var readContext = testDatabase.CreateContext();
        var saved = await readContext.StockLevels
            .SingleAsync(s => s.SiteId == site.Id && s.IngredientId == flour.Id);

        Assert.Equal(7.625m, saved.Quantity);
    }

    [Fact]
    public async Task StockLevel_DuplicateSiteAndIngredient_IsRejected()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (site, flour) = await SeedSiteAndIngredientAsync(testDatabase);

        await using var context = testDatabase.CreateContext();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO StockLevels (SiteId, IngredientId, Quantity) VALUES ({site.Id}, {flour.Id}, {1})");

        var exception = await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(() =>
            context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO StockLevels (SiteId, IngredientId, Quantity) VALUES ({site.Id}, {flour.Id}, {2})"));

        Assert.Contains("PK_StockLevels", exception.Message);
    }

    [Theory]
    [InlineData("SiteId", "FK_StockLevels_Sites_SiteId")]
    [InlineData("IngredientId", "FK_StockLevels_Ingredients_IngredientId")]
    public async Task StockLevel_UnknownForeignKey_IsRejected(string unknownColumn, string expectedForeignKey)
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (site, flour) = await SeedSiteAndIngredientAsync(testDatabase);

        var siteId = unknownColumn == "SiteId" ? Guid.NewGuid() : site.Id;
        var ingredientId = unknownColumn == "IngredientId" ? Guid.NewGuid() : flour.Id;

        await using var context = testDatabase.CreateContext();
        context.StockLevels.Add(new StockLevel(siteId, ingredientId));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains(expectedForeignKey, exception.InnerException!.Message);
    }

    [Fact]
    public async Task StockLevel_TwoUpdatesFromSameVersion_SecondThrowsConcurrencyException()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (site, flour) = await SeedSiteAndIngredientAsync(testDatabase);

        await using (var seedContext = testDatabase.CreateContext())
        {
            seedContext.StockLevels.Add(new StockLevel(site.Id, flour.Id));
            await seedContext.SaveChangesAsync();
        }

        await using var firstContext = testDatabase.CreateContext();
        await using var secondContext = testDatabase.CreateContext();
        var first = await firstContext.StockLevels.SingleAsync();
        var second = await secondContext.StockLevels.SingleAsync();

        first.Apply(StockMovement.Delivery(site.Id, flour, 5m, Now, null));
        second.Apply(StockMovement.Delivery(site.Id, flour, 3m, Now, null));

        await firstContext.SaveChangesAsync();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync());

        await using var readContext = testDatabase.CreateContext();
        var saved = await readContext.StockLevels.SingleAsync();
        Assert.Equal(5m, saved.Quantity);
    }
}
