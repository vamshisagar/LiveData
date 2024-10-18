using LiveData.Models;
using Microsoft.Azure.Cosmos;

namespace LiveData.Repository
{
    public interface ILiveSiteRepository
    {
        Task<List<LiveSiteRecord>> GetAllRecordsAsync(Container container);
    }
}
