using System.CommandLine;
using Aiursoft.CommandFramework.Framework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.CommandFramework.Services;
using Aiursoft.NetworkTest.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.NetworkTest.Handlers;

public class NATTraversalHandler : ExecutableCommandHandlerBuilder
{
    protected override string Name => "nat-traversal";

    protected override string Description => "Test NAT type and P2P connectivity capability.";

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

        var natTraversalTest = host.Services.GetRequiredService<NATTraversalTestService>();
        await natTraversalTest.RunTestAsync(verbose);

        await host.StopAsync();
    }
}
