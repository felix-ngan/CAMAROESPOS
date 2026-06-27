using System.Collections.Generic;

namespace FinalPOS.Domain
{
    public interface IBrandRepository
    {
        IEnumerable<Brand> GetAll();
        void Add(Brand brand);
        void Update(Brand brand);
        void Delete(int id);
        bool Exists(string brandName, int? excludeId = null);
    }
}
