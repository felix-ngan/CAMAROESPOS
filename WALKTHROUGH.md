# Guide Étape par Étape (Walkthrough)

Ce fichier documente, de manière simple et pédagogique, les actions entreprises pour moderniser l'application. Idéal pour comprendre la progression, même sans connaissances pointues en développement de bureau.

## 1. Mise en place de la Fondation
**Ce que nous avons fait :**
Au lieu de modifier l'ancien projet directement (ce qui serait risqué), nous avons créé un "Nouveau Projet" juste à côté de l'ancien. Il est divisé en trois "couches" pour bien séparer les responsabilités :
- `CamaroesPOS.Domain` : C'est le cerveau. Il ne contient que les règles pures de votre entreprise (ex: "Un client a un portefeuille"). Il ne sait pas comment l'écran s'affiche, ni comment la donnée est sauvegardée.
- `CamaroesPOS.Data` : C'est la mémoire. C'est ici que l'on configure la base de données (SQLite). Son seul rôle est de sauvegarder et lire les informations du "Domain".
- `CamaroesPOS.UI` : C'est l'interface visuelle. Elle utilise WPF et du code XAML pour dessiner de jolis boutons et fenêtres.

## 2. Prochaines Étapes
Nous allons procéder brique par brique. Actuellement, nous migrons les "Entités" (les définitions de ce qu'est un produit, une vente, etc.) depuis l'ancien projet vers le `Domain` de notre nouveau projet.
