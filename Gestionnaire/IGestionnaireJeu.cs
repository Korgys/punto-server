using punto_server.Models;

namespace punto_server.Services;

public interface IGestionnaireJeu
{
    Jeu ObtenirJeu();
    void DemarrerUnJeu();
    void RejoindrePartie(string nomDuJoueur, string identifiant);
    bool PeutJouerTuile(string joueur, int x, int y, int valeur);
    Jeu JouerTuile(string nomDuJoueur, int x, int y, int valeur);
    void GererDeconnexion(string idJoueur);
}