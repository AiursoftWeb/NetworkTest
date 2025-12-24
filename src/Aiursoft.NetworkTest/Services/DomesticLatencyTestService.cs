using System.Diagnostics;
using Aiursoft.NetworkTest.Models;

namespace Aiursoft.NetworkTest.Services;

public class DomesticLatencyTestService : ITestService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TableRenderer _tableRenderer;
    private static readonly Random _random = new();

    public string TestName => "Domestic Web Latency";

    private readonly List<(string Name, string Url)> _endpoints = new()
    {
        ("Xiaomi (MIUI)", "http://connect.rom.miui.com/generate_204"),
        ("Huawei (EMUI)", "http://connectivitycheck.platform.hicloud.com/generate_204"),
        ("Vivo Check", "http://wifi.vivo.com.cn/generate_204"),
        ("Aliyun DNS", "https://dns.alidns.com/resolve?name=www.taobao.com&type=1"),
        ("Tencent DNS", "https://doh.pub/resolve?name=www.qq.com"),
        ("360 DNS", "https://doh.360.cn/resolve?name=www.360.cn"),
        ("Baidu CDN", "https://www.baidu.com/favicon.ico"),
        ("Bilibili CDN", "https://i0.hdslb.com/bfs/face/member/noface.jpg"),
        ("JD (Jingdong)", "https://www.jd.com/favicon.ico"), // 京东 CDN，非常稳
        ("NetEase (163)", "https://www.163.com/favicon.ico"), // 网易门户，老牌稳定
        ("Sogou", "https://www.sogou.com/favicon.ico"), // 搜狗，解析速度通常很快
    };

    public DomesticLatencyTestService(
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
        const double domesticAvgBaselineLatency = 30.0; // 30ms baseline for average latency
        const double domesticMinBaselineLatency = 10.0; // 10ms baseline for minimum latency (stricter)
        
        var score = LatencyScoreCalculator.CalculateComprehensiveScore(
            overallMinLatency, 
            overallAverageLatency, 
            overallStandardDeviation,
            domesticMinBaselineLatency,
            domesticAvgBaselineLatency,
            failuresByEndpoint);

        // Calculate individual component scores for display
        var minLatencyScore = LatencyScoreCalculator.CalculateLatencyScore(overallMinLatency, domesticMinBaselineLatency);
        var avgLatencyScore = LatencyScoreCalculator.CalculateLatencyScore(overallAverageLatency, domesticAvgBaselineLatency);
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
        var client = _httpClientFactory.CreateClient("DomesticQualityTest");
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
