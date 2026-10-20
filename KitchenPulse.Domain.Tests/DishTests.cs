using KitchenPulse.Domain.Entities;

namespace KitchenPulse.Domain.Tests;


public class DishTests
{
    [Fact]

    public void Constructor_ValidValues_SetsProperties()
    {
        // Arrange
        string name = "Classic";
        string description = "Toast the bun. Cook patty 3 min each side. Cheese on in the last\r\nminute. Build: bun base, sauce, lettuce, patty, cheese, top.";
        decimal price = 12.99m;
        // Act
        var dish = new Dish(name, description, price);
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
        string name = "   "; // Blank name
        string description = "Some description";
        decimal price = 10.00m;
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Dish(name, description, price));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_ZeroPrice_ThrowsArgumentException()
    {
        // Arrange
        string name = "Dish Name";
        string description = "Some description";
        decimal price = 0.00m; // Zero price
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Dish(name, description, price));

        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullDescription_SetsEmpty()
    {
        // Arrange
        string name = "Dish Name";
        string? description = null; // Null description
        decimal price = 10.00m;
        // Act
        var dish = new Dish(name, description, price);
        // Assert
        Assert.Equal(string.Empty, dish.Description);
    }

    [Fact]
    public void Constructor_NegativePrice_ThrowsArgumentException()
    {
        // Arrange
        string name = "Dish Name";
        string description = "Some description";
        decimal price = -5.00m; // Negative price
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Dish(name, description, price));

        Assert.Equal("price", exception.ParamName);
    }
}
