using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests
{
    public class OrderTestsk
    {

        [Fact]
        public void Constructor_ValidValues_SetsProperties()
        {
            var orderNumber = 1043;
            var Table = "8";
            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

            var order = new Order(orderNumber, Table, placedAtUtc);

            Assert.NotEqual(Guid.Empty, order.Id);
            Assert.Equal(1043, order.OrderNumber);
            Assert.Equal("8", order.Table);
            Assert.Equal(placedAtUtc, order.PlacedAtUtc);
        }

        [Fact]
        public void Constructor_ZeroOrderNumber_ThrowsArgumentException()
        {
            var orderNumber = 0;
            var Table = "8";
            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);



            var exception = Assert.Throws<ArgumentException>(() => new Order(orderNumber, Table, placedAtUtc));


            Assert.Equal("orderNumber", exception.ParamName);
        }

        [Fact]
        public void Constructor_BlankTable_ThrowsArgumentException()
        {
            var orderNumber = 142;
            var Table = "";
            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);



            var exception = Assert.Throws<ArgumentException>(() => new Order(orderNumber, Table, placedAtUtc));


            Assert.Equal("table", exception.ParamName);
        }

        [Fact]
        public void Constructor_TableWithSpaces_TrimsTable()
        {
            var orderNumber = 142;
            var Table = " 8    ";
            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

            var order = new Order(orderNumber, Table, placedAtUtc);

            Assert.NotEqual(Guid.Empty, order.Id);
            Assert.Equal(142, order.OrderNumber);
            Assert.Equal("8", order.Table);
            Assert.Equal(placedAtUtc, order.PlacedAtUtc);


        }

        [Fact]
        public void AddDish_NewDish_AddsLine()
        {
            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
            var order = new Order(1043, "8", placedAtUtc);
            var classic = new Dish("The Classic", "Classic beef burger", 11.50m);

            order.AddDish(classic, 2);

            var line = Assert.Single(order.Lines);
            Assert.Equal(classic, line.Dish);
            Assert.Equal(2, line.Quantity);
        }

        [Fact]
        public void AddDish_SameDishTwice_AddsTwoLines()
        {
            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
            var order = new Order(1043, "8", placedAtUtc);
            var classic = new Dish("The Classic", "Classic beef burger", 11.50m);

            order.AddDish(classic, 2);
            order.AddDish(classic, 1);

            Assert.Equal(2, order.Lines.Count);

        }

        [Fact]
        public void AddDish_ZeroQuantity_ThrowsArgumentException()
        {
            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
            var order = new Order(1043, "8", placedAtUtc);
            var classic = new Dish("The Classic", "Classic beef burger", 11.50m);


            var exception = Assert.Throws<ArgumentException>(() => order.AddDish(classic, 0));

            Assert.Equal("quantity", exception.ParamName);
        }

        [Fact]
        public void RemoveIngredient_InDish_AddsToRemoved()
        {
            var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);

            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
            var order = new Order(1043, "8", placedAtUtc);
            var classic = new Dish("The Classic", "Classic beef burger", 11.50m);
            classic.AddIngredient(cheese, 1);

            var line = order.AddDish(classic, 1);

            line.RemoveIngredient(cheese);

            Assert.Single(line.Removed);
        }

        [Fact]
        public void AddDish_WithRemovedIngredient_LineHasRemoved()
        {
            // Arrange
            var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
            var classic = new Dish("The Classic", "Classic beef burger", 11.50m);
            classic.AddIngredient(cheese, 1);

            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
            var order = new Order(1043, "8", placedAtUtc);

            // Act
            var line = order.AddDish(classic, 1, [cheese]);

            // Assert
            Assert.Single(line.Removed);
        }

        [Fact]
        public void AddDish_RemoveIngredientNotInDish_ThrowsAndLineNotAdded()
        {
            var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
            var beef = new Ingredient("Beef patty", StockUnit.Each, StorageType.Chilled);
            var classic = new Dish("The Classic", "Classic beef burger", 11.50m);
            classic.AddIngredient(beef, 1);

            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
            var order = new Order(1043, "8", placedAtUtc);
            var exception = Assert.Throws<ArgumentException>(() => order.AddDish(classic, 1, [cheese]));

            Assert.Equal("ingredient", exception.ParamName);

            Assert.Empty(order.Lines);
        }

        [Fact]
        public void AddDish_TwoClassicsOneWithoutCheese_OnlyOneLineChanged()
        {

            var (line, order, cheese) = CreateOrderWithClassic();
            var line2 = order.AddDish(line.Dish, 1, [cheese]);

            Assert.Empty(line.Removed);
            Assert.Single(line2.Removed);

        }

        [Fact]
        public void RemoveIngredient_SameTwice_OnlyOnce()
        {
            var (line, order, cheese) = CreateOrderWithClassic();

            line.RemoveIngredient(cheese);
            line.RemoveIngredient(cheese);

            Assert.Single(line.Removed);
        }

        [Fact]
        public void AddExtra_AnyIngredient_AddsToExtras()
        {
            var (line, order, cheese) = CreateOrderWithClassic();

            var bacon = new Ingredient("Bacon", StockUnit.Each, StorageType.Chilled);

            line.AddExtra(bacon);

            Assert.Single(line.Extras);
        }

        [Fact]
        public void AddExtra_AlreadyRemoved_ThrowsInvalidOperationException()
        {
            var (line, order, cheese) = CreateOrderWithClassic();

            line.RemoveIngredient(cheese);

            var exception = Assert.Throws<InvalidOperationException>(() => line.AddExtra(cheese));

            Assert.Empty(line.Extras);
        }

        [Fact]
        public void RemoveIngredient_AlreadyExtra_ThrowsInvalidOperationException()
        {

            var (line, order, cheese) = CreateOrderWithClassic();

            line.AddExtra(cheese);

            Assert.Throws<InvalidOperationException>(() => line.RemoveIngredient(cheese));

            Assert.Empty(line.Removed);
        }


        private static (OrderLine line, Order order, Ingredient cheese) CreateOrderWithClassic()
        {
            var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
            var beef = new Ingredient("Beef patty", StockUnit.Each, StorageType.Chilled);
            var classic = new Dish("The Classic", "Classic beef burger", 11.50m);
            classic.AddIngredient(beef, 1);
            classic.AddIngredient(cheese, 1);

            var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
            var order = new Order(1043, "8", placedAtUtc);
            var line = order.AddDish(classic, 1);

            return (line, order, cheese);
        }

    }
}

