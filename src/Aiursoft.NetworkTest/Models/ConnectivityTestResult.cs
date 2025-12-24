namespace Aiursoft.NetworkTest.Models;

public class ConnectivityTestResult
{
    public required string TestName { get; init; }
    public int SuccessfulEndpoints { get; init; }
    public int TotalEndpoints { get; init; }
    public double Score { get; init; }
    public string? DetectedIP { get; init; }
    public bool HasPublicIP { get; init; }
    public string Notes { get; init; } = string.Empty;
}
