# TestBlazor - Guide du Projet

Ce projet est une application de démonstration construite avec **Blazor Server** (.NET 8). Il illustre plusieurs concepts clés du développement d'applications web modernes avec C#.

## Structure du Projet

- **Components/** : Contient tous les composants UI (.razor).
    - `Pages/` : Les pages routables de l'application (ex: `/supervision`, `/todo`).
    - `Layout/` : Les mises en page partagées (ex: barre de navigation).
    - `Symbols/` : Composants graphiques métier (Pompes, Vannes, etc.).
- **Services/** : La logique métier et la gestion d'état (ex: `ToDoService`, `CompassService`).
- **Models/** : Les classes de données (ex: `ToDoItem`).
- **Data/** : Le contexte de base de données (Entity Framework Core).
- **wwwroot/** : Fichiers statiques (CSS, XML, Images).

## Concepts Clés de Blazor

### 1. Composants (Components)
Tout est composant dans Blazor. Un composant est une unité autonome d'interface utilisateur et de logique.
- **Paramètres (`[Parameter]`)** : Permettent de passer des données d'un parent à un enfant.
  - Exemple : `<Pump Size="150" />` envoie la valeur 150 à la propriété `Size` du composant `Pump`.
- **Composition** : Nous utilisons un `SymbolWrapper` pour encapsuler la logique commune (sélection, rotation, perte de com) et l'appliquer à différents symboles (Pompe, Vanne, Ventilateur).
- **RenderFragments** : Permettent d'injecter du contenu HTML arbitraire dans un composant. Nous l'utilisons pour passer l'icône SVG spécifique au `SymbolWrapper`.

### 2. Gestion d'État (State Management)
La gestion de l'état définit comment les données sont stockées et partagées.
- **Singleton (`AddSingleton`)** : Une seule instance du service est créée et partagée par **tous les utilisateurs**.
  - *Exemple* : `CompassService`. Si un utilisateur change le cap de la boussole, cela change pour tout le monde en temps réel.
- **Scoped (`AddScoped`)** : Une instance est créée par **connexion utilisateur** (session).
  - *Exemple* : `ToDoService`. Chaque utilisateur a sa propre vue, bien que dans notre démo simple, l'accès Fichier/BDD soit partagé, le service lui-même est instancié par connexion.

### 3. Interactivité et Threading
Blazor Server s'exécute sur le serveur et communique avec le navigateur via SignalR.
- **`@rendermode InteractiveServer`** : Active l'interactivité (événements clics, mises à jour dynamiques) sur une page ou un composant.
- **InvokeAsync** : Lorsqu'un événement se produit sur un thread différent (comme notre `CompassService` Singleton notifiant d'un changement), nous devons utiliser `InvokeAsync(StateHasChanged)` pour demander poliment au thread de l'interface utilisateur (Dispatcher) de rafraîchir l'affichage. Sans cela, une erreur de threading survient.

### 4. Accès aux Données (Entity Framework Core)
Nous utilisons EF Core avec SQLite pour persister les données.
- **DbContext** : `ToDoDbContext` gère la connexion à la base de données.
- **Migrations/Création** : Nous utilisons `EnsureCreated()` au démarrage pour générer automatiquement la base de données `todo_archive.db` si elle n'existe pas.
- **Asynchronisme (`async/await`)** : Toutes les opérations I/O (lecture fichier, accès BDD) sont asynchrones pour ne pas geler le serveur pendant le traitement.

## Fonctionnalités Implémentées

### Supervision
Une page graphique montrant des symboles interactifs.
- Clic pour sélectionner.
- Boutons pour activer/désactiver, simuler une perte de communication, ou afficher un overlay.
- Utilisation intensive de CSS dynamique pour la rotation et le redimensionnement.

### ToDo List
Un gestionnaire de tâches complet.
- **Templates** : Stockés dans un fichier XML (`wwwroot/data/todolists.xml`). Permet de créer des modèles réutilisables.
- **Listes Actives** : Instanciation d'un template pour exécution.
- **Historique** : Archivage des listes terminées dans une base SQLite locale.

### Boussole (Compass)
Démonstration d'un état global temps-réel.
- Le cap est stocké dans un service Singleton.
- Modifiable depuis la page `/counter`.
- Visible en temps réel sur la page `/supervision`.
- Utilise des événements C# (`Action OnChange`) pour notifier les composants des mises à jour.
