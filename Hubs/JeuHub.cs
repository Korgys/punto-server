using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using punto_server.Models;
using punto_server.Services;

namespace punto_server.Hubs;

public class JeuHub : Hub
{
    public IGestionnaireJeu _jeuService { get; set; }

    public JeuHub(IGestionnaireJeu jeuService)
    {
        _jeuService = jeuService;
    }

    public async Task RejoindrePartie(string joueur, string? equipe = null)
    {
        // Crée le jeu
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
                .FirstOrDefault(j => j.OrdreDeJeu == 2); // On commence avec le joueur 2 car le joueur 1 place déjà une carte au centre d'après la règle

            var tuile = jeu.Plateau.TuilesPlacees.FirstOrDefault();
            if (tuile != null)
            {
                await Clients.All.SendAsync("JouerTuile", tuile.Proprietaire.Nom, tuile.PositionX, tuile.PositionY, tuile.Valeur);
            }

            await Clients.All.SendAsync("CommencerTour", joueurQuiDebute.Nom);            
            await Clients.Caller.SendAsync("MettreAJourTuilesEnMain", joueurQuiDebute.TuilesDansLaMain); // ex: 3;6
            await Clients.All.SendAsync("MettreAJourPlateau", JsonConvert.SerializeObject(jeu.Plateau.ObtenirTuilesPlaceesSansDetails())); // json des tuiles placées
        }
    }

    public async Task JouerTuile(string nomDuJoueur, int x, int y, int valeur)
    {
        // Vérifier si le joueur peut jouer cette tuile
        if (!_jeuService.PeutJouerTuile(nomDuJoueur, x, y, valeur))
        {
            // Envoie un message d'erreur au joueur indiquant que le coup n'est pas autorisé
            await Clients.Caller.SendAsync("ErreurCoupNonAutorise", "Coup non autorisé.");
            return;
        }

        // Le joueur joue la tuile
        var jeu = _jeuService.JouerTuile(nomDuJoueur, x, y, valeur);
        var tuile = jeu.Plateau.TuilesPlacees.Last(); // Dernière tuile placée
        await Clients.All.SendAsync("JouerTuile", nomDuJoueur, x, y, valeur);

        // Vérifier si la partie est terminée
        if (jeu.EtatJeu == EtatJeu.Termine)
        {
            await Clients.All.SendAsync("TerminerJeu", jeu.Vainqueur?.Nom ?? "Inconnu");
            return;
        }

        // Envoie les tuiles en main uniquement au joueur appelant (joueur qui vient de jouer)
        var joueur = jeu.Equipes.SelectMany(e => e.Joueur).First(j => j.Nom == nomDuJoueur);
        var tuilesEnMain = joueur.TuilesDansLaMain;
        await Clients.Caller.SendAsync("MettreAJourTuilesEnMain", tuilesEnMain);
        var jsonPlateau = JsonConvert.SerializeObject(jeu.Plateau.ObtenirTuilesPlaceesSansDetails());
        await Clients.All.SendAsync("MettreAJourPlateau", jsonPlateau); // json des tuiles placées

        // Diffuser le tour suivant
        var joueurQuiDoitJouer = jeu.AuTourDuJoueur;
        await Clients.All.SendAsync("CommencerTour", joueurQuiDoitJouer.Nom);
    }
}
