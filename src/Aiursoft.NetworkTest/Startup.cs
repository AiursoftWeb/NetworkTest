using Aiursoft.CommandFramework.Abstracts;
using Aiursoft.NetworkTest.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.NetworkTest;

public class Startup : IStartUp
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Quality testing services
        services.AddSingleton<TableRenderer>();
        services.AddScoped<DomesticLatencyTestService>();

        // Configure HTTP client for quality tests
        services.AddHttpClient("QualityTest")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(500);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5
            });
    }
}
