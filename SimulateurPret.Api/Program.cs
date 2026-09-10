using Microsoft.EntityFrameworkCore;
using SimulateurPret.Api.Donnees;
using SimulateurPret.Api.Modeles;
using SimulateurPret.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PolitiqueDev", politique =>
    {
        politique.WithOrigins("http://localhost:5026").AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddDbContext<SimulateurPretDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SimulateurPretDb")
));

builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<SimulateurPretDbContext>();

var cleJwt = builder.Configuration["Jwt:Cle"]
    ?? throw new InvalidOperationException("La cle JWT n'est pas configuree (Jwt:Cle).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {

        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Emetteur"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cleJwt)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

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
app.UseCors("PolitiqueDev");
app.UseAuthentication();
app.UseAuthorization();

string GenererTokenJwt(IdentityUser utilisateur, IConfiguration config)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, utilisateur.Id),
        new Claim(JwtRegisteredClaimNames.Email, utilisateur.Email ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var cle = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Cle"]!));
    var identifiants = new SigningCredentials(cle, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: config["Jwt:Emetteur"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: identifiants
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/api/inscription", async (InscriptionRequete req, UserManager<IdentityUser> UserManager) =>
{
    var utilisateur = new IdentityUser { UserName = req.Email, Email = req.Email };
    var resultat = await UserManager.CreateAsync(utilisateur, req.MotDePasse);

    if (!resultat.Succeeded)
    {
        return Results.BadRequest(resultat.Errors.Select(e => e.Description));
    }

    return Results.Ok(new { message = "Utilisateur Cree avec succes"});
}).WithName("Inscription");

app.MapPost("api/connexion", async (ConnexionRequete req, UserManager<IdentityUser> UserManager, IConfiguration config) =>
{
    var utilisateur = await UserManager.FindByEmailAsync(req.Email);

    if (utilisateur is null)
    {
        return Results.Unauthorized();
    }

    var motDePasseValide = await UserManager.CheckPasswordAsync(utilisateur, req.MotDePasse);

    if (!motDePasseValide)
    {
        return Results.Unauthorized();
    }

    string token = GenererTokenJwt(utilisateur, config);
    return Results.Ok(new { token });
}).WithName("Connexion");

app.MapPost("/api/simulation", async (SimulationRequete req, ServiceMoteurCobol moteur, SimulateurPretDbContext db, ClaimsPrincipal utilisateurConnecte) => {
    var resultat = await moteur.CalculerAsync(req.Montant, req.TauxAnnuel, req.DureeMois);

    string userId = utilisateurConnecte.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

    var simulation = new Simulation
    {
        Montant = req.Montant,
        TauxAnnuel = req.TauxAnnuel,
        DureeMois = req.DureeMois,
        Mensualite = resultat.Mensualite,
        CoutTotal = resultat.CoutTotal,
        InteretsTotal = resultat.InteretsTotal,
        DateCreation = DateTime.UtcNow,
        UserId = userId
    };

    db.Simulations.Add(simulation);
    await db.SaveChangesAsync();

    return Results.Ok(resultat);
}).WithName("CalculerSimulation").RequireAuthorization();

app.MapGet("api/simulations", async (SimulateurPretDbContext db, ClaimsPrincipal utilisateurConnecte) =>
{

    string userId = utilisateurConnecte.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

    var simulations = await db.Simulations
        .Where(s => s.UserId == userId)
        .OrderByDescending(s => s.DateCreation)
        .ToListAsync();

    return Results.Ok(simulations);
}).WithName("ListerSimulations").RequireAuthorization();

app.Run();

record SimulationRequete(decimal Montant, decimal TauxAnnuel, int DureeMois);
record InscriptionRequete(string Email, string MotDePasse);
record ConnexionRequete(string Email, string MotDePasse);