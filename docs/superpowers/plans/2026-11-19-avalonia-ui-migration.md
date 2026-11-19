# Migration Avalonia UI (Approche A) — Plan d'Implémentation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Créer un projet `CamaroesPOS.UI.Avalonia` (`net8.0`, Avalonia 11) cohabitant avec `CamaroesPOS.UI` (WPF intact) afin de compiler, tester et exécuter CAMAROES POS nativement sur macOS (et Windows/Linux) avec parité fonctionnelle complète.

**Architecture:** Approche A — un projet UI Avalonia indépendant référençant les couches `CamaroesPOS.Domain` et `CamaroesPOS.Data` existantes (SQLite + EF Core), accompagné d'une solution cross-platform `CamaroesPOS_Avalonia.sln` (et d'un projet de tests `CamaroesPOS.Tests`) pour la compilation et les tests sur macOS/Linux, tout en ajoutant `CamaroesPOS.UI.Avalonia` dans `CamaroesPOS_Modern.slnx` pour Visual Studio Windows. Les API WPF (`MessageBox`, `SaveFileDialog`, `PrintDialog`, `CommandManager`, `Application.Current.Properties`) sont remplacées par des services dédiés (`DialogService`, `FilePickerService`, `SessionService`, `RelayCommand<T>.RaiseCanExecuteChanged()`).

**Tech Stack:** .NET 8.0 SDK, Avalonia UI 11 (`Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`, `Avalonia.Controls.DataGrid`), `Material.Icons.Avalonia`, `LiveChartsCore.SkiaSharpView.Avalonia`, `QuestPDF`, Entity Framework Core 8 + SQLite, xUnit.

