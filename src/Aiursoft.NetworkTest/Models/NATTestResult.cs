namespace Aiursoft.NetworkTest.Models;

/// <summary>
/// Result of NAT type and P2P traversal capability test.
/// </summary>
public class NATTestResult
{
    /// <summary>
    /// Detected NAT type.
    /// </summary>
    public required NATType NATType { get; init; }

    /// <summary>
    /// Local IP address used for testing.
    /// </summary>
    public string? LocalIP { get; init; }

    /// <summary>
    /// Public IP address as seen by STUN servers (mapped address).
    /// </summary>
    public string? MappedPublicIP { get; init; }

    /// <summary>
    /// External port as seen by primary STUN server.
    /// </summary>
    public int? MappedPublicPort { get; init; }

    /// <summary>
    /// Whether the machine is behind NAT.
    /// </summary>
    public bool BehindNAT { get; init; }

    /// <summary>
    /// Whether UPnP port mapping is available and working.
    /// </summary>
    public bool UPnPAvailable { get; init; }

    /// <summary>
    /// Port number tested for UPnP mapping (if UPnP test was performed).
    /// </summary>
    public int? UPnPTestedPort { get; init; }

    /// <summary>
    /// Base score before UPnP bonus (0-100).
    /// </summary>
    public double BaseScore { get; init; }

    /// <summary>
    /// Bonus points awarded for UPnP capability (0-35).
    /// </summary>
    public double UPnPBonus { get; init; }

    /// <summary>
    /// Final score (BaseScore + UPnPBonus), range 0-100.
    /// </summary>
    public double Score => BaseScore + UPnPBonus;

    /// <summary>
    /// Detailed test information for verbose output.
    /// </summary>
    public List<string> TestDetails { get; init; } = new();

    /// <summary>
    /// User-friendly description of the NAT type.
    /// </summary>
    public string NATTypeDescription => NATType switch
    {
        NATType.OpenInternet => "Open Internet (No NAT)",
        NATType.FullCone => "Full Cone NAT",
        NATType.RestrictedCone => "Restricted Cone NAT",
        NATType.PortRestrictedCone => "Port Restricted Cone NAT",
        NATType.Symmetric => "Symmetric NAT",
        NATType.UDPBlocked => "UDP Blocked",
        _ => "Unknown"
    };

    /// <summary>
    /// User-friendly assessment of P2P connectivity capability.
    /// </summary>
    public string P2PCapability => NATType switch
    {
        NATType.OpenInternet => "Perfect",
        NATType.FullCone => "Excellent",
        NATType.RestrictedCone => "Good",
        NATType.PortRestrictedCone when UPnPAvailable => "Good (UPnP assisted)",
        NATType.PortRestrictedCone => "Fair",
        NATType.Symmetric => "Poor (P2P difficult)",
        NATType.UDPBlocked => "Blocked",
        _ => "Unknown"
    };
}
