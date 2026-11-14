CONTEXT:
We created the Site and StockLevel entities (#Site.cs, #StockLevel.cs), but there are no
database tables for them yet, so their data is lost when the application stops.
Ingredient is already stored using #IngredientConfiguration.cs and #KitchenPulseDbContext.cs.
Many orders at the same site can update the same StockLevel row at the same time.
Follow the style of #IngredientConfiguration.cs for the new configuration classes.

GOAL:
Store Site and StockLevel in SQL Server, so that the data is kept
and no stock update is lost when many orders change the same row.

RULES:
KitchenPulseDbContext (#KitchenPulseDbContext.cs):
- Add DbSets for Site and StockLevel.

Site (new SiteConfiguration.cs):
- Table "Sites". Key: Id.
- Name: required, max length 100.
- Unique index on Name (two sites can't have the same name).

StockLevel (new StockLevelConfiguration.cs):
- Table "StockLevels". Key: (SiteId, IngredientId).
- SiteId is a foreign key to Sites; IngredientId is a foreign key to Ingredients.
  Both use DeleteBehavior.Restrict (never delete stock rows automatically).
- Index on IngredientId (for reports across all sites).
- Quantity: decimal(18,3), required.
- Ignore IsStockBelowZero (it's calculated, not stored).
- Add a RowVersion shadow property configured as a rowversion concurrency token.
  Don't add it to the StockLevel class.

ACCEPTANCE:
New tests in KitchenPulse.Infrastructure.Tests/SitePersistenceTests.cs:
- Site_SaveAndLoad_RoundTripsIdAndName
- Site_DuplicateName_IsRejected
- Site_DeleteWithStock_IsRejected

New tests in KitchenPulse.Infrastructure.Tests/StockLevelPersistenceTests.cs:
- StockLevel_SaveAndLoad_RoundTripsQuantityAfterApply
- StockLevel_DuplicateSiteAndIngredient_IsRejected
- StockLevel_UnknownForeignKey_IsRejected ([Theory]: "SiteId", "IngredientId";
  assert the error message names the failing foreign key)
- StockLevel_TwoUpdatesFromSameVersion_SecondThrowsConcurrencyException

Everything else:
- Use the TestDatabase helper, like IngredientPersistenceTests.
- - All existing tests still pass after I add the migration.

OUT OF SCOPE:
- Don't create or run migrations, and don't run Update-Database (I will do the migration myself).
- Don't change the Domain project (Site, StockLevel and the other entities are finished).
- Don't change Application or Api (endpoints and use cases come later).

STEPS:
1. Explain your plan. Wait for my OK.
2. Add the configurations and the DbContext changes, so the code knows the new tables.
   Build and show me the changed files. Wait for my OK.
3. I will review the code and add the migration myself. Wait until I tell you it's done.
4. Write the tests. Rebuild first, then run all tests (Domain and Infrastructure)
   and show me the fresh results.