using System.CommandLine;
using System.CommandLine.Parsing;
using Aiursoft.CommandFramework.Framework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.CommandFramework.Services;
using Aiursoft.NetworkTest.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.NetworkTest.Handlers;

public class DomesticLatencyHandler : ExecutableCommandHandlerBuilder
{
    protected override string Name => "domestic-latency";

    protected override string Description => "Test domestic (China) web service latency and speed.";

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

        var testService = host.Services.GetRequiredService<DomesticLatencyTestService>();
        await testService.RunTestAsync();

        await host.StopAsync();
    }
}
