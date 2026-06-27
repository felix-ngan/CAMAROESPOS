using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MetroFramework.Controls;

namespace FinalPOS.UI
{
    public static class LanguageManager
    {
        public static string CurrentLanguage { get; set; } = "Français";

        private static readonly Dictionary<string, string> EnglishTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Login Screen (frmSecurity)
            ["SE CONNECTER"] = "LOGIN",
            ["Nom d'utilisateur"] = "Username",
            ["Mot de passe"] = "Password",
            ["Connexion"] = "Login",
            ["QUITTER"] = "EXIT",

            // Admin Main Dashboard (Form1)
            ["Tableau de bord"] = "Dashboard",
            ["Tableau de Bord"] = "Dashboard",
            ["Gestion des Produits"] = "Product Management",
            ["Stocks & Approvisionnements"] = "Inventory & Stocks",
            ["Gestion des Marques"] = "Brands Management",
            ["Gestion des Catégories"] = "Categories Management",
            ["Gestion des Fournisseurs"] = "Vendors Management",
            ["Paramètres Boutique"] = "Store Settings",
            ["Comptes Utilisateurs"] = "User Accounts",
            ["Historique & Rapports"] = "Records & Reports",
            ["DÉCONNEXION"] = "LOGOUT",
            ["Déconnexion"] = "Logout",
            ["Entrées de Stock"] = "Stock Entries",
            ["Ajustement des Stocks"] = "Stock Adjustments",
            ["Fournisseurs"] = "Vendors",
            ["Paramètres Utilisateur"] = "User Settings",
            ["Historique des Ventes"] = "Sales History",

            // Dashboard items (frmDashboard)
            ["VENTES DU JOUR"] = "DAILY SALES",
            ["GAMME PRODUITS"] = "PRODUCT LINE",
            ["STOCKS DISPONIBLES"] = "STOCK ON HAND",
            ["ARTICLES CRITIQUES"] = "CRITICAL ITEMS",

            // Stock Module (frmStockIn, frmAdjustment)
            ["Entrée de Stock"] = "Stock Entry",
            ["Fiche d'Entrée de Stock"] = "Stock Entry Details",
            ["Ajustement de Stock"] = "Stock Adjustment",
            ["Code Produit"] = "Product Code",
            ["Description"] = "Description",
            ["Quantité"] = "Quantity",
            ["Action"] = "Action",
            ["Motif"] = "Reason",
            ["Sauvegarder"] = "Save",
            ["Annuler"] = "Cancel",

            // Cashier screen (frmPOS)
            ["Panier de Vente"] = "Sales Cart",
            ["Caisse / Point de Vente"] = "POS / Cash Register",
            ["Saisir le code-barres ici..."] = "Scan barcode here...",
            ["[F1] Nouvelle Vente"] = "[F1] New Sale",
            ["[F1] Nouveau Panier"] = "[F1] New Cart",
            ["[F2] Chercher Produit"] = "[F2] Search Product",
            ["[F2] Rechercher Article"] = "[F2] Search Item",
            ["[F3] Appliquer Remise"] = "[F3] Apply Discount",
            ["[F3] Ajouter Remise"] = "[F3] Add Discount",
            ["[F4] Régler Paiement"] = "[F4] Settle Payment",
            ["[F4] Règlement"] = "[F4] Settle",
            ["[F5] Vider Panier"] = "[F5] Clear Cart",
            ["[F5] Vider le Panier"] = "[F5] Clear Cart",
            ["[F6] Ventes du Jour"] = "[F6] Daily Sales",
            ["[F7] Changer Mot de Passe"] = "[F7] Change Password",
            ["[F8] Se Déconnecter"] = "[F8] Logout",
            ["[F10] Déconnexion"] = "[F10] Logout",
            ["HORS-TAXE"] = "SUBTOTAL",
            ["REMISE"] = "DISCOUNT",
            ["TVA"] = "VAT",
            ["TOTAL VENTES"] = "SALES TOTAL",
            ["AJOUTER 1"] = "ADD 1",
            ["RETIRER 1"] = "REMOVE 1",
            ["SUPPRIMER"] = "DELETE",
            ["Payer"] = "Pay",
            ["Client"] = "Customer",
            ["Qté"] = "Qty",

            // Settle Screen (frmSettle)
            ["Vente Règlement"] = "Settle Payment",
            ["Montant à payer"] = "Amount to Pay",
            ["Espèces reçues"] = "Cash Received",
            ["Monnaie rendue"] = "Change Due",
            ["Téléphone du Client"] = "Customer Phone",
            ["Payer avec le solde"] = "Pay with balance",
            ["Épargner la monnaie"] = "Save the change",
            ["Solde disponible : 0 FCFA"] = "Available balance: 0 FCFA",

