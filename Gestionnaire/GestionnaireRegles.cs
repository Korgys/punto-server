using punto_server.Models;

namespace punto_server.Gestionnaire;

/// <summary>
/// Classe permettant de gérer les règles du jeu.
/// </summary>
public class GestionnaireRegles
{
    /// <summary>
    /// Renvoie VRAI si le joueur peut placer la tuile sur le plateau.
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="joueur"></param>
    /// <param name="tuile"></param>
    /// <returns></returns>
    public static bool PeutPlacerTuile(Plateau plateau, Joueur joueur, Tuile tuile)
    {
        if (tuile == null || plateau?.TuilesPlacees == null) return false;

        // Récupérer les positions des tuiles déjà placées
        var tuilesPlacees = plateau.TuilesPlacees;

        // Détermine les bornes dynamiques de la grille (min et max X et Y)
        int minX = tuilesPlacees.Min(t => t.PositionX);
        int maxX = tuilesPlacees.Max(t => t.PositionX);
        int minY = tuilesPlacees.Min(t => t.PositionY);
        int maxY = tuilesPlacees.Max(t => t.PositionY);

        // Calcule la taille actuelle de la grille
        int largeurGrille = maxX - minX + 1;
        int hauteurGrille = maxY - minY + 1;

        // Vérifie que la tuile peut être placée dans une grille 6x6
        bool estDansLaGrille = Math.Max(tuile.PositionX, maxX) - Math.Min(tuile.PositionX, minX) < 6
                            && Math.Max(tuile.PositionY, maxY) - Math.Min(tuile.PositionY, minY) < 6
                            && tuile.PositionX > -6 && tuile.PositionX < 6
                            && tuile.PositionY > -6 && tuile.PositionX < 6;

        // Vérifie si la tuile est adjacente à une tuile existante
        bool estAdjacent = tuilesPlacees.Any(t =>
            Math.Abs(t.PositionX - tuile.PositionX) <= 1 &&
            Math.Abs(t.PositionY - tuile.PositionY) <= 1);

        // Vérifie s'il existe déjà une tuile à cet emplacement
        Tuile tuileExistante = tuilesPlacees
            .FirstOrDefault(t => t.PositionX == tuile.PositionX && t.PositionY == tuile.PositionY);

        // Vérifie si la tuile placée est plus forte que celle existante (ou s'il n'y a pas de tuile)
        bool estPlusForteSiPoseeSurTuileExistante = tuileExistante == null || tuileExistante.Valeur < tuile.Valeur;

        // Vérifie si toutes les conditions de placement sont remplies
        var coupAutorise = joueur != null                       // Etre un joueur de la partie
            && joueur.TuilesDansLaMain.Contains(tuile.Valeur)   // Jouer une tuile de sa main
            && estAdjacent                                      // La tuile doit être adjacente à une tuile existante
            && estPlusForteSiPoseeSurTuileExistante             // La tuile doit être plus puissante si posée sur une tuile existante
            && 1 <= tuile.Valeur && tuile.Valeur <= 9           // La valeur de la tuile doit être comprise entre 1 et 9
            && estDansLaGrille;                                 // La tuile doit être placée dans une grille 6x6

        return coupAutorise;
    }

    /// <summary>
    /// Vérifie si le joueur a aligné 4 tuiles horizontalement, verticalement ou en diagonale
    /// </summary>
    /// <param name="joueur"></param>
    /// <returns></returns>
    public static bool VerifierConditionsVictoire(Plateau plateau, Joueur joueur, int tuilesAligneesPourGagner = 4)
    {
        // Récupère les tuiles du joueur
        var tuilesJoueur = plateau.TuilesPlacees
                            .Where(t => t.Proprietaire.Nom == joueur.Nom)
                            .ToList();

        // Parcourir chaque tuile du joueur pour vérifier les alignements
        foreach (var tuile in tuilesJoueur)
        {
            // Vérification horizontale
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, 0, tuilesAligneesPourGagner)) return true;

            // Vérification verticale
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 0, 1, tuilesAligneesPourGagner)) return true;

            // Vérification diagonale gauche-droite (bas-droite)
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, 1, tuilesAligneesPourGagner)) return true;

            // Vérification diagonale droite-gauche (bas-gauche)
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, -1, tuilesAligneesPourGagner)) return true;
        }

        return false; // Aucun alignement trouvé
    }

    /// <summary>
    /// Cette méthode vérifie si 4 tuiles sont alignées dans une direction spécifique
    /// </summary>
    /// <param name="tuilesJoueur"></param>
    /// <param name="tuile"></param>
    /// <param name="deltaX"></param>
    /// <param name="deltaY"></param>
    /// <returns></returns>
    public static bool VerifierAlignementDirection(List<Tuile> tuilesJoueur, Tuile tuile, int deltaX, int deltaY, int tuilesAligneesPourGagner = 4)
    {
        int count = 1; // Compte la tuile actuelle

        // Vérifie dans la direction positive (droite/bas)
        for (int i = 1; i < tuilesAligneesPourGagner; i++)
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
        for (int i = 1; i < tuilesAligneesPourGagner; i++)
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
        return count >= tuilesAligneesPourGagner;
    }
}
