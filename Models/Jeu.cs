namespace punto_server.Models;

public class Jeu
{
    public int NombreMaxDeJoueurs { get; } = 2; // 2 joueurs max
    public EtatJeu EtatJeu { get; set; } = EtatJeu.EnAttente; // Au début, le jeu est en attente de joueurs
    public Joueur AuTourDuJoueur { get; set; }
    public Joueur Vainqueur { get; set; }
    public Plateau Plateau { get; set; } = new Plateau();
    public List<Equipe> Equipes { get; set; } = new List<Equipe>();
}
