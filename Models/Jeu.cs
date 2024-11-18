namespace punto_server.Models;

public class Jeu
{
    public Jeu() { }
    public Jeu(int nombreJoueursMax) 
    {
        NombreMaxDeJoueurs = nombreJoueursMax;
    }

    public int NombreMaxDeJoueurs { get; } = 2; // 2 joueurs max par défaut
    public EtatJeu EtatJeu { get; set; } = EtatJeu.EnAttente; // Au début, le jeu est en attente de joueurs
    public Joueur AuTourDuJoueur { get; set; }
    public Joueur Vainqueur { get; set; }
    public Plateau Plateau { get; set; } = new Plateau();
    public List<Joueur> Joueurs { get; set; } = new List<Joueur>();
}
