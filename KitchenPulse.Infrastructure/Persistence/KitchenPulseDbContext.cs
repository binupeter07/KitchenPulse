using KitchenPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KitchenPulse.Infrastructure.Persistence;

public class KitchenPulseDbContext : DbContext
{
    public KitchenPulseDbContext(DbContextOptions<KitchenPulseDbContext> options) : base(options)
    {
    }

    public DbSet<Dish> Dishes => Set<Dish>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureStockMovementsAreAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnsureStockMovementsAreAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureStockMovementsAreAppendOnly()
    {
        var hasChangedMovement = ChangeTracker.Entries<StockMovement>()
            .Any(e => e.State is EntityState.Modified or EntityState.Deleted);

        if (hasChangedMovement)
        {
            throw new InvalidOperationException("Stock movements are append-only and cannot be modified or deleted.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KitchenPulseDbContext).Assembly);
    }
}
