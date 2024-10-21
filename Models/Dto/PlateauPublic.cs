namespace punto_server.Models.Dto;

public class PlateauPublic
{
    public List<TuilePublique> TuilesPlacees { get; set; } = new List<TuilePublique>();

    public static PlateauPublic Convertir(Plateau plateau)
    {
        PlateauPublic plateauPublic = new PlateauPublic();
        plateauPublic.TuilesPlacees = new List<TuilePublique>();

        foreach (var tuile in plateau.TuilesPlacees)
        {
            plateauPublic.TuilesPlacees.Add(new TuilePublique
            {
                PositionX = tuile.PositionX,
                PositionY = tuile.PositionY,
                Valeur = tuile.Valeur,
                Proprietaire = new JoueurPublique()
                {
                    Nom = tuile.Proprietaire.Nom
                }
            });
        }

        return plateauPublic;
    }
}
