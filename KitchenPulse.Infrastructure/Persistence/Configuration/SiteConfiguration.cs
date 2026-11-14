using KitchenPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KitchenPulse.Infrastructure.Persistence.Configuration;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("Sites");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

        builder.HasIndex(s => s.Name)
                .IsUnique();
    }
}
