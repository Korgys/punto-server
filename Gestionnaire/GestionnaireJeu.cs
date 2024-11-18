using punto_server.Gestionnaire;
using punto_server.Models;

namespace punto_server.Services;

public class GestionnaireJeu : IGestionnaireJeu
{
    public Jeu Jeu { get; set; }

    public Jeu ObtenirJeu() => Jeu;

    public void DemarrerUnJeu()
    {
        Console.WriteLine("Entrez le nombre de joueurs dans la partie : ");
        var nbJoueurs = int.Parse(Console.ReadLine());

        Jeu = new Jeu(nbJoueurs);
        Console.WriteLine($"Nouvelle partie initialisée. En attente de {nbJoueurs} joueurs ...");
    }

    public void RejoindrePartie(string nomDuJoueur, string identifiant)
    {
        // Initialise le joueur
        var joueur = new Joueur
        {
            Nom = nomDuJoueur,
            Identifiant = identifiant,
            OrdreDeJeu =
                Jeu.Joueurs.Count()
                + 1 // nombre de joueurs + 1
            ,
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
                PositionY = 0,
            };
            Jeu.Plateau.TuilesPlacees.Add(tuileCentre);
            Console.WriteLine(
                $"La première tuile ({tuileCentre.Valeur}) de {joueur.Nom} a été placée au centre du plateau ({tuileCentre.PositionX},{tuileCentre.PositionY})."
            );
        }

