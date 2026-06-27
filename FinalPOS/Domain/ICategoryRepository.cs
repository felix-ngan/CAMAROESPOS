using System.Collections.Generic;

namespace FinalPOS.Domain
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAll();
        void Add(Category category);
        void Update(Category category);
        void Delete(int id);
        bool Exists(string categoryName, int? excludeId = null);
    }
}
