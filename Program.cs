using punto_server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Ajoute SignalR aux services
builder.Services.AddSignalR();

var app = builder.Build();

// Configure les routes pour le Hub SignalR
app.MapHub<JeuHub>("/gameHub");

// Affiche un message personnalisé lorsque le serveur démarrera
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Serveur démarré, en attente de joueurs sur http://localhost:5000/gameHub");
});

// Lance le serveur
app.Run();