        // Pioche 2 tuiles pour le joueur
        joueur.TuilesDansLaMain.Add(PiocherTuilePourJoueur(joueur));
        joueur.TuilesDansLaMain.Add(PiocherTuilePourJoueur(joueur));

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
        try
        {
            var joueurDeconnecte = Jeu?.Joueurs?.FirstOrDefault(j => j.Identifiant == idJoueur);
            if (Jeu?.Joueurs != null && joueurDeconnecte != null)
            {
                Jeu.Joueurs.Remove(joueurDeconnecte);
            }
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    public bool PeutJouerTuile(string nomDuJoueur, int x, int y, int valeur)
    {
        // Cherche le joueur en question
        var joueur = Jeu?.Joueurs?.FirstOrDefault(j => j.Nom == nomDuJoueur);
        if (joueur == null) return false;

        // Définit la tuile à jouer
        var tuile = new Tuile
        {
            PositionX = x,
            PositionY = y,
            Valeur = valeur,
            Proprietaire = joueur
        };

        // Regarde si le joueur peut jouer la tuile
        bool coupAutorise = GestionnaireRegles.PeutPlacerTuile(Jeu.Plateau, joueur, tuile);

        // Ajoute une pénalité en cas de coup non-autorisé
        if (!coupAutorise)
        {
            joueur.Penalite++;
            Console.WriteLine($"Le joueur {joueur.Nom} reçoit une pénalité pour coup non-autorisé (tuile {valeur} en {x}, {y}).");

            // Disqualifie le joueur après 3 pénalités
            if (joueur.Penalite >= 3)
            {
                Console.WriteLine($"Le joueur {joueur.Nom} a été disqualifié.");
                Jeu.Joueurs.Remove(joueur);
                if (Jeu.Joueurs.Count == 1) // Il reste un seul joueur : il est désigné comme vainqueur
                {
                    // Actualise l'état de la partie
                    Jeu.EtatJeu = EtatJeu.Termine;
                    Jeu.Vainqueur = Jeu.Joueurs.Last();

                    // Affiche le plateau et le nom du vainqueur
                    AfficherPlateau();
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
            // Joue la tuile
            var tuile = new Tuile
            {
                Valeur = valeur,
                Proprietaire = joueur,
                PositionX = x,
                PositionY = y,
            };

            // Si on recouvre une tuile, on la supprime
            Tuile? tuileASupp = Jeu.Plateau.TuilesPlacees.FirstOrDefault(t =>
                t.PositionX == x && t.PositionY == y
            );
            if (tuileASupp != null)
                Jeu.Plateau.TuilesPlacees.Remove(tuileASupp);

            Jeu.Plateau.TuilesPlacees.Add(tuile);

            // Affiche le plateau
            AfficherPlateau();

            // Retirer la tuile de la main du joueur
            joueur.TuilesDansLaMain.Remove(valeur);
            Console.WriteLine($"{joueur.Nom} a placé la tuile {valeur} en position ({x},{y}).");

            // Vérifier si le joueur a gagné en alignant 4 tuiles
            if (VerifierAlignement(joueur))
            {
                // Actualise l'état de la partie
                Jeu.EtatJeu = EtatJeu.Termine;
                Jeu.Vainqueur = joueur;

                // Affiche le plateau et le nom du vainqueur
                AfficherPlateau();
                Console.WriteLine($"{joueur.Nom} a gagné la partie !");
            }
            else
            {
                // Pioche une nouvelle tuile
                joueur.TuilesDansLaMain.Add(PiocherTuilePourJoueur(joueur));

                // Passer au tour suivant
                PasserAuJoueurSuivant();
            }
        }

        return Jeu;
    }

    /// <summary>
    /// Pioche une tuile pour le joueur et ajoute cette tuile dans sa main.
    /// </summary>
    /// <param name="joueur"></param>
    /// <returns></returns>
    public int PiocherTuilePourJoueur(Joueur joueur)
    {
        var tuilesDansLaPioche = joueur.TuilesDansLeJeu;

        // Si le joueur n'a plus de tuile, la partie se termine
        if (tuilesDansLaPioche.Count == 0) 
        {
            Jeu.EtatJeu = EtatJeu.Termine;
            return 0; // On pourrait également utiliser null. Signifie qu'il n'y a plus de tuile.
        }

        var tuilePiochee = tuilesDansLaPioche.First(); // Pioche la première tuile
        tuilesDansLaPioche.Remove(tuilePiochee); // Retire la tuile de la liste

        string main = tuilePiochee + (joueur.TuilesDansLaMain.Any() ? ", " : "") + string.Join(',', joueur.TuilesDansLaMain);
        Console.WriteLine($"{joueur.Nom} pioche la tuile {tuilePiochee} (main: {main}).");

        return tuilePiochee;
    }

    private void PasserAuJoueurSuivant()
    {
        if (Jeu?.Joueurs == null || Jeu.EtatJeu != EtatJeu.EnCours) return;

        var indexJoueurActuel = Jeu.Joueurs.IndexOf(Jeu.AuTourDuJoueur);
        var indexJoueurSuivant = (indexJoueurActuel + 1) % Jeu.Joueurs.Count;
        Jeu.AuTourDuJoueur = Jeu.Joueurs[indexJoueurSuivant];

        AfficherMessageDeJoueur(Jeu.AuTourDuJoueur.OrdreDeJeu, $"C'est au tour de {Jeu.AuTourDuJoueur.Nom} de jouer.\n");

        // Cas où le joueur n'a plus de tuiles
        if (Jeu.AuTourDuJoueur.TuilesDansLaMain.Count == 0)
        {
            Console.WriteLine("Le joueur n'a plus de tuile. Fin de la partie.");
            Jeu.EtatJeu = EtatJeu.Termine;

            // Désigne le vainqueur en prenant le 1er joueur au hasard ayant réussi à aligner 3 tuiles.
            var aleatoire = new Random();
            Jeu.Vainqueur = Jeu.Joueurs
                .OrderBy(j => aleatoire.Next())
                .FirstOrDefault(j => GestionnaireRegles.VerifierConditionsVictoire(Jeu.Plateau, j, 3));
        }
    }

    public bool VerifierAlignement(Joueur joueur)
    {
        // Récupère les tuiles du joueur
        var tuilesJoueur = Jeu
            .Plateau.TuilesPlacees.Where(t => t.Proprietaire.Nom == joueur.Nom)
            .ToList();

        // Parcourir chaque tuile du joueur pour vérifier les alignements
        foreach (var tuile in tuilesJoueur)
        {
            // Vérification horizontale
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, 0))
                return true;

            // Vérification verticale
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 0, 1))
                return true;

            // Vérification diagonale gauche-droite (bas-droite)
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, 1))
                return true;

            // Vérification diagonale droite-gauche (bas-gauche)
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, -1))
                return true;
        }

        return false; // Aucun alignement trouvé
    }

    // Cette méthode vérifie si 4 tuiles sont alignées dans une direction spécifique
    private static bool VerifierAlignementDirection(List<Tuile> tuilesJoueur, Tuile tuile, int deltaX, int deltaY)
    {
        int count = 1; // Compte la tuile actuelle

        // Vérifie dans la direction positive (droite/bas)
        for (int i = 1; i < 4; i++)
        {
            var tuileSuivante = tuilesJoueur.FirstOrDefault(t =>
                t.PositionX == tuile.PositionX + i * deltaX
                && t.PositionY == tuile.PositionY + i * deltaY
            );
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
                t.PositionX == tuile.PositionX - i * deltaX
                && t.PositionY == tuile.PositionY - i * deltaY
            );
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

    /// <summary>
    /// Affiche le plateau en console.
    /// </summary>
    public void AfficherPlateau()
    {
        int tailleGrille = 6 * 2 - 1; // 6 de rayon sur une grille de 11 de diametre
        var grille = new string[tailleGrille, tailleGrille];
        var tuilesPlacees = Jeu.Plateau.TuilesPlacees;

        // Initialise la grille avec des "." pour les emplacements vides
        for (int i = 0; i < tailleGrille; i++)
        {
            for (int j = 0; j < tailleGrille; j++)
            {
                grille[i, j] = " ";
            }
        }

        // Remplit la grille avec les tuiles placées (sans affichage de couleur ici)
        foreach (var tuile in tuilesPlacees)
        {
            if (tuile.PositionX > -6 && tuile.PositionX < 6 && tuile.PositionY > -6 && tuile.PositionY < 6)
            {
                grille[tuile.PositionY + 5, tuile.PositionX + 5] = tuile.Valeur.ToString();
            }
        }

        // Affiche la grille dans la console avec des couleurs
        Console.WriteLine("Plateau :");
        Console.WriteLine(" X 5 4 3 2 1 0 1 2 3 4 5 6");
        Console.WriteLine("  ________________________");
        for (int i = 0; i < tailleGrille; i++)
        {
            if (i == 0) Console.Write(" Y|");
            else if (i >= 5) Console.Write($" {i - 5}|"); // Affichage des nombres positifs avec un espace devant
            else Console.Write($"{i - 5}|");

            for (int j = 0; j < tailleGrille; j++)
            {
                // Cherche la tuile à cette position
                var tuile = tuilesPlacees.FirstOrDefault(t => t.PositionX == j - 5 && t.PositionY == i - 5);
                if (tuile != null)
                {
                    // Change la couleur en fonction du joueur
                    AfficherMessageDeJoueur(tuile.Proprietaire.OrdreDeJeu, grille[i, j] + " ");
                }
                else
                {
                    // Affiche les emplacements vides avec la couleur par défaut
                    Console.Write(grille[i, j] + " ");
                }
            }
            Console.WriteLine(); // Retour à la ligne après chaque rangée
        }

        // Réinitialise la couleur à la fin
        Console.ResetColor();
    }

    /// <summary>
    /// Affiche un message en couleur dans la console en fonction de l'index du joueur.
    /// </summary>
    /// <param name="numeroJoueur"></param>
    /// <param name="message"></param>
    public static void AfficherMessageDeJoueur(int numeroJoueur, string message)
    {
        Console.ForegroundColor = numeroJoueur switch
        {
            1 => ConsoleColor.Magenta,// Joueur 1 - Magenta
            2 => ConsoleColor.Cyan,// Joueur 2 - Cyan
            3 => ConsoleColor.Green,// Joueur 3 - Vert
            4 => ConsoleColor.Yellow,// Joueur 4 - Jaune
            5 => ConsoleColor.Gray,// Joueur 5 - Gris
            6 => ConsoleColor.Red,// Joueur 6 - Rouge
            7 => ConsoleColor.DarkYellow,// Joueur 7 - Jaune foncé
            8 => ConsoleColor.DarkRed,// Joueur 8 - rouge foncé
            9 => ConsoleColor.DarkGray,// Joueur 9 - gris foncé
            10 => ConsoleColor.DarkCyan,// Joueur 10 - cyan foncé
            _ => ConsoleColor.White,// Autres joueurs ou cas par défaut
        };
        Console.Write(message);
        Console.ResetColor();
    }

}
