using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void Constructor_ValidValues_SetsProperties()
    {
        var orderNumber = 1043;
        var table = "8";
        var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

        var order = new Order(orderNumber, table, placedAtUtc);

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(1043, order.OrderNumber);
        Assert.Equal("8", order.Table);
        Assert.Equal(placedAtUtc, order.PlacedAtUtc);
    }

    [Fact]
    public void Constructor_ZeroOrderNumber_ThrowsArgumentException()
    {
        var orderNumber = 0;
        var table = "8";
        var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

        var exception = Assert.Throws<ArgumentException>(() => new Order(orderNumber, table, placedAtUtc));

        Assert.Equal("orderNumber", exception.ParamName);
    }

    [Fact]
    public void Constructor_BlankTable_ThrowsArgumentException()
    {
        var orderNumber = 142;
        var table = "";
        var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

        var exception = Assert.Throws<ArgumentException>(() => new Order(orderNumber, table, placedAtUtc));

        Assert.Equal("table", exception.ParamName);
    }

    [Fact]
    public void Constructor_TableWithSpaces_TrimsTable()
    {
        var orderNumber = 142;
        var table = " 8    ";
        var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

        var order = new Order(orderNumber, table, placedAtUtc);

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
        var classic = new Dish("The Classic", "Classic beef burger", 11.50m, Station.Grill);

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
        var classic = new Dish("The Classic", "Classic beef burger", 11.50m, Station.Grill);

        order.AddDish(classic, 2);
        order.AddDish(classic, 1);

        Assert.Equal(2, order.Lines.Count);
    }

    [Fact]
    public void AddDish_ZeroQuantity_ThrowsArgumentException()
    {
        var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
        var order = new Order(1043, "8", placedAtUtc);
        var classic = new Dish("The Classic", "Classic beef burger", 11.50m, Station.Grill);

        var exception = Assert.Throws<ArgumentException>(() => order.AddDish(classic, 0));

        Assert.Equal("quantity", exception.ParamName);
    }

    [Fact]
    public void RemoveIngredient_InDish_AddsToRemoved()
    {
        var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
        var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
        var order = new Order(1043, "8", placedAtUtc);
        var classic = new Dish("The Classic", "Classic beef burger", 11.50m, Station.Grill);
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
        var classic = new Dish("The Classic", "Classic beef burger", 11.50m, Station.Grill);
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
        var classic = new Dish("The Classic", "Classic beef burger", 11.50m, Station.Grill);
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
        var (line, order, cheese, _) = CreateClassicWithMilkyCheese();

        var line2 = order.AddDish(line.Dish, 1, [cheese]);

        Assert.Empty(line.Removed);
        Assert.Single(line2.Removed);
    }

    [Fact]
    public void AddDish_DefaultCourse_IsMain()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        Assert.Equal(Course.Main, line.Course);
    }

    [Fact]
    public void AddDish_UndefinedCourse_Throws()
    {
        var (line, order, _, _) = CreateClassicWithMilkyCheese();

        Assert.Throws<ArgumentException>(() => order.AddDish(line.Dish, 1, course: (Course)99));

        Assert.Single(order.Lines);
    }

    [Fact]
    public void RemoveIngredient_SameTwice_OnlyOnce()
    {
        var (line, _, cheese, _) = CreateClassicWithMilkyCheese();

        line.RemoveIngredient(cheese);
        line.RemoveIngredient(cheese);

        Assert.Single(line.Removed);
    }

    [Fact]
    public void AddExtra_AnyIngredient_AddsToExtras()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();
        var bacon = new Ingredient("Bacon", StockUnit.Each, StorageType.Chilled);
        bacon.SetExtraPortion(2m);

        line.AddExtra(bacon);

        Assert.Single(line.Extras);
    }

    [Fact]
    public void AddExtra_AlreadyRemoved_ThrowsInvalidOperationException()
    {
        var (line, _, cheese, _) = CreateClassicWithMilkyCheese();
        line.RemoveIngredient(cheese);

        Assert.Throws<InvalidOperationException>(() => line.AddExtra(cheese));

        Assert.Empty(line.Extras);
    }

    [Fact]
    public void AddExtra_SameTwice_OnlyOnce()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();
        var bacon = new Ingredient("Bacon", StockUnit.Each, StorageType.Chilled);
        bacon.SetExtraPortion(2m);

        line.AddExtra(bacon);
        line.AddExtra(bacon);

        Assert.Single(line.Extras);
    }

    [Fact]
    public void AddExtra_NoExtraPortionSet_Throws()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();
        var bacon = new Ingredient("Bacon", StockUnit.Each, StorageType.Chilled);

        Assert.Throws<InvalidOperationException>(() => line.AddExtra(bacon));

        Assert.Empty(line.Extras);
    }

    [Fact]
    public void RemoveIngredient_AlreadyExtra_ThrowsInvalidOperationException()
    {
        var (line, _, cheese, _) = CreateClassicWithMilkyCheese();
        line.AddExtra(cheese);

        Assert.Throws<InvalidOperationException>(() => line.RemoveIngredient(cheese));

        Assert.Empty(line.Removed);
    }

    [Fact]
    public void Allergens_IngredientRemoved_AllergenGone()
    {
        var (line, _, cheese, sauce) = CreateClassicWithMilkyCheese();

        line.RemoveIngredient(cheese);
        line.RemoveIngredient(sauce);

        Assert.DoesNotContain(Allergen.Milk, line.Allergens);
    }

    [Fact]
    public void Allergens_ExtraAdded_AllergenIncluded()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();
        var egg = new Ingredient("Egg", StockUnit.Each, StorageType.Chilled);
        egg.AddAllergen(Allergen.Eggs);
        egg.SetExtraPortion(1m);

        line.AddExtra(egg);

        Assert.Contains(Allergen.Eggs, line.Allergens);
    }

    [Fact]
    public void Allergens_OtherIngredientSameAllergen_StillIncluded()
    {
        var (line, _, cheese, _) = CreateClassicWithMilkyCheese();

        line.RemoveIngredient(cheese);

        Assert.Contains(Allergen.Milk, line.Allergens);
    }

    [Fact]
    public void AddCustomerAllergy_NewAllergy_AddsToCustomerAllergies()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        line.AddCustomerAllergy(Allergen.Milk);

        Assert.Contains(Allergen.Milk, line.CustomerAllergies);
    }

    [Fact]
    public void AddCustomerAllergy_UndefinedValue_Throws()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        Assert.Throws<ArgumentException>(() => line.AddCustomerAllergy((Allergen)999));

        Assert.Empty(line.CustomerAllergies);
    }

    [Fact]
    public void AddCustomerAllergy_SameTwice_OnlyOnce()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        line.AddCustomerAllergy(Allergen.Milk);
        line.AddCustomerAllergy(Allergen.Milk);

        Assert.Single(line.CustomerAllergies);
    }

    [Fact]
    public void UnsafeAllergens_FoodHasCustomerAllergy_ReturnsIt()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        line.AddCustomerAllergy(Allergen.Milk);

        Assert.Contains(Allergen.Milk, line.UnsafeAllergens);
    }

    [Fact]
    public void UnsafeAllergens_FoodDoesNotHaveIt_IsEmpty()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        line.AddCustomerAllergy(Allergen.Lupin);

        Assert.Empty(line.UnsafeAllergens);
    }

    [Fact]
    public void UnsafeIngredients_IngredientHasCustomerAllergy_ReturnsIt()
    {
        var (line, _, cheese, _) = CreateClassicWithMilkyCheese();

        line.AddCustomerAllergy(Allergen.Milk);

        Assert.Contains(cheese, line.UnsafeIngredients);
    }

    [Fact]
    public void UnsafeIngredients_NoMatch_IsEmpty()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        line.AddCustomerAllergy(Allergen.Lupin);

        Assert.Empty(line.UnsafeIngredients);
    }

    [Fact]
    public void UnsafeIngredients_IngredientRemoved_NotIncluded()
    {
        var (line, _, cheese, sauce) = CreateClassicWithMilkyCheese();
        line.RemoveIngredient(cheese);

        line.AddCustomerAllergy(Allergen.Milk);

        Assert.DoesNotContain(cheese, line.UnsafeIngredients);
        Assert.Contains(sauce, line.UnsafeIngredients);
    }

    [Fact]
    public void MarkDone_NewLine_IsDone()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        line.MarkDone();

        Assert.True(line.IsDone);
    }

    [Fact]
    public void MarkDone_Twice_StillDone()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        line.MarkDone();
        line.MarkDone();

        Assert.True(line.IsDone);
    }

    [Fact]
    public void IsCourseReady_AllKitchenLinesDone_True()
    {
        var (line, order, _, _) = CreateClassicWithMilkyCheese();
        var fries = new Dish("Fries", "Skin-on fries", 3.50m, Station.Fryer);
        var friesLine = order.AddDish(fries, 2);

        line.MarkDone();
        friesLine.MarkDone();

        Assert.True(order.IsCourseReady(Course.Main));
    }

    [Fact]
    public void IsCourseReady_OneLineNotDone_False()
    {
        var (line, order, _, _) = CreateClassicWithMilkyCheese();
        var fries = new Dish("Fries", "Skin-on fries", 3.50m, Station.Fryer);
        order.AddDish(fries, 2);

        line.MarkDone();

        Assert.False(order.IsCourseReady(Course.Main));
    }

    [Fact]
    public void IsCourseReady_FrontLineNotDone_StillTrue()
    {
        var (line, order, _, _) = CreateClassicWithMilkyCheese();
        var fries = new Dish("Fries", "Skin-on fries", 3.50m, Station.Fryer);
        var milkshake = new Dish("Milkshake", "Chocolate milkshake", 5.99m, Station.Front);
        var friesLine = order.AddDish(fries, 2);
        order.AddDish(milkshake, 1);

        line.MarkDone();
        friesLine.MarkDone();

        Assert.True(order.IsCourseReady(Course.Main));
    }

    [Fact]
    public void IsCourseReady_OtherCourseNotDone_OnlyChecksThisCourse()
    {
        var (_, order, _, _) = CreateClassicWithMilkyCheese();
        var fries = new Dish("Fries", "Skin-on fries", 3.50m, Station.Fryer);
        var wings = new Dish("Wings", "Spicy wings", 8.50m, Station.Fryer);
        var milkshake = new Dish("Milkshake", "Chocolate milkshake", 5.99m, Station.Front);
        order.AddDish(fries, 2);
        order.AddDish(milkshake, 1);
        var wingsLine = order.AddDish(wings, 1, course: Course.Starter);

        wingsLine.MarkDone();

        Assert.True(order.IsCourseReady(Course.Starter));
    }

    [Fact]
    public void IsCourseReady_NoLinesInCourse_False()
    {
        var (line, order, _, _) = CreateClassicWithMilkyCheese();

        line.MarkDone();

        Assert.False(order.IsCourseReady(Course.Starter));
    }

    [Fact]
    public void CollectCourse_WhenReady_MarksCollected()
    {
        var (line, order, _, _) = CreateClassicWithMilkyCheese();
        line.MarkDone();

        order.CollectCourse(Course.Main);

        Assert.True(order.IsCourseCollected(Course.Main));
    }

    [Fact]
    public void CollectCourse_WhenNotReady_Throws()
    {
        var (_, order, _, _) = CreateClassicWithMilkyCheese();

        Assert.Throws<InvalidOperationException>(() => order.CollectCourse(Course.Main));

        Assert.False(order.IsCourseCollected(Course.Main));
    }

    [Fact]
    public void CollectCourse_Twice_NothingChanges()
    {
        var (line, order, _, _) = CreateClassicWithMilkyCheese();
        line.MarkDone();

        order.CollectCourse(Course.Main);
        order.CollectCourse(Course.Main);

        Assert.True(order.IsCourseCollected(Course.Main));
    }

    [Fact]
    public void IsMainsOnHold_StartersNotCollected_True()
    {
        var (_, order, _, _) = CreateClassicWithMilkyCheese();
        var wings = new Dish("Wings", "Spicy wings", 8.50m, Station.Fryer);

        order.AddDish(wings, 1, course: Course.Starter);

        Assert.True(order.IsMainsOnHold);
    }

    [Fact]
    public void IsMainsOnHold_StartersReadyButNotCollected_True()
    {
        var (_, order, _, _) = CreateClassicWithMilkyCheese();
        var wings = new Dish("Wings", "Spicy wings", 8.50m, Station.Fryer);
        var wingsLine = order.AddDish(wings, 1, course: Course.Starter);

        wingsLine.MarkDone();

        Assert.True(order.IsMainsOnHold);
    }

    [Fact]
    public void IsMainsOnHold_StartersCollected_False()
    {
        var (_, order, _, _) = CreateClassicWithMilkyCheese();
        var wings = new Dish("Wings", "Spicy wings", 8.50m, Station.Fryer);
        var wingsLine = order.AddDish(wings, 1, course: Course.Starter);
        wingsLine.MarkDone();

        order.CollectCourse(Course.Starter);

        Assert.False(order.IsMainsOnHold);
    }

    [Fact]
    public void IsMainsOnHold_NoStarters_False()
    {
        var (_, order, _, _) = CreateClassicWithMilkyCheese();

        Assert.False(order.IsMainsOnHold);
    }

    [Fact]
    public void RemoveUnsafeIngredients_MilkAllergy_RemovesCheeseAndSauce()
    {
        var (line, _, cheese, sauce) = CreateClassicWithMilkyCheese();
        line.AddCustomerAllergy(Allergen.Milk);

        line.RemoveUnsafeIngredients();

        Assert.Empty(line.UnsafeIngredients);
        Assert.Contains(cheese, line.Removed);
        Assert.Contains(sauce, line.Removed);
    }

    [Fact]
    public void RemoveUnsafeIngredients_UnsafeRecipeAndExtra_RemovesAll()
    {
        var (line, _, cheese, sauce) = CreateClassicWithMilkyCheese();
        var cheesySauce = new Ingredient("Cheese sauce", StockUnit.Gram, StorageType.Chilled);
        cheesySauce.AddAllergen(Allergen.Milk);
        cheesySauce.SetExtraPortion(50m);
        line.AddExtra(cheesySauce);
        line.AddCustomerAllergy(Allergen.Milk);

        line.RemoveUnsafeIngredients();

        Assert.Empty(line.UnsafeIngredients);
        Assert.Contains(cheese, line.Removed);
        Assert.Contains(sauce, line.Removed);
        Assert.DoesNotContain(cheesySauce, line.Extras);
        Assert.DoesNotContain(cheesySauce, line.Removed);
    }

    [Fact]
    public void RemoveUnsafeIngredients_MilkAllergy_KeepsSafeIngredients()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();
        line.AddCustomerAllergy(Allergen.Milk);

        line.RemoveUnsafeIngredients();

        Assert.Empty(line.UnsafeIngredients);
        Assert.Equal(2, line.Removed.Count);
    }

    [Fact]
    public void RemoveUnsafeIngredients_NoAllergy_RemovesNothing()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        line.RemoveUnsafeIngredients();

        Assert.Empty(line.Removed);
    }

    [Fact]
    public void RemoveUnsafeIngredients_CalledTwice_NoErrorAndNoDuplicates()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();
        line.AddCustomerAllergy(Allergen.Milk);

        line.RemoveUnsafeIngredients();
        line.RemoveUnsafeIngredients();

        Assert.Empty(line.UnsafeIngredients);
        Assert.Equal(2, line.Removed.Count);
    }

    [Fact]
    public void RemoveUnsafeIngredients_MilkAndEggAllergy_RemovesBoth()
    {
        var (line, _, cheese, sauce) = CreateClassicWithMilkyCheese();
        var mayo = new Ingredient("Mayonnaise", StockUnit.Gram, StorageType.Chilled);
        mayo.AddAllergen(Allergen.Eggs);
        mayo.SetExtraPortion(30m);
        line.AddExtra(mayo);
        line.AddCustomerAllergy(Allergen.Milk);
        line.AddCustomerAllergy(Allergen.Eggs);

        line.RemoveUnsafeIngredients();

        Assert.Empty(line.UnsafeIngredients);
        Assert.Contains(cheese, line.Removed);
        Assert.Contains(sauce, line.Removed);
        Assert.DoesNotContain(mayo, line.Extras);
    }

    [Fact]
    public void HasCustomerAllergy_NoAllergy_False()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        Assert.False(line.HasCustomerAllergy);
    }

    [Fact]
    public void HasCustomerAllergy_AllergyAdded_True()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();

        line.AddCustomerAllergy(Allergen.Milk);

        Assert.True(line.HasCustomerAllergy);
    }

    [Fact]
    public void HasCustomerAllergy_AfterUnsafeRemoved_StillTrue()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();
        line.AddCustomerAllergy(Allergen.Milk);

        line.RemoveUnsafeIngredients();

        Assert.True(line.HasCustomerAllergy);
    }

    [Fact]
    public void SendToKitchen_NewOrder_SendsAllLines()
    {
        var (_, order, _, _) = CreateClassicWithMilkyCheese();
        var fries = new Dish("Fries", "Skin-on fries", 3.50m, Station.Fryer);
        var friesLine = order.AddDish(fries, 1);

        var docket = order.SendToKitchen();

        Assert.Equal(2, docket.Count);
        Assert.True(order.Lines.All(l => l.IsSent));
    }

    [Fact]
    public void SendToKitchen_SentTwice_SecondReturnsEmpty()
    {
        var (_, order, _, _) = CreateClassicWithMilkyCheese();

        var docket1 = order.SendToKitchen();
        var docket2 = order.SendToKitchen();

        Assert.Empty(docket2);
    }

    [Fact]
    public void SendToKitchen_LineAddedAfterSend_SendsOnlyNewLine()
    {
        var (_, order, _, _) = CreateClassicWithMilkyCheese();

        var docket = order.SendToKitchen();
        Assert.Single(docket);
        var fries = new Dish("Fries", "Skin-on fries", 3.50m, Station.Fryer);
        var friesLine = order.AddDish(fries, 1);

        var secondDocket = order.SendToKitchen();

        var sent = Assert.Single(secondDocket);
        Assert.Equal(friesLine, sent);
    }

    [Fact]
    public void SendToKitchen_LineWithAllergy_RemovesUnsafeIngredients()
    {
        var (line, order, cheese, sauce) = CreateClassicWithMilkyCheese();
        line.AddCustomerAllergy(Allergen.Milk);

        order.SendToKitchen();

        Assert.Empty(line.UnsafeIngredients);
        Assert.Contains(cheese, line.Removed);
        Assert.Contains(sauce, line.Removed);
    }

    [Fact]
    public void CreateSaleMovements_PlainDish_OneMovementPerIngredient()
    {
        var (line, _, _, _) = CreateClassicWithMilkyCheese();
        var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

        var movements = line.CreateSaleMovements(occurredAtUtc, "Order 1043");

        Assert.Equal(3, movements.Count);
        Assert.True(movements.All(m => m.Type == StockMovementType.Sale));
    }

    [Fact]
    public void CreateSaleMovements_QuantityTwo_DoublesAmounts()
    {
        var (line, order, _, sauce) = CreateClassicWithMilkyCheese();
        var twoClassics = order.AddDish(line.Dish, 2);
        var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

        var movements = twoClassics.CreateSaleMovements(occurredAtUtc, "Order 1043");

        var sauceMovement = movements.Single(m => m.Ingredient == sauce);
        Assert.Equal(60m, sauceMovement.Quantity);
    }

    [Fact]
    public void CreateSaleMovements_IngredientRemoved_NoMovementForIt()
    {
        var (line, _, cheese, sauce) = CreateClassicWithMilkyCheese();
        line.RemoveIngredient(cheese);
        var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

        var movements = line.CreateSaleMovements(occurredAtUtc, "Order 1043");

        Assert.DoesNotContain(movements, m => m.Ingredient == cheese);
        Assert.Equal(2, movements.Count);

    }

    [Fact]
    public void CreateSaleMovements_Extra_UsesExtraPortion()
    {
        var (line, order, _, _) = CreateClassicWithMilkyCheese();
        var twoClassics = order.AddDish(line.Dish, 2);
        var bacon = new Ingredient("Bacon", StockUnit.Each, StorageType.Chilled);
        bacon.SetExtraPortion(2m);
        twoClassics.AddExtra(bacon);
        var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

        var movements = twoClassics.CreateSaleMovements(occurredAtUtc, "Order 1043");

        var baconMovement = movements.Single(m => m.Ingredient == bacon);
        Assert.Equal(4m, baconMovement.Quantity);
    }

    [Fact]
    public void RecordSale_PlainClassic_CreatesMovementsAndReducesStock()
    {
        var (_, order, cheese, sauce) = CreateClassicWithMilkyCheese();
        var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);

        var cheeseMovement = StockMovement.Delivery(cheese, 60m, occurredAtUtc, "Delivery-001");
        cheese.ApplyMovement(cheeseMovement);
        var sauceMovement = StockMovement.Delivery(sauce, 10000m, occurredAtUtc, "Delivery-001");
        sauce.ApplyMovement(sauceMovement);

        var docket = order.SendToKitchen();
        var movements = order.RecordSale(docket, occurredAtUtc);

        Assert.Equal(59m, cheese.CurrentStock);
        Assert.Equal(9970m, sauce.CurrentStock);
        Assert.Equal(3, movements.Count);
    }

    [Fact]
    public void RecordSale_ClassicAndFries_CreatesMovementsAndReducesStock()
    {
        var (_, order, cheese, sauce) = CreateClassicWithMilkyCheese();
        var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
        var fries = new Ingredient("Fries", StockUnit.Gram, StorageType.Frozen);
        var friesDish = new Dish("Fries", "Spicy fries", 5.60m, Station.Fryer);
        friesDish.AddIngredient(fries, 250m);
        order.AddDish(friesDish, 1);
        cheese.ApplyMovement(StockMovement.Delivery(cheese, 60m, occurredAtUtc, "Delivery-001"));
        sauce.ApplyMovement(StockMovement.Delivery(sauce, 10000m, occurredAtUtc, "Delivery-001"));
        fries.ApplyMovement(StockMovement.Delivery(fries, 1000m, occurredAtUtc, "Delivery-001"));

        var docket = order.SendToKitchen();
        var movements = order.RecordSale(docket, occurredAtUtc);

        Assert.Equal(4, movements.Count);
        Assert.Equal(59m, cheese.CurrentStock);
        Assert.Equal(9970m, sauce.CurrentStock);
        Assert.Equal(750m, fries.CurrentStock);
    }

    [Fact]
    public void RecordSale_ClassicWithNoCheese_CheeseStockUnchanged()
    {
        var (line, order, cheese, sauce) = CreateClassicWithMilkyCheese();
        var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
        line.RemoveIngredient(cheese);
        cheese.ApplyMovement(StockMovement.Delivery(cheese, 60m, occurredAtUtc, "Delivery-001"));
        sauce.ApplyMovement(StockMovement.Delivery(sauce, 10000m, occurredAtUtc, "Delivery-001"));

        var docket = order.SendToKitchen();
        var movements = order.RecordSale(docket, occurredAtUtc);

        Assert.Equal(2, movements.Count);
        Assert.Equal(60m, cheese.CurrentStock);
        Assert.Equal(9970m, sauce.CurrentStock);
    }

    [Fact]
    public void RecordSale_SecondDocket_OnlyReducesNewItems()
    {
        var (_, order, _, sauce) = CreateClassicWithMilkyCheese();
        var occurredAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
        var fries = new Ingredient("Fries", StockUnit.Gram, StorageType.Frozen);
        var friesDish = new Dish("Fries", "Spicy fries", 5.60m, Station.Fryer);
        friesDish.AddIngredient(fries, 250m);
        sauce.ApplyMovement(StockMovement.Delivery(sauce, 10000m, occurredAtUtc, "Delivery-001"));
        fries.ApplyMovement(StockMovement.Delivery(fries, 1000m, occurredAtUtc, "Delivery-001"));
        order.RecordSale(order.SendToKitchen(), occurredAtUtc);
        order.AddDish(friesDish, 1);

        var secondMovements = order.RecordSale(order.SendToKitchen(), occurredAtUtc);

        Assert.Single(secondMovements);
        Assert.Equal(750m, fries.CurrentStock);
        Assert.Equal(9970m, sauce.CurrentStock);
    }
    private static (OrderLine line, Order order, Ingredient cheese, Ingredient sauce) CreateClassicWithMilkyCheese()
    {
        var beef = new Ingredient("Beef patty", StockUnit.Each, StorageType.Chilled);
        beef.SetExtraPortion(1m);

        var cheese = new Ingredient("Cheese slice", StockUnit.Each, StorageType.Chilled);
        cheese.AddAllergen(Allergen.Milk);
        cheese.SetExtraPortion(1m);

        var sauce = new Ingredient("Burger sauce", StockUnit.Milliliter, StorageType.Chilled);
        sauce.AddAllergen(Allergen.Milk);
        sauce.SetExtraPortion(20m);

        var classic = new Dish("The Classic", "Classic beef burger", 11.50m, Station.Grill);
        classic.AddIngredient(beef, 1);
        classic.AddIngredient(cheese, 1);
        classic.AddIngredient(sauce, 30m);

        var placedAtUtc = new DateTime(2026, 10, 20, 18, 42, 0, DateTimeKind.Utc);
        var order = new Order(1043, "8", placedAtUtc);
        var line = order.AddDish(classic, 1);

        return (line, order, cheese, sauce);
    }
}