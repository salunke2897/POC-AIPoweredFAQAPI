namespace POC_AIPoweredFAQAPI.Models;

public class RagOptions
{
    public string Provider { get; set; } = "Sqlite";
    public int MaxResults { get; set; } = 5;
}
