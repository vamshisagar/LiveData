using LiveData.Models;
using Microsoft.Azure.Cosmos;

namespace LiveData.Repository
{
    public interface ILiveSiteRepository
    {
        Task<CountOfIncidents> GetIncidentCountsAsync();
        Task<List<LiveSiteRecord>> GetRecordsAsync(string containerId);
    }
}
