using LiveData.Models;
using Microsoft.Azure.Cosmos;

namespace LiveData.Repository
{
    public interface ILiveSiteRepository
    {
        Task<CountOfIncidents> GetAllIncidentsRecordsAsync(Container container);
        Task<List<LiveSiteRecord>> GetAllRecordsAsync(Container container);
    }
}
