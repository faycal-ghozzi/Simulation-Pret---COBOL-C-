using Microsoft.EntityFrameworkCore;
using SimulateurPret.Api.Donnees;
using SimulateurPret.Api.Modeles;
using SimulateurPret.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<SimulateurPretDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SimulateurPretDb")
));

builder.Services.AddSingleton(new ServiceMoteurCobol(
    Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "cobol", "calcul_pret"))
));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/simulation", async (SimulationRequete req, ServiceMoteurCobol moteur, SimulateurPretDbContext db) => {
    var resultat = await moteur.CalculerAsync(req.Montant, req.TauxAnnuel, req.DureeMois);

    var simulation = new Simulation
    {
        Montant = req.Montant,
        TauxAnnuel = req.TauxAnnuel,
        DureeMois = req.DureeMois,
        Mensualite = resultat.Mensualite,
        CoutTotal = resultat.CoutTotal,
        InteretsTotal = resultat.InteretsTotal,
        DateCreation = DateTime.UtcNow
    };

    db.Simulations.Add(simulation);
    await db.SaveChangesAsync();

    return Results.Ok(resultat);
}).WithName("CalculerSimulation");

app.MapGet("api/simulations", async (SimulateurPretDbContext db) =>
{
    var simulations = await db.Simulations
        .OrderByDescending(s => s.DateCreation)
        .ToListAsync();

    return Results.Ok(simulations);
}).WithName("ListerSimulations");

app.Run();

record SimulationRequete(decimal Montant, decimal TauxAnnuel, int DureeMois);
