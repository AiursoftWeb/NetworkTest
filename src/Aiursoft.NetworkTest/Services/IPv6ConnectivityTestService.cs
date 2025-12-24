using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using Aiursoft.NetworkTest.Models;

namespace Aiursoft.NetworkTest.Services;

public class IPv6ConnectivityTestService : ITestService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TableRenderer _tableRenderer;

    public string TestName => "IPv6 Connectivity";

    // IPv4-only endpoints (no AAAA records)
    private readonly List<string> _ipv4OnlyEndpoints = new()
    {
        "https://ipv4.icanhazip.com",
        "https://v4.ident.me",
        "https://ipv4.wtfismyip.com/text",
        "https://api.ipify.org",
        "https://checkip.amazonaws.com"
    };

    // IPv6-only endpoints (no A records)
    private readonly List<string> _ipv6OnlyEndpoints = new()
    {
        "https://ipv6.icanhazip.com",
        "https://v6.ident.me",
        "https://ipv6.wtfismyip.com/text",
        "https://api6.ipify.org",
        "https://v6.test-ipv6.com/ip/"
    };

    // IP detection endpoints that return the client's IP
    private readonly string _ipv4DetectionEndpoint = "https://api.ipify.org?format=json";
    private readonly string _ipv6DetectionEndpoint = "https://api6.ipify.org?format=json";

    public IPv6ConnectivityTestService(
        IHttpClientFactory httpClientFactory,
        TableRenderer tableRenderer)
    {
        _httpClientFactory = httpClientFactory;
        _tableRenderer = tableRenderer;
    }

    public async Task<double> RunTestAsync(bool verbose = false)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {TestName} Test ===");
        Console.WriteLine();

        var results = new List<ConnectivityTestResult>();
        double totalScore = 0;

        // Test 1: IPv4 connectivity
        var (ipv4Score, ipv4Success, ipv4Total) = await TestIPv4ConnectivityAsync(verbose);
        totalScore += ipv4Score;
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv4 Connectivity",
            SuccessfulEndpoints = ipv4Success,
            TotalEndpoints = ipv4Total,
            Score = ipv4Score,
            DetectedIP = null,
            HasPublicIP = false,
            Notes = ipv4Success >= 2 ? "Can access IPv4 internet" : "Cannot access IPv4 internet"
        });

        // Test 2: IPv6 connectivity
        var (ipv6Score, ipv6Success, ipv6Total) = await TestIPv6ConnectivityAsync(verbose);
        totalScore += ipv6Score;
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv6 Connectivity",
            SuccessfulEndpoints = ipv6Success,
            TotalEndpoints = ipv6Total,
            Score = ipv6Score,
            DetectedIP = null,
            HasPublicIP = false,
            Notes = ipv6Success >= 2 ? "Can access IPv6 internet" : "Cannot access IPv6 internet"
        });

        // Test 3: IPv4 public IP detection (NAT check)
        var (ipv4NATScore, ipv4IP, ipv4HasPublic) = await TestIPv4PublicIPAsync(verbose);
        totalScore += ipv4NATScore;
        var ipv4Notes = ipv4HasPublic 
            ? $"Seems {ipv4IP} is your public IPv4" 
            : "Is this machine behind proxy?";
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv4 Public IP",
            SuccessfulEndpoints = ipv4HasPublic ? 1 : 0,
            TotalEndpoints = 1,
            Score = ipv4NATScore,
            DetectedIP = ipv4IP,
            HasPublicIP = ipv4HasPublic,
            Notes = ipv4Notes
        });

        // Test 4: IPv6 public IP detection (NAT check)
        var (ipv6NATScore, ipv6IP, ipv6HasPublic) = await TestIPv6PublicIPAsync(verbose);
        totalScore += ipv6NATScore;
        var ipv6Notes = ipv6HasPublic 
            ? $"Seems {ipv6IP} is your public IPv6" 
            : "Is this machine behind proxy?";
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv6 Public IP",
            SuccessfulEndpoints = ipv6HasPublic ? 1 : 0,
            TotalEndpoints = 1,
            Score = ipv6NATScore,
            DetectedIP = ipv6IP,
            HasPublicIP = ipv6HasPublic,
            Notes = ipv6Notes
        });

        // Render results table
        _tableRenderer.RenderConnectivityTestsTable(results);

        _tableRenderer.RenderScoreSummary(TestName, totalScore);

        return totalScore;
    }

    private async Task<(double score, int successCount, int totalCount)> TestIPv4ConnectivityAsync(bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine("Testing IPv4 connectivity...");
        }

        var successCount = 0;

        foreach (var endpoint in _ipv4OnlyEndpoints)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("IPv4ConnectivityTest");
                var response = await client.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    successCount++;
                    if (verbose)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ {endpoint}");
                        Console.ResetColor();
                    }
                }
                else if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ! {endpoint} - Status: {response.StatusCode}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ {endpoint} - {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        // +25 points if 2 or more endpoints are accessible
        var score = successCount >= 2 ? 25.0 : 0.0;
        return (score, successCount, _ipv4OnlyEndpoints.Count);
    }

    private async Task<(double score, int successCount, int totalCount)> TestIPv6ConnectivityAsync(bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine("Testing IPv6 connectivity...");
        }

        var successCount = 0;

        foreach (var endpoint in _ipv6OnlyEndpoints)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("IPv6ConnectivityTest");
                var response = await client.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    successCount++;
                    if (verbose)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ {endpoint}");
                        Console.ResetColor();
                    }
                }
                else if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ! {endpoint} - Status: {response.StatusCode}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ {endpoint} - {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        // +25 points if 2 or more endpoints are accessible
        var score = successCount >= 2 ? 25.0 : 0.0;
        return (score, successCount, _ipv6OnlyEndpoints.Count);
    }

    private async Task<(double score, string? detectedIP, bool hasPublicIP)> TestIPv4PublicIPAsync(bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine("Testing IPv4 public IP (NAT check)...");
        }

        try
        {
            // Get the IP address seen by the server
            var client = _httpClientFactory.CreateClient("IPv4ConnectivityTest");
            var response = await client.GetStringAsync(_ipv4DetectionEndpoint);
            var serverSeenIP = JsonDocument.Parse(response).RootElement.GetProperty("ip").GetString();

            if (verbose)
            {
                Console.WriteLine($"  Server sees IPv4: {serverSeenIP}");
            }

            // Get local network interface IPs
            var localIPs = GetLocalIPv4Addresses();

            if (verbose)
            {
                Console.WriteLine($"  Local IPv4 addresses: {string.Join(", ", localIPs)}");
            }

            // Check if server-seen IP matches any local IP
            if (localIPs.Any(ip => ip == serverSeenIP))
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ Match found! Public IPv4 detected (no NAT)");
                    Console.ResetColor();
                }
                return (25.0, serverSeenIP, true);
            }
            else
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ! No match - likely behind NAT");
                    Console.ResetColor();
                }
                return (0.0, serverSeenIP, false);
            }
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ IPv4 public IP test failed: {ex.Message}");
                Console.ResetColor();
            }
            return (0.0, null, false);
        }
    }

    private async Task<(double score, string? detectedIP, bool hasPublicIP)> TestIPv6PublicIPAsync(bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine("Testing IPv6 public IP (NAT check)...");
        }

        try
        {
            // Get the IP address seen by the server
            var client = _httpClientFactory.CreateClient("IPv6ConnectivityTest");
            var response = await client.GetStringAsync(_ipv6DetectionEndpoint);
            var serverSeenIP = JsonDocument.Parse(response).RootElement.GetProperty("ip").GetString();

            if (verbose)
            {
                Console.WriteLine($"  Server sees IPv6: {serverSeenIP}");
            }

            // Get local network interface IPs
            var localIPs = GetLocalIPv6Addresses();

            if (verbose)
            {
                Console.WriteLine($"  Local IPv6 addresses: {string.Join(", ", localIPs)}");
            }

            // Check if server-seen IP matches any local IP
            if (localIPs.Any(ip => ip == serverSeenIP))
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ Match found! Public IPv6 detected (no NAT)");
                    Console.ResetColor();
                }
                return (25.0, serverSeenIP, true);
            }
            else
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ! No match - likely behind NAT or IPv6 translation");
                    Console.ResetColor();
                }
                return (0.0, serverSeenIP, false);
            }
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ IPv6 public IP test failed: {ex.Message}");
                Console.ResetColor();
            }
            return (0.0, null, false);
        }
    }

    private List<string> GetLocalIPv4Addresses()
    {
        var ipAddresses = new List<string>();

        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up)
                continue;

            var properties = networkInterface.GetIPProperties();
            foreach (var address in properties.UnicastAddresses)
            {
                if (address.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(address.Address))
                {
                    ipAddresses.Add(address.Address.ToString());
                }
            }
        }

        return ipAddresses;
    }

    private List<string> GetLocalIPv6Addresses()
    {
        var ipAddresses = new List<string>();

        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up)
                continue;

            var properties = networkInterface.GetIPProperties();
            foreach (var address in properties.UnicastAddresses)
            {
                if (address.Address.AddressFamily == AddressFamily.InterNetworkV6 &&
                    !IPAddress.IsLoopback(address.Address) &&
                    !address.Address.IsIPv6LinkLocal)
                {
                    ipAddresses.Add(address.Address.ToString());
                }
            }
        }

        return ipAddresses;
    }
}
