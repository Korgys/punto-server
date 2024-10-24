using punto_server.Hubs;
using punto_server.Models;
using punto_server.Services;
using Raylib_cs;

var builder = WebApplication.CreateBuilder(args);

// Ajoute SignalR aux services
builder.Services.AddSignalR();

// POlitique des CORS
builder.Services.AddCors(options =>
    options.AddPolicy(
        "CorsPolicy",
        builder =>
        {
            builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithOrigins( /* omitted */
                )
                .AllowCredentials()
                .SetIsOriginAllowed(o => true)
                .WithMethods("GET", "POST");
        }
    )
);

// Enregistre l'implémentation de IGestionnaireJeu pour l'injection de dépendances
builder.Services.AddSingleton<IGestionnaireJeu, GestionnaireJeu>();

var app = builder.Build();

// Configure les routes pour le Hub SignalR
app.MapHub<JeuHub>("/punto");

// Affiche un message personnalis� lorsque le serveur d�marrera
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Serveur démarré, en attente de joueurs sur http://localhost:5000/punto");
});

// Lance le serveur dans un thread séparé
var serverThread = new Thread(() => app.Run());
serverThread.Start();

// Initialise l'interface graphique
int boardSize = 600;
int logWidth = 300;

Raylib.InitWindow(boardSize + logWidth, boardSize, "Punto Server");

// Récupérer l'état du jeu depuis IGestionnaireJeu
var gestionnaireJeu = app.Services.GetRequiredService<IGestionnaireJeu>();

Raylib.SetTargetFPS(10);

while (!Raylib.WindowShouldClose())
{
    // On obtient toutes nos données à jour
    Jeu jeu = gestionnaireJeu.ObtenirJeu();

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.White);

    // On commence à faire des choses uniquement si on a l'instance du jeu
    if (jeu != null)
    {
        EtatJeu etatJeu = jeu.EtatJeu;
        Joueur aQui = jeu.AuTourDuJoueur;
        List<Joueur> joueurs = jeu.Joueurs;
        Plateau plateau = jeu.Plateau;
        Joueur vainqueur = jeu.Vainqueur;

        if (etatJeu == EtatJeu.EnAttente)
        {
            GestionnaireUI.AfficherEcranAttente(boardSize, logWidth);
            GestionnaireUI.AfficherInfosJoueurs(jeu, boardSize, logWidth);
        }

        if (etatJeu == EtatJeu.EnCours)
        {
            // Affiche le plateau
            GestionnaireUI.AfficherEcranDeJeu(jeu, boardSize, logWidth);
        }

        if (etatJeu == EtatJeu.Termine)
        {
            if (joueurs != null && aQui != null)
                GestionnaireUI.AfficherEcranFinJeu(plateau, vainqueur, boardSize, logWidth);
        }
    }
    else
    {
        // Si on a pas d'instance de jeu, on affiche l'écran d'accueil
        GestionnaireUI.AfficherEcranAccueil(boardSize, logWidth);
    }

    Raylib.EndDrawing();
}

Raylib.CloseWindow();
Environment.Exit(0);
