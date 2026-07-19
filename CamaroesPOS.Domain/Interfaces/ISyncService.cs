using System.Threading.Tasks;

namespace CamaroesPOS.Domain.Interfaces
{
    public interface ISyncService
    {
        /// <summary>
        /// Synchronise les données locales vers le serveur central.
        /// </summary>
        Task PushDataAsync();

        /// <summary>
        /// Récupère les mises à jour du serveur central (nouveaux produits, prix).
        /// </summary>
        Task PullDataAsync();
    }
}
