CONTEXT:
Dish (#Dish.cs) holds a menu item: name, description, price and the station that cooks it.
Its recipe is a list of RecipeLines (#RecipeLine.cs), each with an Ingredient and a quantity.
A dish's allergens are not stored: they are calculated from its recipe lines' ingredients.
There are no tables for dishes or recipe lines yet, so the data is lost when the application stops.
#IngredientConfiguration.cs already stores a list in a second table (IngredientAllergens);
follow that style.

GOAL:
Store Dish and its RecipeLines in SQL Server, so that nothing is lost and a loaded dish
comes back with its recipe lines and their ingredients, so its allergens are still correct.

RULES:
KitchenPulseDbContext (#KitchenPulseDbContext.cs):
- Add a DbSet for Dish. (No DbSet for RecipeLine: recipe lines are saved and loaded through their dish.)

Dish (new DishConfiguration.cs):
- Table "Dishes". Key: Id (Guid).
- Name: required, max length 100, unique index (no two dishes with the same name).
- Description: required (the Domain stores an empty string, never null), max length 500.
- Price: decimal(10,2), with a check constraint that Price > 0.
- Station: stored as smallint (HasConversion<short>()), with a check constraint for its range,
  the same way as CK_StockMovements_Type.
- Ignore Allergens (it's calculated from the recipe lines, not stored).

RecipeLines (inside DishConfiguration.cs, the same way IngredientConfiguration maps _allergens):
- OwnsMany on the private field "_recipeLines", table "RecipeLines".
- Shadow foreign key "DishId" back to the dish (deleting a dish deletes its recipe lines).
- Key: (DishId, IngredientId), so an ingredient can appear only once per dish.
- Ingredient: map the Ingredient navigation with a shadow foreign key "IngredientId", required,
  DeleteBehavior.Restrict (an ingredient used in a recipe can't be deleted).
- Quantity: decimal(18,3), with a check constraint that Quantity > 0.
- A loaded dish must come back with its recipe lines, their ingredients and those
  ingredients' allergens.

Domain (#Station.cs, #RecipeLine.cs):
- Give every Station value a fixed number: Grill = 1, Fryer = 2, Front = 3 (same order).
- Add a private parameterless constructor to RecipeLine, only for EF.

ACCEPTANCE:
New tests in KitchenPulse.Domain.Tests/StationTests.cs:
- Station_Values_AreLocked (follow #AllergenEnumTests; don't modify it)

New tests in KitchenPulse.Infrastructure.Tests/DishPersistenceTests.cs:
- Dish_SaveAndLoad_RoundTripsAllValues (name, description, price, station)
- Dish_LoadedInNewContext_HasRecipeLinesIngredientsAndAllergens
  (a dish with a milk ingredient must show Milk in Allergens)
- Dish_DuplicateName_IsRejected (DbUpdateException naming the unique index)
- Dish_Delete_DeletesItsRecipeLines
- Dish_CheckConstraints_AreEnforced ([Theory]: "Price", "Station", "Quantity";
  raw SQL with an invalid value; SqlException naming the check constraint)

New tests in KitchenPulse.Infrastructure.Tests/RecipeLinePersistenceTests.cs:
- RecipeLine_UnknownForeignKey_IsRejected ([Theory]: "DishId", "IngredientId";
  raw SQL; SqlException naming the failing foreign key)
- RecipeLine_DuplicateDishAndIngredient_IsRejected (raw SQL; SqlException naming the primary key)
- Ingredient_DeleteWhileUsedInRecipe_IsRejected (DbUpdateException naming the foreign key)

Everything else:
- All existing tests still pass after I add the migration.

OUT OF SCOPE:
- Don't create or run migrations, and don't run Update-Database (I will do the migration myself).
- In the Domain, only change Station.cs and RecipeLine.cs as described in RULES.
- Don't change Application or Api (endpoints and use cases come later).

STEPS:
1. Explain your plan. Wait for my OK.
2. Domain first: write the new Domain tests, run them and show me they fail.
   Then make the Domain changes, run all Domain tests and show me they pass. Wait for my OK.
3. Add the configuration and the DbContext changes. Rebuild and show me the changed files.
   Wait for my OK.
4. I will review the code and add the migration myself. Wait until I tell you it's done.
5. Write the persistence tests. Rebuild first, then run all tests (Domain and Infrastructure)
   and show me the fresh results.