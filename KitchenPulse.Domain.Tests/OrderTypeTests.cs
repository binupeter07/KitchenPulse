using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests;

public class OrderTypeTests
{
    [Fact]
    public void OrderType_Values_AreLocked()
    {
        Assert.Equal(1, (int)OrderType.EatIn);
        Assert.Equal(2, (int)OrderType.Takeaway);

        Assert.Equal(2, Enum.GetValues<OrderType>().Length);
    }
}
