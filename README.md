<h1 align="center">CAMAROES POS - Modern Edition</h1>

<p align="center">
  <strong>Système de Point de Vente (POS) moderne, élégant et complet</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Plateforme-Windows-blue.svg" alt="Platform">
  <img src="https://img.shields.io/badge/Language-C%23-239120.svg" alt="C#">
  <img src="https://img.shields.io/badge/Framework-.NET%208.0%20WPF-512BD4.svg" alt=".NET WPF">
  <img src="https://img.shields.io/badge/Architecture-MVVM-FF4081.svg" alt="MVVM">
  <img src="https://img.shields.io/badge/Database-SQLite-003B57.svg" alt="SQLite">
</p>

---

## 📖 Description

**CAMAROES POS** a été entièrement repensé et réécrit pour offrir une expérience utilisateur de premier ordre. Cette nouvelle version abandonne Windows Forms et SQL Server au profit d'une architecture **WPF moderne** (basée sur le pattern MVVM) et d'une base de données locale ultra-rapide **SQLite** via **Entity Framework Core**.

L'interface graphique s'appuie sur `MaterialDesignInXamlToolkit` pour un design épuré, fluide et professionnel, digne des meilleurs standards de l'industrie. Les reçus et les rapports sont générés dynamiquement en PDF de haute qualité grâce à `QuestPDF`, et les tableaux de bord interactifs prennent vie grâce à `LiveCharts`.

## ✨ Fonctionnalités Principales

*   🛍️ **Caisse (POS) Rapide et Intuitive** : Gestion de panier, recherche instantanée par code-barres ou description, application de réductions (en pourcentage calculé automatiquement), et encaissements multi-moyens de paiement (Espèces, Mobile Money).
*   📦 **Gestion Avancée des Stocks** : Inventaire complet, ajustements de stocks tracés (Entrées, Sorties, Ajustements d'inventaire), et gestion centralisée des Marques et Catégories.
*   📊 **Tableau de Bord Interactif** : Vision globale de la santé financière. Graphiques d'évolution des ventes, mouvements de stocks sur 7 jours, calcul du bénéfice net (Revenus - Coûts d'achat - Dépenses), et cartes dynamiques du volume de stock.
*   👥 **CRM & Fidélité (Portefeuilles Clients)** : Gestion des clients, suivi des achats, accumulation de points de fidélité et paiements directs par portefeuille virtuel.
*   🧾 **Génération de PDF Professionnels** : Reçus de caisse détaillés et rapports des ventes (quotidiens, hebdomadaires, mensuels) exportables en PDF.
*   🔒 **Sécurité et Rôles** : Accès différencié (Administrateur / Caissier) protégeant les opérations sensibles comme l'ajustement des stocks, la gestion des utilisateurs ou l'annulation de commandes.
*   🔍 **Recherche Globale Intelligente** : Recherche instantanée et filtrage en temps réel dans les listes de produits, fournisseurs et utilisateurs.
*   🌍 **Bilingue (Français / Anglais)** : Basculez l'interface instantanément d'une langue à l'autre grâce à un système dynamique de ressources XAML (i18n), idéal pour le contexte bilingue du Cameroun.

---

## 🛠 Stack Technique (Technologies Utilisées)

Ce projet est une vitrine de l'écosystème .NET moderne pour le développement Desktop Windows :

*   **Langage** : C# 12
*   **Framework** : .NET 8.0 (Windows Presentation Foundation - WPF)
*   **Architecture** : MVVM (Model-View-ViewModel) garantissant un code découplé et testable.
*   **ORM (Database)** : Entity Framework Core 8.0 couplé à SQLite (aucune installation de serveur requise).
*   **UI & Design** : Material Design In XAML Toolkit pour les contrôles et les icônes.
*   **Graphiques** : LiveCharts.Wpf pour la Data Visualization interactive.
*   **Génération PDF** : QuestPDF (génération fluide basée sur un modèle de mise en page moderne).

---

## 🚀 Guide d'Installation et Lancement Rapide

Grâce à SQLite, l'installation est d'une simplicité déconcertante. Aucun serveur de base de données complexe n'est requis.

### Prérequis
*   [SDK .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) ou Visual Studio 2022.

### Compilation et Lancement
1. Clonez ce dépôt :
   ```bash
   git clone https://github.com/felix-ngan/CAMAROESPOS.git
   ```
2. Placez-vous dans le répertoire du projet :
   ```bash
   cd "CAMAROESPOS - ASP/CamaroesPOS.UI"
   ```
3. Compilez et exécutez le projet :
   ```bash
   dotnet run
   ```
*Note : Au premier lancement, la base de données SQLite (`camaroespos.db`) sera générée automatiquement dans le dossier d'exécution et pré-remplie avec un compte Administrateur par défaut (voir les instructions dans l'application ou initialisez votre propre compte).*

---

## 📸 Aperçu du Code (Pour les Développeurs)

La structure du projet respecte les principes de séparation des responsabilités :
- `CamaroesPOS.Domain` : Contient les entités métiers (`Product`, `SoldItem`, `CustomerWallet`, etc.).
- `CamaroesPOS.Data` : Contexte Entity Framework (`PosDbContext`).
- `CamaroesPOS.UI` : L'application principale contenant les `Views` (Fichiers XAML) et les `ViewModels` (Logique de présentation C#).
- `CamaroesPOS.UI/Services/PdfService.cs` : Service dédié à la génération complexe de documents PDF.

L'utilisation généralisée de `INotifyPropertyChanged` et de `RelayCommand` assure un binding de données (Data Binding) fluide et réactif entre le C# et le XAML.

---

<p align="center">
  <i>Développé avec passion pour illustrer la puissance du C# moderne dans la conception d'applications Desktop professionnelles.</i>
</p>
