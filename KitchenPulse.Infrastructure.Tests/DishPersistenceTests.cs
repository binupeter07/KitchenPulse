using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KitchenPulse.Infrastructure.Tests;

public class DishPersistenceTests
{
    private static Dish CreateBurgerWithMilkyCheese(out Ingredient cheese)
    {
        cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
        cheese.AddAllergen(Allergen.Milk);

        var burger = new Dish("Cheese Burger", "Beef burger with cheese", 9.50m, Station.Grill);
        burger.AddIngredient(cheese, 2m);
        return burger;
    }

    [Fact]
    public async Task Dish_SaveAndLoad_RoundTripsAllValues()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();

        var dish = new Dish("Chips", "Skin-on chips", 4.25m, Station.Fryer);
        await using (var context = testDatabase.CreateContext())
        {
            context.Dishes.Add(dish);
            await context.SaveChangesAsync();
        }

        await using var readContext = testDatabase.CreateContext();
        var saved = await readContext.Dishes.SingleAsync(d => d.Id == dish.Id);

        Assert.Equal("Chips", saved.Name);
        Assert.Equal("Skin-on chips", saved.Description);
        Assert.Equal(4.25m, saved.Price);
        Assert.Equal(Station.Fryer, saved.Station);
    }

    [Fact]
    public async Task Dish_LoadedInNewContext_HasRecipeLinesIngredientsAndAllergens()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();

        var dish = CreateBurgerWithMilkyCheese(out var cheese);
        await using (var context = testDatabase.CreateContext())
        {
            context.Dishes.Add(dish);
            await context.SaveChangesAsync();
        }

        await using var readContext = testDatabase.CreateContext();
        var saved = await readContext.Dishes.SingleAsync(d => d.Id == dish.Id);

        var line = Assert.Single(saved.RecipeLines);
        Assert.Equal(2m, line.Quantity);
        Assert.Equal(cheese.Id, line.Ingredient.Id);
        Assert.Equal("Cheese slice", line.Ingredient.Name);
        Assert.Equal(new[] { Allergen.Milk }, saved.Allergens);
    }

    [Fact]
    public async Task Dish_DuplicateName_IsRejected()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();

        await using var context = testDatabase.CreateContext();
        context.Dishes.Add(new Dish("Chips", null, 4m, Station.Fryer));
        await context.SaveChangesAsync();

        context.Dishes.Add(new Dish("Chips", null, 5m, Station.Front));
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains("IX_Dishes_Name", exception.InnerException!.Message);
    }

    [Fact]
    public async Task Dish_Delete_DeletesItsRecipeLines()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();

        var dish = CreateBurgerWithMilkyCheese(out _);
        await using (var context = testDatabase.CreateContext())
        {
            context.Dishes.Add(dish);
            await context.SaveChangesAsync();
        }

        await using (var deleteContext = testDatabase.CreateContext())
        {
            var saved = await deleteContext.Dishes.SingleAsync(d => d.Id == dish.Id);
            deleteContext.Dishes.Remove(saved);
            await deleteContext.SaveChangesAsync();
        }

        await using var readContext = testDatabase.CreateContext();
        var lineCount = await readContext.Database
            .SqlQuery<int>($"SELECT COUNT(*) AS Value FROM RecipeLines")
            .SingleAsync();

        Assert.Equal(0, await readContext.Dishes.CountAsync());
        Assert.Equal(0, lineCount);
    }

    [Theory]
    [InlineData("Price", "CK_Dishes_Price")]
    [InlineData("Station", "CK_Dishes_Station")]
    [InlineData("Quantity", "CK_RecipeLines_Quantity")]
    public async Task Dish_CheckConstraints_AreEnforced(string invalidColumn, string expectedConstraint)
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();

        var dish = CreateBurgerWithMilkyCheese(out var cheese);
        var emptyDish = new Dish("Plain Plate", null, 3m, Station.Front);
        await using var context = testDatabase.CreateContext();
        context.Dishes.AddRange(dish, emptyDish);
        await context.SaveChangesAsync();

        var price = invalidColumn == "Price" ? 0m : 5m;
        var station = invalidColumn == "Station" ? 99 : 1;

        var exception = await Assert.ThrowsAsync<SqlException>(() =>
            invalidColumn == "Quantity"
                ? context.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO RecipeLines (DishId, IngredientId, Quantity) VALUES ({emptyDish.Id}, {cheese.Id}, {0m})")
                : context.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO Dishes (Id, Name, Description, Price, Station) VALUES ({Guid.CreateVersion7()}, {"Other"}, {""}, {price}, {station})"));

        Assert.Contains(expectedConstraint, exception.Message);
    }
}
