# Design : Migration de l'interface CAMAROES POS vers Avalonia UI (Approche A)

**Date** : 2026-11-19  
**Branche de travail** : `feat/avalonia-ui`  
**Sauvegarde WPF d'origine** : branche `backup-wpf-windows` et tag `v1.0-wpf-windows` (commit `a36dd82`, inchangés)

---

## 1. Intention et Pourquoi

CAMAROES POS a été développé initialement sur Windows avec **WPF** (`net8.0-windows`). Le développeur travaille désormais sur **macOS** et souhaite pouvoir compiler, lancer et déployer l'application nativement sur Mac (tout en restant déployable sur Windows et Linux), sans perdre ni altérer la version WPF existante.

L'architecture actuelle en 3 couches (`CamaroesPOS.Domain`, `CamaroesPOS.Data`, `CamaroesPOS.UI`) permet d'ajouter une nouvelle interface graphique multiplateforme en **Avalonia UI** qui réutilise intégralement les couches métier (`Domain`) et données (`Data` / SQLite / EF Core).

---

## 2. Objectifs, Non-objectifs et Anti-objectifs

### Objectifs (Goals)
1. **Exécution et compilation natives sur macOS** (et compatibilité Windows/Linux) via `dotnet build` et `dotnet run` avec le SDK .NET 8 installé sur le Mac.
2. **Parité fonctionnelle complète** avec les 13 vues/fenêtres WPF existantes :
   - Authentification (`LoginWindow`) et gestion des rôles (`Admin` vs `Cashier`).
   - Navigation principale (`MainWindow`) avec bascule bilingue dynamique **FR / EN**.
   - Tableau de bord (`DashboardView`) avec indicateurs KPI, alertes de stock critique, recherche utilisateurs et graphiques interactifs.
   - Caisse / Point de Vente (`POSView`) avec scan code-barres, catalogue, sélection quantité, panier, réduction %, paiement Espèces (pavé numérique tactile + rendu monnaie) et paiement Portefeuille Client + points de fidélité.
   - Reçu de caisse (`ReceiptWindow`) avec export PDF (`QuestPDF`) et impression via ouverture du PDF généré.
   - Gestion des stocks (`InventoryView`) + gestion Marques & Catégories (`BrandCategoryWindow`).
   - Clients / CRM & Portefeuille (`WalletView`).
   - Fournisseurs (`VendorView`).
   - Rapports & Ventes (`ReportsView`) avec filtrage, annulation de commande (Admin) et export PDF (`QuestPDF`).
   - Dépenses & Charges (`ExpenseView`).
   - Paramètres (`SettingsView`) + changement de mot de passe (`ChangePasswordWindow`).
3. **Cohabitation propre des deux projets UI** : `CamaroesPOS.UI` (WPF) et `CamaroesPOS.UI.Avalonia` (Avalonia) coexistent dans la solution sans entraver le déploiement indépendant de l'un ou l'autre (`dotnet publish CamaroesPOS.UI.Avalonia` ou `dotnet publish CamaroesPOS.UI`).
4. **Reconstruction des ressources i18n FR/EN** : les fichiers `Strings.fr.xaml` / `Strings.en.xaml` étant absents du dépôt Git au commit `a36dd82`, reconstruire intégralement les dictionnaires `Strings.fr.axaml` et `Strings.en.axaml` à partir des textes présents avant le commit `a36dd82` (`git diff a36dd82~1 a36dd82`).
5. **Historique Git propre** : chaque étape de modification fait l'objet d'un commit dédié sur `feat/avalonia-ui`, et les commandes `git push` sont fournies à l'utilisateur.

### Non-objectifs (Non-goals)
- Ne pas réécrire ni refactoriser `CamaroesPOS.Domain` ou `CamaroesPOS.Data` (sauf incompatibilité avérée de compilation `net8.0`).
- Ne pas modifier `CamaroesPOS.UI` (WPF).
- Ne pas mettre en place de pipeline CI/CD ni d'installeurs `.dmg`/`.msi` dans cette phase (le déploiement se fera ultérieurement via `dotnet publish`).
- Ne pas ajouter de nouvelles fonctionnalités métier absentes de la version WPF.

### Anti-objectifs (Anti-goals)
- Casser la version WPF Windows existante ou écraser la sauvegarde `backup-wpf-windows`.
- Livrer une interface qui compile mais qui plante au démarrage ou manque des écrans clés (Caisse, Dashboard, Reçu PDF, Login).
- Laisser les développeurs macOS/Linux sans point d'entrée de compilation simple (une solution dédiée `CamaroesPOS_Avalonia.sln` sans WPF doit compiler et s'exécuter sans erreur).

---

## 3. Contraintes

- **Environnement local** : macOS (Apple Silicon / Homebrew présent), **SDK .NET 8 absent** à l'état initial → installation du **SDK .NET 8** (`dotnet-sdk8` / script officiel `dotnet-install.sh --channel 8.0`) pour correspondre exactement au `<TargetFramework>net8.0</TargetFramework>` des projets (SDK + runtime `net8.0`).
- **Format de solution cross-platform** : utilisation du format standard `CamaroesPOS_Avalonia.sln` (100 % compatible avec le SDK .NET 8 CLI, contrairement au format `.slnx` qui requiert .NET 9.0.200+). `CamaroesPOS_Modern.slnx` est mis à jour pour référencer les deux UI pour Visual Studio sous Windows. Sur macOS/Linux, les commandes standard sont `dotnet build CamaroesPOS_Avalonia.sln` et `dotnet run --project CamaroesPOS.UI.Avalonia`.
- **Base de données** : même fichier SQLite local (`PosDbContext`) et mêmes migrations SQL manuelles au démarrage + données de démonstration par défaut (`admin` / `123`, `caissier1` / `123`).
- **Git** :
  - `backup-wpf-windows` et `v1.0-wpf-windows` restent strictement figés sur `a36dd82`.
  - Chaque modification logique est commitée sur `feat/avalonia-ui`.
  - Ne pas pousser automatiquement sur `origin` sans action/validation de l'utilisateur ; fournir les commandes `git push` explicites.