            // Wallet & Change Reports
            ["Portefeuilles & Monnaie"] = "Wallets & Change",
            ["Portefeuilles"] = "Wallets",
            ["Rechercher téléphone :"] = "Search phone:",
            ["Du :"] = "From:",
            ["Au :"] = "To:",
            ["Charger"] = "Load",
            ["Soldes des Portefeuilles"] = "Wallet Balances",
            ["Numéro Client"] = "Customer Number",
            ["Solde Porte-monnaie"] = "Wallet Balance",
            ["Historique des Transactions"] = "Transaction History",
            ["Date & Heure"] = "Date & Time",
            ["Téléphone"] = "Phone",
            ["Facture"] = "Invoice",
            ["Opération"] = "Operation",
            ["Montant"] = "Amount",
            ["Épargne"] = "Savings",
            ["Débit"] = "Debit",

            // Reports dashboard tabs & buttons (frmRecords)
            ["RAPPORTS ET DONNÉES"] = "REPORTS & DATA",
            ["Meilleures Ventes"] = "Top Selling",
            ["Articles vendus"] = "Sold Items",
            ["Stocks Critiques"] = "Critical Stocks",
            ["Liste d'Inventaire"] = "Inventory List",
            ["Commandes Annulées"] = "Cancelled Orders",
            ["Historique des Entrées"] = "Stock Entry History",
            ["CHARGER DONNÉES"] = "LOAD DATA",
            ["Aperçu Impression"] = "Print Preview",
            ["Filtrer par"] = "Filter by",
            ["CHARGER GRAPH."] = "LOAD CHART",
            ["IMPRIMER"] = "PRINT",
            ["TRIER PAR QTÉ"] = "SORT BY QTY",
            ["TRIER PAR MONTANT TOTAL"] = "SORT BY TOTAL AMOUNT",

            // Grid headers in Reports dashboard
            ["VENTES TOTALES"] = "TOTAL SALES",
            ["CODE PROD."] = "PRODUCT CODE",
            ["SEUIL ALERTE"] = "ALERT THRESHOLD",
            ["STOCK DISPO."] = "STOCK ON HAND",
            ["N° TRANS."] = "TRANS NO.",
            ["ANNULÉ PAR"] = "VOIDED BY",
            ["CONFIRMÉ PAR"] = "CONFIRMED BY",
            ["MOTIF"] = "REASON",
            ["RÉF N°"] = "REF NO.",
            ["DATE ENTRÉE"] = "STOCK IN DATE",
            ["ENTRÉ PAR"] = "STOCK IN BY",

            // Store Settings (frmStore)
            ["Paramètres de la Boutique"] = "Store Settings",
            ["Adresse de la Boutique"] = "Store Address",
            ["Nom de la Boutique"] = "Store Name",
            ["Code MTN MoMo Marchand"] = "MTN MoMo Merchant Code",
            ["Code Orange Money Marchand"] = "Orange Money Merchant Code",
            ["ID Boutique"] = "Store ID",
            ["ID Caisse"] = "Register ID",
            ["Connexion Base Centrale"] = "Central DB Connection",
            ["ENREGISTRER"] = "SAVE",
            ["ANNULER"] = "CANCEL",

            // Dashboard card descriptions
            ["Chiffre d'affaires de la journée"] = "Daily revenue",
            ["Nombre de produits enregistrés"] = "Number of registered products",
            ["Quantité totale d'articles en stock"] = "Total stock quantity",
            ["Articles sous le seuil critique"] = "Items below critical threshold",

            // User Accounts (frmUserAccounts)
            ["COMPTES UTILISATEURS"] = "USER ACCOUNTS",
            ["Créer un Compte"] = "Create Account",
            ["Modifier le Mot de Passe"] = "Change Password",
            ["ACTIVER / DÉSACTIVER LE COMPTE"] = "ACTIVATE / DEACTIVATE ACCOUNT",
            ["SAUVEGARDE ET RESTAURATION"] = "BACKUP AND RESTORE",
            ["Nom"] = "Name",
            ["Rôle"] = "Role",
            ["Confirmer mot de passe"] = "Confirm Password",
            ["Ancien mot de passe"] = "Old Password",
            ["Confirmer nouveau mot de passe"] = "Confirm New Password",
            ["Nouveau mot de passe"] = "New Password",
            ["Actif"] = "Active",
            ["SAUVEGARDER"] = "BACKUP",
            ["RESTAURER"] = "RESTORE",
            ["PARCOURIR"] = "BROWSE",

