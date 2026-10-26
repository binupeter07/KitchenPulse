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
    }
}
