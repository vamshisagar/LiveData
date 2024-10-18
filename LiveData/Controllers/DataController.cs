using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.RecordIO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _databaseId = "MyDatabase";
    private readonly string _table1ContainerId = "Table1Container";
    private readonly string _table2ContainerId = "Table2Container";

    public DataController(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    [HttpGet("table1")]
    public async Task<IActionResult> GetTable1Data()
    {
        var container = _cosmosClient.GetContainer(_databaseId, _table1ContainerId);
        var records = await GetAllRecordsAsync(container);
        return Ok(records);
    }

    [HttpGet("table2")]
    public async Task<IActionResult> GetTable2Data()
    {
        var container = _cosmosClient.GetContainer(_databaseId, _table2ContainerId);
        var records = await GetAllRecordsAsync(container);
        return Ok(records);
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
    public class Record
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

}
