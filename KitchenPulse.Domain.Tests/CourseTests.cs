using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests;

public class CourseTests
{
    [Fact]
    public void Course_Values_AreLocked()
    {
        Assert.Equal(1, (int)Course.Starter);
        Assert.Equal(2, (int)Course.Main);

        Assert.Equal(2, Enum.GetValues<Course>().Length);
    }
}
