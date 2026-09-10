using Microsoft.EntityFrameworkCore;
using SimulateurPret.Api.Modeles;

namespace SimulateurPret.Api.Donnees;

public class SimulateurPretDbContext : DbContext
{
    public SimulateurPretDbContext(DbContextOptions<SimulateurPretDbContext> options)
        : base(options)
    {
    }

    public DbSet<Simulation> Simulations => Set<Simulation>();
}