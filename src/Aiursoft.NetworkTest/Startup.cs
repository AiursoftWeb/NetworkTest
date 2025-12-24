using System.Diagnostics.CodeAnalysis;
using Aiursoft.CommandFramework.Abstracts;
using Aiursoft.NetworkTest.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.NetworkTest;

[ExcludeFromCodeCoverage]
public class Startup : IStartUp
{
    public void ConfigureServices(IServiceCollection services)
    {

        // Quality testing services
        services.AddSingleton<TableRenderer>();
        services.AddScoped<DomesticLatencyTestService>();
        services.AddScoped<InternationalLatencyTestService>();
        services.AddScoped<IPv6ConnectivityTestService>();
        services.AddScoped<NATTraversalTestService>();
        services.AddScoped<UdpGameReliabilityTestService>();

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

        // Configure HTTP client for IPv4-only connectivity tests
        services.AddHttpClient("IPv4ConnectivityTest")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
                ConnectCallback = async (context, cancellationToken) =>
                {
                    // Force IPv4 connection
                    var socket = new System.Net.Sockets.Socket(
                        System.Net.Sockets.AddressFamily.InterNetwork,
                        System.Net.Sockets.SocketType.Stream,
                        System.Net.Sockets.ProtocolType.Tcp);
                    
                    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
                    return new System.Net.Sockets.NetworkStream(socket, ownsSocket: true);
                }
            });

        // Configure HTTP client for IPv6-only connectivity tests
        services.AddHttpClient("IPv6ConnectivityTest")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
                ConnectCallback = async (context, cancellationToken) =>
                {
                    // Force IPv6 connection
                    var socket = new System.Net.Sockets.Socket(
                        System.Net.Sockets.AddressFamily.InterNetworkV6,
                        System.Net.Sockets.SocketType.Stream,
                        System.Net.Sockets.ProtocolType.Tcp);
                    
                    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
                    return new System.Net.Sockets.NetworkStream(socket, ownsSocket: true);
                }
            });
    }
}
