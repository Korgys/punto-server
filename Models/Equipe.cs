namespace punto_server.Models;

public class Equipe
{
    public string Nom { get; set; }
    public int Ordre { get; set; }
    public List<Joueur> Joueur { get; set; }
}
