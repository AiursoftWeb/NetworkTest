using System.CommandLine;
using Aiursoft.CommandFramework.Framework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.CommandFramework.Services;
using Aiursoft.NetworkTest.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.NetworkTest.Handlers;

public class InternationalLatencyHandler : ExecutableCommandHandlerBuilder
{
    protected override string Name => "international-latency";

    protected override string Description => "Test international web service latency and speed.";

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

        var testService = host.Services.GetRequiredService<InternationalLatencyTestService>();
        await testService.RunTestAsync(verbose);

        await host.StopAsync();
    }
}
