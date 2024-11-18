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

// Enregistre l'impl�mentation de IGestionnaireJeu pour l'injection de d�pendances
builder.Services.AddSingleton<IGestionnaireJeu, GestionnaireJeu>();

var app = builder.Build();

// Appel de la m�thode `DemarrerUnJeu` lors du d�marrage de l'application
app.Lifetime.ApplicationStarted.Register(() =>
{
    using (var scope = app.Services.CreateScope())
    {
        var gestionnaireJeu = scope.ServiceProvider.GetRequiredService<IGestionnaireJeu>();

        try
        {
            Console.WriteLine("D�marrage d'une nouvelle partie...");
            gestionnaireJeu.DemarrerUnJeu();
            Console.WriteLine("Partie d�marr�e !");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du d�marrage de la partie : {ex.Message}");
        }
    }
});

// Configure les routes pour le Hub SignalR
app.MapHub<JeuHub>("/punto");

// Affiche un message personnalis� lorsque le serveur d�marrera
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Serveur d�marr�, en attente de joueurs sur http://localhost:5000/punto");
});

// Lance le serveur
app.Run();