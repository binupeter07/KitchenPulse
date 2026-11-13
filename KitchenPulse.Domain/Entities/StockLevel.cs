namespace KitchenPulse.Domain.Entities
{
    public class StockLevel
    {
        public Guid SiteId { get; private set; }
        public Guid IngredientId { get; private set; }
        public decimal Quantity { get; private set; }
        public bool IsStockBelowZero => Quantity < 0;

        public StockLevel(Guid siteId, Guid ingredientId)
        {
            if (siteId == Guid.Empty)
            {
                throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
            }

            if (ingredientId == Guid.Empty)
            {
                throw new ArgumentException("IngredientId cannot be empty", nameof(ingredientId));
            }

            SiteId = siteId;
            IngredientId = ingredientId;
        }

        public void Apply(StockMovement movement)
        {
            ArgumentNullException.ThrowIfNull(movement);

            if (movement.SiteId != SiteId)
            {
                throw new ArgumentException("Movement belongs to a different site.", nameof(movement));
            }

            if (movement.Ingredient.Id != IngredientId)
            {
                throw new ArgumentException("Movement belongs to a different ingredient.", nameof(movement));
            }

            Quantity += movement.Change;
        }
    }
}
