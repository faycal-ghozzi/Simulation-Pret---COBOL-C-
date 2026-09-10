using Microsoft.EntityFrameworkCore;
using SimulateurPret.Api.Donnees;

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

app.MapPost("/api/simulation", async (SimulationRequete req, ServiceMoteurCobol moteur) => {
    var resultat = await moteur.CalculerAsync(req.Montant, req.TauxAnnuel, req.DureeMois);
    return Results.Ok(resultat);
}).WithName("CalculerSimulation");

app.Run();

record SimulationRequete(decimal Montant, decimal TauxAnnuel, int DureeMois);
