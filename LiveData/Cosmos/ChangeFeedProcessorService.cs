using LiveData.Hubs;
using LiveData.Models;
using LiveData.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LiveData.Cosmos
{
    public class ChangeFeedProcessorService
    {
        private readonly ILiveSiteRepository _liveSiteRepository;
        private readonly IHubContext<DataHub> _hubContext;
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseId = "MyDatabase";
        private readonly string _leaseContainerId = "leases";

        private ChangeFeedProcessor _table1Processor;
        private ChangeFeedProcessor _table2Processor;
        private ChangeFeedProcessor _incidentsProcessor;

        public ChangeFeedProcessorService(CosmosClient cosmosClient, IHubContext<DataHub> hubContext, ILiveSiteRepository liveSiteRepository)
        {
            _cosmosClient = cosmosClient;
            _hubContext = hubContext;
            _liveSiteRepository = liveSiteRepository;
        }

        public async Task StartChangeFeedProcessorAsync()
        {
            Container leaseContainer = _cosmosClient.GetContainer(_databaseId, _leaseContainerId);

            // Initialize change feed processors
            _table1Processor = _cosmosClient.GetContainer(_databaseId, "Table1Container")
                                            .GetChangeFeedProcessorBuilder<dynamic>("table1Processor", (changes, token) => ProcessTable1ChangesAsync(changes, token))
                                            .WithInstanceName("table1Processor")
                                            .WithLeaseContainer(leaseContainer)
                                            .Build();

            _table2Processor = _cosmosClient.GetContainer(_databaseId, "Table2Container")
                                            .GetChangeFeedProcessorBuilder<dynamic>("table2Processor", (changes, token) => ProcessTable2ChangesAsync(changes, token))
                                            .WithInstanceName("table2Processor")
                                            .WithLeaseContainer(leaseContainer)
                                            .Build();

            _incidentsProcessor = _cosmosClient.GetContainer(_databaseId, "Incidents")
                                               .GetChangeFeedProcessorBuilder<dynamic>("incidentsProcessor", (changes, token) => ProcessIncidentChangesAsync(changes, token))
                                               .WithInstanceName("incidentsProcessor")
                                               .WithLeaseContainer(leaseContainer)
                                               .Build();

            // Start all processors
            await _table1Processor.StartAsync();
            await _table2Processor.StartAsync();
            await _incidentsProcessor.StartAsync();
        }


        public async Task StopChangeFeedProcessorAsync()
        {
            // Stop all processors gracefully
            if (_table1Processor != null) await _table1Processor.StopAsync();
            if (_table2Processor != null) await _table2Processor.StopAsync();
            if (_incidentsProcessor != null) await _incidentsProcessor.StopAsync();
        }


        private async Task ProcessTable1ChangesAsync(IReadOnlyCollection<dynamic> changes, CancellationToken cancellationToken)
        {
            // Fetch all records from Table1Container and send them via SignalR
            var table1Records = await _liveSiteRepository.GetRecordsAsync("Table1Container");
            await _hubContext.Clients.All.SendAsync("ReceiveTable1Update", table1Records, cancellationToken);
        }

        private async Task ProcessTable2ChangesAsync(IReadOnlyCollection<dynamic> changes, CancellationToken cancellationToken)
        {
            // Fetch all records from Table2Container and send them via SignalR
            var table2Records = await _liveSiteRepository.GetRecordsAsync("Table2Container");
            await _hubContext.Clients.All.SendAsync("ReceiveTable2Update", table2Records, cancellationToken);
        }

        private async Task ProcessIncidentChangesAsync(IReadOnlyCollection<dynamic> changes, CancellationToken cancellationToken)
        {
            // Fetch incident counts and send them via SignalR
            var incidentCounts = await _liveSiteRepository.GetIncidentCountsAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveIncidentUpdate", incidentCounts, cancellationToken);
        }
    }
}
