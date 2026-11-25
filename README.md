<h1 align="center">CAMAROES POS — v2 Portfolio Edition</h1>

<p align="center">
  <strong>Système de Point de Vente (POS) moderne .NET 8 multiplateforme (Web PWA · macOS · Windows · Linux)</strong><br />
  <a href="https://camaroespos.vercel.app/">🚀 Démo Live (Blazor WebAssembly PWA sur Vercel)</a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Framework-.NET%208.0-512BD4.svg" alt=".NET 8">
  <img src="https://img.shields.io/badge/Web-Blazor%20WASM%20PWA-512BD4.svg" alt="Blazor WASM">
  <img src="https://img.shields.io/badge/Desktop-Avalonia%20UI%20%2B%20WPF-1D72E8.svg" alt="Avalonia UI">
  <img src="https://img.shields.io/badge/Architecture-Clean%20Architecture-15803D.svg" alt="Clean Architecture">
  <img src="https://img.shields.io/badge/Security-PBKDF2%20%2B%20HMAC%20%2B%20CSP-DC2626.svg" alt="Security">
</p>

---

## 📖 Présentation

**CAMAROES POS v2** est une application de Point de Vente (POS) complète conçue pour le contexte commercial **camerounais / CEMAC** (FCFA, **TVA 19,25 %** HT/TTC, **NIU/RCCM**, **MTN MoMo & Orange Money**, bilingue **FR/EN**), architecturée en **C# 12 / .NET 8** selon les principes de Clean Architecture.

La solution partage un cœur métier unique (`CamaroesPOS.Domain` + `CamaroesPOS.Application`) entre une **PWA Blazor WebAssembly** déployée gratuitement sur Vercel et une application **Desktop multiplateforme (Avalonia UI)** + version **Windows WPF**.

---

## ✨ Fonctionnalités Clés (v2)

* 🛒 **Caisse POS Tactile Split-Screen (Style Square / Loyverse)** : Catalogue produits visible en permanence avec filtrage par catégorie et ajout en **1 clic**, steppers `-`/`+` inline, blocage strict de la survente de stock, mise en attente de tickets (**Hold / Resume**), et raccourcis clavier (`F2`, `F9`, `Esc`).
* 📱 **Paiements Multi-Canaux Cameroun** : **Espèces** (pavé tactile FCFA), **MTN MoMo** & **Orange Money** (simulation réaliste de Push USSD exploitant les codes marchands de la boutique), **Portefeuille Client (CRM)**, et **Paiement Mixte (Split Payment)**.
* 🧾 **Facturation & Reçus Conformes** : Décomposition automatique **HT / TVA (19,25 %) / Remise / TTC** en précision exacte `decimal`, affichage **NIU / RCCM / Réf. paiement**, **QR Code SVG** de vérification, aperçu et impression **Ticket Thermique 80 mm** ou **Format A4**.
* 💼 **Sessions de Caisse (Rapport Z) & Remboursements Fiables** : Ouverture/clôture de caisse avec calcul automatique de l'écart de caisse ; annulation/remboursement de ticket complet avec **restauration du stock**, **recrédit automatique du portefeuille client** et inversion des points fidélité.
* 📊 **Tableau de Bord & Rapports Groupés** : Sélecteur de période cohérent (`Aujourd'hui`, `7 j`, `30 j`, `Tout`), graphiques SVG natifs avec axes Y gradués et tooltips tactiles, ventes regroupées par ticket, **Journal d'Audit**, **Export CSV** (Excel UTF-8) et **Sauvegarde/Restauration JSON**.
* 🔒 **Sécurité & Accessibilité (WCAG 2.1 AA)** : Mots de passe hachés **PBKDF2-SHA256** (100 000 itérations + sel aléatoire), sessions signées **HMAC-SHA256**, en-têtes HTTP de sécurité (`CSP`, `X-Frame-Options`, `nosniff`), modales de confirmation sur toutes les actions destructives, **Mode Sombre** et support **PWA hors-ligne**.

---

## 🏗️ Architecture de la Solution

```text
CAMAROESPOS/
├── CamaroesPOS.Domain/        # Entités métier pures en précision decimal (Product, SoldItem, CashShift, AuditEntry, Store...)
├── CamaroesPOS.Application/   # Logique partagée : PasswordHasher (PBKDF2), SessionTokenService (HMAC), PosCalculator (TVA 19,25 %), TransactionNumberGenerator
├── CamaroesPOS.Data/          # PosDbContext (EF Core 8 + SQLite, PK & Index)
├── CamaroesPOS.Web/           # PWA Blazor WebAssembly (.NET 8) déployée sur Vercel
├── CamaroesPOS.UI.Avalonia/   # Client Desktop multiplateforme (macOS, Windows, Linux)
├── CamaroesPOS.UI/            # Client Desktop Windows WPF (maintenu compatible decimal)
└── CamaroesPOS.Tests/         # Suite de tests unitaires xUnit (sécurité, intégrité financière, stock, remboursements, i18n)
```

---

## 🚀 Lancer Localement & Tests

### Prérequis
* [SDK .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

### 1. Version Web (Blazor WebAssembly PWA)
```bash
dotnet run --project CamaroesPOS.Web
```
Build statique de production Vercel (`dist/wwwroot`) :
```bash
bash vercel-build.sh
```

### 2. Version Desktop Multiplateforme (Avalonia UI)
```bash
dotnet build CamaroesPOS_Avalonia.sln
dotnet run --project CamaroesPOS.UI.Avalonia
```

### 3. Suite de Tests Automatisés (xUnit)
```bash
dotnet test CamaroesPOS_Avalonia.sln --verbosity normal
```

---

<p align="center">
  <i>Développé par <a href="https://github.com/felix-ngan">felix-ngan</a> — Vitrine d'ingénierie .NET 8 / C# moderne.</i>
</p>
