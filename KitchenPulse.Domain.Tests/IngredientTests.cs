
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
        Assert.NotNull(ingredient);
        Assert.Equal(name, ingredient.Name);
        Assert.Equal(unit, ingredient.Unit);
        Assert.Equal(storageType, ingredient.StorageType);
    }
    [Fact]
    public void CreateIngredient_NullOrBlankName_ShouldThrowArgumentException()
    {
        // Arrange
        string name = "   "; // Blank name
        StockUnit unit = StockUnit.Gram;
        StorageType storageType = StorageType.Ambient;
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Ingredient(name, unit, storageType));
        Assert.Equal("Ingredient name cannot be null or blank. (Parameter 'name')", exception.Message);
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
}

