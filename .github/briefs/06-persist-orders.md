CONTEXT:
Order (#Order.cs) holds one order at a site: Id, SiteId, OrderNumber, Table, PlacedAtUtc,
IsCollected, and a small list of courses already collected (_collectedCourses).
An order is either eat-in (served at a table) or takeaway (no table).
Eat-in and takeaway have separate order number sequences (shown as E001 and T001 on screen),
and both restart every trading day at each site. So an order needs an OrderType and a
BusinessDate (the trading day decided by the till, not the calendar date).
Its lines are OrderLines (#OrderLine.cs): Dish, Quantity, Course, IsDone, IsSent.
Each line also has three customer lists (_removed, _extras, _customerAllergies) and some
calculated properties (Allergens, UnsafeAllergens, UnsafeIngredients, HasCustomerAllergy).
There are no tables for orders yet, so the data is lost when the application stops.
Orders will be one of the largest and busiest tables, so design for high volume.
The three customer lists on a line will be stored in the next task, not this one.
Delivery orders (Just Eat etc.) are not part of this task.
Follow the style of #DishConfiguration.cs (a parent with a list of child rows).

GOAL:
Store Order and its OrderLines in SQL Server, so that nothing is lost, a loaded order comes back
with its lines and each line's dish (with its recipe, ingredients and allergens), and the database
never allows two orders with the same number, type and trading day at the same site.

RULES:
Domain:
- New enum Domain/Enums/OrderType.cs: EatIn = 1, Takeaway = 2.
- Course (#Course.cs): fixed numbers Starter = 1, Main = 2 (same order).
- Order (#Order.cs): the constructor also takes OrderType orderType and DateOnly businessDate.
  The order number is still passed in (the till decides it, not the Order).
- Reject an undefined order type (Guard.EnumDefined) and a business date that was never set
  (default(DateOnly)), with ArgumentException and nameof.
- Table becomes optional (string?). Eat-in must have a non-blank table; takeaway must not have one.
- OrderLine (#OrderLine.cs): add Id (Guid), created with Guid.CreateVersion7() in the constructor,
  so screens can point at one specific line.
- OrderLine: add a private parameterless constructor, only for EF.
- Update existing tests that create an Order to pass an order type and business date.

KitchenPulseDbContext (#KitchenPulseDbContext.cs):
- Add a DbSet for Order. (No DbSet for OrderLine: lines are saved and loaded through their order.)

Order (new OrderConfiguration.cs):
- Table "Orders". Key: Id (Guid).
- SiteId: foreign key to Sites, DeleteBehavior.Restrict (orders are sales history, never deleted).
- OrderType: smallint, with a check constraint for its range (like CK_StockMovements_Type).
- BusinessDate: date. OrderNumber: int, with a check constraint that it is > 0.
- Table: optional, max length 20.
- Unique index on (SiteId, BusinessDate, OrderType, OrderNumber).
- Index on (SiteId, PlacedAtUtc), for "a site's orders by time".
- Collected courses: store the private field _collectedCourses as a simple list in one column
  (it holds at most two values and is never searched).
- Ignore Lines, CollectedCourse and IsMainsOnHold (views and calculated values).

OrderLines (inside OrderConfiguration.cs, the same way DishConfiguration maps _recipeLines):
- OwnsMany on the private field "_lines", table "OrderLines".
- Key: Id (the new OrderLine.Id). Shadow foreign key "OrderId" back to the order
  (deleting an order deletes its lines).
- Dish: map the Dish navigation with a shadow foreign key "DishId", required,
  DeleteBehavior.Restrict (an ordered dish is sales history), and AutoInclude it,
  so a loaded order brings each line's dish with its recipe, ingredients and allergens.
- Quantity: int, with a check constraint that Quantity > 0.
- Course: stored as smallint (HasConversion<short>()), with a check constraint for its range.
- IsDone, IsSent: plain true/false columns.
- Ignore Removed, Extras and CustomerAllergies (they are stored in the next task),
  and the calculated properties HasCustomerAllergy, Allergens, UnsafeAllergens, UnsafeIngredients.

ACCEPTANCE:
New tests in KitchenPulse.Domain.Tests/OrderTypeTests.cs:
- OrderType_Values_AreLocked (follow #AllergenEnumTests; don't modify it)

New tests in KitchenPulse.Domain.Tests/CourseTests.cs:
- Course_Values_AreLocked (same style)

In KitchenPulse.Domain.Tests/OrderTests.cs:
- Constructor_UndefinedOrderType_Throws ([Theory]: 0, 99; ArgumentException, ParamName "orderType")
- Constructor_UnsetBusinessDate_Throws (ArgumentException, ParamName "businessDate")
- Constructor_EatInWithNoTable_Throws ([Theory]: null, "", "  "; ParamName "table")
- Constructor_TakeawayWithTable_Throws (ParamName "table")
- Constructor_TakeawayWithNoTable_HasNoTable
- AddDish_NewLines_HaveDifferentNonEmptyIds

New tests in KitchenPulse.Infrastructure.Tests/OrderPersistenceTests.cs:
- Order_SaveAndLoad_RoundTripsAllValues
  (site, type, business date, order number, table, placed at, is collected, collected courses)
- Order_LoadedInNewContext_HasLinesWithDishesAndAllergens
  (a line whose dish has a milk ingredient must show Milk in Allergens)
- Order_DuplicateSiteDayTypeAndNumber_IsRejected (DbUpdateException naming the unique index)
- Order_SameNumberWhenOneValueDiffers_IsAccepted ([Theory]: "Type", "Day", "Site";
  each time, two orders share the number and differ only in that one value)
- Site_DeleteWithOrders_IsRejected (DbUpdateException naming the foreign key)
- Dish_DeleteWhileOrdered_IsRejected (DbUpdateException naming the foreign key)
- Order_CheckConstraints_AreEnforced ([Theory]: "OrderType", "OrderNumber", "Quantity", "Course";
  raw SQL with only one invalid value; SqlException naming the check constraint)

Everything else:
- All existing tests still pass after I add the migration (including the updated Order tests).

OUT OF SCOPE:
- Don't create or run migrations, and don't run Update-Database (I will do the migration myself).
- In the Domain, only change Order.cs, OrderLine.cs and Course.cs, and add OrderType.cs,
  as described in RULES. Update existing tests only where they create an Order.
- Don't store Removed, Extras or CustomerAllergies yet (next task).
- No delivery orders (Just Eat etc.).
- Don't change Application or Api.

STEPS:
1. Explain your plan. Wait for my OK.
2. Domain first: write the new Domain tests, run them and show me they fail.
   Then make the Domain changes and update the existing tests that create an Order.
   Run all Domain tests and show me they pass. Wait for my OK.
3. Add the configuration and the DbContext changes. Rebuild and show me the changed files.
   Wait for my OK.
4. I will review the code and add the migration myself. Wait until I tell you it's done.
5. Write the persistence tests. Rebuild first, then run all tests (Domain and Infrastructure)
   and show me the fresh results.