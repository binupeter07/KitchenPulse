using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KitchenPulse.Infrastructure.Tests;

public class StockMovementPersistenceTests
{
    private static readonly DateTime OccurredAtUtc = new(2026, 1, 1, 12, 30, 45, DateTimeKind.Utc);

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

    private static async Task<Guid> SeedMovementAsync(TestDatabase testDatabase, Guid siteId, Guid ingredientId)
    {
        await using var context = testDatabase.CreateContext();

        var ingredient = await context.Ingredients.SingleAsync(i => i.Id == ingredientId);
        var movement = StockMovement.Delivery(siteId, ingredient, 10m, OccurredAtUtc, "DEL-1");
        context.StockMovements.Add(movement);
        await context.SaveChangesAsync();

        return movement.Id;
    }

    [Fact]
    public async Task StockMovement_SaveAndLoad_RoundTripsAllValues()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (site, flour) = await SeedSiteAndIngredientAsync(testDatabase);

        Guid movementId;
        await using (var context = testDatabase.CreateContext())
        {
            var ingredient = await context.Ingredients.SingleAsync(i => i.Id == flour.Id);
            var movement = StockMovement.StocktakeLoss(site.Id, ingredient, 12.345m, OccurredAtUtc, "  COUNT-7  ");
            context.StockMovements.Add(movement);
            await context.SaveChangesAsync();
            movementId = movement.Id;
        }

        await using var readContext = testDatabase.CreateContext();
        var saved = await readContext.StockMovements
            .Include(m => m.Ingredient)
            .SingleAsync(m => m.Id == movementId);

        Assert.Equal(site.Id, saved.SiteId);
        Assert.Equal(StockMovementType.StocktakeLoss, saved.Type);
        Assert.Equal(12.345m, saved.Quantity);
        Assert.Equal(OccurredAtUtc, saved.OccurredAtUtc);
        Assert.Equal("COUNT-7", saved.Reference);
        Assert.Equal(flour.Id, saved.Ingredient.Id);
        Assert.Equal("Flour", saved.Ingredient.Name);
        Assert.Equal(-12.345m, saved.Change);
    }

    [Theory]
    [InlineData("SiteId", "FK_StockMovements_Sites_SiteId")]
    [InlineData("IngredientId", "FK_StockMovements_Ingredients_IngredientId")]
    public async Task StockMovement_UnknownForeignKey_IsRejected(string unknownColumn, string expectedForeignKey)
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (site, flour) = await SeedSiteAndIngredientAsync(testDatabase);

        await using var context = testDatabase.CreateContext();
        var ingredient = await context.Ingredients.SingleAsync(i => i.Id == flour.Id);
        var siteId = unknownColumn == "SiteId" ? Guid.CreateVersion7() : site.Id;
        var movement = StockMovement.Sale(siteId, ingredient, 1m, OccurredAtUtc, null);
        context.StockMovements.Add(movement);

        if (unknownColumn == "IngredientId")
        {
            context.Entry(movement).Property("IngredientId").CurrentValue = Guid.CreateVersion7();
        }

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains(expectedForeignKey, exception.InnerException!.Message);
    }

    [Fact]
    public async Task Ingredient_DeleteWithMovements_IsRejected()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (site, flour) = await SeedSiteAndIngredientAsync(testDatabase);
        await SeedMovementAsync(testDatabase, site.Id, flour.Id);

        await using var context = testDatabase.CreateContext();
        var ingredient = await context.Ingredients.SingleAsync(i => i.Id == flour.Id);
        context.Ingredients.Remove(ingredient);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains("FK_StockMovements_Ingredients_IngredientId", exception.InnerException!.Message);
    }

    [Fact]
    public async Task StockMovement_Delete_ThrowsInvalidOperationException()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (site, flour) = await SeedSiteAndIngredientAsync(testDatabase);
        var movementId = await SeedMovementAsync(testDatabase, site.Id, flour.Id);

        await using (var context = testDatabase.CreateContext())
        {
            var movement = await context.StockMovements.SingleAsync(m => m.Id == movementId);
            context.StockMovements.Remove(movement);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        }

        await using var readContext = testDatabase.CreateContext();
        Assert.Equal(1, await readContext.StockMovements.CountAsync());
    }

    [Fact]
    public async Task StockMovement_Update_ThrowsInvalidOperationException()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (site, flour) = await SeedSiteAndIngredientAsync(testDatabase);
        var movementId = await SeedMovementAsync(testDatabase, site.Id, flour.Id);

        await using (var context = testDatabase.CreateContext())
        {
            var movement = await context.StockMovements.SingleAsync(m => m.Id == movementId);
            context.Entry(movement).Property(m => m.Quantity).CurrentValue = 99m;

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        }

        await using var readContext = testDatabase.CreateContext();
        var saved = await readContext.StockMovements.SingleAsync(m => m.Id == movementId);
        Assert.Equal(10m, saved.Quantity);
    }

    [Fact]
    public async Task StockMovement_InvalidType_IsRejectedByDatabase()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (site, flour) = await SeedSiteAndIngredientAsync(testDatabase);

        await using var context = testDatabase.CreateContext();
        var id = Guid.CreateVersion7();

        var exception = await Assert.ThrowsAsync<SqlException>(() =>
            context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO StockMovements (Id, SiteId, IngredientId, Type, Quantity, OccurredAtUtc) VALUES ({id}, {site.Id}, {flour.Id}, {99}, {1}, {OccurredAtUtc})"));

        Assert.Contains("CK_StockMovements_Type", exception.Message);
    }
}
