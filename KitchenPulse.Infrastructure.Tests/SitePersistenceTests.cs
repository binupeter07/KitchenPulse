using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KitchenPulse.Infrastructure.Tests;

public class SitePersistenceTests
{
    [Fact]
    public async Task Site_SaveAndLoad_RoundTripsIdAndName()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();

        var site = new Site("Leeds");
        await using (var context = testDatabase.CreateContext())
        {
            context.Sites.Add(site);
            await context.SaveChangesAsync();
        }

        await using var readContext = testDatabase.CreateContext();
        var saved = await readContext.Sites.SingleAsync(s => s.Id == site.Id);

        Assert.Equal(site.Id, saved.Id);
        Assert.Equal("Leeds", saved.Name);
    }

    [Fact]
    public async Task Site_DuplicateName_IsRejected()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        await using var context = testDatabase.CreateContext();

        context.Sites.Add(new Site("Leeds"));
        await context.SaveChangesAsync();

        context.Sites.Add(new Site("Leeds"));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains("IX_Sites_Name", exception.InnerException!.Message);
    }

    [Fact]
    public async Task Site_DeleteWithStock_IsRejected()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        await using var context = testDatabase.CreateContext();

        var site = new Site("Leeds");
        var flour = new Ingredient("Flour", StockUnit.Each, StorageType.Ambient);
        context.Sites.Add(site);
        context.Ingredients.Add(flour);
        context.StockLevels.Add(new StockLevel(site.Id, flour.Id));
        await context.SaveChangesAsync();

        await using var deleteContext = testDatabase.CreateContext();
        var siteToDelete = await deleteContext.Sites.SingleAsync(s => s.Id == site.Id);
        deleteContext.Sites.Remove(siteToDelete);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => deleteContext.SaveChangesAsync());

        Assert.Contains("FK_StockLevels_Sites_SiteId", exception.InnerException!.Message);
    }
}
