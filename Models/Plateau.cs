
namespace punto_server.Models;

public class Plateau
{
    public List<Tuile> TuilesPlacees { get; set; } = new List<Tuile>();

    public List<Tuile> ObtenirTuilesPlaceesSansDetails()
    {
        // Copie la liste des tuiles placées
        List<Tuile> tuilesPlaceesSansDetails = new List<Tuile>(TuilesPlacees);

        // Enlève les infos secrètes (tuiles dans la pioche, tuiles dans la main des adversaires, etc).
        foreach (Tuile tuile in tuilesPlaceesSansDetails)
        {
            tuile.Proprietaire.TuilesDansLeJeu = null;
            tuile.Proprietaire.TuilesDansLaMain = null;
        }

        return tuilesPlaceesSansDetails;
    }
}
