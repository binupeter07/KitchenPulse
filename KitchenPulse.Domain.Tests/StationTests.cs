using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests;

public class StationTests
{
    [Fact]
    public void Station_Values_AreLocked()
    {
        Assert.Equal(1, (int)Station.Grill);
        Assert.Equal(2, (int)Station.Fryer);
        Assert.Equal(3, (int)Station.Front);

        Assert.Equal(3, Enum.GetValues<Station>().Length);
    }
}
