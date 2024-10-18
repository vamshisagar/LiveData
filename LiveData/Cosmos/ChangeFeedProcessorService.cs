using LiveData.Hubs;
using LiveData.Models;
using LiveData.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.RecordIO;
using static DataController;

namespace LiveData.Cosmos
{
    public class ChangeFeedProcessorService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IHubContext<DataHub> _hubContext;
        private readonly ILiveSiteRepository liveSiteRepository;
        private readonly string _databaseId = "MyDatabase";
        private readonly string _table1ContainerId = "Table1Container";
        private readonly string _table2ContainerId = "Table2Container";
        private readonly string _incidentsContainerId = "Incidents";
        private Container _table1Container;
        private Container _table2Container;
        private Container _incidentsContainer;

        public ChangeFeedProcessorService(CosmosClient cosmosClient, IHubContext<DataHub> hubContext, ILiveSiteRepository liveSiteRepository)
        {
            _cosmosClient = cosmosClient;
            _hubContext = hubContext;
            this.liveSiteRepository = liveSiteRepository;
            _table1Container = _cosmosClient.GetContainer(_databaseId, _table1ContainerId);
            _table2Container = _cosmosClient.GetContainer(_databaseId, _table2ContainerId);
            _incidentsContainer = _cosmosClient.GetContainer(_databaseId, _incidentsContainerId);
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

            // Change feed processor for incidents
            ChangeFeedProcessor incidentsProcessor = _incidentsContainer
                .GetChangeFeedProcessorBuilder<dynamic>("incidentsProcessor", ProcessIncidentChangesAsync)
                .WithInstanceName("incidentsProcessor")
                .WithLeaseContainer(leaseContainer)
                .Build();

            await table1Processor.StartAsync();
            await table2Processor.StartAsync();
            await incidentsProcessor.StartAsync();

        }

        private async Task ProcessTable1ChangesAsync(IReadOnlyCollection<dynamic> changes, CancellationToken cancellationToken)
        {
            var allTable1Records = await liveSiteRepository.GetAllRecordsAsync(_table1Container);
            await _hubContext.Clients.All.SendAsync("ReceiveTable1Update", allTable1Records);
        }

        private async Task ProcessTable2ChangesAsync(IReadOnlyCollection<dynamic> changes, CancellationToken cancellationToken)
        {
            var allTable2Records = await liveSiteRepository.GetAllRecordsAsync(_table2Container);
            await _hubContext.Clients.All.SendAsync("ReceiveTable2Update", allTable2Records);
        }

        private async Task ProcessIncidentChangesAsync(IReadOnlyCollection<dynamic> changes, CancellationToken cancellationToken)
        {
            // Fetch all records from the incidents container when any change is detected
            var allIncidents = await liveSiteRepository.GetAllIncidentsRecordsAsync(_incidentsContainer);

            // Send the updated incidents to all connected clients
            await _hubContext.Clients.All.SendAsync("ReceiveIncidentUpdate", allIncidents);
        }
    }
}
