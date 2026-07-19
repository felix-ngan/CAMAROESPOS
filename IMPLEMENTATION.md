# Détails d'Implémentation Technique

Ce document trace les choix techniques majeurs de l'architecture moderne de CAMAROES POS.

## 1. Stack Technologique (Refactoring 2026)
- **UI Framework** : Windows Presentation Foundation (WPF) sous .NET 8.
- **UI Design System** : `MaterialDesignThemes` (MaterialDesignInXamlToolkit) pour apporter les codes esthétiques modernes (Ombres, Animations, Dark Mode).
- **Architecture Logique** : Modèle MVVM (Model-View-ViewModel) associé au pattern "Clean Architecture".
- **ORM** : Entity Framework Core (EF Core)
- **Base de Données Locale** : SQLite (via `Microsoft.EntityFrameworkCore.Sqlite`) remplace SQL Server Express pour le mode autonome (Offline-first) de chaque caisse.

## 2. Implémentation du Domain
La couche Domain contient les entités de base (POCO classes) de l'application : `Product`, `Category`, `CartItem`, `CustomerWallet`, etc. Ces classes ne dépendent d'aucune technologie d'interface ni d'aucun framework de base de données (Zero dependency).

## 3. Implémentation de la Couche Data
Un unique `AppDbContext` dérive de `DbContext` d'EF Core. Il configure la création du fichier local `camaroes_pos.db` et map les entités du Domain vers les tables relationnelles SQLite via la *Fluent API* ou des Data Annotations.
