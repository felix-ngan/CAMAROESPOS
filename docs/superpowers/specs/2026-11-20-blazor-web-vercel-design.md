# Design : Version Web CAMAROES POS en Blazor WebAssembly sur Vercel

**Date** : 2026-11-20  
**Branche de travail** : `feat/blazor-web-vercel`  
**Projets Desktop préservés** : `CamaroesPOS.UI` (WPF Windows), `CamaroesPOS.UI.Avalonia` (Desktop macOS/Windows/Linux), `CamaroesPOS.Data` (SQLite) — **100 % intacts**  
**Couche partagée** : `CamaroesPOS.Domain` (entités C# `.NET 8` partagées directement entre Desktop et Web)

---

## 1. Intention et Pourquoi

L'auteur souhaite publier une **vraie version Web complète de CAMAROES POS**, accessible gratuitement par tout le monde (et en particulier par les recruteurs depuis un simple lien), **tout en conservant les versions Desktop existantes** et en **partageant la stack C# / .NET 8** entre Desktop et Web.

**Blazor WebAssembly (.NET 8)** déployé sur **Vercel** répond exactement à ce besoin :
- Réutilise directement le projet C# [CamaroesPOS.Domain](file:///Users/felix/CAMAROESPOS/CamaroesPOS.Domain/CamaroesPOS.Domain.csproj) (`Product`, `SoldItem`, `CustomerWallet`, `WalletTransaction`, `Stock`, `CancelledOrder`, `Vendor`, `Brand`, `Category`, `Expense`, `User`, `Store`, `CartItem`).
- Génère une application Web HTML/CSS responsive native (PC, tablette, smartphone).
- Compile en artefacts statiques (`wwwroot/`) hébergés **100 % gratuitement sur le CDN Vercel**, sans serveur à payer ni mise en veille.
- Persiste les données localement dans le navigateur de chaque visiteur (`localStorage`), afin que chaque recruteur dispose d'une instance isolée, pré-remplie et réinitialisable en 1 clic.

---

## 2. Objectifs, Non-objectifs et Anti-objectifs

### Objectifs (Goals)
1. **Stack C#/.NET 8 partagée** : créer `CamaroesPOS.Web` (`Microsoft.NET.Sdk.BlazorWebAssembly`, `net8.0`) référençant directement `CamaroesPOS.Domain`.
2. **Parité fonctionnelle complète avec Desktop** :
   - **Authentification (`/login`)** : connexion `Username`/`Password` (`admin`/`123`, `caissier1`/`123`) + boutons d'accès rapide 1-clic (*Démo Admin* / *Démo Caissier*) pour les recruteurs + gestion stricte des rôles (`Admin` vs `Cashier`).
     - **Matrice des permissions `Cashier`** : accès direct par URL vers `/`, `/expenses` ou `/settings` redirige automatiquement vers `/pos` ; `/inventory`, `/vendors` et `/reports` sont en lecture seule (boutons Ajout/Édition/Suppression/Ajustement/Marques-Catégories/Annulation masqués et bloqués côté `PosStoreState`) ; sur `/wallets`, un `Cashier` peut créer/recharger un portefeuille client au comptoir mais seule la suppression est réservée `Admin`.
   - **Navigation principale (Layout responsive)** : barre latérale sur desktop / menu tiroir sur mobile, badge utilisateur + rôle, bascule bilingue dynamique **FR / EN**, bouton **Réinitialiser démo** (accessible aux deux rôles dans le pied de sidebar), lien discret vers le dépôt GitHub, bouton Déconnexion.
   - **Tableau de bord (`/`)** : cartes KPI (Ventes du jour, Chiffre d'affaires, Bénéfice Net, Volume de stock, Alertes stock critique, Articles vendus, Portefeuilles/Fidélité, Utilisateurs), graphiques SVG interactifs (Évolution CA 7j, Modes de paiement, Mouvements de stock 7j, Rentabilité, Top 5 stock, Top 5 clients, Top 5 vendus, Répartition rôles, etc.), table des stocks critiques, recherche utilisateurs et bouton Rafraîchir.
   - **Caisse / Point de Vente (`/pos`)** : recherche/scan code-barres, catalogue produits avec indicateur visuel de stock et sélection de quantité, panier avec suppression/vidage, remise `%` (`[0..100]`), paiement **Espèces** (pavé tactile + raccourcis Exact / 5 000 / 10 000 / 20 000 + rendu monnaie), paiement **Portefeuille Client** (vérification solde + points fidélité `1 pt / 1000 FCFA`), décrémentation du stock bornée à zéro (`Math.Max(0, qty - sold)`, parité stricte Desktop), et **Reçu imprimable / Export PDF** (modale dédiée + `window.print()` avec mise en page `@media print`).
   - **Gestion des Stocks (`/inventory`)** : recherche, CRUD produits (Admin), sélection Marque / Catégorie / Fournisseur, modale Marques & Catégories, ajustements de stock (`Entrée`, `Sortie` avec garde stock insuffisant, `Ajustement (Inventaire)` non négatif) tracés dans l'historique.
   - **Clients CRM & Portefeuilles (`/wallets`)** : création, modification, recharge/crédit, suppression, points de fidélité et total achats.
   - **Fournisseurs (`/vendors`)** : recherche et CRUD complet (Admin, garde suppression si lié à un produit).
   - **Rapports & Ventes (`/reports`)** : filtres date début/fin + recherche globale, 5 onglets (*Ventes*, *Stock Actuel*, *Historique Stocks*, *Ordres Annulés*, *Opérations Portefeuille*), annulation de vente réservée Admin (restaure le stock), export/impression PDF du rapport via `@media print`.
   - **Dépenses & Charges (`/expenses`)** : filtrage par période, total dépenses, CRUD réservé Admin.
   - **Paramètres (`/settings`)** : gestion des utilisateurs, informations boutique & codes Mobile Money (MTN MoMo / Orange Money), changement de mot de passe, et bouton **Réinitialiser les données de démo** (`localStorage`).
3. **Données de démo réalistes pré-remplies** : au premier chargement dans un navigateur vierge, `localStorage` est initialisé avec des produits, marques, catégories, fournisseurs, portefeuilles clients, utilisateurs, dépenses et **quelques ventes réparties sur les 7 derniers jours** afin que le Dashboard et les Rapports soient immédiatement vivants et parlants pour un recruteur.
4. **Déploiement gratuit sur Vercel** :
   - Configuration complète (`vercel.json` avec réécriture SPA `/(.*) -> /index.html` + script `vercel-build.sh` pour builds Git automatiques sur Vercel).
   - Déploiement concret via le skill `deploy-to-vercel` afin de fournir un lien public accessible immédiatement.
5. **Tests unitaires C#** : couvrir toute la logique métier de `PosStoreState` (checkout espèces/portefeuille, remises, stocks négatifs, annulation commande, permissions `Admin` vs `Cashier`, seed & sérialisation JSON snapshot, parité FR/EN) dans `CamaroesPOS.Tests`.

### Non-objectifs (Non-goals)
- Ne pas modifier `CamaroesPOS.Domain`, `CamaroesPOS.Data`, `CamaroesPOS.UI` (WPF) ni `CamaroesPOS.UI.Avalonia`.
- Ne pas mettre en place de serveur backend ni de base de données distante (choix explicite : `localStorage` par visiteur).
- Ne pas embarquer `QuestPDF` dans le bundle WebAssembly (lourd et natif Skia C++) : utiliser le moteur d'impression/PDF natif du navigateur (`window.print()` + feuilles `@media print` dédiées pour le Reçu et le Rapport des Ventes).

### Anti-objectifs (Anti-goals)
- Casser la compilation ou les 13 tests existants de `CamaroesPOS_Avalonia.sln`.
- Livrer un site Vercel qui renvoie une erreur `404` au rechargement d'une route (`/pos`, `/inventory`, etc.) faute de règle SPA fallback.
- Présenter un Dashboard vide sans graphiques lors de la première visite d'un recruteur.

---

## 3. Contraintes

- **SDK local** : `.NET 8.0.428` (`~/.dotnet/dotnet`) ; `Microsoft.NET.Sdk.BlazorWebAssembly` compile et publie sans nécessiter le workload `wasm-tools` (mode interprété standard, rapide à compiler).
- **Hébergement** : Vercel gratuit (artefacts statiques de `dotnet publish CamaroesPOS.Web -c Release`, dossier `wwwroot/`).
- **Stockage navigateur** : `window.localStorage` sous clé versionnée `camaroes_pos_web_v1` (sérialisation `System.Text.Json` d'un objet `PosStoreSnapshot` contenant toutes les listes d'entités `CamaroesPOS.Domain`).
- **Git** : travail sur `feat/blazor-web-vercel`, commits atomiques par étape, `backup-wpf-windows` et `v1.0-wpf-windows` intacts.

---

## 4. Approches considérées

### Approche retenue : **Blazor WebAssembly (`CamaroesPOS.Web`, .NET 8) + `localStorage` + Vercel**
- Partage directement `CamaroesPOS.Domain` (100 % C#/.NET 8 sur Desktop et Web).
- Vraie UI HTML/CSS responsive, hébergement statique gratuit et sans mise en veille sur Vercel, isolation complète par visiteur.

### Approches écartées
- **Next.js / React sur Vercel** : écartée car ne partage pas la stack C#/.NET avec les versions Desktop.
- **Blazor Server sur Render / Azure** : écartée car Vercel ne supporte pas les serveurs ASP.NET Core et Render gratuit impose une mise en veille (~30–50 s).
- **Avalonia Browser (WASM Canvas)** : écartée car bundle très lourd (~30 Mo) et ergonomie canvas moins adaptée au Web/mobile.

---

## 5. Décisions techniques (Choix proposés par l'assistant)

1. **Architecture interne de `CamaroesPOS.Web`** :
   - `Services/PosStoreState.cs` : cœur métier pur C# (aucune dépendance DOM/JS), opère directement sur les entités de `CamaroesPOS.Domain` et expose `ExportSnapshot()` / `ImportSnapshot()` + `SeedDemoData()`. Testable directement en xUnit.
   - `Services/BrowserStorageService.cs` : enveloppe légère `IJSRuntime` qui charge/sauvegarde `PosStoreSnapshot` JSON dans `localStorage` (avec repli silencieux en mémoire si `localStorage` est bloqué en navigation privée).
   - `Services/LocalizationService.cs` : dictionnaire bilingue FR/EN couvrant toute l'interface Web + événement `OnLanguageChanged`.
   - `Services/SessionState.cs` : utilisateur courant, rôle (`Admin` / `Cashier`), `IsAdmin`, persistance de session dans `sessionStorage`/`localStorage`.
2. **Design & Graphiques sans dépendances JS externes fragiles** :
   - CSS personnalisé (`wwwroot/css/app.css`) reproduisant fidèlement la charte CAMAROES POS (`#F4F7FE`, cartes blanches `#FFFFFF` bordées `#E0E5F2`, `#1E88E5`, `#2B3674`, `#2E7D32`, `#E53935`, responsive mobile/desktop) + styles `@media print` propres pour les reçus et rapports PDF.
   - Composants Razor SVG réutilisables (`Components/Charts/LineChartSvg.razor`, `BarChartSvg.razor`, `PieChartSvg.razor`) — zéro dépendance CDN externe, rendu instantané et net sur Retina/mobile.
3. **Solutions** :
   - Ajouter `CamaroesPOS.Web` à `CamaroesPOS_Avalonia.sln` (qui devient la solution cross-platform complète : Domain, Data, UI.Avalonia, Web, Tests) et à `CamaroesPOS_Modern.slnx`.
4. **Déploiement Vercel** :
   - À la racine du dépôt : `vercel.json` configuré avec `buildCommand` (`bash vercel-build.sh`), `outputDirectory` (`dist/wwwroot`) et `rewrites` SPA (`/(.*) -> /index.html`) pour les déploiements Git automatiques permanents une fois le dépôt relié au compte Vercel de l'utilisateur.
   - Pour le déploiement CLI direct d'un dossier précompilé `dist/wwwroot` (utile en mode `--temporary` sans authentification préalable) : génération d'un `dist/wwwroot/vercel.json` dédié contenant uniquement la règle `rewrites` SPA (sans `buildCommand`), garantissant zéro 404 au rechargement de `/pos`, `/inventory`, etc.
   - En fin d'implémentation, déploiement via le skill `deploy-to-vercel` (`npx vercel@latest deploy dist/wwwroot --temporary -y` pour une URL testable immédiatement + lien de réclamation vers le compte Vercel permanent, ou déploiement lié si l'utilisateur s'authentifie).

---

## 6. Ce qui reste à la discrétion de l'implémenteur

- Le détail de la mise en page responsive (breakpoints mobile/tablette, icônes SVG inline dans la sidebar et les cartes).
- Les données exactes des ventes de démonstration sur 7 jours (quantités, dates relatives à `DateTime.Today`).
- L'organisation fine des modales Razor (ajout/édition produit, pavé tactile espèces, reçu imprimable, marques/catégories).
