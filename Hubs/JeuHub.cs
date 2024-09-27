using Microsoft.AspNetCore.SignalR;
using punto_server.Models;
using punto_server.Services;

namespace punto_server.Hubs;

public class JeuHub : Hub
{
    public IJeuService _jeuService { get; set; }

    public JeuHub(IJeuService jeuService) 
    {
        _jeuService = jeuService;
    }

    public async Task RejoindrePartie(string joueur, string? equipe = null)
    {
        // Créé le jeu
        var jeu = _jeuService.ObtenirJeu();
        if (jeu == null)
        {
            _jeuService.DemarrerUnJeu();
        }

        // Permet au joueur de rejoindre la partie
        _jeuService.RejoindrePartie(joueur, equipe);

        // Diffuse que le joueur a rejoint la partie
        await Clients.All.SendAsync("RejoindrePartie", joueur);

        // Récupère la variable mise à jour
        jeu = _jeuService.ObtenirJeu();
        if (jeu.EtatJeu == EtatJeu.EnCours)
        {
            // Diffuse que la partie commence
            var joueurQuiDebute = jeu.Equipes
                .SelectMany(e => e.Joueur)
                .FirstOrDefault(j => j.OrdreDeJeu == 2);
            var tuile = jeu.Plateau.TuilesPlacees.First();
            await Clients.All.SendAsync("JouerTuile", tuile.Proprietaire.Nom, tuile.PositionX, tuile.PositionY, tuile.Valeur);
            await Clients.All.SendAsync("CommencerTour", joueurQuiDebute);
        }
    }

    public async Task JouerTuile(string joueur, int x, int y, int valeur)
    {
        if(!_jeuService.PeutJouerTuile(joueur, x, y, valeur))
        {
            // TODO: Envoye au joueur un coup non-autorisé
            return;
        }

        var jeu = _jeuService.JouerTuile(joueur, x, y, valeur);
        var tuile = jeu.Plateau.TuilesPlacees.Last(); // Dernière tuile placée
        await Clients.All.SendAsync("JouerTuile", joueur, x, y, valeur);

        // En cas de victoire
        if (jeu.EtatJeu == EtatJeu.Termine)
        {
            await Clients.All.SendAsync("TerminerJeu", jeu.Vainqueur.Nom);
        }

        // TODO : Communique la tuile piochée uniquement au joueur appelant
        var tuilePiochee = 

        var joueurQuiDoitJouer = jeu.AuTourDuJoueur;
        // Diffuse la tuile jouée à tous les clients connectés
        await Clients.All.SendAsync("CommencerTour", joueurQuiDoitJouer.Nom);
    }
}

