using KitchenPulse.Domain.Entities;
using Xunit.Sdk;

namespace KitchenPulse.Domain.Tests;

public class SiteTests
{
    [Fact]
    public void Constructor_ValidName_SetsNameAndId()
    {
        string siteName = "Waterford";

        var waterfordSite = new Site(siteName);

        Assert.Equal(siteName, waterfordSite.Name);
        Assert.NotEqual(Guid.Empty, waterfordSite.Id);
    }

    [Fact]
    public void Constructor_NameWithSpaces_TrimsName()
    {
        string siteName = " Waterford ";

        var waterfordSite = new Site(siteName);

        Assert.Equal("Waterford", waterfordSite.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
        public void Constructor_NameWithBlank_ThrowError(string? siteName)
    {
        var exception = Assert.Throws<ArgumentException>(() =>  new Site(siteName));
        Assert.Equal("siteName", exception.ParamName);
    }
}