            // Product List & Management
            ["LISTE DES PRODUITS"] = "PRODUCT LIST",
            ["Rechercher ici..."] = "Search here...",

            // Stock Entry (frmStockIn)
            ["MODULE D'ENTRÉE EN STOCK"] = "STOCK ENTRY MODULE",
            ["Saisie Stock"] = "Stock Entry",
            ["Effacer"] = "Clear",
            ["[ GÉNÉRER ]"] = "[ GENERATE ]",
            [" PRODUITS "] = " PRODUCTS ",
            ["Date d'entrée :"] = "Stock In Date:",
            ["ID Fournisseur"] = "Vendor ID",
            ["Enregistré par :"] = "Stock In By:",
            ["N° Référence :"] = "Reference No:",
            ["Historique des entrées"] = "Stock Entry History",
            ["Charger les données"] = "Load Data",
            ["Filtrer par date"] = "Filter by Date",

            // Stock Adjustment (frmAdjustment)
            ["AJUSTEMENT DE STOCK"] = "STOCK ADJUSTMENT",
            ["N° RÉFÉRENCE"] = "REFERENCE NO",
            ["REMARQUES"] = "REMARKS",

            // General DataGridView Headers
            ["CODE PRODUIT"] = "PRODUCT CODE",
            ["CODE-BARRES"] = "BARCODE",
            ["DESCRIPTION"] = "DESCRIPTION",
            ["PRIX"] = "PRICE",
            ["QTÉ"] = "QTY",
            ["TOTAL LIGNE"] = "LINE TOTAL",
            ["MARQUE"] = "BRAND",
            ["CATÉGORIE"] = "CATEGORY",
            ["STOCK REEL"] = "STOCK ON HAND",
            ["SEUIL D'ALERTE"] = "ALERT THRESHOLD",
            ["UTILISATEUR"] = "USER",
            ["STATUT"] = "STATUS",

            // Vendor/Fournisseur Module
            ["Détails du Fournisseur"] = "Vendor Details",
            ["FOURNISSEUR"] = "VENDOR",
            ["ADRESSE"] = "ADDRESS",
            ["PERSONNE DE CONTACT"] = "CONTACT PERSON",
            ["TÉLÉPHONE"] = "PHONE",
            ["EMAIL"] = "EMAIL",
            ["METTRE À JOUR"] = "UPDATE",
            ["METTRE A JOUR"] = "UPDATE",
            ["LISTE DES FOURNISSEURS"] = "VENDORS LIST",
            ["Fournisseur"] = "Vendor",

            // Category Module
            ["Module Catégorie"] = "Category Module",
            ["Nom de la Catégorie"] = "Category Name",
            ["LISTE DES CATÉGORIES"] = "CATEGORIES LIST",
            ["CATÉGORIE"] = "CATEGORY",

            // Brand Module
            ["Module Marque"] = "Brand Module",
            ["Nom de la Marque"] = "Brand Name",
            ["LISTE DES MARQUES"] = "BRANDS LIST",
            ["MARQUE"] = "BRAND",

            // Product Module
            ["Module Produit"] = "Product Module",
            ["Code-barres"] = "Barcode",
            ["Prix"] = "Price",
            ["Seuil réapprovisionnement"] = "Reorder Threshold",
            ["SEUIL REAPPRO."] = "REORDER THRESHOLD",
            ["Rechercher des Produits"] = "Search Products",

            // General & Common Buttons
            ["Mettre à jour"] = "Update",
            ["Enregistrer"] = "Save",
            ["Saisir la Quantité"] = "Enter Quantity",

            // Search Bar placeholders & related
            ["Rechercher ici.."] = "Search here...",
            ["Confirmer le nouveau mot de passe"] = "Confirm New Password",
            ["Sélectionner"] = "Select",
            ["&Sélectionner"] = "&Select",
            ["Sélectionner.."] = "Select...",

