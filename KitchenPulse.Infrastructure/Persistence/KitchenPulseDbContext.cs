using KitchenPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KitchenPulse.Infrastructure.Persistence;

public class KitchenPulseDbContext : DbContext

{
    public KitchenPulseDbContext(DbContextOptions<KitchenPulseDbContext> options) : base(options)
    {

    }

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KitchenPulseDbContext).Assembly);
    }
}
