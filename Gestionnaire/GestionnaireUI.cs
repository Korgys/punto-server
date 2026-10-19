using Microsoft.AspNetCore.StaticFiles;
using punto_server.Models;
using Raylib_cs;

namespace punto_server.Services;

class GestionnaireUI
{
    public static void AfficherEcranDeJeu(Jeu jeu, int boardSize, int logWidth)
    {
        AfficherPlateau(jeu.Plateau, boardSize, logWidth);
        AfficherInfosJoueurs(jeu, boardSize, logWidth);
    }

    private static Color ObtenirCouleurJoueur(int ordreDeJeu)
    {
        // On définit la couleur en fonction de l'ordre de jeu
        Color color = ordreDeJeu switch
        {
            1 => Color.DarkBlue,
            2 => Color.DarkGreen,
            _ => Color.Gold,
        };

        return color;
    }

    private static void AfficherPlateau(Plateau plateau, int boardSize, int logWidth)
    {
        int cellSize = boardSize / 12;
        int delta = cellSize * 6;

        // On dessine d'abord les tuiles
        foreach (Tuile tuile in plateau.TuilesPlacees)
        {
            Color color = ObtenirCouleurJoueur(tuile.Proprietaire.OrdreDeJeu);

            // On dessine la tuile
            Raylib.DrawRectangle(
                tuile.PositionX * cellSize + delta,
                tuile.PositionY * cellSize + delta,
                cellSize,
                cellSize,
                color
            );
            Raylib.DrawRectangleLines(
                tuile.PositionX * cellSize + delta,
                tuile.PositionY * cellSize + delta,
                cellSize,
                cellSize,
                Color.Black
            );

            // On inscirt la valeur au centre de la tuile
            EcrireAuCentre(
                tuile.Valeur.ToString(),
                20,
                cellSize,
                cellSize,
                tuile.PositionX * cellSize + delta,
                tuile.PositionY * cellSize + delta
            );
        }
    }

    public static void AfficherInfosJoueurs(Jeu jeu, int boardSize, int logWidth)
    {
        int containerHeight = 100;
        int containerX = boardSize;

        foreach (var joueur in jeu.Joueurs)
        {
            int containerY = (joueur.OrdreDeJeu - 1) * containerHeight;
            AfficherNomDuJoueur(joueur.Nom, logWidth, containerHeight / 2, containerX, containerY);
            AfficherCarteJoueur(
                joueur,
                logWidth,
                containerHeight / 2,
                containerX,
                containerY + containerHeight / 2
            );
        }
    }

    private static void AfficherNomDuJoueur(
        string nomDuJoueur,
        int containerWidth,
        int containerHeight,
        int containerX,
        int containerY
    )
    {
        EcrireAuCentre(nomDuJoueur, 20, containerWidth, containerHeight, containerX, containerY);
    }

    private static void AfficherCarteJoueur(
        Joueur joueur,
        int containerWidth,
        int containerHeight,
        int containerX,
        int containerY
    )
    {
        int nombreDeSections = 5; // 2 cartes et 3 espcaces #T#T#

        int cardHeight = containerHeight;
        int cardWidth = containerWidth / nombreDeSections;
        Color color = ObtenirCouleurJoueur(joueur.OrdreDeJeu);

        for (int i = 0; i < 5; i++)
        {
            int? valeurCarte = null;
            try
            {
                valeurCarte = joueur.TuilesDansLaMain[i];
            }
            catch { }

            if (valeurCarte != null)
            {
                int cardContainerX = containerX + cardWidth + (i * cardWidth * 2);

                Raylib.DrawRectangle(cardContainerX, containerY, cardWidth, cardHeight, color);
                Raylib.DrawRectangleLines(
                    cardContainerX,
                    containerY,
                    cardWidth,
                    cardHeight,
                    Color.Black
                );

                EcrireAuCentre(
                    valeurCarte.ToString() ?? "",
                    20,
                    cardWidth,
                    cardHeight,
                    cardContainerX,
                    containerY
                );
            }
        }
    }

    public static void AfficherEcranAccueil(int boardSize, int logWidth)
    {
        string text = "Le serveur est démarré";
        int fontSize = 20;

        EcrireAuCentre(text, fontSize, boardSize + logWidth, boardSize);
    }

    public static void AfficherEcranAttente(int boardSize, int logWidth)
    {
        string text = "Attente de joueurs";
        int fontSize = 20;

        EcrireAuCentre(text, fontSize, boardSize + logWidth, boardSize);
    }

    public static void AfficherEcranFinJeu(
        Plateau plateau,
        Joueur vainqueur,
        int boardSize,
        int logWidth
    )
    {
        string text = $"Le vainqueur est le joueur {vainqueur?.Nom}";

        int fontSize = 100;
        // On dépasse volontaire pour rentrer dans la boucle
        System.Numerics.Vector2 tailleTexteRendu = new(boardSize + 1, 0);

        // On calcule la taille du texte pour remplir la fenêtre
        while (tailleTexteRendu.X > boardSize)
        {
            fontSize--;
            tailleTexteRendu = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text, fontSize, 0);
        }

        AfficherPlateau(plateau, boardSize, logWidth);

        // On dessine un rectangle blanc sur lequel le texte sera affiché
        Raylib.DrawRectangle(
            0,
            (boardSize / 2) - (int)(tailleTexteRendu.Y / 2),
            boardSize + logWidth,
            (int)tailleTexteRendu.Y,
            Color.White
        );
        EcrireAuCentre(text, fontSize, boardSize + logWidth / 2, boardSize);
    }

    private static void EcrireAuCentre(
        string text,
        int fontSize,
        int containerWidth,
        int containerHeight,
        int x = 0,
        int y = 0,
        Color? color = null
    )
    {
        Tuple<int, int> position = _ObtenirPositionCentre(
            text,
            fontSize,
            containerWidth / 2,
            containerHeight / 2
        );

        Raylib.DrawText(
            text,
            x + position.Item1,
            y + position.Item2,
            fontSize,
            color ?? Color.Black
        );
    }

    private static Tuple<int, int> _ObtenirPositionCentre(string text, int fontSize, int x, int y)
    {
        // Mesure la taille du texte avec un espacement de 0 entre les lettres
        System.Numerics.Vector2 textSize = Raylib.MeasureTextEx(
            Raylib.GetFontDefault(),
            text,
            fontSize,
            0
        );

        // Calcule la position pour que le texte soit centré
        int centeredX = (int)(x - textSize.X / 2);
        int centeredY = (int)(y - textSize.Y / 2);

        return new Tuple<int, int>(centeredX, centeredY);
    }
}