            // Sales Report (frmSoldItems)
            ["Rapport des Ventes"] = "Sales Report",
            ["N° FACTURE"] = "INVOICE NO.",
            ["CODE PROD"] = "PRODUCT CODE",
            ["ANNULER ARTICLE(S)"] = "CANCEL ITEM(S)",
            ["Annulé par (Caissier)"] = "Cancelled by (Cashier)",
            ["Raison(s)"] = "Reason(s)",
            ["Remettre en stock"] = "Return to Stock",
            ["Quantité à annuler"] = "Quantity to Cancel",
            ["Nombre de produits enregistrés"] = "Number of Registered Products",
            ["Quantité totale d'articles en stock"] = "Total Stock Quantity",
            ["Annulé par (Admin)"] = "Cancelled by (Admin)",
            ["Rapport Graphique d'Inventaire"] = "Graphical Inventory Report",
            ["Annulation de Commande"] = "Order Cancellation",
            ["DÉTAILS D'ANNULATION DE COMMANDE"] = "ORDER CANCELLATION DETAILS",
            ["Appliquer une Remise"] = "Apply Discount",

            // Consolidation
            ["Consolidation Multi-Boutiques"] = "Multi-Store Consolidation",
            ["Chiffre d'affaires par Boutique"] = "Revenue by Store",
            ["Boutique ID"] = "Store ID",
            ["Chiffre d'affaires"] = "Revenue",
            ["Consolidation"] = "Consolidation",
            ["Dernière Vente"] = "Last Sale"
        };

        private static readonly Dictionary<string, string> FrenchTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static LanguageManager()
        {
            // Auto-populate reverse lookup dictionary
            foreach (var kvp in EnglishTranslations)
            {
                if (!FrenchTranslations.ContainsKey(kvp.Value))
                {
                    FrenchTranslations.Add(kvp.Value, kvp.Key);
                }
            }

            // Manual additions for translating English defaults/watermarks back to French
            FrenchTranslations["Search Here"] = "Rechercher ici...";
            FrenchTranslations["Search here..."] = "Rechercher ici...";
        }

        public static void ApplyLanguage(Form form)
        {
            // Translate Form Title
            string trimmedTitle = form.Text?.Trim();
            if (!string.IsNullOrEmpty(trimmedTitle))
            {
                var dict = CurrentLanguage == "Français" ? FrenchTranslations : EnglishTranslations;
                if (dict.ContainsKey(trimmedTitle))
                {
                    form.Text = dict[trimmedTitle];
                }
            }

            TranslateControls(form.Controls);
        }

        private static void TranslateControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                TranslateSingleControl(ctrl);

                if (ctrl.HasChildren)
                {
                    TranslateControls(ctrl.Controls);
                }
            }
        }

        private static void TranslateSingleControl(Control ctrl)
        {
            var dict = CurrentLanguage == "Français" ? FrenchTranslations : EnglishTranslations;

            // Simple text translation (preserving leading/trailing spaces)
            if (ctrl is Label || ctrl is Button || ctrl is CheckBox || ctrl is RadioButton || ctrl is GroupBox)
            {
                string trimmedText = ctrl.Text?.Trim();
                if (!string.IsNullOrEmpty(trimmedText) && dict.ContainsKey(trimmedText))
                {
                    string leadingSpaces = ctrl.Text.Substring(0, ctrl.Text.Length - ctrl.Text.TrimStart().Length);
                    string trailingSpaces = ctrl.Text.Substring(ctrl.Text.TrimEnd().Length);
                    ctrl.Text = leadingSpaces + dict[trimmedText] + trailingSpaces;
                }
            }
            // MetroTextBox placeholder
            else if (ctrl is MetroTextBox metroTxt)
            {
                string trimmedWatermark = metroTxt.WaterMark?.Trim();
                if (!string.IsNullOrEmpty(trimmedWatermark) && dict.ContainsKey(trimmedWatermark))
                {
                    metroTxt.WaterMark = dict[trimmedWatermark];
                }

                string trimmedText = metroTxt.Text?.Trim();
                if (!string.IsNullOrEmpty(trimmedText) && dict.ContainsKey(trimmedText))
                {
                    metroTxt.Text = dict[trimmedText];
                }
            }
            // TabPages in TabControls
            else if (ctrl is TabControl tabControl)
            {
                foreach (TabPage page in tabControl.TabPages)
                {
                    string trimmedText = page.Text?.Trim();
                    if (!string.IsNullOrEmpty(trimmedText) && dict.ContainsKey(trimmedText))
                    {
                        page.Text = dict[trimmedText];
                    }
                }
            }
            // DataGridView Columns
            else if (ctrl is DataGridView dgv)
            {
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    string trimmedText = col.HeaderText?.Trim();
                    if (!string.IsNullOrEmpty(trimmedText) && dict.ContainsKey(trimmedText))
                    {
                        col.HeaderText = dict[trimmedText];
                    }
                }
            }
        }
    }
}
