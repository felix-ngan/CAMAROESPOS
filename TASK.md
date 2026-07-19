# Plan de Refactoring : CAMAROES POS Moderne

## Objectif Principal
Transformer l'ancienne application Windows Forms "CAMAROES POS" en une application de bureau moderne, esthétique et performante, capable de rivaliser avec les leaders du marché.

## Objectifs Secondaires
1. **Design** : Utiliser WPF et Material Design pour une interface fluide, animée et "Touch-Friendly".
2. **Architecture** : Mettre en place le modèle MVVM (Model-View-ViewModel) et une architecture propre (Clean Architecture) répartie en trois couches : `Domain`, `Data`, `UI`.
3. **Base de données** : Remplacer l'instance lourde de SQL Server Express par une base de données embarquée légère **SQLite** avec Entity Framework Core.
4. **Historique Git** : Réaliser des commits réguliers, clairs et documentés en français pour chaque petite étape validée.

## Suivi des Phases
- [x] **Phase 1** : Création de la nouvelle structure de projet (Projets Domain, Data, UI).
- [x] **Phase 2** : Migration du modèle de données (Entités) et configuration de SQLite.
- [x] **Phase 3** : Création de l'interface principale (Dashboard) avec Material Design.
- [x] **Phase 4** : Refonte de la Caisse (Point of Sale) et logique du Panier.
- [ ] **Phase 5** : Refonte de la gestion des stocks et du Portefeuille Client (Customer Wallet).
- [ ] **Phase 6** : Mise en place du nouveau système de synchronisation et finitions.
