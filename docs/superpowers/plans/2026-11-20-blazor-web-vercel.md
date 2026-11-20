# Version Web Blazor WebAssembly sur Vercel — Plan d'Implémentation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Créer une version Web complète et responsive de CAMAROES POS (`CamaroesPOS.Web`, Blazor WebAssembly `.NET 8`) partageant directement `CamaroesPOS.Domain`, persistée par visiteur dans `localStorage`, testée en xUnit, et déployée gratuitement sur Vercel tout en gardant les versions Desktop (WPF + Avalonia) 100 % intactes.

**Architecture:** `CamaroesPOS.Web` (`Microsoft.NET.Sdk.BlazorWebAssembly`, `net8.0`) référence `CamaroesPOS.Domain` (14 entités C# partagées) et concentre toute la logique métier dans un service pur C# (`PosStoreState`) testable sans navigateur. Les données sont sérialisées en JSON dans `window.localStorage` via `BrowserStorageService` (`camaroes_pos_web_v1`), les graphiques sont rendus en composants Razor SVG natifs, et les reçus/rapports PDF utilisent `window.print()` avec `@media print`. Le déploiement Vercel repose sur `vercel.json` + `vercel-build.sh` (builds Git automatiques permanents) et le dossier publié `dist/wwwroot` (avec réécriture SPA `/(.*) -> /index.html`).

**Tech Stack:** .NET 8.0 SDK, Blazor WebAssembly (`Microsoft.AspNetCore.Components.WebAssembly`), `CamaroesPOS.Domain`, `System.Text.Json`, CSS moderne responsive + `@media print`, SVG Razor charts, xUnit, Vercel.

**Spec:** [docs/superpowers/specs/2026-11-20-blazor-web-vercel-design.md](file:///Users/felix/CAMAROESPOS/docs/superpowers/specs/2026-11-20-blazor-web-vercel-design.md)

## Global Constraints

- **Branche Git** : tous les commits sont faits sur `feat/blazor-web-vercel` (un commit atomique par tâche) ; `main`, `backup-wpf-windows` et `v1.0-wpf-windows` restent intacts.
- **Intégrité Desktop & Domain/Data** : ne modifier aucun fichier dans `CamaroesPOS.Domain/`, `CamaroesPOS.Data/`, `CamaroesPOS.UI/` ni `CamaroesPOS.UI.Avalonia/`.
- **Target Framework** : `net8.0` compilable avec le SDK `.NET 8.0.428` local sans workload `wasm-tools`.
- **Persistance navigateur** : `window.localStorage` sous clé `camaroes_pos_web_v1` avec repli silencieux en mémoire si bloqué, données de démo réalistes (ventes sur 7 jours) au premier accès, et bouton de réinitialisation en 1 clic.
- **Permissions `Cashier`** : redirection vers `/pos` pour `/`, `/expenses`, `/settings` ; lecture seule sur `/inventory`, `/vendors`, `/reports` ; sur `/wallets`, création/recharge autorisée mais suppression réservée `Admin`.
- **Stock POS vs Inventaire** : ajustement d'inventaire bloque quantités `< 0` et sorties > stock ; checkout POS borne `Product.Qty = Math.Max(0, qty - sold)` (parité Desktop).
- **Déploiement Vercel** : SPA fallback `/(.*) -> /index.html` dans `vercel.json` racine **et** `dist/wwwroot/vercel.json` ; ne jamais pousser sur `origin` sans action explicite de l'utilisateur.

## Review Focus

1. **Rechargement direct d'une sous-route sur Vercel (`/pos`, `/inventory`, `/reports`)** : sans règle SPA rewrite dans `vercel.json`, Vercel retourne `404` → vérifié dans Task 8 (`vercel.json` racine + `dist/wwwroot/vercel.json`).
2. **Accès direct URL par un `Cashier` ou utilisateur non connecté** : un visiteur tapant `/settings` ou `/expenses` en tant que `Cashier` doit être redirigé vers `/pos` (ou `/login` si déconnecté) et ne jamais pouvoir exécuter de mutation Admin dans `PosStoreState` → couvert par tests xUnit (Task 2) et `RoleGuard` (Task 3).
3. **`localStorage` corrompu, vide ou indisponible (navigation privée)** : un JSON invalide ou une erreur JS `localStorage` ne doit pas bloquer le démarrage de l'app ; fallback propre sur `SeedDemoData()` → couvert par test `ImportSnapshot` (Task 2) + `try/catch` dans `BrowserStorageService`.
4. **Remise POS, paiement `Exact` fractionnaire et solde portefeuille insuffisant** : remise clampée `[0..100]`, `NaN` rejeté, `TotalAmount <= 0` bloqué, solde portefeuille insuffisant bloqué avant toute écriture, `TransNo` unique partagé entre `WalletTransaction` et `SoldItem` → couvert par tests xUnit (Task 2).
5. **Parité FR/EN sur la version Web** : toutes les clés utilisées dans l'UI existent en FR et EN sans clé manquante → couvert par test xUnit (Task 2).

---

## Carte des fichiers créés / modifiés

```text
vercel.json                                     (Créé — config build Git Vercel + SPA rewrite)
vercel-build.sh                                 (Créé — install .NET 8 si absent + dotnet publish Release)
CamaroesPOS_Avalonia.sln                        (Modifié — ajout CamaroesPOS.Web)
CamaroesPOS_Modern.slnx                         (Modifié — ajout CamaroesPOS.Web)
README.md                                       (Modifié — section Version Web Blazor & lien Vercel)

CamaroesPOS.Web/
├── CamaroesPOS.Web.csproj
├── Program.cs
├── App.razor
├── _Imports.razor
├── wwwroot/
│   ├── index.html
│   ├── css/app.css
│   └── js/posInterop.js
├── Services/
│   ├── PosStoreSnapshot.cs
│   ├── PosStoreState.cs
│   ├── BrowserStorageService.cs
│   ├── SessionState.cs
│   └── LocalizationService.cs
├── Layout/
│   └── MainLayout.razor
├── Components/
│   ├── RoleGuard.razor
│   ├── ReceiptModal.razor
│   ├── BrandCategoryModal.razor
│   └── Charts/
│       ├── LineChartSvg.razor
│       ├── BarChartSvg.razor
│       └── PieChartSvg.razor
└── Pages/
    ├── Login.razor
    ├── Dashboard.razor
    ├── Pos.razor
    ├── Inventory.razor
    ├── Wallets.razor
    ├── Vendors.razor
    ├── Reports.razor
    ├── Expenses.razor
    └── Settings.razor

CamaroesPOS.Tests/
├── CamaroesPOS.Tests.csproj                    (Modifié — référence CamaroesPOS.Web)
└── WebStoreStateTests.cs                       (Créé — tests métier, rôles, i18n, snapshot JSON)
```

---

### Task 1: Échafaudage `CamaroesPOS.Web` (Blazor WebAssembly `.NET 8`) & Solutions

**Files:**
- Create: `CamaroesPOS.Web/CamaroesPOS.Web.csproj`
- Create: `CamaroesPOS.Web/Program.cs`
- Create: `CamaroesPOS.Web/App.razor`
- Create: `CamaroesPOS.Web/_Imports.razor`
- Create: `CamaroesPOS.Web/wwwroot/index.html`
- Create: `CamaroesPOS.Web/wwwroot/css/app.css`
- Create: `CamaroesPOS.Web/wwwroot/js/posInterop.js`
- Modify: `CamaroesPOS_Avalonia.sln`
- Modify: `CamaroesPOS_Modern.slnx`
- Modify: `CamaroesPOS.Tests/CamaroesPOS.Tests.csproj`

**Interfaces:**
- Consumes: `CamaroesPOS.Domain`
- Produces: Projet Blazor WebAssembly compilable (`CamaroesPOS.Web`), feuilles CSS responsive + `@media print`, bridge JS `window.posInterop` (`getItem`, `setItem`, `removeItem`, `printWindow`).

- [ ] **Step 1: Créer `CamaroesPOS.Web.csproj`, `Program.cs`, `App.razor`, `_Imports.razor` et `wwwroot/`**

- `CamaroesPOS.Web.csproj` : SDK `Microsoft.NET.Sdk.BlazorWebAssembly`, `<TargetFramework>net8.0</TargetFramework>`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, référence `CamaroesPOS.Domain`, package `Microsoft.AspNetCore.Components.WebAssembly` `8.0.10`.
- `wwwroot/index.html` : titre `CAMAROES POS — Web`, lien `css/app.css`, script `js/posInterop.js`, écran de chargement élégant pendant le boot WASM.
- `wwwroot/css/app.css` : design system CAMAROES POS (`#F4F7FE`, cartes blanches `#FFFFFF` bordées `#E0E5F2`, `#1E88E5`, `#2B3674`, `#2E7D32`, `#E53935`), grilles responsives, tables, modales, pavé tactile et règles `@media print`.
- `wwwroot/js/posInterop.js` : helpers sûrs `try/catch` pour `localStorage` + `window.print()`.

- [ ] **Step 2: Ajouter `CamaroesPOS.Web` aux solutions et référencer depuis `CamaroesPOS.Tests`, puis restaurer NuGet**

Run: `dotnet sln CamaroesPOS_Avalonia.sln add CamaroesPOS.Web/CamaroesPOS.Web.csproj && dotnet restore CamaroesPOS.Web/CamaroesPOS.Web.csproj && dotnet build CamaroesPOS_Avalonia.sln --no-restore`  
Expected: `Build succeeded.` (`0 Erreur(s)`).

- [ ] **Step 3: Commit**

```bash
git add CamaroesPOS.Web CamaroesPOS_Avalonia.sln CamaroesPOS_Modern.slnx CamaroesPOS.Tests/CamaroesPOS.Tests.csproj
git commit -m "feat(web): échafaudage du projet Blazor WebAssembly CamaroesPOS.Web"
```

---

### Task 2: Cœur métier (`PosStoreState`), Stockage navigateur, Session & i18n FR/EN (TDD)

**Files:**
- Create: `CamaroesPOS.Tests/WebStoreStateTests.cs`
- Create: `CamaroesPOS.Web/Services/PosStoreSnapshot.cs`
- Create: `CamaroesPOS.Web/Services/PosStoreState.cs`
- Create: `CamaroesPOS.Web/Services/BrowserStorageService.cs`
- Create: `CamaroesPOS.Web/Services/SessionState.cs`
- Create: `CamaroesPOS.Web/Services/LocalizationService.cs`
- Modify: `CamaroesPOS.Web/Program.cs`

**Interfaces:**
- Consumes: `CamaroesPOS.Domain.Entities.*`
- Produces:
  - `PosStoreState` : `SeedDemoData()`, `ExportSnapshotJson()`, `ImportSnapshotJson(string? json)`, `Authenticate(string username, string password) -> User?`, `CheckoutCash(List<CartItem>, double discountPercent, double received, string? customerPhone, string cashier) -> CheckoutResult`, `CheckoutWallet(List<CartItem>, double discountPercent, string customerPhone, string cashier) -> CheckoutResult`, `SaveProduct(Product, bool isNew, bool isAdmin) -> OperationResult`, `DeleteProduct(string code, bool isAdmin) -> OperationResult`, `AdjustStock(string code, string type, int qty, string justification, string user, bool isAdmin) -> OperationResult`, `SaveBrand(...)`, `DeleteBrand(...)`, `SaveCategory(...)`, `DeleteCategory(...)`, `SaveWallet(...)`, `DeleteWallet(string phone, bool isAdmin)`, `SaveVendor(...)`, `DeleteVendor(...)`, `CancelSale(int soldItemId, string cancelledBy, bool isAdmin)`, `SaveExpense(...)`, `DeleteExpense(...)`, `SaveUser(...)`, `SaveStore(...)`, `ChangePassword(...)`, `GetDashboardMetrics()`.
  - `BrowserStorageService` : `LoadIntoAsync(PosStoreState)`, `PersistAsync(PosStoreState)`, `ResetAsync(PosStoreState)`.
  - `SessionState` : `CurrentUser`, `IsAuthenticated`, `IsAdmin`, `Login(User)`, `Logout()`, `OnChange`.
  - `LocalizationService` : `CurrentLang` (`"fr"`/`"en"`), `Toggle()`, `T(string key)`, `AllFrenchKeys`, `AllEnglishKeys`, `OnChange`.

- [ ] **Step 1: Écrire les tests unitaires `CamaroesPOS.Tests/WebStoreStateTests.cs` (RED)**

Couvrir :
1. `SeedDemoData_PopulatesProductsUsersAndSevenDaySales()`
2. `CheckoutCash_AppliesClampedDiscount_DecrementsStock_AndCreatesSoldItems()`
3. `CheckoutWallet_RejectsInsufficientBalance_AndZeroTotal_AndSharesTransNoOnSuccess()`
4. `AdjustStock_RejectsNegativeQuantity_AndInsufficientStockOut()`
5. `AdminGuards_BlockCashierFromMutatingInventoryVendorsExpensesOrdersAndDeletingWallets()`
6. `SnapshotJson_RoundTripsAllEntities_AndFallsBackToSeedOnCorruptJson()`
7. `LocalizationService_FrenchAndEnglishHaveIdenticalKeys()`

- [ ] **Step 2: Exécuter les tests pour vérifier qu'ils échouent**

Run: `dotnet test CamaroesPOS_Avalonia.sln --no-restore`  
Expected: FAIL (`PosStoreState` / `LocalizationService` non définis).

- [ ] **Step 3: Implémenter les 5 services et les enregistrer en DI dans `Program.cs`**

Enregistrer `PosStoreState`, `BrowserStorageService`, `SessionState` et `LocalizationService` comme singletons dans `CamaroesPOS.Web/Program.cs`.

- [ ] **Step 4: Exécuter la suite complète de tests pour vérifier GREEN**

Run: `dotnet test CamaroesPOS_Avalonia.sln --no-restore`  
Expected: PASS (20/20 tests, 0 failures).

- [ ] **Step 5: Commit**

```bash
git add CamaroesPOS.Web CamaroesPOS.Tests
git commit -m "feat(web): moteur métier PosStoreState, persistance localStorage, i18n FR/EN et tests xUnit"
```

---

### Task 3: Layout principal responsive (`MainLayout`), Authentification (`/login`) & `RoleGuard`

**Files:**
- Create: `CamaroesPOS.Web/Layout/MainLayout.razor`
- Create: `CamaroesPOS.Web/Components/RoleGuard.razor`
- Create: `CamaroesPOS.Web/Pages/Login.razor`
- Modify: `CamaroesPOS.Web/App.razor`

**Interfaces:**
- Consumes: `PosStoreState`, `BrowserStorageService`, `SessionState`, `LocalizationService`
- Produces: Shell complet responsive (sidebar desktop + drawer mobile + toggle FR/EN + bouton Reset Démo + badge rôle), page `/login` avec connexion rapide 1-clic *Admin* / *Caissier*, et composant `<RoleGuard RequireAdmin="true|false">` redirigeant les anonymes vers `/login` et les `Cashier` sur routes Admin vers `/pos`.

- [ ] **Step 1: Implémenter `RoleGuard.razor`, `MainLayout.razor` et `Pages/Login.razor`, et brancher dans `App.razor`**

Initialiser le stockage (`BrowserStorageService.LoadIntoAsync`) au montage de `MainLayout` / `Login` une seule fois afin que toute visite charge `localStorage` ou les données de démo.

- [ ] **Step 2: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln --no-restore`  
Expected: Build succeeded, 0 errors, 20/20 tests PASS.

- [ ] **Step 3: Commit**

```bash
git add CamaroesPOS.Web
git commit -m "feat(web): MainLayout responsive, authentification 1-clic recruteur et RoleGuard"
```

---

### Task 4: Composants Graphiques SVG & Tableau de bord (`/`)

**Files:**
- Create: `CamaroesPOS.Web/Components/Charts/LineChartSvg.razor`
- Create: `CamaroesPOS.Web/Components/Charts/BarChartSvg.razor`
- Create: `CamaroesPOS.Web/Components/Charts/PieChartSvg.razor`
- Create: `CamaroesPOS.Web/Pages/Dashboard.razor`

**Interfaces:**
- Consumes: `PosStoreState.GetDashboardMetrics()`, `LocalizationService`, `RoleGuard`
- Produces: Graphiques SVG réutilisables (courbe multi-séries, barres/colonnes, anneau/camembert avec légende) et page `/` (Dashboard Admin) avec 8 cartes KPI, badge alerte stock critique, bouton Rafraîchir, 8 graphiques SVG, tableau stocks critiques et recherche utilisateurs.

- [ ] **Step 1: Implémenter les 3 composants SVG et `Pages/Dashboard.razor`**

- [ ] **Step 2: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln --no-restore`  
Expected: Build succeeded, 0 errors, all tests PASS.

- [ ] **Step 3: Commit**

```bash
git add CamaroesPOS.Web
git commit -m "feat(web): composants graphiques SVG natifs et Tableau de bord interactif"
```

---

### Task 5: Caisse / Point de Vente (`/pos`) & Reçu imprimable/PDF (`ReceiptModal`)

**Files:**
- Create: `CamaroesPOS.Web/Components/ReceiptModal.razor`
- Create: `CamaroesPOS.Web/Pages/Pos.razor`

**Interfaces:**
- Consumes: `PosStoreState`, `BrowserStorageService`, `SessionState`, `LocalizationService`, `RoleGuard`, `window.posInterop.printWindow`
- Produces: Caisse complète (`/pos`) avec scan/saisie code-barres, catalogue produits modal + quantité, panier, remise `%` (`[0..100]`), pavé tactile Espèces (Exact / 5 000 / 10 000 / 20 000 + rendu), paiement Portefeuille + points fidélité, persistance immédiate dans `localStorage`, et modale Reçu imprimable/PDF.

- [ ] **Step 1: Implémenter `ReceiptModal.razor` et `Pages/Pos.razor`**

- [ ] **Step 2: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln --no-restore`  
Expected: Build succeeded, 0 errors, all tests PASS.

- [ ] **Step 3: Commit**

```bash
git add CamaroesPOS.Web
git commit -m "feat(web): caisse POS tactile, paiement espèces/portefeuille et reçu imprimable PDF"
```

---

### Task 6: Stocks (`/inventory` + `BrandCategoryModal`), Clients CRM (`/wallets`) & Fournisseurs (`/vendors`)

**Files:**
- Create: `CamaroesPOS.Web/Components/BrandCategoryModal.razor`
- Create: `CamaroesPOS.Web/Pages/Inventory.razor`
- Create: `CamaroesPOS.Web/Pages/Wallets.razor`
- Create: `CamaroesPOS.Web/Pages/Vendors.razor`

**Interfaces:**
- Consumes: `PosStoreState`, `BrowserStorageService`, `SessionState`, `LocalizationService`, `RoleGuard`
- Produces: Pages `/inventory` (avec sélecteurs Marque/Catégorie/Fournisseur, ajustement de stock et modale Marques/Catégories), `/wallets` (création/recharge pour tous, suppression Admin) et `/vendors` (CRUD Admin avec garde produits liés).

- [ ] **Step 1: Implémenter `BrandCategoryModal.razor`, `Pages/Inventory.razor`, `Pages/Wallets.razor` et `Pages/Vendors.razor`**

- [ ] **Step 2: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln --no-restore`  
Expected: Build succeeded, 0 errors, all tests PASS.

- [ ] **Step 3: Commit**

```bash
git add CamaroesPOS.Web
git commit -m "feat(web): pages Gestion des Stocks, Marques/Catégories, Clients CRM et Fournisseurs"
```

---

### Task 7: Rapports & Ventes (`/reports`), Dépenses (`/expenses`) & Paramètres (`/settings`)

**Files:**
- Create: `CamaroesPOS.Web/Pages/Reports.razor`
- Create: `CamaroesPOS.Web/Pages/Expenses.razor`
- Create: `CamaroesPOS.Web/Pages/Settings.razor`

**Interfaces:**
- Consumes: `PosStoreState`, `BrowserStorageService`, `SessionState`, `LocalizationService`, `RoleGuard`, `window.posInterop.printWindow`
- Produces: Page `/reports` (filtres période + recherche globale, 5 onglets, annulation Admin restaurant le stock, impression/export PDF), page `/expenses` (Admin, CRUD + total) et page `/settings` (Admin, utilisateurs, boutique & Mobile Money, mot de passe, réinitialisation démo).

- [ ] **Step 1: Implémenter `Pages/Reports.razor`, `Pages/Expenses.razor` et `Pages/Settings.razor`**

- [ ] **Step 2: Vérifier compilation et tests**

Run: `dotnet test CamaroesPOS_Avalonia.sln --no-restore`  
Expected: Build succeeded, 0 errors, all tests PASS.

- [ ] **Step 3: Commit**

```bash
git add CamaroesPOS.Web
git commit -m "feat(web): pages Rapports & Ventes (PDF), Dépenses et Paramètres"
```

---

### Task 8: Configuration Vercel, Publication Release, Tests finaux, README & Déploiement live

**Files:**
- Create: `vercel.json`
- Create: `vercel-build.sh`
- Modify: `README.md`

**Interfaces:**
- Consumes: `CamaroesPOS.Web`, `CamaroesPOS_Avalonia.sln`, skill `deploy-to-vercel`
- Produces: Configuration Vercel prête pour Git-push auto-deploys (`vercel.json` + `vercel-build.sh`), build publié vérifié (`dist/wwwroot` + `dist/wwwroot/vercel.json`), documentation à jour, et URL Vercel live retournée à l'utilisateur.

- [ ] **Step 1: Créer `vercel.json` et `vercel-build.sh` (exécutable)**

- `vercel.json` : `buildCommand: "bash vercel-build.sh"`, `outputDirectory: "dist/wwwroot"`, `rewrites: [{ "source": "/(.*)", "destination": "/index.html" }]`.
- `vercel-build.sh` : vérifie si `dotnet` 8 est installé (sinon télécharge et exécute `dotnet-install.sh --channel 8.0`), lance `dotnet publish CamaroesPOS.Web/CamaroesPOS.Web.csproj -c Release -o dist`, et écrit le fichier `dist/wwwroot/vercel.json` (rewrites SPA uniquement).

- [ ] **Step 2: Tester `dotnet publish` Release localement, exécuter toute la suite xUnit et vérifier l'intégrité Desktop**

Run:
```bash
dotnet publish CamaroesPOS.Web/CamaroesPOS.Web.csproj -c Release -o dist --no-restore
test -f dist/wwwroot/index.html && test -f dist/wwwroot/_framework/blazor.boot.json
dotnet test CamaroesPOS_Avalonia.sln --no-restore
git diff main -- CamaroesPOS.Domain/ CamaroesPOS.Data/ CamaroesPOS.UI/ CamaroesPOS.UI.Avalonia/
```
Expected: Publication OK, fichiers WASM présents, 20/20 tests PASS, diff vide sur Desktop/Domain/Data.

- [ ] **Step 3: Mettre à jour `README.md` et commiter**

Ajouter la section **Version Web (Blazor WebAssembly sur Vercel)** avec instructions locales et lien de déploiement. Ajouter `dist/` dans `.gitignore` pour ne pas commiter les artefacts de build.

```bash
git add .gitignore vercel.json vercel-build.sh README.md
git commit -m "chore(web): configuration déploiement Vercel (SPA + build .NET 8) et mise à jour README"
```

- [ ] **Step 4: Déployer sur Vercel via le skill `deploy-to-vercel` (après revue finale de branche)**

Utiliser `npx vercel@latest deploy dist/wwwroot --temporary -y` (ou liaison directe selon état d'auth Vercel) pour livrer immédiatement une URL publique + lien de réclamation vers le compte Vercel GitHub de l'utilisateur.
