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

    public DbSet<Simulation> Simulations => Set<Simulation>();
}