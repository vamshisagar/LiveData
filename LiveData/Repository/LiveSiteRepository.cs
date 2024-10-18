using LiveData.Models;
using Microsoft.Azure.Cosmos;

namespace LiveData.Repository
{
    public class LiveSiteRepository : ILiveSiteRepository
    {
        public LiveSiteRepository() { }

        async Task<List<LiveSiteRecord>> ILiveSiteRepository.GetAllRecordsAsync(Container container)
        {
            var records = new List<LiveSiteRecord>();
            var iterator = container.GetItemQueryIterator<LiveSiteRecord>("SELECT * FROM c");

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                records.AddRange(response);
            }

            return records;
        }
    }
}
