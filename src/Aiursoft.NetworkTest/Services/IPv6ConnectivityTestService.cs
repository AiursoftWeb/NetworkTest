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

    // IP detection endpoints that return the client's IP (in order of preference, will try with fallback)
    private readonly List<(string url, string format)> _ipv4DetectionEndpoints = new()
    {
        ("https://ipv4.icanhazip.com", "plain"),          // Usually works in China
        ("https://v4.ident.me", "plain"),                 // Usually works in China
        ("https://checkip.amazonaws.com", "plain"),       // AWS service, may work
        ("https://api.ipify.org?format=json", "json"),    // May be blocked in China
        ("https://ipv4.wtfismyip.com/text", "plain")      // Fallback
    };
    
    private readonly List<(string url, string format)> _ipv6DetectionEndpoints = new()
    {
        ("https://ipv6.icanhazip.com", "plain"),          // Usually works in China
        ("https://v6.ident.me", "plain"),                 // Usually works in China
        ("https://api6.ipify.org?format=json", "json"),   // May be blocked in China
        ("https://ipv6.wtfismyip.com/text", "plain")      // Fallback
    };

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

        // 1. IPv4 Ping Baidu
        var (p4BaiduScore, p4BaiduSuccess, p4BaiduNote) = await TestPingAsync("www.baidu.com", AddressFamily.InterNetwork, verbose);
        totalScore += p4BaiduScore;
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv4 Ping Baidu",
            SuccessfulEndpoints = p4BaiduSuccess ? 1 : 0,
            TotalEndpoints = 1,
            Score = p4BaiduScore,
            Notes = p4BaiduNote
        });

        // 2. IPv6 Ping Baidu
        var (p6BaiduScore, p6BaiduSuccess, p6BaiduNote) = await TestPingAsync("www.baidu.com", AddressFamily.InterNetworkV6, verbose);
        totalScore += p6BaiduScore;
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv6 Ping Baidu",
            SuccessfulEndpoints = p6BaiduSuccess ? 1 : 0,
            TotalEndpoints = 1,
            Score = p6BaiduScore,
            Notes = p6BaiduNote
        });

        // 3. IPv4 Ping Google
        var (p4GoogleScore, p4GoogleSuccess, p4GoogleNote) = await TestPingAsync("www.google.com", AddressFamily.InterNetwork, verbose);
        totalScore += p4GoogleScore;
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv4 Ping Google",
            SuccessfulEndpoints = p4GoogleSuccess ? 1 : 0,
            TotalEndpoints = 1,
            Score = p4GoogleScore,
            Notes = p4GoogleNote
        });

        // 4. IPv6 Ping Google
        var (p6GoogleScore, p6GoogleSuccess, p6GoogleNote) = await TestPingAsync("www.google.com", AddressFamily.InterNetworkV6, verbose);
        totalScore += p6GoogleScore;
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv6 Ping Google",
            SuccessfulEndpoints = p6GoogleSuccess ? 1 : 0,
            TotalEndpoints = 1,
            Score = p6GoogleScore,
            Notes = p6GoogleNote
        });

        // 5. IPv4 HTTP Baidu
        var (h4BaiduScore, h4BaiduSuccess, h4BaiduNote) = await TestHttpAsync("www.baidu.com", "IPv4ConnectivityTest", verbose);
        totalScore += h4BaiduScore;
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv4 HTTP Baidu",
            SuccessfulEndpoints = h4BaiduSuccess ? 1 : 0,
            TotalEndpoints = 1,
            Score = h4BaiduScore,
            Notes = h4BaiduNote
        });

        // 6. IPv6 HTTP Baidu
        var (h6BaiduScore, h6BaiduSuccess, h6BaiduNote) = await TestHttpAsync("www.baidu.com", "IPv6ConnectivityTest", verbose);
        totalScore += h6BaiduScore;
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv6 HTTP Baidu",
            SuccessfulEndpoints = h6BaiduSuccess ? 1 : 0,
            TotalEndpoints = 1,
            Score = h6BaiduScore,
            Notes = h6BaiduNote
        });

        // 7. IPv4 HTTP Google
        var (h4GoogleScore, h4GoogleSuccess, h4GoogleNote) = await TestHttpAsync("www.google.com", "IPv4ConnectivityTest", verbose);
        totalScore += h4GoogleScore;
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv4 HTTP Google",
            SuccessfulEndpoints = h4GoogleSuccess ? 1 : 0,
            TotalEndpoints = 1,
            Score = h4GoogleScore,
            Notes = h4GoogleNote
        });

        // 8. IPv6 HTTP Google
        var (h6GoogleScore, h6GoogleSuccess, h6GoogleNote) = await TestHttpAsync("www.google.com", "IPv6ConnectivityTest", verbose);
        totalScore += h6GoogleScore;
        results.Add(new ConnectivityTestResult
        {
            TestName = "IPv6 HTTP Google",
            SuccessfulEndpoints = h6GoogleSuccess ? 1 : 0,
            TotalEndpoints = 1,
            Score = h6GoogleScore,
            Notes = h6GoogleNote
        });

        // 9. IPv4 public IP detection (NAT check)
        var (ipv4NATScore, ipv4IP, ipv4HasPublic) = await TestIPv4PublicIPAsync(verbose);
        totalScore += ipv4NATScore;
        var ipv4Notes = ipv4HasPublic 
            ? $"Seems {ipv4IP} is your public IPv4" 
            : "Is this machine behind NAT?";
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

        // 10. IPv6 public IP detection (NAT check)
        var (ipv6NATScore, ipv6IP, ipv6HasPublic) = await TestIPv6PublicIPAsync(verbose);
        totalScore += ipv6NATScore;
        var ipv6Notes = ipv6HasPublic 
            ? $"Seems {ipv6IP} is your public IPv6" 
            : "Is this machine behind NAT?";
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

    private async Task<(double score, bool success, string note)> TestPingAsync(string host, AddressFamily family, bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine($"Testing {family} Ping to {host}...");
        }

        try
        {
            var ips = await Dns.GetHostAddressesAsync(host);
            var targetIp = ips.FirstOrDefault(ip => ip.AddressFamily == family);
            if (targetIp == null)
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ No {family} address found for {host}");
                    Console.ResetColor();
                }
                return (0, false, $"No {family} address found");
            }

            using var ping = new Ping();
            var reply = await ping.SendPingAsync(targetIp, 2000);
            if (reply.Status == IPStatus.Success)
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ Ping {host} ({targetIp}) success: {reply.RoundtripTime}ms");
                    Console.ResetColor();
                }
                return (10, true, $"{reply.RoundtripTime}ms");
            }

            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Ping {host} ({targetIp}) failed: {reply.Status}");
                Console.ResetColor();
            }
            return (0, false, reply.Status.ToString());
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Ping {host} failed: {ex.Message}");
                Console.ResetColor();
            }
            return (0, false, "Error");
        }
    }

    private async Task<(double score, bool success, string note)> TestHttpAsync(string host, string clientName, bool verbose)
    {
        var url = $"https://{host}";
        if (verbose)
        {
            Console.WriteLine($"Testing {clientName} HTTP GET to {url}...");
        }

        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ HTTP GET {url} success: {response.StatusCode}");
                    Console.ResetColor();
                }
                return (10, true, "Success");
            }

            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ! HTTP GET {url} failed: {response.StatusCode}");
                Console.ResetColor();
            }
            return (0, false, response.StatusCode.ToString());
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ HTTP GET {url} failed: {ex.Message}");
                Console.ResetColor();
            }
            return (0, false, "Error");
        }
    }

    private async Task<(double score, string? detectedIP, bool hasPublicIP)> TestIPv4PublicIPAsync(bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine("Testing IPv4 public IP (NAT check)...");
        }

        string? serverSeenIP = null;
        Exception? lastException = null;

        // Try each endpoint until one succeeds
        foreach (var (url, format) in _ipv4DetectionEndpoints)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("IPv4ConnectivityTest");
                var response = await client.GetStringAsync(url);
                
                // Parse the response based on format
                serverSeenIP = format switch
                {
                    "json" => JsonDocument.Parse(response).RootElement.GetProperty("ip").GetString(),
                    "plain" => response.Trim(),
                    _ => throw new InvalidOperationException($"Unknown format: {format}")
                };

                // Success! Break out of the loop
                if (!string.IsNullOrWhiteSpace(serverSeenIP))
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ! Failed to query {url}: {ex.Message}");
                    Console.ResetColor();
                }
                // Continue to next endpoint
            }
        }

        // If all endpoints failed
        if (string.IsNullOrWhiteSpace(serverSeenIP))
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ IPv4 public IP test failed: All detection endpoints failed");
                if (lastException != null)
                {
                    Console.WriteLine($"    Last error: {lastException.Message}");
                }
                Console.ResetColor();
            }
            return (0.0, null, false);
        }

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
            return (10.0, serverSeenIP, true);
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

    private async Task<(double score, string? detectedIP, bool hasPublicIP)> TestIPv6PublicIPAsync(bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine("Testing IPv6 public IP (NAT check)...");
        }

        string? serverSeenIP = null;
        Exception? lastException = null;

        // Try each endpoint until one succeeds
        foreach (var (url, format) in _ipv6DetectionEndpoints)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("IPv6ConnectivityTest");
                var response = await client.GetStringAsync(url);
                
                // Parse the response based on format
                serverSeenIP = format switch
                {
                    "json" => JsonDocument.Parse(response).RootElement.GetProperty("ip").GetString(),
                    "plain" => response.Trim(),
                    _ => throw new InvalidOperationException($"Unknown format: {format}")
                };

                // Success! Break out of the loop
                if (!string.IsNullOrWhiteSpace(serverSeenIP))
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ! Failed to query {url}: {ex.Message}");
                    Console.ResetColor();
                }
                // Continue to next endpoint
            }
        }

        // If all endpoints failed
        if (string.IsNullOrWhiteSpace(serverSeenIP))
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ IPv6 public IP test failed: All detection endpoints failed");
                if (lastException != null)
                {
                    Console.WriteLine($"    Last error: {lastException.Message}");
                }
                Console.ResetColor();
            }
            return (0.0, null, false);
        }

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
            return (10.0, serverSeenIP, true);
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
