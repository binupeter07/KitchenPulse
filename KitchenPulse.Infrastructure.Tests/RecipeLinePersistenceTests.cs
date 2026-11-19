using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KitchenPulse.Infrastructure.Tests;

public class RecipeLinePersistenceTests
{
    private static async Task<(Dish Dish, Ingredient Ingredient)> SeedDishWithRecipeLineAsync(TestDatabase testDatabase)
    {
        await using var context = testDatabase.CreateContext();

        var flour = new Ingredient("Flour", StockUnit.Each, StorageType.Ambient);
        var bread = new Dish("Bread", "Fresh bread", 3m, Station.Front);
        bread.AddIngredient(flour, 1.5m);
        context.Dishes.Add(bread);
        await context.SaveChangesAsync();

        return (bread, flour);
    }

    [Theory]
    [InlineData("DishId", "FK_RecipeLines_Dishes_DishId")]
    [InlineData("IngredientId", "FK_RecipeLines_Ingredients_IngredientId")]
    public async Task RecipeLine_UnknownForeignKey_IsRejected(string unknownColumn, string expectedForeignKey)
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (bread, flour) = await SeedDishWithRecipeLineAsync(testDatabase);

        await using var context = testDatabase.CreateContext();
        var dishId = unknownColumn == "DishId" ? Guid.CreateVersion7() : bread.Id;
        var ingredientId = unknownColumn == "IngredientId" ? Guid.CreateVersion7() : flour.Id;

        var exception = await Assert.ThrowsAsync<SqlException>(() =>
            context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO RecipeLines (DishId, IngredientId, Quantity) VALUES ({dishId}, {ingredientId}, {1m})"));

        Assert.Contains(expectedForeignKey, exception.Message);
    }

    [Fact]
    public async Task RecipeLine_DuplicateDishAndIngredient_IsRejected()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (bread, flour) = await SeedDishWithRecipeLineAsync(testDatabase);

        await using var context = testDatabase.CreateContext();

        var exception = await Assert.ThrowsAsync<SqlException>(() =>
            context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO RecipeLines (DishId, IngredientId, Quantity) VALUES ({bread.Id}, {flour.Id}, {2m})"));

        Assert.Contains("PK_RecipeLines", exception.Message);
    }

    [Fact]
    public async Task Ingredient_DeleteWhileUsedInRecipe_IsRejected()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        var (_, flour) = await SeedDishWithRecipeLineAsync(testDatabase);

        await using var context = testDatabase.CreateContext();
        var ingredient = await context.Ingredients.SingleAsync(i => i.Id == flour.Id);
        context.Ingredients.Remove(ingredient);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains("FK_RecipeLines_Ingredients_IngredientId", exception.InnerException!.Message);
    }
}
