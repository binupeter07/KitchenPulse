using KitchenPulse.Domain.Entities;
using KitchenPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KitchenPulse.Infrastructure.Tests;

public class KitchenPulseDbContextModelTests
{

    private static IModel GetKitchenDbModel()
    {
        var options = new DbContextOptionsBuilder<KitchenPulseDbContext>()
             .UseSqlServer(
           "Server=(localdb)\\MSSQLLocalDB;Database=KitchenPulseDbTest;Trusted_Connection=True;")
           .Options;

        using var context = new KitchenPulseDbContext(options);

        return context.Model;
    }


    [Fact]
    public void KitchenDbModel_WhenAccessed_ShouldInitialize()
    {
        var model = GetKitchenDbModel();

        Assert.NotNull(model);
    }

    [Fact]
    public void Ingredient_WhenModelCreated_ShouldBeRegistered()
    {
        var model = GetKitchenDbModel();

        var ingredientEntity = model.FindEntityType(typeof(Ingredient));

        Assert.NotNull(ingredientEntity);
    }

    [Fact]
    public void Ingredient_ShouldHaveIdAsPrimaryKey()
    {
        var model = GetKitchenDbModel();

        var ingredientEntity = model.FindEntityType(typeof(Ingredient));

        var primaryKey = ingredientEntity.FindPrimaryKey();
        var propertyName = primaryKey.Properties.Single().Name;

        Assert.Equal("Id", propertyName);
    }
}
