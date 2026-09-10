using SimulateurPret.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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


// app.MapPost("/api/simulation", async (SimulationRequete req, ServiceMoteurCobol moteur) => {
//     Console.WriteLine($"Requete recue : montant={req.Montant}, taux={req.TauxAnnuel}, duree={req.DureeMois}");
//     var resultat = await moteur.CalculerAsync(req.Montant, req.TauxAnnuel, req.DureeMois);
//     Console.WriteLine($"Resultat : {resultat}");
//     return Results.Ok(resultat);
// }).WithName("CalculerSimulation");


app.Run();

record SimulationRequete(decimal Montant, decimal TauxAnnuel, int DureeMois);
