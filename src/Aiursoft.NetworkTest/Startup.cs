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
        services.AddScoped<InternationalLatencyTestService>();

        // Configure HTTP client for domestic quality tests (800ms timeout)
        services.AddHttpClient("DomesticQualityTest")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(800);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5
            });

        // Configure HTTP client for international quality tests (1000ms timeout)
        services.AddHttpClient("InternationalQualityTest")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(1000);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5
            });
    }
}
