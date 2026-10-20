using KitchenPulse.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

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
    }
}

