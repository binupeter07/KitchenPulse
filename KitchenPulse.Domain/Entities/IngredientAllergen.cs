using KitchenPulse.Domain.Common;
using KitchenPulse.Domain.Enums;

namespace KitchenPulse.Domain.Entities;

public sealed class IngredientAllergen
{

    public Allergen Allergen { get; private set; }

    internal IngredientAllergen(Allergen allergen)

    {
        Guard.EnumDefined(allergen, nameof(allergen));
        Allergen = allergen;
    }
}
