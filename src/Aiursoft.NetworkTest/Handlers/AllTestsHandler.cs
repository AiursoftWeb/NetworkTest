using System.CommandLine;
using Aiursoft.CommandFramework.Framework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.CommandFramework.Services;
using Aiursoft.NetworkTest.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.NetworkTest.Handlers;

public class AllTestsHandler : ExecutableCommandHandlerBuilder
{
    protected override string Name => "all";

    protected override string Description => "Run all network quality tests and calculate overall score.";

    protected override Option[] GetCommandOptions() =>
    [
        CommonOptionsProvider.VerboseOption
    ];

    protected override async Task Execute(ParseResult parseResult)
    {
        var verbose = parseResult.GetValue(CommonOptionsProvider.VerboseOption);

        var host = ServiceBuilder
            .CreateCommandHostBuilder<Startup>(verbose)
            .Build();

        await host.StartAsync();

        var testScores = new Dictionary<string, double>();

        // Run all available tests
        var domesticLatencyTest = host.Services.GetRequiredService<DomesticLatencyTestService>();
        var domesticScore = await domesticLatencyTest.RunTestAsync(verbose);
        testScores[domesticLatencyTest.TestName] = domesticScore;

        var internationalLatencyTest = host.Services.GetRequiredService<InternationalLatencyTestService>();
        var internationalScore = await internationalLatencyTest.RunTestAsync(verbose);
        testScores[internationalLatencyTest.TestName] = internationalScore;

        var ipv6ConnectivityTest = host.Services.GetRequiredService<IPv6ConnectivityTestService>();
        var ipv6ConnectivityScore = await ipv6ConnectivityTest.RunTestAsync(verbose);
        testScores[ipv6ConnectivityTest.TestName] = ipv6ConnectivityScore;

        // TODO: Add more tests here as they are implemented
        // var domesticSpeedTest = host.Services.GetRequiredService<DomesticSpeedTestService>();
        // var domesticSpeedScore = await domesticSpeedTest.RunTestAsync(verbose);
        // testScores[domesticSpeedTest.TestName] = domesticSpeedScore;

        // Calculate overall score
        var overallScore = testScores.Values.Average();

        // Render summary
        var tableRenderer = host.Services.GetRequiredService<TableRenderer>();
        tableRenderer.RenderOverallScoreSummary(testScores, overallScore);

        await host.StopAsync();
    }
}
