using System;
using System.Globalization;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class ProductPresenter
    {
        private readonly IProductView _view;
        private readonly IProductRepository _repository;

        public ProductPresenter(IProductView view, IProductRepository repository)
        {
            _view = view;
            _repository = repository;
        }

        public void LoadBrandsAndCategories()
        {
            try
            {
                var brands = _repository.GetBrandNames();
                var categories = _repository.GetCategoryNames();
                _view.SetBrands(brands);
                _view.SetCategories(categories);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public bool Save()
        {
            string pcode = _view.ProductCode.Trim();
            string barcode = _view.Barcode.Trim();
            string description = _view.Description.Trim();
            string brandName = _view.BrandName;
            string categoryName = _view.CategoryName;

            if (string.IsNullOrEmpty(pcode))
            {
                _view.ShowMessage("Le code produit ne peut pas être vide.", "Champs requis", true);
                return false;
            }
            if (string.IsNullOrEmpty(description))
            {
                _view.ShowMessage("La description ne peut pas être vide.", "Champs requis", true);
                return false;
            }
            if (string.IsNullOrEmpty(brandName))
            {
                _view.ShowMessage("Veuillez sélectionner une marque.", "Champs requis", true);
                return false;
            }
            if (string.IsNullOrEmpty(categoryName))
            {
                _view.ShowMessage("Veuillez sélectionner une catégorie.", "Champs requis", true);
                return false;
            }

            double price = 0;
            string priceStr = _view.Price.Replace(" FCFA", "").Replace(" ", "").Trim();
            if (!double.TryParse(priceStr, NumberStyles.Any, CultureInfo.CurrentCulture, out price) &&
                !double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
            {
                _view.ShowMessage("Veuillez saisir un prix valide.", "Erreur de format", true);
                return false;
            }

            int reorder = 0;
            if (!int.TryParse(_view.Reorder.Trim(), out reorder))
            {
                _view.ShowMessage("Veuillez saisir un seuil de réapprovisionnement valide.", "Erreur de format", true);
                return false;
            }

            if (_repository.Exists(pcode))
            {
                _view.ShowMessage("Ce code produit existe déjà.", "Erreur de doublon", true);
                return false;
            }

            if (_view.ConfirmMessage("Voulez-vous vraiment enregistrer ce produit ?", "Enregistrer le Produit"))
            {
                try
                {
                    int bid = _repository.GetBrandIdByName(brandName);
                    int cid = _repository.GetCategoryIdByName(categoryName);

                    var product = new Product
                    {
                        ProductCode = pcode,
                        Barcode = barcode,
                        Description = description,
                        BrandId = bid,
                        CategoryId = cid,
                        Price = price,
                        Reorder = reorder
                    };

                    _repository.Add(product);
                    _view.ShowMessage("Le produit a été enregistré avec succès.", "Enregistrer le Produit");
                    _view.ClearView();
                    return true;
                }
                catch (Exception ex)
                {
                    _view.ShowMessage(ex.Message, "Erreur", true);
                }
            }
            return false;
        }

        public bool Update()
        {
            string pcode = _view.ProductCode.Trim();
            string barcode = _view.Barcode.Trim();
            string description = _view.Description.Trim();
            string brandName = _view.BrandName;
            string categoryName = _view.CategoryName;

            if (string.IsNullOrEmpty(pcode))
            {
                _view.ShowMessage("Le code produit ne peut pas être vide.", "Champs requis", true);
                return false;
            }
            if (string.IsNullOrEmpty(description))
            {
                _view.ShowMessage("La description ne peut pas être vide.", "Champs requis", true);
                return false;
            }
            if (string.IsNullOrEmpty(brandName))
            {
                _view.ShowMessage("Veuillez sélectionner une marque.", "Champs requis", true);
                return false;
            }
            if (string.IsNullOrEmpty(categoryName))
            {
                _view.ShowMessage("Veuillez sélectionner une catégorie.", "Champs requis", true);
                return false;
            }

            double price = 0;
            string priceStr = _view.Price.Replace(" FCFA", "").Replace(" ", "").Trim();
            if (!double.TryParse(priceStr, NumberStyles.Any, CultureInfo.CurrentCulture, out price) &&
                !double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
            {
                _view.ShowMessage("Veuillez saisir un prix valide.", "Erreur de format", true);
                return false;
            }

            int reorder = 0;
            if (!int.TryParse(_view.Reorder.Trim(), out reorder))
            {
                _view.ShowMessage("Veuillez saisir un seuil de réapprovisionnement valide.", "Erreur de format", true);
                return false;
            }

            if (_view.ConfirmMessage("Voulez-vous vraiment modifier ce produit ?", "Modifier le Produit"))
            {
                try
                {
                    int bid = _repository.GetBrandIdByName(brandName);
                    int cid = _repository.GetCategoryIdByName(categoryName);

                    var product = new Product
                    {
                        ProductCode = pcode,
                        Barcode = barcode,
                        Description = description,
                        BrandId = bid,
                        CategoryId = cid,
                        Price = price,
                        Reorder = reorder
                    };

                    _repository.Update(product);
                    _view.ShowMessage("Le produit a été mis à jour avec succès.", "Modifier le Produit");
                    _view.ClearView();
                    return true;
                }
                catch (Exception ex)
                {
                    _view.ShowMessage(ex.Message, "Erreur", true);
                }
            }
            return false;
        }
    }
}
