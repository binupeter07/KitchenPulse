using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KitchenPulse.Infrastructure.Persistence.Configuration;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(100);

        builder.Property(i => i.Unit)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

        builder.Property(i => i.StorageType)
                .HasConversion<string>()
                .HasMaxLength(20);

        builder.Property(i => i.CurrentStock)
                .HasPrecision(18, 3);

        builder.Property(i => i.ExtraPortion)
                .HasPrecision(18, 3);

        var min = (int)Enum.GetValues<Allergen>().Min();
        var max = (int)Enum.GetValues<Allergen>().Max();

        builder.OwnsMany<IngredientAllergen>("_allergens", allergenBuilder =>
        {
            allergenBuilder.ToTable("IngredientAllergens", t => 
                            t.HasCheckConstraint(
                                    "CK_IngredientAllergens_AllergenId",
                                        $"[AllergenId] BETWEEN {min} AND {max}"));

            allergenBuilder.WithOwner().HasForeignKey("IngredientId");
            allergenBuilder.HasKey("IngredientId", "Allergen");
            allergenBuilder.Property(i => i.Allergen).HasColumnName("AllergenId").HasConversion<short>();
            allergenBuilder.HasIndex("Allergen");
            
        });
        builder.Ignore(i=> i.Allergens);

    }
}
