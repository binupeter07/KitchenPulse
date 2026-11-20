using KitchenPulse.Domain.Entities;
using KitchenPulse.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KitchenPulse.Infrastructure.Persistence.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        var typeMin = (int)Enum.GetValues<OrderType>().Min();
        var typeMax = (int)Enum.GetValues<OrderType>().Max();
        var courseMin = (int)Enum.GetValues<Course>().Min();
        var courseMax = (int)Enum.GetValues<Course>().Max();

        builder.ToTable("Orders", t =>
        {
            t.HasCheckConstraint("CK_Orders_OrderType", $"[OrderType] BETWEEN {typeMin} AND {typeMax}");
            t.HasCheckConstraint("CK_Orders_OrderNumber", "[OrderNumber] > 0");
        });

        builder.HasKey(o => o.Id);

        builder.HasOne<Site>()
                .WithMany()
                .HasForeignKey(o => o.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.Property(o => o.OrderType)
                .IsRequired()
                .HasConversion<short>();

        builder.Property(o => o.BusinessDate)
                .HasColumnType("date");

        builder.Property(o => o.OrderNumber);

        builder.Property(o => o.Table)
                .HasMaxLength(20);

        builder.Property(o => o.PlacedAtUtc)
                .IsRequired();

        builder.Property<List<Course>>("_collectedCourses")
                .HasColumnName("CollectedCourses");

        builder.HasIndex(o => new { o.SiteId, o.BusinessDate, o.OrderType, o.OrderNumber })
                .IsUnique();

        builder.HasIndex(o => new { o.SiteId, o.PlacedAtUtc });

        builder.OwnsMany<OrderLine>("_lines", lineBuilder =>
        {
            lineBuilder.ToTable("OrderLines", t =>
            {
                t.HasCheckConstraint("CK_OrderLines_Quantity", "[Quantity] > 0");
                t.HasCheckConstraint("CK_OrderLines_Course", $"[Course] BETWEEN {courseMin} AND {courseMax}");
            });

            lineBuilder.WithOwner().HasForeignKey("OrderId");

            lineBuilder.HasKey(l => l.Id);

            lineBuilder.HasOne(l => l.Dish)
                        .WithMany()
                        .HasForeignKey("DishId")
                        .IsRequired()
                        .OnDelete(DeleteBehavior.Restrict);

            lineBuilder.Navigation(l => l.Dish).AutoInclude();

            lineBuilder.Property(l => l.Course)
                        .HasConversion<short>();

            lineBuilder.Ignore(l => l.Removed);
            lineBuilder.Ignore(l => l.Extras);
            lineBuilder.Ignore(l => l.CustomerAllergies);
            lineBuilder.Ignore(l => l.HasCustomerAllergy);
            lineBuilder.Ignore(l => l.Allergens);
            lineBuilder.Ignore(l => l.UnsafeAllergens);
            lineBuilder.Ignore(l => l.UnsafeIngredients);
        });

        builder.Ignore(o => o.Lines);
        builder.Ignore(o => o.CollectedCourse);
        builder.Ignore(o => o.IsMainsOnHold);
    }
}
