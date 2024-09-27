namespace punto_server.Models;

public class Joueur
{
    public int OrdreDeJeu { get; set; }
    public string Nom { get; set; }
    public List<int> TuilesDansLaMain { get; set; } = new List<int>();
    public List<int> TuilesDansLeJeu { get; set; } = CreerTuilesPourJoueur();
    public int Penalite { get; set; } = 0;

    public static List<int> CreerTuilesPourJoueur()
    {
        // Mélange des tuiles
        var tuiles = new List<int>
        {
            1, 1,
            2, 2,
            3, 3,
            4, 4,
            5, 5,
            6, 6,
            7, 7,
            8, 8,
            9, 9
        };

        // Mélange des tuiles pour plus d'aléatoire
        var random = new Random();
        return tuiles.OrderBy(t => random.Next()).ToList();
    }
}
