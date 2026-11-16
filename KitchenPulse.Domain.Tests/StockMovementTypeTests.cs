using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests;

public class StockMovementTypeTests
{
    [Fact]
    public void StockMovementType_Values_AreLocked()
    {
        Assert.Equal(1, (int)StockMovementType.Sale);
        Assert.Equal(2, (int)StockMovementType.Delivery);
        Assert.Equal(3, (int)StockMovementType.Waste);
        Assert.Equal(4, (int)StockMovementType.StaffMeal);
        Assert.Equal(5, (int)StockMovementType.StocktakeGain);
        Assert.Equal(6, (int)StockMovementType.StocktakeLoss);

        Assert.Equal(6, Enum.GetValues<StockMovementType>().Length);
    }
}
