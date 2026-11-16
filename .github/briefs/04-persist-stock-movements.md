CONTEXT:
StockMovement (#StockMovement.cs) records every stock change (sales, deliveries, waste...)
and is our stock audit trail. There is no table for it yet, so the data is lost when the
application stops.
It will be the largest table in the system, and rows are only ever added, never updated or deleted.
Register it in #KitchenPulseDbContext.cs and follow the style of #StockLevelConfiguration.cs.

GOAL:
Store StockMovement in SQL Server, so that no movement is lost
and a site's movements can be found quickly by date.

RULES:
KitchenPulseDbContext (#KitchenPulseDbContext.cs):
- Add a DbSet for StockMovement.
- Movements are append-only: if SaveChanges finds a StockMovement that is modified or deleted,
  throw InvalidOperationException before anything is saved.

StockMovement (new StockMovementConfiguration.cs):
- Table "StockMovements". Key: Id (Guid).
- SiteId is a foreign key to Sites.
- Map the Ingredient navigation with HasOne(m => m.Ingredient), using a shadow foreign key "IngredientId".
- Both foreign keys use DeleteBehavior.Restrict (movements are audit history, never deleted).
- Type: stored as smallint (HasConversion<short>()), like Allergen in IngredientConfiguration.
- Quantity: decimal(18,3). OccurredAtUtc: required.
- Reference: max length 100.
- Ignore Change (it's calculated, not stored).
- Index on (SiteId, OccurredAtUtc), for "a site's movements by date".

Domain (#StockMovement.cs, #StockMovementType.cs):
- Give every StockMovementType value a fixed number (Sale = 1, Delivery = 2, ...),
  keeping the current order.
- Add a private parameterless constructor to StockMovement, only for EF.
- Reference longer than 100 characters throws ArgumentException with nameof(reference).

ACCEPTANCE:
New tests in KitchenPulse.Domain.Tests/StockMovementTypeTests.cs:
- StockMovementType_Values_AreLocked (follow #AllergenEnumTests; don't modify it)

In KitchenPulse.Domain.Tests/StockMovementTests.cs:
- Create_Reference100Characters_IsAccepted
- Create_ReferenceOver100Characters_Throws ([Theory]: 101, 500; assert ParamName "reference")

New tests in KitchenPulse.Infrastructure.Tests/StockMovementPersistenceTests.cs:
- StockMovement_SaveAndLoad_RoundTripsAllValues (including Type and Ingredient)
- StockMovement_UnknownForeignKey_IsRejected ([Theory]: "SiteId", "IngredientId";
  DbUpdateException naming the failing foreign key)
- Ingredient_DeleteWithMovements_IsRejected
- StockMovement_Delete_ThrowsInvalidOperationException
- StockMovement_Update_ThrowsInvalidOperationException

Everything else:
- All existing tests still pass after I add the migration.

OUT OF SCOPE:
- Don't create or run migrations, and don't run Update-Database (I will do the migration myself).
- In the Domain, only change StockMovement.cs and StockMovementType.cs as described in RULES.
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