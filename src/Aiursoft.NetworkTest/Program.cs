using Aiursoft.CommandFramework;
using Aiursoft.NetworkTest;

return await new SingleCommandApp<PingHandler>()
    .WithDefaultOption(OptionsProvider.ServerOption)
    .RunAsync(args);
