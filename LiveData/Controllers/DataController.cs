using LiveData.Models;
using LiveData.Repository;
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
    private readonly ILiveSiteRepository liveSiteRepository;
    private readonly string _databaseId = "MyDatabase";
    private readonly string _table1ContainerId = "Table1Container";
    private readonly string _table2ContainerId = "Table2Container";
    private readonly string _incidentsContainerId = "Incidents";


    public DataController(CosmosClient cosmosClient, ILiveSiteRepository liveSiteRepository)
    {
        _cosmosClient = cosmosClient;
        this.liveSiteRepository = liveSiteRepository;
    }

    [HttpGet("table1")]
    public async Task<IActionResult> GetTable1Data()
    {
        var container = _cosmosClient.GetContainer(_databaseId, _table1ContainerId);
        var records = await liveSiteRepository.GetAllRecordsAsync(container);
        return Ok(records);
    }

    [HttpGet("table2")]
    public async Task<IActionResult> GetTable2Data()
    {
        var container = _cosmosClient.GetContainer(_databaseId, _table2ContainerId);
        var records = await liveSiteRepository.GetAllRecordsAsync(container);
        return Ok(records);
    }

    [HttpGet("incidents/counts")]
    public async Task<IActionResult> GetIncidentsData()
    {
        var container = _cosmosClient.GetContainer(_databaseId, _incidentsContainerId);
        var records = await liveSiteRepository.GetAllIncidentsRecordsAsync(container);
        return Ok(records);
    }
}
