namespace FinalPOS.Domain
{
    public interface IStoreRepository
    {
        Store GetStoreDetails();
        void SaveStoreDetails(Store store);
        string GetLocalSetting(string key);
        void SaveLocalSetting(string key, string value);
    }
}
