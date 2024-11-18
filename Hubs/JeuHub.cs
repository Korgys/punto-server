using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using punto_server.Models;
using punto_server.Models.Dto;
using punto_server.Services;

namespace punto_server.Hubs;

public class JeuHub : Hub
{
    public IGestionnaireJeu _gestionnaireJeu { get; set; }

    public JeuHub(IGestionnaireJeu gestionnaireJeu)
    {
        _gestionnaireJeu = gestionnaireJeu;
    }

    public void DemarrerJeu()
    {
        // Crée le jeu si aucune partie en cours
        var jeu = _gestionnaireJeu.ObtenirJeu();
        if (jeu == null || jeu.EtatJeu == EtatJeu.Termine)
        {
            _gestionnaireJeu.DemarrerUnJeu();
        }
    }

    public async Task RejoindrePartie(string joueur)
    {
        // Crée le jeu si aucune partie en cours
        var jeu = _gestionnaireJeu.ObtenirJeu();
        if (jeu == null)
        {
            await Clients.Caller.SendAsync("Erreur", "Pas de partie en cours, démarrez un nouveau jeu.");
            return;
        }
        else if (jeu.EtatJeu == EtatJeu.Termine)
        {
            await Clients.Caller.SendAsync("Erreur", "Partie terminée, démarrez un nouveau jeu.");
            return;
        }
        else if (jeu.EtatJeu == EtatJeu.EnCours)
        {
            await Clients.Caller.SendAsync("Erreur", "Partie en cours, veuillez patienter");
            return;
        }

        // Gestion des reconnexions de joueurs existants
        var joueurExistant = jeu.Joueurs.FirstOrDefault(j => j.Nom == joueur);
        if (joueurExistant != null)
        {
            // Mise à jour du ConnectionId pour le joueur existant
            joueurExistant.Identifiant = Context.ConnectionId;
            _gestionnaireJeu.AssocierJoueur(joueur, Context.ConnectionId);

            Console.WriteLine($"Joueur {joueur} a reconfirmé sa connexion ({Context.ConnectionId}).");
            await Clients.Caller.SendAsync("Reconnecte", joueur);
        }
        else
        {
            // Nouveau joueur
            _gestionnaireJeu.RejoindrePartie(joueur, Context.ConnectionId);
            _gestionnaireJeu.AssocierJoueur(joueur, Context.ConnectionId);

            // Diffuse que le joueur a rejoint la partie
            Console.WriteLine($"Nouveau joueur {joueur} ({Context.ConnectionId}) a rejoint la partie !");
            await Clients.All.SendAsync("RejoindrePartie", joueur);
        }

        // Récupère la variable mise à jour
        jeu = _gestionnaireJeu.ObtenirJeu();
        if (jeu.EtatJeu == EtatJeu.EnCours)
        {
            // Diffuse que la partie commence
            Console.WriteLine("La partie commence !");
            await Clients.All.SendAsync("CommencerPartie", jeu.Joueurs.OrderBy(j => j.OrdreDeJeu).ToList());

            // Commence le tour
            var joueurQuiDebute = jeu.Joueurs.FirstOrDefault(j => j.OrdreDeJeu == 2); // On commence avec le joueur 2 car le joueur 1 place déjà une carte au centre d'après la règle

            var tuile = jeu.Plateau.TuilesPlacees.FirstOrDefault();
            if (tuile != null)
            {
                await Clients.All.SendAsync(
                    "JouerTuile",
                    tuile.Proprietaire.Nom,
                    tuile.PositionX,
                    tuile.PositionY,
                    tuile.Valeur
                );
            }

            await Clients.All.SendAsync(
                "MettreAJourPlateau",
                JsonConvert.SerializeObject(PlateauPublic.Convertir(jeu.Plateau).TuilesPlacees)
            ); // json des tuiles placées
            await Clients.All.SendAsync("CommencerTour", joueurQuiDebute.Nom);
            await Clients.Caller.SendAsync(
                "MettreAJourTuilesEnMain",
                joueurQuiDebute.TuilesDansLaMain
            ); // ex: 3;6
        }
    }

    public async Task JouerTuile(string nomDuJoueur, int x, int y, int valeur)
    {
        var jeu = _gestionnaireJeu.ObtenirJeu();

        // Vérifier si le joueur peut jouer cette tuile
        if (!_gestionnaireJeu.PeutJouerTuile(nomDuJoueur, x, y, valeur))
        {
            // Envoie un message d'erreur au joueur indiquant que le coup n'est pas autorisé
            await Clients.Caller.SendAsync("ErreurCoupNonAutorise", "Coup non autorisé.");

            jeu = _gestionnaireJeu.JouerTuile(nomDuJoueur, x, y, valeur);
            var joueurAvecPenalite = jeu.Joueurs.FirstOrDefault(j => j.Identifiant == Context.ConnectionId);

            // Joueur disqualifié car plus présent dans la liste des joueurs
            if (joueurAvecPenalite == null)
            {
                // Notifie les autres clients de la déconnexion si nécessaire
                await Clients.Others.SendAsync("JoueurDeconnecte", Context.ConnectionId);

                // Appele la méthode de la classe de base
                await base.OnDisconnectedAsync(new Exception());
            }

            return;
        }

        // Le joueur joue la tuile
        jeu = _gestionnaireJeu.JouerTuile(nomDuJoueur, x, y, valeur);
        var tuile = jeu.Plateau.TuilesPlacees.Last(); // Dernière tuile placée
        await Clients.All.SendAsync("JouerTuile", nomDuJoueur, x, y, valeur);

        // Vérifier si la partie est terminée
        if (jeu.EtatJeu == EtatJeu.Termine)
        {
            await Clients.All.SendAsync("MettreAJourPlateau", JsonConvert.SerializeObject(PlateauPublic.Convertir(jeu.Plateau).TuilesPlacees));
            await Clients.All.SendAsync("TerminerJeu", jeu.Vainqueur?.Nom ?? "Inconnu");
            return;
        }

        // Envoie les tuiles en main uniquement au joueur appelant (joueur qui vient de jouer)
        var joueur = jeu.Joueurs.First(j => j.Nom == nomDuJoueur);
        var tuilesEnMain = joueur.TuilesDansLaMain;
        await Clients.Caller.SendAsync("MettreAJourTuilesEnMain", tuilesEnMain);
        var jsonPlateau = JsonConvert.SerializeObject(
            PlateauPublic.Convertir(jeu.Plateau).TuilesPlacees
        );
        await Clients.All.SendAsync("MettreAJourPlateau", jsonPlateau); // json des tuiles placées

        // Diffuser le tour suivant
        var joueurQuiDoitJouer = jeu.AuTourDuJoueur;
        await Clients.All.SendAsync("CommencerTour", joueurQuiDoitJouer.Nom);
        await Clients
            .Client(joueurQuiDoitJouer.Identifiant)
            .SendAsync("MettreAJourTuilesEnMain", joueurQuiDoitJouer.TuilesDansLaMain);
    }

