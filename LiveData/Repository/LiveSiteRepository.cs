using LiveData.Models;
using Microsoft.Azure.Cosmos;

namespace LiveData.Repository
{
    public class LiveSiteRepository : ILiveSiteRepository
    {
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseId = "MyDatabase";

        public LiveSiteRepository(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }

        private Container GetContainer(string containerId)
        {
            return _cosmosClient.GetContainer(_databaseId, containerId);
        }

        public async Task<CountOfIncidents> GetIncidentCountsAsync()
        {
            var container = GetContainer("Incidents");
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

        public async Task<List<LiveSiteRecord>> GetRecordsAsync(string containerId)
        {
            var container = GetContainer(containerId);
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
