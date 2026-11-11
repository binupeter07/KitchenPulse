using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KitchenPulse.Infrastructure.Tests;

public class IngredientPersistenceTests
{
    [Fact]
    public async Task Ingredient_WithAllergens_WhenSavedAndReloaded_ShouldPreserveAllergens()
    {
        await using var testDatabase = new TestDatabase();

        await testDatabase.InitializeAsync();

        await using var context = testDatabase.CreateContext();

        var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
        cheese.AddAllergen(Allergen.Milk);
        cheese.AddAllergen(Allergen.Eggs);

        context.Ingredients.Add(cheese);
        await context.SaveChangesAsync();

        await using (var readContext = testDatabase.CreateContext())
        {
            var savedIngredient = await readContext.Ingredients
                .SingleAsync(i => i.Id == cheese.Id);

            Assert.Equal(cheese.Id, savedIngredient.Id);
            Assert.Equal("Cheese slice", savedIngredient.Name);
            Assert.Equal(2, savedIngredient.Allergens.Count);
            Assert.Contains(Allergen.Milk, savedIngredient.Allergens);
            Assert.Contains(Allergen.Eggs, savedIngredient.Allergens);
        }
    }



    [Fact]
    public async Task Ingredient_WhenAllergenRemoved_ShouldPersistRemoval()
    {
        await using var testDatabase = new TestDatabase();

        await testDatabase.InitializeAsync();

        await using var context = testDatabase.CreateContext();

        var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
        cheese.AddAllergen(Allergen.Milk);
        cheese.AddAllergen(Allergen.Eggs);

        context.Ingredients.Add(cheese);

        await context.SaveChangesAsync();

        await using (var updateContext = testDatabase.CreateContext())
        {
            var updateCheese = await updateContext.Ingredients.SingleAsync(i => i.Id == cheese.Id);

            updateCheese.RemoveAllergen(Allergen.Milk);

            await updateContext.SaveChangesAsync();
        }

        await using (var readContext = testDatabase.CreateContext())
        {
            var savedIngredient = await readContext.Ingredients
                .SingleAsync(i => i.Id == cheese.Id);

            Assert.Equal(cheese.Id, savedIngredient.Id);
            Assert.Equal("Cheese slice", savedIngredient.Name);
            Assert.DoesNotContain(Allergen.Milk, savedIngredient.Allergens);
            Assert.Contains(Allergen.Eggs, savedIngredient.Allergens);
        }

    }

    [Fact]
    public async Task IngredientAllergen_InvalidAllergenId_IsRejectedByDatabase()
    {
        await using var testDatabase = new TestDatabase();
        await testDatabase.InitializeAsync();
        await using var context = testDatabase.CreateContext();

        var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
        context.Ingredients.Add(cheese);
        await context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<SqlException>(() =>
            context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO IngredientAllergens (IngredientId, AllergenId) VALUES ({cheese.Id}, {99})")
        );

        Assert.Contains("CK_IngredientAllergens_AllergenId", exception.Message);
    }

}
