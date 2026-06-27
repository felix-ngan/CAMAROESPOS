using System.Collections.Generic;

namespace FinalPOS.Domain
{
    public interface IVendorRepository
    {
        IEnumerable<Vendor> GetAll();
        void Add(Vendor vendor);
        void Update(Vendor vendor);
        void Delete(int id);
        bool Exists(string vendorName, int? excludeId = null);
    }
}
