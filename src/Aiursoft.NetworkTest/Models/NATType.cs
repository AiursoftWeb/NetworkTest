namespace Aiursoft.NetworkTest.Models;

/// <summary>
/// NAT (Network Address Translation) type classification based on RFC 3489/5389.
/// </summary>
public enum NATType
{
    /// <summary>
    /// NAT type could not be determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// UDP packets are blocked by firewall.
    /// </summary>
    UDPBlocked,

    /// <summary>
    /// Machine has a direct public IP address with no NAT.
    /// Best case scenario for P2P connectivity.
    /// </summary>
    OpenInternet,

    /// <summary>
    /// Full Cone NAT - Once an internal address (iAddr:iPort) is mapped to an external address (eAddr:ePort),
    /// any external host can send packets to iAddr:iPort by sending packets to eAddr:ePort.
    /// Excellent for P2P connectivity.
    /// </summary>
    FullCone,

    /// <summary>
    /// Restricted Cone NAT - Once an internal address (iAddr:iPort) is mapped to an external address (eAddr:ePort),
    /// only external hosts that iAddr:iPort has previously sent packets to can send packets back.
    /// Good for P2P connectivity with hole punching.
    /// </summary>
    RestrictedCone,

    /// <summary>
    /// Port Restricted Cone NAT - Similar to Restricted Cone, but the restriction includes port numbers.
    /// An external host (hostAddr:hostPort) can send packets to iAddr:iPort by sending to eAddr:ePort
    /// only if iAddr:iPort has previously sent a packet to hostAddr:hostPort.
    /// Fair for P2P connectivity, may require STUN/TURN assistance.
    /// </summary>
    PortRestrictedCone,

    /// <summary>
    /// Symmetric NAT - All requests from the same internal IP:port to a specific destination IP:port
    /// are mapped to the same external IP:port. If the same internal host sends a packet with the same
    /// source address and port but to a different destination, a different mapping is used.
    /// Very difficult for P2P connectivity, usually requires TURN relay.
    /// </summary>
    Symmetric
}
