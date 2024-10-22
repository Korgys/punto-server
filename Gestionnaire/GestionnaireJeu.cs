using punto_server.Models;

namespace punto_server.Services;

public class GestionnaireJeu : IGestionnaireJeu
{
    public Jeu Jeu { get; set; }
    public Jeu ObtenirJeu() => Jeu;
    public void DemarrerUnJeu()
    {
        Jeu = new Jeu();
    }

    public void RejoindrePartie(string nomDuJoueur, string identifiant)
    {
        // Initialise le joueur
        var joueur = new Joueur
        {
            Nom = nomDuJoueur,
            Identifiant = identifiant,
            OrdreDeJeu = Jeu.Joueurs.Count() + 1 // nombre de joueurs + 1
        };

        // Si c'est le 1er joueur, on tire une tuile et on la place au centre du plateau
        if (joueur.OrdreDeJeu == 1)
        {
            var valeurTuileCentre = PiocherTuilePourJoueur(joueur); // Pioche une tuile
            var tuileCentre = new Tuile
            {
                Valeur = valeurTuileCentre,
                Proprietaire = joueur,
                PositionX = 0,
                PositionY = 0
            };
            Jeu.Plateau.TuilesPlacees.Add(tuileCentre);
            Console.WriteLine($"La première tuile ({tuileCentre.Valeur}) de {joueur.Nom} a été placée au centre du plateau ({tuileCentre.PositionX},{tuileCentre.PositionY}).");
        }

        // Pioche 2 tuiles pour le joueur
        joueur.TuilesDansLaMain.Add(PiocherTuilePourJoueur(joueur));
        joueur.TuilesDansLaMain.Add(PiocherTuilePourJoueur(joueur));
        Console.WriteLine($"{joueur.Nom} pioche 2 tuiles.");

        Jeu.Joueurs.Add(joueur);

        // Lancement de la partie si nombre max de joueurs atteint
        if (joueur.OrdreDeJeu == Jeu.NombreMaxDeJoueurs)
        {
            Jeu.EtatJeu = EtatJeu.EnCours;
            Jeu.AuTourDuJoueur = Jeu.Joueurs.FirstOrDefault(j => j.OrdreDeJeu == 2);
        }
    }

    public void GererDeconnexion(string idJoueur)
    {
        var joueurDeconnecte = Jeu?.Joueurs?.FirstOrDefault(j => j.Identifiant == idJoueur);
        if (joueurDeconnecte != null)
        {
            Jeu.Joueurs.Remove(joueurDeconnecte);
        }
    }

    public bool PeutJouerTuile(string nomDuJoueur, int x, int y, int valeur)
    {
        // Cherche le joueur en question
        var joueur = Jeu.Joueurs.FirstOrDefault(j => j.Nom == nomDuJoueur);

        // Conditions pour pouvoir jouer une tuile
        bool estAdjacent = Jeu.Plateau.TuilesPlacees.Any(t =>
            Math.Abs(t.PositionX - x) <= 1 && Math.Abs(t.PositionY - y) <= 1);

        var coupAutorise = Jeu.EtatJeu == EtatJeu.EnCours   // Partie en cours
            && joueur != null                               // Etre un joueur de la partie
            && joueur.OrdreDeJeu == Jeu.AuTourDuJoueur.OrdreDeJeu     // Etre le joueur à qui c'est le tour de jouer
            && joueur.TuilesDansLaMain.Contains(valeur)     // Jouer une tuile de sa main
            && estAdjacent;                                 // La tuile doit être adjacente à une tuile existante

        // Ajoute une pénalité en cas de coup non-autorisé
        if (joueur != null && !coupAutorise)
        {
            joueur.Penalite++;
            Console.WriteLine($"Le joueur {joueur.Nom} reçoit une pénalité pour coup non-autorisé.");

            if (joueur.Penalite >= 3) // Disqualifie le joueur après 3 pénalités
            {
                Console.WriteLine($"Le joueur {joueur.Nom} a été disqualifié.");
                Jeu.Joueurs.Remove(joueur);
                if (Jeu.Joueurs.Count == 1) // Il reste un seul joueur : il est désigné comme vainqueur
                {
                    Jeu.EtatJeu = EtatJeu.Termine;
                    Jeu.Vainqueur = Jeu.Joueurs.Last();
                    Console.WriteLine($"{Jeu.Vainqueur.Nom} a gagné la partie !");
                    return false;
                }
                else
                {
                    PasserAuJoueurSuivant();
                }
            }
        }

        return coupAutorise;
    }

