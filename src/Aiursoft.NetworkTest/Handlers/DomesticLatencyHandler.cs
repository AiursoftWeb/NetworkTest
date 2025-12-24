using System.Diagnostics.CodeAnalysis;
using System.CommandLine;
using Aiursoft.CommandFramework.Framework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.CommandFramework.Services;
using Aiursoft.NetworkTest.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.NetworkTest.Handlers;

[ExcludeFromCodeCoverage]
public class DomesticLatencyHandler : ExecutableCommandHandlerBuilder
{
    protected override string Name => "china-products";

    protected override string Description => "Test China-based internet products latency.";

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
        await testService.RunTestAsync(verbose);

        await host.StopAsync();
    }
}
