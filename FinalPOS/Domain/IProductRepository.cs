using System.Collections.Generic;

namespace FinalPOS.Domain
{
    public interface IProductRepository
    {
        IEnumerable<Product> Search(string searchText);
        void Add(Product product);
        void Update(Product product);
        void Delete(string pcode);
        bool Exists(string pcode);
        IEnumerable<string> GetBrandNames();
        IEnumerable<string> GetCategoryNames();
        int GetBrandIdByName(string brandName);
        int GetCategoryIdByName(string categoryName);
        IEnumerable<Product> GetCriticalProducts();
        Product GetByBarcode(string barcode);
    }
}
