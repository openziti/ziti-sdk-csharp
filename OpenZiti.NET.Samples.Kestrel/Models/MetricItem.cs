namespace ZitiRestServerCSharp.Models;
public class MetricItem
{
    public long Id { get; set; }
    public string? SensorGuid { get; set; }
    public string? Name { get; set; }
    public int value { get; set; }
}