**Spec:** [docs/superpowers/specs/2026-11-19-avalonia-ui-migration-design.md](file:///Users/felix/CAMAROESPOS/docs/superpowers/specs/2026-11-19-avalonia-ui-migration-design.md)

## Global Constraints

- **Branche Git** : tous les commits sont faits sur `feat/avalonia-ui` (un commit atomique par tâche) ; `backup-wpf-windows` et `v1.0-wpf-windows` restent strictement figés sur `a36dd82`.
- **Intégrité WPF** : ne modifier aucun fichier de `CamaroesPOS.UI/` (`git diff backup-wpf-windows -- CamaroesPOS.UI/` doit rester vide).
- **Couches Domain & Data** : ne pas modifier `CamaroesPOS.Domain/` ni `CamaroesPOS.Data/` sauf blocage de compilation `net8.0` avéré.
- **Target Framework** : `net8.0` avec SDK .NET 8 installé localement sur macOS ; solution cross-platform au format `.sln` (`CamaroesPOS_Avalonia.sln`).
- **Base de données** : utiliser `PosDbContext` existant (`LocalApplicationData/CamaroesPOS/camaroes_pos.db`), mêmes migrations SQL manuelles et mêmes comptes seed (`admin`/`123`, `caissier1`/`123`).
- **Design System** : `FluentTheme` + `Material.Icons.Avalonia` + styles personnalisés (`#F4F7FE` fond, cartes blanches `#FFFFFF` bordées `#E0E5F2`, primaire `#1E88E5`, marine `#2B3674`, danger `#E53935`).
- **i18n FR/EN** : parité stricte XAML avec `a36dd82` (`Strings.fr.axaml` reconstruit depuis `git diff a36dd82~1 a36dd82` + `Strings.en.axaml` avec 100 % des clés identiques ; dialogues C# et `PdfService.cs` en français).
- **Push Git** : ne jamais pousser automatiquement sur `origin` ; fournir les commandes `git push` explicites.

## Review Focus

1. **Clé i18n manquante ou désynchronisée entre FR et EN** : une clé `{DynamicResource Str_...}` absente d'un des deux dictionnaires provoque un texte vide ou une exception XAML lors de la bascule FR/EN → couvert par un test automatisé de parité des clés (Task 2).
2. **Base SQLite vide au premier lancement sur macOS** : `DashboardViewModel.LoadStats()` sur une base sans ventes/dépenses/fournisseurs ne doit lever aucune `NullReferenceException` ni erreur de série LiveChartsCore → couvert par un test unitaire (Task 4).
3. **Remise et paiement POS invalides** : remise hors `[0, 100]`, montant reçu insuffisant, ou solde portefeuille insuffisant doivent être bloqués proprement sans corrompre le panier ni la BD → couvert par des tests unitaires (Task 5).
4. **Rôle `Cashier` vs `Admin`** : un caissier connecté ne doit pas voir Dashboard/Paramètres/Dépenses et ne doit pas pouvoir supprimer/ajuster de stock ou annuler de vente → vérifié dans `SessionService` et ViewModels (Tasks 3, 6, 8).
5. **Export PDF & impression sur macOS** : `QuestPDF` doit générer un fichier PDF valide sans dépendance Windows (`PrintDialog` / `Microsoft.Win32`) → couvert par un test de génération PDF dans un fichier temporaire (Task 2).

---

## Carte des fichiers créés / modifiés

```text
CamaroesPOS_Avalonia.sln                                 (Créé — Domain, Data, UI.Avalonia, Tests)
CamaroesPOS_Modern.slnx                                  (Modifié — ajout CamaroesPOS.UI.Avalonia)
README.md                                                (Modifié — section macOS / Avalonia)

CamaroesPOS.UI.Avalonia/
├── CamaroesPOS.UI.Avalonia.csproj
├── Program.cs
├── App.axaml / App.axaml.cs
├── Styles/MaterialStyles.axaml
├── Resources/Langs/
│   ├── Strings.fr.axaml
│   └── Strings.en.axaml
├── Converters/CommonConverters.cs
├── Services/
│   ├── SessionService.cs
│   ├── DialogService.cs
│   ├── FilePickerService.cs
│   └── PdfService.cs
├── ViewModels/
│   ├── RelayCommand.cs
│   ├── LoginViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── POSViewModel.cs
│   ├── InventoryViewModel.cs
│   ├── BrandCategoryViewModel.cs
│   ├── WalletViewModel.cs
│   ├── VendorViewModel.cs
│   ├── ReportsViewModel.cs
│   ├── ExpenseViewModel.cs
│   └── SettingsViewModel.cs
└── Views/
    ├── MessageDialogWindow.axaml / .cs
    ├── LoginWindow.axaml / .cs
    ├── MainWindow.axaml / .cs
    ├── DashboardView.axaml / .cs
    ├── POSView.axaml / .cs
    ├── ReceiptWindow.axaml / .cs
    ├── InventoryView.axaml / .cs
    ├── BrandCategoryWindow.axaml / .cs
    ├── WalletView.axaml / .cs
    ├── VendorView.axaml / .cs
    ├── ReportsView.axaml / .cs
    ├── ExpenseView.axaml / .cs
    ├── SettingsView.axaml / .cs
    └── ChangePasswordWindow.axaml / .cs

CamaroesPOS.Tests/
├── CamaroesPOS.Tests.csproj
├── LocalizationTests.cs
├── ServicesTests.cs
├── DashboardViewModelTests.cs
└── POSViewModelTests.cs
```

---

### Task 1: Installation du SDK .NET 8 & Échafaudage `CamaroesPOS.UI.Avalonia`

**Files:**
- Create: `CamaroesPOS.UI.Avalonia/CamaroesPOS.UI.Avalonia.csproj`
- Create: `CamaroesPOS.UI.Avalonia/Program.cs`
- Create: `CamaroesPOS.UI.Avalonia/App.axaml`
- Create: `CamaroesPOS.UI.Avalonia/App.axaml.cs`
- Create: `CamaroesPOS_Avalonia.sln`
- Modify: `CamaroesPOS_Modern.slnx`

**Interfaces:**
- Consumes: `CamaroesPOS.Domain`, `CamaroesPOS.Data.PosDbContext`
- Produces: Squelette exécutable Avalonia 11 (`CamaroesPOS.UI.Avalonia`) avec initialisation SQLite (`DatabaseInitializer.Initialize()`) et solution compilable `CamaroesPOS_Avalonia.sln`.

- [ ] **Step 1: Installer le SDK .NET 8 sur macOS et vérifier le binaire `dotnet`**

Installer via Homebrew (`brew install --cask dotnet-sdk8` ou script officiel Microsoft `dotnet-install.sh --channel 8.0` dans `~/.dotnet`).  
Run: `dotnet --info`  
Expected: SDK `8.0.x` listé et exécutable sans erreur.

- [ ] **Step 2: Créer `CamaroesPOS.UI.Avalonia/CamaroesPOS.UI.Avalonia.csproj`**

Configurer `<OutputType>WinExe</OutputType>`, `<TargetFramework>net8.0</TargetFramework>`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<BuiltInComInteropSupport>true</BuiltInComInteropSupport>`, `<AvaloniaUseCompiledBindingsByDefault>false</AvaloniaUseCompiledBindingsByDefault>`, références projet vers `CamaroesPOS.Domain` et `CamaroesPOS.Data`, et packages NuGet (`Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`, `Avalonia.Controls.DataGrid`, `Material.Icons.Avalonia`, `LiveChartsCore.SkiaSharpView.Avalonia`, `QuestPDF` 2026.7.2 ou version stable compatible .NET 8).

- [ ] **Step 3: Créer `Program.cs`, `App.axaml` et `App.axaml.cs`**

- `Program.cs` : point d'entrée standard Avalonia (`BuildAvaloniaApp().StartWithClassicDesktopLifetime(args)`).
- `App.axaml` : charge `FluentTheme`, `MaterialIconStyles`, et `DataGrid` Fluent theme.
- `App.axaml.cs` : dans `OnFrameworkInitializationCompleted()`, configure `QuestPDF.Settings.License = LicenseType.Community`, exécute l'initialisation SQLite (`EnsureCreated()`, mêmes 21 requêtes de migration SQL que WPF, mêmes produits/portefeuilles/utilisateurs seed), puis ouvre une fenêtre minimale temporaire (remplacée par `LoginWindow` à la Task 3).

- [ ] **Step 4: Créer `CamaroesPOS_Avalonia.sln` et mettre à jour `CamaroesPOS_Modern.slnx`**

- Générer `CamaroesPOS_Avalonia.sln` via `dotnet new sln -n CamaroesPOS_Avalonia` + `dotnet sln CamaroesPOS_Avalonia.sln add CamaroesPOS.Domain CamaroesPOS.Data CamaroesPOS.UI.Avalonia`.
- Ajouter `<Project Path="CamaroesPOS.UI.Avalonia/CamaroesPOS.UI.Avalonia.csproj" />` dans `CamaroesPOS_Modern.slnx`.

- [ ] **Step 5: Vérifier la compilation sur macOS**

Run: `dotnet build CamaroesPOS_Avalonia.sln`  
Expected: `Build succeeded.` avec `0 Error(s)`.

- [ ] **Step 6: Commit**

```bash
git add CamaroesPOS.UI.Avalonia CamaroesPOS_Avalonia.sln CamaroesPOS_Modern.slnx
git commit -m "feat(avalonia): échafaudage du projet CamaroesPOS.UI.Avalonia et solution cross-platform"
```

---

### Task 2: Infrastructure transversale, Thème Material, i18n FR/EN & Suite de tests

**Files:**
- Create: `CamaroesPOS.UI.Avalonia/Services/SessionService.cs`
- Create: `CamaroesPOS.UI.Avalonia/Services/DialogService.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/MessageDialogWindow.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/MessageDialogWindow.axaml.cs`
- Create: `CamaroesPOS.UI.Avalonia/Services/FilePickerService.cs`
- Create: `CamaroesPOS.UI.Avalonia/Services/PdfService.cs`
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/RelayCommand.cs`
- Create: `CamaroesPOS.UI.Avalonia/Converters/CommonConverters.cs`
- Create: `CamaroesPOS.UI.Avalonia/Styles/MaterialStyles.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Resources/Langs/Strings.fr.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Resources/Langs/Strings.en.axaml`
- Modify: `CamaroesPOS.UI.Avalonia/App.axaml`
- Create: `CamaroesPOS.Tests/CamaroesPOS.Tests.csproj`
- Create: `CamaroesPOS.Tests/LocalizationTests.cs`
- Create: `CamaroesPOS.Tests/ServicesTests.cs`
- Modify: `CamaroesPOS_Avalonia.sln`

**Interfaces:**
- Consumes: `CamaroesPOS.Domain.Entities`, `QuestPDF`
- Produces:
  - `SessionService` : `CurrentUser` (`string?`), `CurrentUserName` (`string?`), `CurrentUserRole` (`string?`), `IsAdmin` (`bool`), `SetSession(User)`, `Clear()`.
  - `DialogService` : `ShowInfoAsync(string message, string title)`, `ShowWarningAsync(string message, string title)`, `ShowErrorAsync(string message, string title)`, `ConfirmAsync(string message, string title) -> Task<bool>`.
  - `FilePickerService` : `PickSavePdfPathAsync(string suggestedFileName, string title) -> Task<string?>`, `OpenFileWithDefaultApp(string path)`.
  - `PdfService` : `GenerateReceiptPdf(...)`, `GenerateSalesReportPdf(...)` (mêmes signatures que WPF).
  - `RelayCommand<T>` : `ICommand` avec `RaiseCanExecuteChanged()`.
  - `Strings.fr.axaml` / `Strings.en.axaml` : toutes les clés `Str_*` extraites de `git diff a36dd82~1 a36dd82`.

- [ ] **Step 1: Créer le projet de tests `CamaroesPOS.Tests` et écrire les tests qui échouent**

Ajouter `CamaroesPOS.Tests` (xUnit) à `CamaroesPOS_Avalonia.sln`, référençant `CamaroesPOS.UI.Avalonia`.
- `LocalizationTests.FrenchAndEnglishDictionariesHaveIdenticalKeys()` : parse `Strings.fr.axaml` et `Strings.en.axaml`, vérifie qu'aucun fichier n'est vide et que les ensembles de `x:Key` sont strictement égaux.
- `ServicesTests.SessionServiceSetsAndClearsUserRole()` : vérifie `IsAdmin == true` pour `"Admin"`, `false` pour `"Cashier"`, et `Clear()` remet tout à `null`.
- `ServicesTests.RelayCommandRaisesCanExecuteChanged()` : vérifie que `RaiseCanExecuteChanged()` déclenche bien `CanExecuteChanged`.
- `ServicesTests.PdfServiceGeneratesNonEmptyReceiptPdf()` : génère un reçu dans `Path.GetTempFileName() + ".pdf"` et vérifie que le fichier existe et commence par les octets `%PDF`.

- [ ] **Step 2: Exécuter les tests pour vérifier qu'ils échouent**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: FAIL (types/fichiers non encore créés).

- [ ] **Step 3: Implémenter les services, `RelayCommand<T>`, `CommonConverters` et `MaterialStyles.axaml`**

- `SessionService.cs` : propriétés statiques + `SetSession(User user)` + `Clear()`.
- `MessageDialogWindow.axaml(.cs)` + `DialogService.cs` : fenêtre modale centrée sur la fenêtre active (`IClassicDesktopStyleApplicationLifetime.MainWindow`) avec icône/couleur selon le type (Info/Warning/Error/Confirm).
- `FilePickerService.cs` : utilise `TopLevel.GetTopLevel(window)?.StorageProvider.SaveFilePickerAsync(...)` pour choisir le chemin PDF, et `Process.Start` (`open` sur macOS, `xdg-open` sur Linux, `UseShellExecute = true` sur Windows) pour ouvrir/imprimer un PDF.
- `PdfService.cs` : portage identique de `CamaroesPOS.UI/Services/PdfService.cs`.
- `RelayCommand.cs` & `CommonConverters.cs`.
- `Styles/MaterialStyles.axaml` : styles réutilisables (`Card`, `PrimaryButton`, `OutlinedButton`, `DangerOutlinedButton`, `NavButton`, champs texte arrondis) alignés sur la palette `#F4F7FE` / `#1E88E5` / `#2B3674` / `#E0E5F2`.

- [ ] **Step 4: Reconstruire `Strings.fr.axaml` et créer `Strings.en.axaml` + brancher dans `App.axaml`**

Extraire toutes les clés `{DynamicResource Str_...}` et leurs textes français originaux depuis `git diff a36dd82~1 a36dd82`, générer `Resources/Langs/Strings.fr.axaml` (avec `CopyToOutputDirectory` ou `AvaloniaResource`), créer la traduction anglaise complète dans `Resources/Langs/Strings.en.axaml`, et fusionner `MaterialStyles.axaml` + `Strings.fr.axaml` dans `App.axaml`.

- [ ] **Step 5: Vérifier que tous les tests passent**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: PASS (4/4 tests, 0 failures).

- [ ] **Step 6: Commit**

```bash
git add CamaroesPOS.UI.Avalonia CamaroesPOS.Tests CamaroesPOS_Avalonia.sln
git commit -m "feat(avalonia): services transversaux, thème Material, i18n FR/EN et tests de base"
```

---

### Task 3: Authentification (`LoginWindow`) et Shell Principal (`MainWindow`)

**Files:**
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/LoginViewModel.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/LoginWindow.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/LoginWindow.axaml.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/MainWindow.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/MainWindow.axaml.cs`
- Modify: `CamaroesPOS.UI.Avalonia/App.axaml.cs`

**Interfaces:**
- Consumes: `SessionService`, `PosDbContext`, `MaterialStyles.axaml`, `Strings.*.axaml`
- Produces:
  - `LoginWindow` (fenêtre de démarrage dans `App.axaml.cs`) qui authentifie via `PosDbContext.Users`, renseigne `SessionService` et ouvre `MainWindow`.
  - `MainWindow` : sidebar avec boutons de navigation (`BtnDashboard`, `BtnPOS`, `BtnInventory`, `BtnWallet`, `BtnVendors`, `BtnReports`, `BtnExpenses`, `BtnSettings`), masquage rôle `Cashier`, `BtnLangToggle` (bascule dynamique FR/EN dans `Application.Current.Resources.MergedDictionaries`), `BtnLogout`, et méthode `SetContent(Control view, string header)`.

- [ ] **Step 1: Porter `LoginViewModel` et `LoginWindow.axaml(.cs)`**

Adapter `CamaroesPOS.UI/ViewModels/LoginViewModel.cs` et `LoginWindow.xaml(.cs)` : champ mot de passe `TextBox PasswordChar="•"`, validation par touche `Enter`, message d'erreur en cas d'identifiants invalides, remplissage de `SessionService.SetSession(user)` et transition vers `MainWindow`.

- [ ] **Step 2: Porter `MainWindow.axaml(.cs)` avec placeholders de vues et bascule FR/EN**

Reproduire la mise en page exacte de `CamaroesPOS.UI/MainWindow.xaml` (sidebar 250px blanche + zone contenu `#F4F7FE` + header + bouton `BtnLangToggle`), avec les `MaterialIcon` correspondants. Les vues non encore migrées affichent un placeholder propre qui sera branché au fil des tâches 4 à 9. Mettre `LoginWindow` comme fenêtre principale dans `App.axaml.cs`.

- [ ] **Step 3: Vérifier la compilation et les tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: Build succeeded, 0 errors, all tests PASS.

- [ ] **Step 4: Commit**

```bash
git add CamaroesPOS.UI.Avalonia
git commit -m "feat(avalonia): fenêtre de connexion, MainWindow, rôles et bascule bilingue FR/EN"
```

---

### Task 4: Tableau de bord (`DashboardViewModel`, `DashboardView`) avec LiveChartsCore

**Files:**
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/DashboardViewModel.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/DashboardView.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/DashboardView.axaml.cs`
- Modify: `CamaroesPOS.UI.Avalonia/Views/MainWindow.axaml.cs`
- Create: `CamaroesPOS.Tests/DashboardViewModelTests.cs`

**Interfaces:**
- Consumes: `PosDbContext`, `LiveChartsCore.SkiaSharpView`, `MaterialStyles.axaml`, `Strings.*.axaml`
- Produces: `DashboardView` + `DashboardViewModel` (`LoadStats()`, 12 séries `ISeries[]`, axes `Axis[]`, KPIs, `CriticalStockList`, `UsersList` + `SearchText`).

- [ ] **Step 1: Écrire le test unitaire `DashboardViewModelTests` (cas base vide)**

Dans `CamaroesPOS.Tests/DashboardViewModelTests.cs`, écrire `LoadStats_DoesNotThrow_AndPopulatesAllCharts()` : instancie `DashboardViewModel`, appelle `LoadStats()`, vérifie qu'aucune exception n'est levée et que `SalesChart`, `StockMovementsChart`, `ProfitChart`, `PaymentModesChart` sont non nuls.

- [ ] **Step 2: Exécuter le test pour vérifier qu'il échoue**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: FAIL (`DashboardViewModel` non trouvé).

- [ ] **Step 3: Implémenter `DashboardViewModel.cs` avec `LiveChartsCore.SkiaSharpView`**

Porter toutes les métriques et les 12 graphiques de `CamaroesPOS.UI/ViewModels/DashboardViewModel.cs` vers `ISeries[]` (`PieSeries<double>`, `PieSeries<int>`, `LineSeries<double>`, `LineSeries<int>`, `ColumnSeries<int>`, `ColumnSeries<double>`, `RowSeries<double>`) et `Axis[]` (`XAxes` / `YAxes`) avec couleurs `SolidColorPaint(SKColors.*)`.

- [ ] **Step 4: Implémenter `DashboardView.axaml(.cs)` et brancher dans `MainWindow`**

Reproduire les cartes KPI, la liste d'alertes stock critique, la recherche utilisateurs et les graphiques `lvc:CartesianChart` / `lvc:PieChart`. Brancher `DashboardView` dans `MainWindow.axaml.cs` (`ShowDashboard()`).

- [ ] **Step 5: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: PASS (0 errors, 0 failures).

- [ ] **Step 6: Commit**

```bash
git add CamaroesPOS.UI.Avalonia CamaroesPOS.Tests
git commit -m "feat(avalonia): tableau de bord complet avec graphiques LiveChartsCore"
```

---

### Task 5: Caisse / Point de Vente (`POSView`) et Reçu (`ReceiptWindow`)

**Files:**
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/POSViewModel.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/POSView.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/POSView.axaml.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/ReceiptWindow.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/ReceiptWindow.axaml.cs`
- Modify: `CamaroesPOS.UI.Avalonia/Views/MainWindow.axaml.cs`
- Create: `CamaroesPOS.Tests/POSViewModelTests.cs`

**Interfaces:**
- Consumes: `PosDbContext`, `SessionService`, `DialogService`, `FilePickerService`, `PdfService`, `RelayCommand<T>`
- Produces: `POSView`, `POSViewModel`, `ReceiptWindow(string transNo, IEnumerable<CartItem> items, double total, double received, double change, string paymentMode, double discount)`.

- [ ] **Step 1: Écrire les tests unitaires `POSViewModelTests`**

- `DiscountPercentage_ClampsBetweenZeroAndHundred()` : vérifie `-10 -> 0` et `150 -> 100`.
- `ChangeAmount_CalculatesCorrectly_AndGuardsConfirmCashPayment()` : avec un article à 10 000 FCFA et `ReceivedAmountString = "5000"`, `ChangeAmount == -5000` et `ConfirmCashPaymentCommand.CanExecute(null) == false` ; avec `"15000"`, `ChangeAmount == 5000` et `CanExecute == true`.

- [ ] **Step 2: Exécuter les tests pour vérifier qu'ils échouent**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: FAIL (`POSViewModel` non trouvé).

- [ ] **Step 3: Implémenter `ReceiptWindow.axaml(.cs)`**

Reproduire l'aperçu du reçu de caisse (`CamaroesPOS.UI/Views/ReceiptWindow.xaml(.cs)`), bouton **Exporter PDF** via `FilePickerService.PickSavePdfPathAsync` + `PdfService.GenerateReceiptPdf`, et bouton **Imprimer** générant un PDF temporaire puis l'ouvrant avec `FilePickerService.OpenFileWithDefaultApp`.

- [ ] **Step 4: Implémenter `POSViewModel.cs` et `POSView.axaml(.cs)` + brancher dans `MainWindow`**

Porter toute la logique POS (scan code-barres, catalogue produits en overlay, dialogue quantité en overlay, panier, réduction %, paiement Espèces avec pavé tactile, paiement Portefeuille avec vérification solde et attribution des points fidélité, ouverture de `ReceiptWindow`). Remplacer tous les `CommandManager.InvalidateRequerySuggested()` par `RaiseCanExecuteChanged()` et `MessageBox.Show` par `DialogService`. Brancher `POSView` dans `MainWindow.axaml.cs`.

- [ ] **Step 5: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: PASS (0 errors, 0 failures).

- [ ] **Step 6: Commit**

```bash
git add CamaroesPOS.UI.Avalonia CamaroesPOS.Tests
git commit -m "feat(avalonia): caisse POS tactile, gestion panier/portefeuille et reçu PDF"
```

---

### Task 6: Gestion des Stocks (`InventoryView`) et Marques/Catégories (`BrandCategoryWindow`)

**Files:**
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/BrandCategoryViewModel.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/BrandCategoryWindow.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/BrandCategoryWindow.axaml.cs`
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/InventoryViewModel.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/InventoryView.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/InventoryView.axaml.cs`
- Modify: `CamaroesPOS.UI.Avalonia/Views/MainWindow.axaml.cs`

**Interfaces:**
- Consumes: `PosDbContext`, `SessionService`, `DialogService`, `RelayCommand<T>`
- Produces: `InventoryView`, `InventoryViewModel`, `BrandCategoryWindow`, `BrandCategoryViewModel`.

- [ ] **Step 1: Porter `BrandCategoryViewModel` et `BrandCategoryWindow.axaml(.cs)`**

CRUD complet pour `Brands` et `Categories` avec confirmations via `DialogService`.

- [ ] **Step 2: Porter `InventoryViewModel` et `InventoryView.axaml(.cs)` + brancher dans `MainWindow`**

Liste filtrable (`DataGrid`), CRUD produits réservé Admin (`SessionService.IsAdmin`), ajustement de stock (Entrée/Sortie avec justification obligatoire et garde stock insuffisant), bouton ouvrant `BrandCategoryWindow` en dialogue modal. Brancher dans `MainWindow.axaml.cs`.

- [ ] **Step 3: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: PASS (0 errors, 0 failures).

- [ ] **Step 4: Commit**

```bash
git add CamaroesPOS.UI.Avalonia
git commit -m "feat(avalonia): gestion des stocks, ajustements et fenêtre Marques/Catégories"
```

---

### Task 7: Clients CRM & Portefeuille (`WalletView`) et Fournisseurs (`VendorView`)

**Files:**
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/WalletViewModel.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/WalletView.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/WalletView.axaml.cs`
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/VendorViewModel.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/VendorView.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/VendorView.axaml.cs`
- Modify: `CamaroesPOS.UI.Avalonia/Views/MainWindow.axaml.cs`

**Interfaces:**
- Consumes: `PosDbContext`, `SessionService`, `DialogService`, `RelayCommand<T>`
- Produces: `WalletView`, `WalletViewModel`, `VendorView`, `VendorViewModel`.

- [ ] **Step 1: Porter `WalletViewModel` et `WalletView.axaml(.cs)`**

Gestion des clients (`CustomerWallet`), recharge/crédit de solde avec enregistrement dans `WalletTransactions`, historique des transactions et recherche.

- [ ] **Step 2: Porter `VendorViewModel` et `VendorView.axaml(.cs)` + brancher les deux vues dans `MainWindow`**

CRUD fournisseurs (`Vendor`) et recherche temps réel. Brancher `WalletView` et `VendorView` dans `MainWindow.axaml.cs`.

- [ ] **Step 3: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: PASS (0 errors, 0 failures).

- [ ] **Step 4: Commit**

```bash
git add CamaroesPOS.UI.Avalonia
git commit -m "feat(avalonia): vues Clients/Portefeuilles CRM et Fournisseurs"
```

---

### Task 8: Rapports & Ventes (`ReportsView`) et Dépenses (`ExpenseView`)

**Files:**
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/ReportsViewModel.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/ReportsView.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/ReportsView.axaml.cs`
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/ExpenseViewModel.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/ExpenseView.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/ExpenseView.axaml.cs`
- Modify: `CamaroesPOS.UI.Avalonia/Views/MainWindow.axaml.cs`

**Interfaces:**
- Consumes: `PosDbContext`, `SessionService`, `DialogService`, `FilePickerService`, `PdfService`, `RelayCommand<T>`
- Produces: `ReportsView`, `ReportsViewModel` (`LoadTransactions()`), `ExpenseView`, `ExpenseViewModel` (`LoadExpenses()`).

- [ ] **Step 1: Porter `ReportsViewModel` et `ReportsView.axaml(.cs)`**

Filtrage par période (`DatePicker` / presets Jour, Semaine, Mois), totaux ventes/réductions/transactions, annulation de commande réservée Admin (restauration du stock + insertion `CancelledOrders`), export PDF via `FilePickerService` + `PdfService.GenerateSalesReportPdf`.

- [ ] **Step 2: Porter `ExpenseViewModel` et `ExpenseView.axaml(.cs)` + brancher les deux vues dans `MainWindow`**

CRUD dépenses réservé Admin (`SessionService.IsAdmin`), calcul total dépenses. Brancher `ReportsView` et `ExpenseView` dans `MainWindow.axaml.cs` (avec appels `LoadTransactions()` et `LoadExpenses()` à l'ouverture).

- [ ] **Step 3: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: PASS (0 errors, 0 failures).

- [ ] **Step 4: Commit**

```bash
git add CamaroesPOS.UI.Avalonia
git commit -m "feat(avalonia): vues Rapports & Ventes (export PDF) et Dépenses"
```

---

### Task 9: Paramètres (`SettingsView`) et Changement de Mot de Passe (`ChangePasswordWindow`)

**Files:**
- Create: `CamaroesPOS.UI.Avalonia/ViewModels/SettingsViewModel.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/SettingsView.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/SettingsView.axaml.cs`
- Create: `CamaroesPOS.UI.Avalonia/Views/ChangePasswordWindow.axaml`
- Create: `CamaroesPOS.UI.Avalonia/Views/ChangePasswordWindow.axaml.cs`
- Modify: `CamaroesPOS.UI.Avalonia/Views/MainWindow.axaml.cs`

**Interfaces:**
- Consumes: `PosDbContext`, `SessionService`, `DialogService`, `RelayCommand<T>`
- Produces: `SettingsView`, `SettingsViewModel`, `ChangePasswordWindow`.

- [ ] **Step 1: Porter `ChangePasswordWindow.axaml(.cs)`**

Validation ancien mot de passe, confirmation nouveau mot de passe, mise à jour en base via `PosDbContext` et `SessionService.CurrentUser`.

- [ ] **Step 2: Porter `SettingsViewModel` et `SettingsView.axaml(.cs)` + brancher dans `MainWindow`**

Gestion des utilisateurs (`User` CRUD + recherche), informations magasin (`Store`), formulaire changement de mot de passe. Brancher `SettingsView` dans `MainWindow.axaml.cs`.

- [ ] **Step 3: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln`  
Expected: PASS (0 errors, 0 failures).

- [ ] **Step 4: Commit**

```bash
git add CamaroesPOS.UI.Avalonia
git commit -m "feat(avalonia): vue Paramètres et fenêtre Changement de mot de passe"
```

---

### Task 10: Vérification complète macOS, Intégrité WPF & Mise à jour README

**Files:**
- Modify: `README.md`

**Interfaces:**
- Consumes: `CamaroesPOS_Avalonia.sln`, `CamaroesPOS.UI.Avalonia`, `CamaroesPOS.Tests`
- Produces: Validation complète (build + tests + lancement macOS + diff WPF vide) et documentation à jour.

- [ ] **Step 1: Exécuter la compilation propre et toute la suite de tests**

Run: `dotnet clean CamaroesPOS_Avalonia.sln && dotnet build CamaroesPOS_Avalonia.sln && dotnet test CamaroesPOS_Avalonia.sln`  
Expected: `Build succeeded.`, `0 Error(s)`, all tests PASS.

- [ ] **Step 2: Vérifier le lancement natif sur macOS (smoke test)**

Lancer `dotnet run --project CamaroesPOS.UI.Avalonia` brièvement et vérifier que le processus démarre sans exception XAML ni erreur SQLite, puis le fermer proprement.

- [ ] **Step 3: Vérifier que la version WPF d'origine est 100 % intacte**

Run: `git diff backup-wpf-windows -- CamaroesPOS.UI/`  
Expected: sortie vide (aucune modification dans `CamaroesPOS.UI/`).

- [ ] **Step 4: Mettre à jour `README.md`**

Ajouter une section documentant la compilation et l'exécution sur macOS / Windows / Linux avec `CamaroesPOS_Avalonia.sln` et `dotnet run --project CamaroesPOS.UI.Avalonia`, tout en conservant la section WPF Windows existante.

- [ ] **Step 5: Commit final**

```bash
git add README.md
git commit -m "docs: ajout des instructions de compilation et lancement Avalonia (macOS/Windows/Linux)"
```
