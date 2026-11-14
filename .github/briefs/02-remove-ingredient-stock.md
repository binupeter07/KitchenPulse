CONTEXT:
We added StockLevel (#StockLevel.cs), which holds the current quantity of one ingredient at one site.
The old whole-chain stock code is still in #Ingredient.cs (CurrentStock, IsStockBelowZero, ApplyMovement)
and #Order.cs (RecordSale calls ingredient.ApplyMovement).
Now stock is in two places, so we need to remove the old code to avoid inconsistencies.
Keep the existing code style.

GOAL:
Remove the old stock code from Ingredient and Order, so that stock lives only in StockLevel.

RULES:
Ingredient (#Ingredient.cs):
- Remove CurrentStock, IsStockBelowZero and the ApplyMovement method.

Order (#Order.cs):
- Rename RecordSale to CreateSaleMovements.
- It still takes docket (a collection of OrderLine) and occurredAtUtc.
- It creates the sale movements for each line, using the order's own SiteId.
- It no longer calls ingredient.ApplyMovement.
- It returns the created sale movements.

Infrastructure (#IngredientConfiguration.cs):
- Remove only the CurrentStock mapping.

ACCEPTANCE:
IngredientStockTests:
- First check each test's behaviour is covered in StockLevelTests; add any missing ones there.
- Then delete IngredientStockTests.

OrderTests:
- Rename tests starting with RecordSale_ to CreateSaleMovements_.
- Tests that checked ingredient stock now check the returned movements instead
  (count, quantity, SiteId, Sale type).
- Keep the test that movements use the order's own SiteId.

Everything else:
- The solution builds and all Domain tests pass.
- Infrastructure tests pass after I add the migration.

OUT OF SCOPE:
- Don't create or run migrations, and don't run Update-Database (I will do the migration myself).
- Don't change StockLevel.cs (finished and tested). Adding tests to StockLevelTests is allowed.

STEPS:
1. Explain your plan. Wait for my OK.
2. Compare IngredientStockTests with StockLevelTests, add any missing tests to StockLevelTests
   and run them. Show me which tests you added and why. Wait for my OK.
3. Remove the old code and update the tests. Build, run all Domain tests, and show me the result.