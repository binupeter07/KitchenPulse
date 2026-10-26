
using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenPulse.Domain.Tests;

public class IngredientTests
{
    [Fact]
    public void CreateIngredient_ValidParameters_ShouldCreateIngredient()
    {
        // Arrange
        string name = "Sugar";
        StockUnit unit = StockUnit.Gram;
        StorageType storageType = StorageType.Ambient;
        // Act
        var ingredient = new Ingredient(name, unit, storageType);
        // Assert
        Assert.NotEqual(Guid.Empty, ingredient.Id);
        Assert.Equal(name, ingredient.Name);
        Assert.Equal(unit, ingredient.Unit);
        Assert.Equal(storageType, ingredient.StorageType);
    }
    [Fact]
    public void Constructor_BlankName_ThrowsArgumentException()
    {
        // Arrange
        string name = "   "; // Blank name
        StockUnit unit = StockUnit.Gram;
        StorageType storageType = StorageType.Ambient;
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Ingredient(name, unit, storageType));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void CreateIngredient_TrimmedName_ShouldTrimWhitespace()
    {
        // Arrange
        string name = "  Salt  "; // Name with leading and trailing whitespace
        StockUnit unit = StockUnit.Gram;
        StorageType storageType = StorageType.Ambient;
        // Act
        var ingredient = new Ingredient(name, unit, storageType);
        // Assert
        Assert.Equal("Salt", ingredient.Name); // Name should be trimmed
    }

    [Fact]
    public void AddAllergen_NewAllergen_AddsToAllergens()
    {
        var ingredient = new Ingredient("Milk", StockUnit.Milliliter, StorageType.Chilled);

        ingredient.AddAllergen(Allergen.Milk);

        Assert.Contains(Allergen.Milk, ingredient.Allergens);
    }

    [Fact]
    public void RemoveAllergen_ExistingAllergen_RemovesIt()
    {
        var ingredient = new Ingredient("Peanut Butter", StockUnit.Gram, StorageType.Ambient);

        ingredient.AddAllergen(Allergen.Peanuts);
        ingredient.RemoveAllergen(Allergen.Peanuts);

        Assert.DoesNotContain(Allergen.Peanuts, ingredient.Allergens);
    }

    [Fact]
    public void AddAllergen_DuplicateAllergen_ShouldNotAddDuplicate()
    {
        var ingredient = new Ingredient("Eggs", StockUnit.Gram, StorageType.Chilled);

        ingredient.AddAllergen(Allergen.Eggs);
        ingredient.AddAllergen(Allergen.Eggs); // Attempt to add duplicate

        Assert.Single(ingredient.Allergens); // Should only contain one instance of the allergen
    }

    [Fact]
    public void RemoveAllergen_NonExistentAllergen_ShouldNotThrow()
    {
        var ingredient = new Ingredient("Fish Sauce", StockUnit.Milliliter, StorageType.Ambient);

        ingredient.RemoveAllergen(Allergen.Fish); // Attempt to remove an allergen that was never added

        Assert.Empty(ingredient.Allergens); // Allergens collection should still be empty
    }

    [Fact]
    public void AddAllergen_MultipleAllergens_ShouldContainAllAdded()
    {
        var ingredient = new Ingredient("Mixed Nuts", StockUnit.Gram, StorageType.Ambient);

        ingredient.AddAllergen(Allergen.TreeNuts);
        ingredient.AddAllergen(Allergen.SesameSeeds);

        Assert.Contains(Allergen.TreeNuts, ingredient.Allergens);
        Assert.Contains(Allergen.SesameSeeds, ingredient.Allergens);
    }

    [Fact]
    public void AddAllergen_UndefinedValue_ThrowsArgumentException()
    {
        var ingredient = new Ingredient("Unknown", StockUnit.Gram, StorageType.Ambient);

        // Attempt to add an invalid allergen (not in the Allergen enum)
        Assert.Throws<ArgumentException>(() => ingredient.AddAllergen((Allergen)999));
    }

    [Fact]
    public void UnsafeIngredients_IngredientRemoved_NotIncluded()
    {
        var (line, order, cheese) = CreateClassicWithMilkyCheese();
        line.AddCustomerAllergy(Allergen.Milk);
        line.RemoveIngredient(cheese);

        Assert.Empty(line.UnsafeIngredients);
    }

    [Fact]
    public void Rename_ValidNewName_ChangesName()
    {
        var ingredient = new Ingredient("Old Name", StockUnit.Gram, StorageType.Ambient);

        ingredient.Rename("New Name");

        Assert.Equal("New Name", ingredient.Name);
    }

    [Fact]
    public void Rename_BlankNewName_ThrowsArgumentException()
    {
        var ingredient = new Ingredient("Old Name", StockUnit.Gram, StorageType.Ambient);

        Assert.Throws<ArgumentException>(() => ingredient.Rename("   "));

        Assert.Equal("Old Name", ingredient.Name);
    }

    private static (OrderLine line, Order order, Ingredient cheese) CreateClassicWithMilkyCheese()
    {
        var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
        cheese.SetExtraPortion(1m);
        var sauce = new Ingredient("Cheese sauce", StockUnit.Milliliter, StorageType.Chilled);
        sauce.SetExtraPortion(20m);
        var beef = new Ingredient("Beef patty", StockUnit.Each, StorageType.Chilled);
        beef.SetExtraPortion(1m);
        var classic = new Dish("The Classic", "Classic beef burger", 11.50m, Station.Grill);
        classic.AddIngredient(beef, 1);
        classic.AddIngredient(cheese, 1);
        cheese.AddAllergen(Allergen.Milk);

        var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
        var order = new Order(1043, "8", placedAtUtc);
        var line = order.AddDish(classic, 1);

        return (line, order, cheese);
    }
}

