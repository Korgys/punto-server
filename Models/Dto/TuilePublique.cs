namespace punto_server.Models.Dto;

public class TuilePublique
{
    public int Valeur { get; set; }
    public JoueurPublique Proprietaire { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
}
