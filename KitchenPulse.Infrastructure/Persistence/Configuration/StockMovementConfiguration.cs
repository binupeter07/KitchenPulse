using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KitchenPulse.Infrastructure.Persistence.Configuration;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        var min = (int)Enum.GetValues<StockMovementType>().Min();
        var max = (int)Enum.GetValues<StockMovementType>().Max();

        builder.ToTable("StockMovements", t =>
                t.HasCheckConstraint(
                        "CK_StockMovements_Type",
                        $"[Type] BETWEEN {min} AND {max}"));

        builder.HasKey(m => m.Id);

        builder.HasOne<Site>()
                .WithMany()
                .HasForeignKey(m => m.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Ingredient)
                .WithMany()
                .HasForeignKey("IngredientId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.Type)
                .IsRequired()
                .HasConversion<short>();

        builder.Property(m => m.Quantity)
                .IsRequired()
                .HasPrecision(18, 3);

        builder.Property(m => m.OccurredAtUtc)
                .IsRequired();

        builder.Property(m => m.Reference)
                .HasMaxLength(StockMovement.MaxReferenceLength);

        builder.Ignore(m => m.Change);

        builder.HasIndex(m => new { m.SiteId, m.OccurredAtUtc });
    }
}
