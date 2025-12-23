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
        ("Cloudflare Trace", "https://www.cloudflare.com/cdn-cgi/trace"),
        ("Google Gen204", "https://www.google.com/generate_204"),
        ("MS Connect Test", "http://www.msftconnecttest.com/connecttest.txt"),
        ("Apple Captive", "http://captive.apple.com/hotspot-detect.html"),
        ("AWS CheckIP", "https://checkip.amazonaws.com"),
        ("Gstatic Gen204", "http://connectivitycheck.gstatic.com/generate_204"),
        ("Firefox Detect", "http://detectportal.firefox.com/success.txt"),
        ("GitHub Zen", "https://api.github.com/zen"),
        ("Cloudflare DNS", "https://1.0.0.1/")
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

        // Calculate score: 170 - average latency, then deduct 5 points per failure
        var overallAverageLatency = results.Average(r => r.AverageLatency);
        var totalFailures = results.Sum(r => r.FailedCount);
        var totalRequests = _endpoints.Count * 6;  // Total number of requests attempted
        var successfulRequests = totalRequests - totalFailures;
        var baseScore = Math.Max(0, Math.Min(100, 170 - overallAverageLatency));
        var score = Math.Max(0, baseScore - (totalFailures * 5));

        // Display scoring breakdown
        Console.WriteLine();
        Console.WriteLine("Scoring Breakdown:");
        Console.WriteLine($"  - Average Latency: {overallAverageLatency:F2} ms");
        Console.WriteLine($"  - Successful Requests: {successfulRequests}/{totalRequests}");
        Console.WriteLine($"  - Failed Requests: {totalFailures}");
        Console.WriteLine($"  - Base Score (170 - avg latency): {baseScore:F2}");
        if (totalFailures > 0)
        {
            Console.WriteLine($"  - Failure Penalty: -{totalFailures * 5} ({totalFailures} × 5)");
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