    public Jeu JouerTuile(string nomDuJoueur, int x, int y, int valeur)
    {
        var joueur = Jeu.Joueurs.FirstOrDefault(j => j.Nom == nomDuJoueur);

        if (joueur != null && PeutJouerTuile(nomDuJoueur, x, y, valeur))
        {
            var tuile = new Tuile
            {
                Valeur = valeur,
                Proprietaire = joueur,
                PositionX = x,
                PositionY = y
            };
            Jeu.Plateau.TuilesPlacees.Add(tuile);

            // Retirer la tuile de la main du joueur
            joueur.TuilesDansLaMain.Remove(valeur);
            Console.WriteLine($"{joueur.Nom} a placé la tuile {valeur} en position ({x},{y}).");

            // Vérifier si le joueur a gagné en alignant 4 tuiles
            if (VerifierAlignement(joueur))
            {
                Jeu.EtatJeu = EtatJeu.Termine;
                Jeu.Vainqueur = joueur;
                Console.WriteLine($"{joueur.Nom} a gagné la partie !");
            }

            // Pioche une nouvelle tuile
            joueur.TuilesDansLaMain.Add(PiocherTuilePourJoueur(joueur));

            // Passer au tour suivant
            PasserAuJoueurSuivant();
        }

        return Jeu;
    }

    public int PiocherTuilePourJoueur(Joueur joueur)
    {
        var tuiles = joueur.TuilesDansLeJeu;

        // Si le joueur n'a plus de tuile, la partie se termine
        if (tuiles.Count == 0) 
        {
            Jeu.EtatJeu = EtatJeu.Termine;
            return 0; // On pourrait également utiliser null. Signifie qu'il n'y a plus de tuile.
        }

        var tuilePiochee = tuiles.First(); // Pioche la première tuile
        tuiles.Remove(tuilePiochee); // Retire la tuile de la liste
        return tuilePiochee;
    }

    private void PasserAuJoueurSuivant()
    {
        if (Jeu.EtatJeu == EtatJeu.EnCours)
        {
            var joueurs = Jeu.Joueurs.ToList();
            int indexActuel = joueurs.FindIndex(j => j.OrdreDeJeu == Jeu.AuTourDuJoueur.OrdreDeJeu);
            Jeu.AuTourDuJoueur = joueurs[(indexActuel + 1) % joueurs.Count];
        }
    }

    public bool VerifierAlignement(Joueur joueur)
    {
        // Récupère les tuiles du joueur
        var tuilesJoueur = Jeu.Plateau.TuilesPlacees
                            .Where(t => t.Proprietaire.Nom == joueur.Nom)
                            .ToList();

        // Parcourir chaque tuile du joueur pour vérifier les alignements
        foreach (var tuile in tuilesJoueur)
        {
            // Vérification horizontale
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, 0)) return true;

            // Vérification verticale
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 0, 1)) return true;

            // Vérification diagonale gauche-droite (bas-droite)
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, 1)) return true;

            // Vérification diagonale droite-gauche (bas-gauche)
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, -1)) return true;
        }

        return false; // Aucun alignement trouvé
    }

    // Cette méthode vérifie si 4 tuiles sont alignées dans une direction spécifique
    private bool VerifierAlignementDirection(List<Tuile> tuilesJoueur, Tuile tuile, int deltaX, int deltaY)
    {
        int count = 1; // Compte la tuile actuelle

        // Vérifie dans la direction positive (droite/bas)
        for (int i = 1; i < 4; i++)
        {
            var tuileSuivante = tuilesJoueur.FirstOrDefault(t =>
                t.PositionX == tuile.PositionX + i * deltaX &&
                t.PositionY == tuile.PositionY + i * deltaY);
            if (tuileSuivante != null)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        // Vérifie dans la direction négative (gauche/haut)
        for (int i = 1; i < 4; i++)
        {
            var tuilePrecedente = tuilesJoueur.FirstOrDefault(t =>
                t.PositionX == tuile.PositionX - i * deltaX &&
                t.PositionY == tuile.PositionY - i * deltaY);
            if (tuilePrecedente != null)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        // Si on a trouvé 4 tuiles alignées
        return count >= 4;
    }
}
