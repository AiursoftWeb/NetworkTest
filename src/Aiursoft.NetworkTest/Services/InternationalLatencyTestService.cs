using System.Diagnostics;
using Aiursoft.NetworkTest.Models;

namespace Aiursoft.NetworkTest.Services;

public class InternationalLatencyTestService : ITestService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TableRenderer _tableRenderer;
    private static readonly Random _random = new();

    public string TestName => "International Web Latency";

    private readonly List<(string Name, string Url)> _endpoints = new()
    {
        // 纯文本/极简返回，适合做 HTTP 连通性测试
        ("Android (Gstatic)", "http://connectivitycheck.gstatic.com/generate_204"), // 安卓原生检测，不同于 google.com，走的是 gstatic CDN
        ("Cloudflare CP", "http://cp.cloudflare.com/"), // 也是 204，很多旁路检测工具喜欢用这个
        ("Cloudflare Trace", "https://www.cloudflare.com/cdn-cgi/trace"),
        ("Google Gen204", "https://www.google.com/generate_204"),
        ("MS Connect Test", "http://www.msftconnecttest.com/connecttest.txt"),
        ("Apple Captive", "http://captive.apple.com/hotspot-detect.html"),
        ("AWS CheckIP", "https://checkip.amazonaws.com"),
        ("Firefox Detect", "http://detectportal.firefox.com/success.txt"),
    };

    public InternationalLatencyTestService(
        IHttpClientFactory httpClientFactory,
        TableRenderer tableRenderer)
    {
        _httpClientFactory = httpClientFactory;
        _tableRenderer = tableRenderer;
    }

    public async Task<double> RunTestAsync(bool verbose = false)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {TestName} Test ===");
        Console.WriteLine($"Testing {_endpoints.Count} endpoints with 6 requests each...");
        Console.WriteLine();

        var results = new List<EndpointTestResult>();

        // Test all endpoints in parallel
        var tasks = _endpoints.Select(endpoint => TestEndpointAsync(endpoint.Name, endpoint.Url, verbose));
        var endpointResults = await Task.WhenAll(tasks);
        results.AddRange(endpointResults);

        // Render final table
        _tableRenderer.RenderTestResultsTable(results);

        // Calculate comprehensive metrics
        var overallAverageLatency = results.Average(r => r.AverageLatency);
        var overallMinLatency = results.Min(r => r.MinLatency > 0 ? r.MinLatency : double.MaxValue);
        if (overallMinLatency == double.MaxValue) overallMinLatency = 0;
        
        // Calculate overall standard deviation (weighted by number of samples)
        var allLatencies = results.SelectMany(r => r.Latencies).ToList();
        var overallStandardDeviation = allLatencies.Count > 0 
            ? Math.Sqrt(allLatencies.Sum(x => Math.Pow(x - overallAverageLatency, 2)) / allLatencies.Count)
            : 0;

        // Collect failures per endpoint for smart penalty calculation
        var failuresByEndpoint = results.Select(r => r.FailedCount).ToList();
        var totalFailures = failuresByEndpoint.Sum();
        var totalRequests = _endpoints.Count * 6;
        var successfulRequests = totalRequests - totalFailures;

        // Use new comprehensive scoring algorithm
        const double internationalAvgBaselineLatency = 70.0; // 70ms baseline for average latency
        const double internationalMinBaselineLatency = 50.0; // 50ms baseline for minimum latency (stricter)
        
        var score = LatencyScoreCalculator.CalculateComprehensiveScore(
            overallMinLatency, 
            overallAverageLatency, 
            overallStandardDeviation,
            internationalMinBaselineLatency,
            internationalAvgBaselineLatency,
            failuresByEndpoint);

        // Calculate individual component scores for display
        var minLatencyScore = LatencyScoreCalculator.CalculateLatencyScore(overallMinLatency, internationalMinBaselineLatency);
        var avgLatencyScore = LatencyScoreCalculator.CalculateLatencyScore(overallAverageLatency, internationalAvgBaselineLatency);
        var stabilityScore = LatencyScoreCalculator.CalculateStabilityScore(overallStandardDeviation);
        var failurePenalty = LatencyScoreCalculator.CalculateSmartFailurePenalty(failuresByEndpoint);

        // Display scoring breakdown
        Console.WriteLine();
        Console.WriteLine("Network Metrics:");
        Console.WriteLine($"  - Minimum Latency: {overallMinLatency:F2} ms (Best link quality)");
        Console.WriteLine($"  - Average Latency: {overallAverageLatency:F2} ms (Overall performance)");
        Console.WriteLine($"  - Standard Deviation: {overallStandardDeviation:F2} ms (Stability)");
        Console.WriteLine($"  - Successful Requests: {successfulRequests}/{totalRequests}");
        if (totalFailures > 0)
        {
            var maxFailuresInSingleEndpoint = failuresByEndpoint.Max();
            var effectiveFailures = totalFailures - maxFailuresInSingleEndpoint;
            Console.WriteLine($"  - Failed Requests: {totalFailures} (worst endpoint: {maxFailuresInSingleEndpoint}, counted: {effectiveFailures})");
        }
        
        Console.WriteLine();
        Console.WriteLine("Scoring Breakdown:");
        Console.WriteLine($"  - Min Latency Score (40% weight): {minLatencyScore:F2}");
        Console.WriteLine($"  - Avg Latency Score (40% weight): {avgLatencyScore:F2}");
        Console.WriteLine($"  - Stability Score (20% weight): {stabilityScore:F2}");
        if (failurePenalty > 0)
        {
            Console.WriteLine($"  - Smart Failure Penalty: -{failurePenalty:F2} (with 10pt hidden tolerance)");
        }

        _tableRenderer.RenderScoreSummary(TestName, score);

        return score;
    }

    private async Task<EndpointTestResult> TestEndpointAsync(string name, string url, bool verbose)
    {
        var latencies = new List<double>();
        var failedCount = 0;
        const int requestCount = 6;

        for (int i = 0; i < requestCount; i++)
        {
            try
            {
                var latency = await MeasureLatencyAsync(url);
                latencies.Add(latency);

                // Log success in verbose mode with colorful output
                if (verbose)
                {
                    Console.Write($"[{name}] Request {i + 1}: ");
                    var color = GetLatencyColor(latency);
                    Console.ForegroundColor = color;
                    Console.Write($"{latency:F2} ms");
                    Console.ResetColor();
                    Console.WriteLine(" ✓");
                }

                // Random delay between 2-5 seconds (except for the last request)
                if (i < requestCount - 1)
                {
                    var delayMs = _random.Next(2000, 5001);
                    await Task.Delay(delayMs);
                }
            }
            catch (Exception ex)
            {
                // Always log failures (both verbose and non-verbose)
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{name}] Request {i + 1}: Failed - {ex.Message}");
                Console.ResetColor();

                failedCount++;

                // Random delay between 2-5 seconds (except for the last request)
                if (i < requestCount - 1)
                {
                    var delayMs = _random.Next(2000, 5001);
                    await Task.Delay(delayMs);
                }
            }
        }

        return new EndpointTestResult
        {
            EndpointName = name,
            Url = url,
            Latencies = latencies,
            FailedCount = failedCount
        };
    }

    private async Task<double> MeasureLatencyAsync(string url)
    {
        var client = _httpClientFactory.CreateClient("InternationalQualityTest");
        var stopwatch = Stopwatch.StartNew();

        var request = new HttpRequestMessage(HttpMethod.Head, url);
        await client.SendAsync(request);

        stopwatch.Stop();

        // Don't check for success - we just want to measure latency
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private ConsoleColor GetLatencyColor(double latency)
    {
        return latency switch
        {
            < 30 => ConsoleColor.Green,
            < 50 => ConsoleColor.DarkGreen,
            < 100 => ConsoleColor.Yellow,
            < 150 => ConsoleColor.DarkYellow,
            < 200 => ConsoleColor.Magenta,
            < 300 => ConsoleColor.DarkMagenta,
            < 500 => ConsoleColor.Red,
            _ => ConsoleColor.DarkRed
        };
    }
}
