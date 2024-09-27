namespace punto_server.Models;

public class Tuile
{
    public int Valeur { get; set; }
    public Joueur Proprietaire { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
}
