using System.CommandLine;
using System.CommandLine.Parsing;
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
        // Currently only domestic latency is implemented
        var domesticLatencyTest = host.Services.GetRequiredService<DomesticLatencyTestService>();
        var domesticScore = await domesticLatencyTest.RunTestAsync();
        testScores[domesticLatencyTest.TestName] = domesticScore;

        // TODO: Add more tests here as they are implemented
        // var domesticSpeedTest = host.Services.GetRequiredService<DomesticSpeedTestService>();
        // var domesticSpeedScore = await domesticSpeedTest.RunTestAsync();
        // testScores[domesticSpeedTest.TestName] = domesticSpeedScore;

        // Calculate overall score
        var overallScore = testScores.Values.Average();

        // Render summary
        var tableRenderer = host.Services.GetRequiredService<TableRenderer>();
        tableRenderer.RenderOverallScoreSummary(testScores, overallScore);

        await host.StopAsync();
    }
}
