using Aiursoft.NetworkTest.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.NetworkTest.Tests;

/// <summary>
/// Integration tests for LatencyTestServiceBase using a concrete implementation
/// Tests against reliable public endpoints (httpbin.org) which are stable and free
/// </summary>
[TestClass]
public class LatencyTestServiceIntegrationTests
{
    private class TestLatencyService : LatencyTestServiceBase
    {
        public override string TestName => "Test Latency";
        protected override string HttpClientName => "TestClient";
        
        // Using reliable public testing endpoints
        protected override List<(string Name, string Url)> Endpoints { get; } = new()
        {
           ("HTTPBin", "https://httpbin.org/status/200"),
            ("Example.com", "https://example.com")
        };

        public TestLatencyService(IHttpClientFactory httpClientFactory, TableRenderer tableRenderer)
            : base(httpClientFactory, tableRenderer)
        {
        }
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TableRenderer>();
        services.AddHttpClient("TestClient")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });
        
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task RunTestAsync_WithValidEndpoints_ReturnsScoreBetween0And100()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var tableRenderer = serviceProvider.GetRequiredService<TableRenderer>();
        var service = new TestLatencyService(httpClientFactory, tableRenderer);

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        var score = await service.RunTestAsync(verbose: false);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        Assert.IsTrue(score >= 0 && score <= 100, $"Score should be between 0 and 100, but was {score}");
        StringAssert.Contains(output, "Test Latency Test", "Should contain test name");
        StringAssert.Contains(output, "Network Metrics", "Should contain metrics");
    }

    [TestMethod]
    public async Task RunTestAsync_VerboseMode_OutputsDetailedInformation()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var tableRenderer = serviceProvider.GetRequiredService<TableRenderer>();
        var service = new TestLatencyService(httpClientFactory, tableRenderer);

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        var score = await service.RunTestAsync(verbose: true);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        Assert.IsTrue(score >= 0 && score <= 100);
        // In verbose mode, should show individual request results
        StringAssert.Contains(output, "Request", "Verbose mode should show individual requests");
    }

    [TestMethod]
    public async Task RunTestAsync_ShowsWarmupMessage()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var tableRenderer = serviceProvider.GetRequiredService<TableRenderer>();
        var service = new TestLatencyService(httpClientFactory, tableRenderer);

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        await service.RunTestAsync(verbose: false);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        StringAssert.Contains(output, "Warming up", "Should show warmup message");
        StringAssert.Contains(output, "Warmup completed", "Should show warmup completed");
    }

    [TestMethod]
    public async Task RunTestAsync_ShowsScoringBreakdown()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory> ();
        var tableRenderer = serviceProvider.GetRequiredService<TableRenderer>();
        var service = new TestLatencyService(httpClientFactory, tableRenderer);

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        await service.RunTestAsync(verbose: false);

        // Reset console
        Console.SetOut(originalOut);
        var output = writer.ToString();

        // Assert
        StringAssert.Contains(output, "Scoring Breakdown", "Should show scoring breakdown");
        StringAssert.Contains(output, "Min Latency Score", "Should show min latency score");
        StringAssert.Contains(output, "Avg Latency Score", "Should show avg latency score");
        StringAssert.Contains(output, "Stability Score", "Should show stability score");
    }
}
