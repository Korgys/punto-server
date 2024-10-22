namespace punto_server.Models.Dto;

public class JoueurPublique
{
    public string Nom { get; set; }
    public int OrdreDeJeu { get; set; }
    public int Penalite { get; set; } = 0;
    public List<int> TuilesDansLaMain { get; set; } = new List<int>();

    public static JoueurPublique Convertir(Joueur joueur)
    {
        return new JoueurPublique()
        {
            Nom = joueur.Nom,
            OrdreDeJeu = joueur.OrdreDeJeu,
            Penalite = joueur.Penalite,
            TuilesDansLaMain = joueur.TuilesDansLaMain
        };
    }
}
