using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests
{
    public class IngredientStockTests
    {
        [Fact]
        public void ApplyMovement_Delivery_IncreasesStock()
        {
            var beef = new Ingredient("Beef patty", StockUnit.Each, StorageType.Chilled);
            var delivery = StockMovement.Delivery(beef, 40m, new DateTime(2026, 10, 26, 10, 0, 0, DateTimeKind.Utc), "D071");

            beef.ApplyMovement(delivery);

            Assert.Equal(40m, beef.CurrentStock);
        }

        [Fact]
        public void ApplyMovement_Sale_DecreasesStock()
        {
            var beef = new Ingredient("Beef patty", StockUnit.Each, StorageType.Chilled);
            var delivery = StockMovement.Delivery(beef, 40m, new DateTime(2026, 10, 26, 10, 0, 0, DateTimeKind.Utc), "D071");
            var sale = StockMovement.Sale(beef, 1m, new DateTime(2026, 10, 26, 10, 0, 0, DateTimeKind.Utc), "S071");

            beef.ApplyMovement(delivery);
            beef.ApplyMovement(sale);

            Assert.Equal(39m, beef.CurrentStock);
        }

        [Fact]
        public void ApplyMovement_OtherIngredient_Throws()
        {
            var beef = new Ingredient("Beef patty", StockUnit.Each, StorageType.Chilled);
            var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);

            var sale = StockMovement.Sale(cheese, 1m, new DateTime(2026, 10, 26, 10, 0, 0, DateTimeKind.Utc), "S071");

            Assert.Throws<ArgumentException>(() => beef.ApplyMovement(sale));
            Assert.Equal(0m, beef.CurrentStock);


        }

        [Fact]
        public void ApplyMovement_SaleWithNoStock_GoesBelowZeroAndFlags()
        {
            var beef = new Ingredient("Beef patty", StockUnit.Each, StorageType.Chilled);

            var sale = StockMovement.Sale(beef, 1m, new DateTime(2026, 10, 26, 10, 0, 0, DateTimeKind.Utc), "S071");

            beef.ApplyMovement(sale);

            Assert.Equal(-1, beef.CurrentStock);
            Assert.True(beef.IsStockBelowZero);
        }

        [Fact]
        public void NewIngredient_ExtraPortion_IsZero()
        {
            var bacon = new Ingredient("Bacon", StockUnit.Gram, StorageType.Chilled);

            Assert.Equal(0m, bacon.ExtraPortion);
        }

        [Fact]
        public void SetExtraPortion_Positive_SetsIt()
        {
            var bacon = new Ingredient("Bacon", StockUnit.Gram, StorageType.Chilled);

            bacon.SetExtraPortion(0.30m);

            Assert.Equal(0.30m, bacon.ExtraPortion);
        }

        [Fact]
        public void SetExtraPortion_Zero_Throws()
        {
            var bacon = new Ingredient("Bacon", StockUnit.Gram, StorageType.Chilled);
            var exception = Assert.Throws<ArgumentException>(()=> bacon.SetExtraPortion(0));

            Assert.Equal("amount", exception.ParamName);
            Assert.Equal(0m, bacon.ExtraPortion);
        }

        [Fact]
        public void AddExtra_NoExtraPortionSet_Throws()
        {
            var (line, order, cheese) = CreateClassicWithMilkyCheese();
            var bacon = new Ingredient("Bacon", StockUnit.Gram, StorageType.Chilled);

            Assert.Throws<InvalidOperationException>(() => line.AddExtra(bacon));
            Assert.Empty(line.Extras);
        }

        [Fact]
        public void ApplyMovement_Waste_DecreasesStock()
        {
            var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
            var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
            cheese.ApplyMovement(StockMovement.Delivery(cheese, 1000m, occurredAtUtc, null));
            var waste = StockMovement.Waste(cheese, 60m, occurredAtUtc, "W-0123");

            cheese.ApplyMovement(waste);

            Assert.Equal(940m, cheese.CurrentStock);
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


}

