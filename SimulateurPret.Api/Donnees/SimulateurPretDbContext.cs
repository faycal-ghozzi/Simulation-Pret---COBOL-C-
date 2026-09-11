using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimulateurPret.Api.Modeles;

namespace SimulateurPret.Api.Donnees;

public class SimulateurPretDbContext : IdentityDbContext<IdentityUser>
{
    public SimulateurPretDbContext(DbContextOptions<SimulateurPretDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Simulation>().HasOne<IdentityUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<Simulation> Simulations => Set<Simulation>();
}