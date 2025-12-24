using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using Aiursoft.NetworkTest.Models;
using Open.Nat;

namespace Aiursoft.NetworkTest.Services;

public class NATTraversalTestService : ITestService
{
    private readonly TableRenderer _tableRenderer;
    private const string PrimaryStunServer = "stun.l.google.com";
    private const int PrimaryStunPort = 19302;
    private const string SecondaryStunServer = "stun1.l.google.com";
    private const int SecondaryStunPort = 19302;
    private const int StunTimeoutMs = 3000;

    public string TestName => "NAT Traversal & P2P";

    public NATTraversalTestService(TableRenderer tableRenderer)
    {
        _tableRenderer = tableRenderer;
    }

    public async Task<double> RunTestAsync(bool verbose = false)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {TestName} Test ===");
        Console.WriteLine("Testing NAT type and P2P connectivity capability...");
        Console.WriteLine();

        var testDetails = new List<string>();
        
        // Step 1: Get local IP address
        var localIP = GetLocalIPAddress();
        if (localIP == null)
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ Failed to get local IP address");
                Console.ResetColor();
            }
            
            var failResult = new NATTestResult
            {
                NATType = NATType.Unknown,
                LocalIP = null,
                MappedPublicIP = null,
                BehindNAT = false,
                UPnPAvailable = false,
                BaseScore = 0,
                UPnPBonus = 0
            };
            
            _tableRenderer.RenderNATTestResults(failResult);
            _tableRenderer.RenderScoreSummary(TestName, 0);
            return 0;
        }

        if (verbose)
        {
            Console.WriteLine($"Local IP: {localIP}");
            testDetails.Add($"Local IP: {localIP}");
        }

        // Step 2: Test basic UDP connectivity with STUN
        // Create a UDP client that we'll reuse for both tests to ensure same local port
        using var udpClient = new UdpClient();
        udpClient.Client.ReceiveTimeout = StunTimeoutMs;
        udpClient.Client.SendTimeout = StunTimeoutMs;

        var (stunSuccess, mappedAddress, mappedPort) = await TestSTUNConnectivityWithClient(
            udpClient, PrimaryStunServer, PrimaryStunPort, verbose);
        
        if (!stunSuccess)
        {
            // UDP is blocked
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ UDP appears to be blocked by firewall");
                Console.ResetColor();
            }
            testDetails.Add("UDP blocked - cannot perform STUN tests");
            
            var udpBlockedResult = new NATTestResult
            {
                NATType = NATType.UDPBlocked,
                LocalIP = localIP,
                MappedPublicIP = null,
                BehindNAT = false,
                UPnPAvailable = false,
                BaseScore = 0,
                UPnPBonus = 0,
                TestDetails = testDetails
            };
            
            _tableRenderer.RenderNATTestResults(udpBlockedResult);
            _tableRenderer.RenderScoreSummary(TestName, 0);
            return 0;
        }

        if (verbose)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ STUN server responded: {mappedAddress}:{mappedPort}");
            Console.ResetColor();
        }
        testDetails.Add($"STUN mapped address: {mappedAddress}:{mappedPort}");

        // Step 3: Check if we have a public IP (no NAT)
        var localIPs = GetAllLocalIPAddresses();
        if (localIPs.Contains(mappedAddress!))
        {
            // Open Internet - no NAT
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Direct public IP detected (no NAT)");
                Console.ResetColor();
            }
            testDetails.Add("No NAT detected - local IP matches public IP");
            
            var openInternetResult = new NATTestResult
            {
                NATType = NATType.OpenInternet,
                LocalIP = localIP,
                MappedPublicIP = mappedAddress,
                MappedPublicPort = mappedPort,
                BehindNAT = false,
                UPnPAvailable = false,
                BaseScore = 100,
                UPnPBonus = 0,
                TestDetails = testDetails
            };
            
            _tableRenderer.RenderNATTestResults(openInternetResult);
            _tableRenderer.RenderScoreSummary(TestName, 100);
            return 100;
        }

        // We're behind NAT
        if (verbose)
        {
            Console.WriteLine("Behind NAT - testing NAT type...");
        }
        testDetails.Add("Behind NAT - classifying NAT type");

        // Step 4: Test for Symmetric NAT by checking port consistency across different destinations
        // CRITICAL: Reuse the SAME UDP client to ensure we're testing from the same local port
        var (stunSuccess2, _, mappedPort2) = await TestSTUNConnectivityWithClient(
            udpClient, SecondaryStunServer, SecondaryStunPort, verbose);
        
        if (stunSuccess2 && mappedPort2 != mappedPort)
        {
            // Port changed - Symmetric NAT
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Symmetric NAT detected (port changed: {mappedPort} → {mappedPort2})");
                Console.ResetColor();
            }
            testDetails.Add($"Symmetric NAT: port mapping changes per destination ({mappedPort} vs {mappedPort2})");
            
            var symmetricResult = new NATTestResult
            {
                NATType = NATType.Symmetric,
                LocalIP = localIP,
                MappedPublicIP = mappedAddress,
                MappedPublicPort = mappedPort,
                BehindNAT = true,
                UPnPAvailable = false,
                BaseScore = 0,
                UPnPBonus = 0,
                TestDetails = testDetails
            };
            
            _tableRenderer.RenderNATTestResults(symmetricResult);
            _tableRenderer.RenderScoreSummary(TestName, 0);
            return 0;
        }

        // Port is consistent - it's a Cone NAT variant
        // Default to Port Restricted Cone NAT (most common)
        var natType = NATType.PortRestrictedCone;
        var baseScore = 40.0;
        
        if (verbose)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Port Restricted Cone NAT detected");
            Console.ResetColor();
        }
        testDetails.Add("Port Restricted Cone NAT: port mapping is consistent");

        // Step 5: Test UPnP capability
        var (upnpSuccess, upnpPort) = await TestUPnPCapability(verbose);
        var upnpBonus = 0.0;
        
        if (upnpSuccess)
        {
            upnpBonus = 35.0;
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ UPnP port mapping successful (port {upnpPort})");
                Console.ResetColor();
            }
            testDetails.Add($"UPnP available - successfully mapped port {upnpPort}");
        }
        else
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("✗ UPnP not available or failed");
                Console.ResetColor();
            }
            testDetails.Add("UPnP not available");
        }

        var finalResult = new NATTestResult
        {
            NATType = natType,
            LocalIP = localIP,
            MappedPublicIP = mappedAddress,
            MappedPublicPort = mappedPort,
            BehindNAT = true,
            UPnPAvailable = upnpSuccess,
            UPnPTestedPort = upnpPort,
            BaseScore = baseScore,
            UPnPBonus = upnpBonus,
            TestDetails = testDetails
        };

        _tableRenderer.RenderNATTestResults(finalResult);
        _tableRenderer.RenderScoreSummary(TestName, finalResult.Score);

        return finalResult.Score;
    }

    private async Task<(bool success, string? mappedIP, int? mappedPort)> TestSTUNConnectivityWithClient(
        UdpClient client, string stunServer, int stunPort, bool verbose)
    {
        try
        {
            // Resolve STUN server
            var addresses = await Dns.GetHostAddressesAsync(stunServer);
            var serverAddress = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            
            if (serverAddress == null)
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ Could not resolve STUN server: {stunServer}");
                    Console.ResetColor();
                }
                return (false, null, null);
            }

            var serverEndpoint = new IPEndPoint(serverAddress, stunPort);

            // Create STUN Binding Request (RFC 5389 format)
            var transactionId = new byte[12];
            RandomNumberGenerator.Fill(transactionId);
            
            var request = new byte[20];
            // Message Type: Binding Request (0x0001)
            request[0] = 0x00;
            request[1] = 0x01;
            // Message Length: 0 (no attributes)
            request[2] = 0x00;
            request[3] = 0x00;
            // Magic Cookie: 0x2112A442
            request[4] = 0x21;
            request[5] = 0x12;
            request[6] = 0xA4;
            request[7] = 0x42;
            // Transaction ID (12 bytes)
            Array.Copy(transactionId, 0, request, 8, 12);

            await client.SendAsync(request, request.Length, serverEndpoint);

            // Wait for response
            var receiveTask = client.ReceiveAsync();
            var timeoutTask = Task.Delay(StunTimeoutMs);
            var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                // Timeout
                return (false, null, null);
            }

            var result = await receiveTask;
            var response = result.Buffer;

            // Parse response
            if (response.Length < 20)
            {
                return (false, null, null);
            }

            // Verify it's a Binding Response (0x0101)
            if (response[0] != 0x01 || response[1] != 0x01)
            {
                return (false, null, null);
            }

            // Parse attributes to find XOR-MAPPED-ADDRESS (0x0020)
            var messageLength = (response[2] << 8) | response[3];
            var offset = 20;

            while (offset < 20 + messageLength)
            {
                if (offset + 4 > response.Length) break;

                var attrType = (response[offset] << 8) | response[offset + 1];
                var attrLength = (response[offset + 2] << 8) | response[offset + 3];
                offset += 4;

                if (attrType == 0x0020 && attrLength >= 8) // XOR-MAPPED-ADDRESS
                {
                    // Skip reserved byte
                    var family = response[offset + 1];
                    if (family == 0x01) // IPv4
                    {
                        // XOR port with magic cookie's first 2 bytes (0x2112)
                        var xorPort = (response[offset + 2] << 8) | response[offset + 3];
                        var port = xorPort ^ 0x2112;

                        // XOR address with magic cookie (0x2112A442)
                        var xorAddr = new byte[4];
                        Array.Copy(response, offset + 4, xorAddr, 0, 4);
                        var magicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };
                        var addr = new byte[4];
                        for (int i = 0; i < 4; i++)
                        {
                            addr[i] = (byte)(xorAddr[i] ^ magicCookie[i]);
                        }

                        var ipAddress = new IPAddress(addr);
                        return (true, ipAddress.ToString(), port);
                    }
                }

                // Padding: attributes are aligned on 4-byte boundaries
                var padding = (4 - (attrLength % 4)) % 4;
                offset += attrLength + padding;
            }

            return (false, null, null);
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ STUN test error: {ex.Message}");
                Console.ResetColor();
            }
            return (false, null, null);
        }
    }

    private async Task<(bool success, int? port)> TestUPnPCapability(bool verbose)
    {
        try
        {
            var discoverer = new NatDiscoverer();
            var cts = new CancellationTokenSource(5000); // 5 second timeout
            var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

            // Try to map a random port
            var testPort = Random.Shared.Next(10000, 60000);
            var mapping = new Mapping(Protocol.Udp, testPort, testPort, "NetworkTest UPnP Test");

            await device.CreatePortMapAsync(mapping);

            // Verify the mapping was created
            var mappings = await device.GetAllMappingsAsync();
            var found = mappings.Any(m => m.PrivatePort == testPort);

            // Clean up
            try
            {
                await device.DeletePortMapAsync(mapping);
            }
            catch
            {
                // Ignore cleanup errors
            }

            return (found, testPort);
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"UPnP test error: {ex.Message}");
                Console.ResetColor();
            }
            return (false, null);
        }
    }

    private string? GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    return ip.ToString();
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    private List<string> GetAllLocalIPAddresses()
    {
        var addresses = new List<string>();
        
        try
        {
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
                        addresses.Add(address.Address.ToString());
                    }
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return addresses;
    }
}
