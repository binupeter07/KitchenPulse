CONTEXT:
I am adding stock per site. For example, if cheese is delivered in Waterford,
only Waterford's cheese stock should change.
StockLevel.Apply will receive a StockMovement (#StockMovement.cs), which has a SiteId
and an Ingredient.
Ingredient.ApplyMovement in #Ingredient.cs does this today for the whole chain;
StockLevel will do the same job per site.
Follow the style of #Site.cs.

GOAL:
Add a StockLevel entity in Domain/Entities that represents the current quantity
of one ingredient at one site.

RULES:
What it holds:
- SiteId (Guid) and IngredientId (Guid): together they identify the row.
  No separate Id: a site has only one stock level per ingredient.
- Quantity (decimal): starts at 0. Changes only through Apply.
- IsStockBelowZero (bool): calculated, true when Quantity is less than 0.

When it's created:
- Constructor takes siteId and ingredientId only.
- Rejects empty Guids: ArgumentException with nameof(parameter).

Apply(movement):
- Throws ArgumentException (nameof(movement)) if movement.SiteId doesn't match SiteId.
- Throws ArgumentException (nameof(movement)) if movement.Ingredient.Id doesn't match IngredientId.
- Otherwise adds movement.Change to Quantity.

ACCEPTANCE:
New tests in KitchenPulse.Domain.Tests/StockLevelTests.cs:
- Constructor_ValidIds_SetsIdsAndStartsAtZero
- Constructor_EmptySiteId_Throws (assert ParamName)
- Constructor_EmptyIngredientId_Throws (assert ParamName)
- Apply_Delivery_IncreasesQuantity
- Apply_Sale_DecreasesQuantity
- Apply_SaleMoreThanStock_SetsIsStockBelowZero
- Apply_SaleToExactlyZero_IsNotBelowZero
- Apply_DifferentSite_ThrowsAndQuantityUnchanged
- Apply_DifferentIngredient_ThrowsAndQuantityUnchanged
All existing tests still pass.

OUT OF SCOPE:
- Don't change Ingredient.cs or Order.cs (removing the old stock code is the next task).
- No database code: don't touch Infrastructure, DbContext or migrations.

STEPS:
1. Explain your plan. Wait for my OK.
2. Write StockLevelTests only (no StockLevel class yet), run them, and show me they fail. Wait for my OK.
3. Write StockLevel to make all tests pass. Run all tests and show me the result.