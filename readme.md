# Documentation serveur

## Règles du jeu Punto :

- But : aligner 4 de ses tuiles sur le plateau.
- Chaque joueur a une pioche de 18 tuiles allant de 1 à 9.
- Chaque joueur a 2 tuiles en main.
- Le jeu se joue en tour par tour.
- Au début de son tour, le joueur a deux options : juxtaposer une tuile sur une tuile existante (en haut/bas, à droite/gauche, en diagonale) ou alors superposer sa tuile sur une tuile existante (à condition que sa tuile soit strictement supérieure).
- Toutes les tuiles doivent être placées dans une grille de 6x6.
- Dès que le joueur aligne 4 de ses tuiles, il remporte la partie.
- Si un joueur arrive à court de tuile, c'est le joueur ayant le plus de séries de 3 tuiles consécutives qui gagne la partie. En cas, d'égalité, c'est celui avec la somme la plus petite de série de 3 qui gagne.

## Démarrer le serveur 

### via Dotnet

Prérequis :
- .NET Framework installé.

Compile le projet : 
```bash
dotnet build
```

Démarre le serveur :
```bash
dotnet run
```

### via Docker

Prérequis :
- Docker installé.
- Service Docker démarré.

Construit l'image docker :
```bash
docker build -t punto-server .
```

Lance l'image docker :
```bash
docker run -p 8080:8080 -p 8081:8081 punto-server
```

## Utilisation

Par défaut le serveur démarre à l'adresse suivante : 
http://localhost:5000/punto

