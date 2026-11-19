using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KitchenPulse.Infrastructure.Persistence.Configuration;

public class DishConfiguration : IEntityTypeConfiguration<Dish>
{
    public void Configure(EntityTypeBuilder<Dish> builder)
    {
        var min = (int)Enum.GetValues<Station>().Min();
        var max = (int)Enum.GetValues<Station>().Max();

        builder.ToTable("Dishes", t =>
        {
            t.HasCheckConstraint("CK_Dishes_Price", "[Price] > 0");
            t.HasCheckConstraint("CK_Dishes_Station", $"[Station] BETWEEN {min} AND {max}");
        });

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

        builder.HasIndex(d => d.Name).IsUnique();

        builder.Property(d => d.Description)
                .IsRequired()
                .HasMaxLength(500);

        builder.Property(d => d.Price)
                .HasPrecision(10, 2);

        builder.Property(d => d.Station)
                .HasConversion<short>();

        builder.OwnsMany<RecipeLine>("_recipeLines", recipeLineBuilder =>
        {
            recipeLineBuilder.ToTable("RecipeLines", t =>
                            t.HasCheckConstraint("CK_RecipeLines_Quantity", "[Quantity] > 0"));

            recipeLineBuilder.WithOwner().HasForeignKey("DishId");

            recipeLineBuilder.HasOne(r => r.Ingredient)
                            .WithMany()
                            .HasForeignKey("IngredientId")
                            .IsRequired()
                            .OnDelete(DeleteBehavior.Restrict);

            recipeLineBuilder.Navigation(r => r.Ingredient).AutoInclude();

            recipeLineBuilder.HasKey("DishId", "IngredientId");

            recipeLineBuilder.Property(r => r.Quantity)
                            .HasPrecision(18, 3);
        });

        builder.Ignore(d => d.Allergens);
        builder.Ignore(d => d.RecipeLines);
    }
}
