using LiveData.Models;
using Microsoft.Azure.Cosmos;

namespace LiveData.Repository
{
    public class LiveSiteRepository : ILiveSiteRepository
    {
        public LiveSiteRepository() { }

        public async Task<CountOfIncidents> GetAllIncidentsRecordsAsync(Container container)
        {
            var records = new List<IncidentsRecords>();
            var iterator = container.GetItemQueryIterator<IncidentsRecords>("SELECT * FROM c");

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                records.AddRange(response);
            }

            // Count incidents by severity
            var severityCounts = records.GroupBy(r => r.severity)
                .Select(g => new SeverityCount { Severity = g.Key, Count = g.Count() })
                .ToList();

            // Count outage incidents
            var outageCount = records.Count(r => r.outage);
            
            return new CountOfIncidents
            {
                SeverityCounts = severityCounts,
                OutageCount = outageCount
            };
        }

        public async Task<List<LiveSiteRecord>> GetAllRecordsAsync(Container container)
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
