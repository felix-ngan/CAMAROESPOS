# CAMAROES POS

## Description
CAMAROES POS est une application de point de vente (Point of Sale) développée en C# avec .NET Windows Forms et SQL Server. Elle intègre une architecture multi-boutiques avec une base de données locale (pour la résilience hors ligne) et une base de données centrale pour la consolidation des données.

## Fonctionnalités Principales
- Gestion des ventes et des produits.
- Mode hors-ligne robuste avec base de données locale.
- Synchronisation bidirectionnelle avec le serveur central.
- Gestion d'utilisateurs et de portefeuilles clients (Customer Wallets).

## Configuration Requise
- .NET Framework (version utilisée dans le projet)
- SQL Server Express (Base de données locale et centrale)

## Installation et Déploiement
1. Cloner ce dépôt.
2. Configurer la chaîne de connexion (Centrale et Locale).
3. Compiler avec Visual Studio ou `dotnet build`.
4. Lancer `FinalPOS.exe`.

## Architecture de Synchronisation
L'application utilise un système de synchronisation pour envoyer les ventes de la caisse locale vers le serveur central, et récupérer les mises à jour des paramètres (par exemple `tbl_LocalSettings`) ou du catalogue.

## Auteurs
CAMAROES POS Development Team.
