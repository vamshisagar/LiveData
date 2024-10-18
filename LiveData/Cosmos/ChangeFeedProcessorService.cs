using LiveData.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;
using static DataController;

namespace LiveData.Cosmos
{
    public class ChangeFeedProcessorService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IHubContext<DataHub> _hubContext;
        private readonly string _databaseId = "MyDatabase";
        private readonly string _table1ContainerId = "Table1Container";
        private readonly string _table2ContainerId = "Table2Container";
        private Container _table1Container;
        private Container _table2Container;

        public ChangeFeedProcessorService(CosmosClient cosmosClient, IHubContext<DataHub> hubContext)
        {
            _cosmosClient = cosmosClient;
            _hubContext = hubContext;
            _table1Container = _cosmosClient.GetContainer(_databaseId, _table1ContainerId);
            _table2Container = _cosmosClient.GetContainer(_databaseId, _table2ContainerId);
        }

        public async Task StartChangeFeedProcessorAsync()
        {
            Container leaseContainer = _cosmosClient.GetContainer(_databaseId, "leases");

            // Table 1 change feed processor
            ChangeFeedProcessor table1Processor = _table1Container
                .GetChangeFeedProcessorBuilder<dynamic>("table1Processor", ProcessTable1ChangesAsync)
                .WithInstanceName("table1Processor")
                .WithLeaseContainer(leaseContainer)
                .Build();

            // Table 2 change feed processor
            ChangeFeedProcessor table2Processor = _table2Container
                .GetChangeFeedProcessorBuilder<dynamic>("table2Processor", ProcessTable2ChangesAsync)
                .WithInstanceName("table2Processor")
                .WithLeaseContainer(leaseContainer)
                .Build();

            await table1Processor.StartAsync();
            await table2Processor.StartAsync();
        }

        private async Task ProcessTable1ChangesAsync(IReadOnlyCollection<dynamic> changes, CancellationToken cancellationToken)
        {
            var allTable1Records = await GetAllRecordsAsync(_table1Container);
            await _hubContext.Clients.All.SendAsync("ReceiveTable1Update", allTable1Records);
        }

        private async Task ProcessTable2ChangesAsync(IReadOnlyCollection<dynamic> changes, CancellationToken cancellationToken)
        {
            var allTable2Records = await GetAllRecordsAsync(_table2Container);
            await _hubContext.Clients.All.SendAsync("ReceiveTable2Update", allTable2Records);
        }

        private static async Task<List<Record>> GetAllRecordsAsync(Container container)
        {
            var records = new List<Record>();
            var iterator = container.GetItemQueryIterator<Record>("SELECT * FROM c");

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                records.AddRange(response);
            }

            return records;
        }
    }
}
