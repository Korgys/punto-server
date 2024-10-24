using punto_server.Hubs;
using punto_server.Services;

var builder = WebApplication.CreateBuilder(args);

// Ajoute SignalR aux services
builder.Services.AddSignalR();

// POlitique des CORS
builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
builder =>
{
    builder
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithOrigins(/* omitted */)
    .AllowCredentials()
    .SetIsOriginAllowed(o => true)
    .WithMethods("GET", "POST");
}));

// Enregistre l'implémentation de IGestionnaireJeu pour l'injection de dépendances
builder.Services.AddSingleton<IGestionnaireJeu, GestionnaireJeu>();

var app = builder.Build();

// Configure les routes pour le Hub SignalR
app.MapHub<JeuHub>("/punto");

// Affiche un message personnalisé lorsque le serveur démarrera
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Serveur démarré, en attente de joueurs sur http://localhost:5000/punto");
});

// Lance le serveur
app.Run();