namespace Aiursoft.NetworkTest.Models;

public class EndpointTestResult
{
    public required string EndpointName { get; init; }
    public required string Url { get; init; }
    public required List<double> Latencies { get; init; }
    public int FailedCount { get; init; }
    public double MinLatency => Latencies.Count > 0 ? Latencies.Min() : 0;
    public double MaxLatency => Latencies.Count > 0 ? Latencies.Max() : 0;
    public double AverageLatency => Latencies.Count > 0 ? Latencies.Average() : 0;
    public double MedianLatency
    {
        get
        {
            if (Latencies.Count == 0) return 0;
            var sorted = Latencies.OrderBy(x => x).ToList();
            var mid = sorted.Count / 2;
            return sorted.Count % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2 : sorted[mid];
        }
    }
    public double PercentageOver100Ms => Latencies.Count > 0 ? (double)Latencies.Count(x => x > 100) / Latencies.Count * 100 : 0;
    public double PercentageOver200Ms => Latencies.Count > 0 ? (double)Latencies.Count(x => x > 200) / Latencies.Count * 100 : 0;
    public double PercentageOver300Ms => Latencies.Count > 0 ? (double)Latencies.Count(x => x > 300) / Latencies.Count * 100 : 0;
    
    public double StandardDeviation
    {
        get
        {
            if (Latencies.Count == 0) return 0;
            var avg = Latencies.Average();
            var sumOfSquares = Latencies.Sum(x => Math.Pow(x - avg, 2));
            return Math.Sqrt(sumOfSquares / Latencies.Count);
        }
    }
}
