<h1 align="center">CAMAROES POS</h1>

<p align="center">
  <strong>Solution de Point de Vente (POS) multi-boutiques robuste et résiliente</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Plateforme-Windows-blue.svg" alt="Platform">
  <img src="https://img.shields.io/badge/Language-C%23-239120.svg" alt="C#">
  <img src="https://img.shields.io/badge/Framework-.NET%20WinForms-512BD4.svg" alt=".NET WinForms">
  <img src="https://img.shields.io/badge/Database-SQL%20Server-CC292B.svg" alt="SQL Server">
</p>

---

## 📖 Description

**CAMAROES POS** est une application professionnelle de gestion de point de vente développée en C# / .NET Windows Forms. 
Conçue spécifiquement pour répondre aux défis de connectivité, elle intègre une **architecture multi-boutiques** innovante permettant aux caisses locales d'encaisser les clients de manière totalement autonome (hors-ligne), tout en synchronisant les données avec un serveur central dès que le réseau est disponible.

## ✨ Fonctionnalités Principales

*   **Encaissement hors-ligne résilient** : Chaque caisse dispose de sa propre base de données locale SQL Server Express.
*   **Synchronisation automatique** : Consolidation asynchrone des ventes vers le serveur central et mise à jour du catalogue produits vers les caisses.
*   **Gestion avancée des utilisateurs** : Contrôle d'accès sécurisé et gestion des rôles.
*   **Portefeuilles clients (Customer Wallets)** : Gestion des comptes clients, recharges et transactions sécurisées par hash cryptographique.
*   **Génération de rapports** : Intégration de rapports (RDLC) pour l'analyse des ventes.

---

## 🏗 Architecture Multi-boutiques

Le système repose sur deux composantes majeures :
1.  **Serveur Central (Machine Principale)** : Centralise toutes les ventes de toutes les boutiques, gère le catalogue global et les paramètres réseau.
2.  **Caisse Locale (Boutiques secondaires)** : Dispose de sa base de données `NewOne` locale. Enregistre les ventes et exécute le script `run_sync.ps1` en arrière-plan pour consolider les données vers le serveur central.

---

## 🚀 Guide d'Installation et de Configuration

### Prérequis
*   [Visual Studio 2019/2022](https://visualstudio.microsoft.com/) ou SDK .NET compatible.
*   [Microsoft SQL Server Express](https://www.microsoft.com/fr-fr/sql-server/sql-server-downloads) installé sur la machine centrale ET sur les caisses locales.
*   SQL Server Management Studio (SSMS) recommandé pour l'administration.

### 1. Configuration du Serveur Central
Pour que les caisses locales puissent communiquer avec le serveur central, ce dernier doit être correctement exposé sur le réseau local :

1. **Activer TCP/IP** : Dans *SQL Server Configuration Manager*, activez le protocole TCP/IP pour l'instance `SQLEXPRESS01` et fixez le port dynamique `IPAll` sur **1433**.
2. **Authentification Mixte** : Dans SSMS, passez le serveur en mode *SQL Server and Windows Authentication mode*.
3. **Mot de passe de l'administrateur (sa)** : Activez le compte `sa` et attribuez-lui un mot de passe fort (ex: `CamaroesPOS2026!`).
4. **Pare-feu Windows** : Autorisez les ports entrants **TCP 1433** (SQL Server) et **UDP 1434** (SQL Server Browser).
5. Démarrez le service **SQL Server Browser** en mode automatique.

### 2. Déploiement de la Base de Données
*   Restaurez le fichier de sauvegarde `NewOne.bak` inclus dans le dépôt sur les instances SQL locales et centrales.
*   Assurez-vous que les tables de paramètres (ex: `tbl_LocalSettings`) sont correctement initialisées sur chaque caisse.

### 3. Configuration de l'Application (Caisse Locale)
1. Ouvrez la base de données locale de la caisse (`NewOne`).
2. Dans la table `tbl_LocalSettings`, insérez/modifiez la clé `CentralConnectionString` avec les informations du serveur central :
   ```text
   Server=<IP_SERVEUR_CENTRAL>,1433;Database=NewOne;User Id=sa;Password=<VOTRE_MOT_DE_PASSE>;TrustServerCertificate=True;
   ```
3. L'application lira dynamiquement cette chaîne de connexion pour synchroniser les données.

### 4. Compilation et Lancement
1. Clonez ce dépôt :
   ```bash
   git clone https://github.com/felix-ngan/CAMAROESPOS.git
   ```
2. Ouvrez `FinalPOS.sln` dans Visual Studio.
3. Restaurez les packages NuGet (Dossier `packages/` déjà inclus ou via l'interface VS).
4. Compilez la solution en mode `Debug` ou `Release`.
5. Lancez `FinalPOS.exe`.

---

## 🔄 Automatisation de la Synchronisation

Pour automatiser la consolidation des ventes de la caisse vers le serveur central, un script PowerShell de synchronisation est mis en place (`run_sync.ps1`).
*   Utilisez le **Planificateur de tâches Windows** sur chaque caisse locale pour exécuter ce script toutes les 3 à 5 minutes.
*   Le script s'appuie sur le moteur interne de `FinalPOS.exe` pour lire `tbl_LocalSettings` et garantir une transmission chiffrée et intègre.

---

## 🛡 Sécurité

*   Les mots de passe des caissiers sont hachés (ex: `PBKDF2`).
*   Les requêtes SQL sont paramétrées pour prévenir les injections SQL.
*   Les transactions financières critiques (comme la recharge d'un Wallet Client) utilisent des blocs `SqlTransaction` pour garantir la norme ACID.

---

<p align="center">
  <i>Développé avec ❤️ pour optimiser la gestion des points de vente.</i>
</p>