    public async Task ObtenirEtatJeu()
    {
        Console.WriteLine($"[{ObtenirNomDuJoueur(Context.ConnectionId)}] Appel à ObtenirEtatJeu.");

        var jeu = _gestionnaireJeu.ObtenirJeu();
        if (jeu != null)
        {
            await Clients.Caller.SendAsync("MettreAJourEtatJeu", jeu.EtatJeu.ToString());
        }
    }

    public async Task ObtenirPlateau()
    {
        Console.WriteLine($"[{ObtenirNomDuJoueur(Context.ConnectionId)}] Appel à ObtenirPlateau.");

        var jeu = _gestionnaireJeu.ObtenirJeu();
        if (jeu != null)
        {
            await Clients.Caller.SendAsync("MettreAJourPlateau", JsonConvert.SerializeObject(PlateauPublic.Convertir(jeu.Plateau).TuilesPlacees));
        }
    }

    public async Task ObtenirMainJoueur()
    {
        Console.WriteLine($"[{ObtenirNomDuJoueur(Context.ConnectionId)}] Appel à ObtenirMainJoueur.");

        var jeu = _gestionnaireJeu.ObtenirJeu();
        var joueur = jeu.Joueurs.FirstOrDefault(j => j.Identifiant == Context.ConnectionId);
        if (joueur != null)
        {
            await Clients.Caller.SendAsync("MettreAJourTuilesEnMain", joueur.TuilesDansLaMain);
        }
    }

    public async Task ObtenirJoueur()
    {
        Console.WriteLine($"[{ObtenirNomDuJoueur(Context.ConnectionId)}] Appel à ObtenirJoueur.");

        var jeu = _gestionnaireJeu.ObtenirJeu();
        var joueur = jeu.Joueurs.FirstOrDefault(j => j.Identifiant == Context.ConnectionId);
        if (joueur != null)
        {
            await Clients.Caller.SendAsync("MettreAJourJoueur", JsonConvert.SerializeObject(JoueurPublique.Convertir(joueur)));
        }
    }

    public async Task ObtenirJoueurs()
    {
        Console.WriteLine($"[{ObtenirNomDuJoueur(Context.ConnectionId)}] Appel à ObtenirJoueurs.");

        // Obtient le joueur et les adversaires
        var jeu = _gestionnaireJeu.ObtenirJeu();
        var joueur = jeu.Joueurs.FirstOrDefault(j => j.Identifiant == Context.ConnectionId);
        var adversaires = jeu
            .Joueurs.Where(j => j.Identifiant != Context.ConnectionId)
            .Select(j => JoueurPublique.Convertir(j));
        if (joueur != null && adversaires != null)
        {
            // Pour le joueur, renvoie toutes les infos
            var joueurPublique = JoueurPublique.Convertir(joueur);

            // Pour les adversaires, ne renvoie pas les infos des tuiles dans la main
            foreach (var adversaire in adversaires)
            {
                adversaire.TuilesDansLaMain = null;
            }

            // Envoie la liste des joueurs avec les infos nécessaires
            var tousLesJoueurs = new List<JoueurPublique>(adversaires) { joueurPublique };
            await Clients.Caller.SendAsync("ObtenirJoueurs", tousLesJoueurs);
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string connectionId = Context.ConnectionId;

        Console.WriteLine($"Client déconnecté : {connectionId}");

        // Marquer le joueur comme déconnecté
        var joueur = _gestionnaireJeu.ObtenirJeu()?.Joueurs.FirstOrDefault(j => j.Identifiant == connectionId);
        if (joueur != null)
        {
            joueur.Identifiant = null; // Marque la déconnexion
            Console.WriteLine($"Le joueur {joueur.Nom} est marqué comme déconnecté.");
        }

        // Notifie les autres joueurs
        await Clients.Others.SendAsync("JoueurDeconnecte", connectionId);

        await base.OnDisconnectedAsync(exception);
    }


    private string ObtenirNomDuJoueur(string connectionId)
    {
        var jeu = _gestionnaireJeu.ObtenirJeu();
        if (jeu == null) return "";

        return jeu.Joueurs.FirstOrDefault(j => j.Identifiant == connectionId)?.Nom ?? ""; 
    }
}
