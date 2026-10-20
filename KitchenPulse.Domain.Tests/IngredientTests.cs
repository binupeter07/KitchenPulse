
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

        ingredient.AddAllergen(Allergen.TreeNuts);
        ingredient.RemoveAllergen(Allergen.TreeNuts);

        Assert.DoesNotContain(Allergen.TreeNuts, ingredient.Allergens);
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

        ingredient.AddAllergen(Allergen.Nuts);
        ingredient.AddAllergen(Allergen.SesameSeeds);

        Assert.Contains(Allergen.Nuts, ingredient.Allergens);
        Assert.Contains(Allergen.SesameSeeds, ingredient.Allergens);
    }

    [Fact]
    public void AddAllergen_UndefinedValue_ThrowsArgumentException()
    {
        var ingredient = new Ingredient("Unknown", StockUnit.Gram, StorageType.Ambient);

        // Attempt to add an invalid allergen (not in the Allergen enum)
        Assert.Throws<ArgumentException>(() => ingredient.AddAllergen((Allergen)999));
    }
}

