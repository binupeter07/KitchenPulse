using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests;
// ← CHANGED: one blank line, not two
public class DishTests
{
    [Fact]                                        // ← CHANGED: no blank line below [Fact]
    public void Constructor_ValidValues_SetsProperties()
    {
        // Arrange
        string name = "Classic";
        string description = "Toast the bun. Cook patty 3 min each side. Cheese on in the last minute. Build: bun base, sauce, lettuce, patty, cheese, top.";
        decimal price = 12.99m;

        // Act
        var dish = new Dish(name, description, price,);

        // Assert
        Assert.NotEqual(Guid.Empty, dish.Id);
        Assert.Equal(name, dish.Name);
        Assert.Equal(description, dish.Description);
        Assert.Equal(price, dish.Price);
    }

    [Fact]
    public void Constructor_BlankName_ThrowsArgumentException()
    {
        // Arrange
        string name = "   ";
        string description = "Some description";
        decimal price = 10.00m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Dish(name, description, price,Station.Grill));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_ZeroPrice_ThrowsArgumentException()
    {
        // Arrange
        string name = "Dish Name";
        string description = "Some description";
        decimal price = 0.00m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Dish(name, description, price,Station.Grill));
        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullDescription_SetsEmpty()
    {
        // Arrange
        string name = "Dish Name";
        string? description = null;
        decimal price = 10.00m;

        // Act
        var dish = new Dish(name, description, price, Station.Grill));

        // Assert
        Assert.Equal(string.Empty, dish.Description);
    }

    [Fact]
    public void Constructor_NegativePrice_ThrowsArgumentException()
    {
        // Arrange
        string name = "Dish Name";
        string description = "Some description";
        decimal price = -5.00m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Dish(name, description, price, Station.Grill));
        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void AddIngredient_NewIngredient_AddsRecipeLine()
    {
        // Arrange
        var dish = new Dish("Dish Name", "Some description", 10.00m, Station.Grill);
        var ingredient = new Ingredient("Ingredient Name", StockUnit.Gram, StorageType.Chilled);

        // Act
        dish.AddIngredient(ingredient, 2.5m);

        // Assert
        var recipeLine = Assert.Single(dish.RecipeLines);    // ← CHANGED: 2 lines became 1
        Assert.Equal(ingredient, recipeLine.Ingredient);
        Assert.Equal(2.5m, recipeLine.Quantity);
    }

    [Fact]
    public void AddIngredient_SameIngredientTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        var dish = new Dish("Dish Name", "Some description", 10.00m, Station.Grill);
        var ingredient = new Ingredient("Ingredient Name", StockUnit.Gram, StorageType.Chilled);
        dish.AddIngredient(ingredient, 2.5m);                // ← CHANGED: this is preparing, so it's Arrange

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => dish.AddIngredient(ingredient, 1.0m));   // ← CHANGED: no "var exception ="
        Assert.Single(dish.RecipeLines);
    }

    [Fact]
    public void AddIngredient_NullIngredient_ThrowsArgumentNullException()
    {
        // Arrange
        var dish = new Dish("Dish Name", "Some description", 10.00m, Station.Grill);
        Ingredient? ingredient = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => dish.AddIngredient(ingredient!, 2.5m));
        Assert.Equal("ingredient", exception.ParamName);
    }

    [Fact]
    public void AddIngredient_ZeroQuantity_ThrowsArgumentException()
    {
        // Arrange
        var dish = new Dish("Dish Name", "Some description", 10.00m, Station.Grill);
        var ingredient = new Ingredient("Ingredient Name", StockUnit.Gram, StorageType.Chilled);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => dish.AddIngredient(ingredient, 0.0m));
        Assert.Equal("quantity", exception.ParamName);
    }

    [Fact]
    public void Allergens_NoIngredients_IsEmpty()
    {
        var dish = new Dish("Dish Name", "Some description", 10.00m, Station.Grill);

        // Assert
        Assert.Empty(dish.Allergens);
    }

    [Fact]
    public void Allergens_SharedAllergen_ShowsOnce()
    {
        var dish = new Dish("Dish Name", "Some description", 10.00m, Station.Grill);
        var ingredient1 = new Ingredient("Ingredient 1", StockUnit.Gram, StorageType.Chilled);
        ingredient1.AddAllergen(Allergen.Milk);
        var ingredient2 = new Ingredient("Ingredient 2", StockUnit.Gram, StorageType.Chilled);
        ingredient2.AddAllergen(Allergen.Milk);
        dish.AddIngredient(ingredient1, 2.5m);
        dish.AddIngredient(ingredient2, 1.0m);
        // Act
        var allergens = dish.Allergens;
        // Assert
        Assert.Single(allergens);
        Assert.Contains(Allergen.Milk, allergens);
    }

    [Fact]
    public void Allergens_IngredientUpdatedLater_DishSeesIt()
    {
        var dish = new Dish("Dish Name", "Some description", 10.00m, Station.Grill);
        var ingredient = new Ingredient("Cheese", StockUnit.Gram, StorageType.Chilled);
        dish.AddIngredient(ingredient, 2.5m);
        // Act
        ingredient.AddAllergen(Allergen.Milk);
        // Assert
        Assert.Contains(Allergen.Milk, dish.Allergens);
    }
}