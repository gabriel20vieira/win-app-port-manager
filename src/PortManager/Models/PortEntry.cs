namespace PortManager.Models;

public enum Protocol
{
    Tcp,
    Udp
}

/// <summary>
/// One open port row: protocol, local endpoint, owning process.
/// </summary>
public sealed class PortEntry
{
    public Protocol Protocol { get; init; }
    public int Port { get; init; }
    public string LocalAddress { get; init; } = "";

    /// <summary>TCP state (e.g. LISTEN). Empty for UDP.</summary>
    public string State { get; init; } = "";

    public int Pid { get; init; }
    public string ProcessName { get; set; } = "";
    public string ProcessPath { get; set; } = "";

    public string ProtocolText => Protocol == Protocol.Tcp ? "TCP" : "UDP";
}
