using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Tests;

public class AllergenEnumTests
{
    [Fact]
    public void Allergens_Ids_ShouldRemainStable()
    {
        Assert.Equal(1, (int)Allergen.Gluten);
        Assert.Equal(2, (int)Allergen.Crustaceans);
        Assert.Equal(3, (int)Allergen.Eggs);
        Assert.Equal(4, (int)Allergen.Fish);
        Assert.Equal(5, (int)Allergen.Peanuts);
        Assert.Equal(6, (int)Allergen.Soybeans);
        Assert.Equal(7, (int)Allergen.Milk);
        Assert.Equal(8, (int)Allergen.TreeNuts);
        Assert.Equal(9, (int)Allergen.Celery);
        Assert.Equal(10, (int)Allergen.Mustard);
        Assert.Equal(11, (int)Allergen.SesameSeeds);
        Assert.Equal(12, (int)Allergen.SulphurDioxideAndSulphites);
        Assert.Equal(13, (int)Allergen.Lupin);
        Assert.Equal(14, (int)Allergen.Molluscs);

        Assert.Equal(14, Enum.GetValues<Allergen>().Length);
    }
}