---

## 4. Approches considérées

### Approche A (Retenue) : Projet `CamaroesPOS.UI.Avalonia` indépendant + `CamaroesPOS.UI` (WPF) intact
- Création de `CamaroesPOS.UI.Avalonia` (`net8.0`) référençant `CamaroesPOS.Domain` et `CamaroesPOS.Data`.
- Ajout de `CamaroesPOS.UI.Avalonia` dans `CamaroesPOS_Modern.slnx` **et** création d'une solution cross-platform `CamaroesPOS_Avalonia.sln` (contenant `Domain`, `Data`, `UI.Avalonia`) pour permettre un `dotnet build CamaroesPOS_Avalonia.sln` / `dotnet run --project CamaroesPOS.UI.Avalonia` propre sur macOS et Linux.
- **Avantages** : zéro risque de régression sur le projet WPF Windows (impossible à exécuter sur Mac), déploiement indépendant et propre.
- **Compromis** : duplication des ViewModels entre les deux projets UI.

### Approche B (Écartée) : Extraction d'un projet partagé `CamaroesPOS.Application`
- Mutualisation des ViewModels et de `PdfService` entre WPF et Avalonia.
- **Raison du rejet** : impose de modifier `CamaroesPOS.UI` (WPF) sans pouvoir tester son exécution graphique sur macOS.

---

## 5. Décisions techniques (Choix de conception)

### Choix actés avec l'utilisateur
- **Approche A** retenue (deux projets UI côte à côte dans la solution).
- **Branche de sauvegarde** préservée + commits atomiques par étape sur la branche de migration + commandes `git push` fournies.

### Choix proposés par l'assistant (à valider lors de la revue)
1. **Stack Avalonia 11 (`net8.0`)** :
   - `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`, `Avalonia.Controls.DataGrid`.
   - **Design System** : `FluentTheme` + `Material.Icons.Avalonia` (pour tous les `PackIcon`) + feuille de styles Avalonia reproduisant fidèlement la charte visuelle existante (`#F4F7FE` fond, cartes blanches arrondies bordées `#E0E5F2`, bleu primaire `#1E88E5`, texte marine `#2B3674`, rouge déconnexion `#E53935`). Ce choix est plus stable sur Avalonia 11 que `Material.Avalonia` (qui présente souvent des conflits de versions).
   - **Graphiques** : `LiveChartsCore.SkiaSharpView.Avalonia` en remplacement de `LiveCharts.Wpf` pour les séries du Dashboard (`PieSeries`, `LineSeries`, `ColumnSeries`, `RowSeries`).
   - **PDF** : `QuestPDF` (même version/API que le projet WPF, `PdfService.cs` porté à l'identique).
2. **Remplacement des API spécifiques WPF** :
   - `Application.Current.Properties["CurrentUser..."]` → `Services/SessionService.cs` (état de session utilisateur typé : `CurrentUser`, `CurrentUserName`, `CurrentUserRole`).
   - `MessageBox.Show(...)` → `Services/DialogService.cs` (fenêtre modale Avalonia pour Info, Avertissement, Erreur, Confirmation Oui/Non).
   - `Microsoft.Win32.SaveFileDialog` → `TopLevel.StorageProvider.SaveFilePickerAsync(...)`.
   - `PrintDialog` dans `ReceiptWindow` → génération du reçu en PDF temporaire via `PdfService` puis ouverture avec le lecteur système (`open` sur macOS, `UseShellExecute = true` sur Windows, `xdg-open` sur Linux) permettant l'impression native multiplateforme.
   - `CommandManager.RequerySuggested` dans `RelayCommand` → `RelayCommand<T>` avec méthode explicite `RaiseCanExecuteChanged()`.
3. **Internationalisation (FR/EN — parité XAML avec `a36dd82`)** :
   - Reconstitution de `Resources/Langs/Strings.fr.axaml` (à partir du diff Git `a36dd82~1..a36dd82`) et création de `Resources/Langs/Strings.en.axaml` avec les traductions anglaises correspondantes pour toutes les clés `{DynamicResource Str_...}` des vues XAML.
   - Bascule dynamique dans `MainWindow` via remplacement du dictionnaire fusionné dans `Application.Current.Resources.MergedDictionaries`.
   - Parité stricte avec la version WPF : les messages C# des dialogues (`DialogService`), les en-têtes code-behind et `PdfService.cs` restent en français comme dans la version d'origine.

---

## 6. Ce qui reste à la discrétion de l'implémenteur

- Les numéros de version exacts (patch) des packages NuGet Avalonia 11 / `LiveChartsCore.SkiaSharpView.Avalonia` / `Material.Icons.Avalonia` compatibles avec `.NET 8.0`.
- L'ajustement fin des marges, hauteurs et tailles de police dans les vues `.axaml` pour un rendu propre sur écran Retina macOS et Windows.
- L'organisation interne des convertisseurs XAML (`IValueConverter` pour `BoolToVisibility`, inversion booléenne, etc.) et des dialogues modaux internes (overlays dans `POSView`, `InventoryView`, etc.).
