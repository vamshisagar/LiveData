namespace LiveData.Models
{
    public class IncidentsRecords
    {
        public string id { get; set; }
        public string severity { get; set; }
        public string description { get; set; }
        public string state { get; set; }
        public bool outage { get; set; }
    }
    public class CountOfIncidents
    {
        public List<SeverityCount> SeverityCounts { get; set; }
        public int OutageCount { get; set; }
    }

    public class SeverityCount
    {
        public string Severity { get; set; }
        public int Count { get; set; }
    }
}
