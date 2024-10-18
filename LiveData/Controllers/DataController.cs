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

    private readonly ILiveSiteRepository _liveSiteRepository;

    public DataController(ILiveSiteRepository liveSiteRepository)
    {
        _liveSiteRepository = liveSiteRepository;
    }

    [HttpGet("table1")]
    public async Task<IActionResult> GetTable1Data()
    {
        var records = await _liveSiteRepository.GetRecordsAsync("Table1Container");
        return Ok(records);
    }

    [HttpGet("table2")]
    public async Task<IActionResult> GetTable2Data()
    {
        var records = await _liveSiteRepository.GetRecordsAsync("Table2Container");
        return Ok(records);
    }

    [HttpGet("incidents/counts")]
    public async Task<IActionResult> GetIncidentsData()
    {
        var incidentCounts = await _liveSiteRepository.GetIncidentCountsAsync();
        return Ok(incidentCounts);

    }
}
