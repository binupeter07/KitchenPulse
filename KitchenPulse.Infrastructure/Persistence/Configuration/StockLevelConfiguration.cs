using KitchenPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KitchenPulse.Infrastructure.Persistence.Configuration;

public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        builder.ToTable("StockLevels");

        builder.HasKey(s => new { s.SiteId, s.IngredientId });

        builder.HasOne<Site>()
                .WithMany()
                .HasForeignKey(s => s.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Ingredient>()
                .WithMany()
                .HasForeignKey(s => s.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.IngredientId);

        builder.Property(s => s.Quantity)
                .IsRequired()
                .HasPrecision(18, 3);
            
        builder.Ignore(s => s.IsStockBelowZero);

        builder.Property<byte[]>("RowVersion")
                .IsRowVersion();
    }
}
