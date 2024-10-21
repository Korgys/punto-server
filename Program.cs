using punto_server.Hubs;
using punto_server.Services;

var builder = WebApplication.CreateBuilder(args);

// Ajoute SignalR aux services
builder.Services.AddSignalR();

// Enregistre l'impl�mentation de IGestionnaireJeu pour l'injection de d�pendances
builder.Services.AddSingleton<IGestionnaireJeu, GestionnaireJeu>();

var app = builder.Build();

// Configure les routes pour le Hub SignalR
app.MapHub<JeuHub>("/gameHub");

// Affiche un message personnalis� lorsque le serveur d�marrera
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Serveur d�marr�, en attente de joueurs sur http://localhost:5000/gameHub");
});

// Lance le serveur
app.Run();