using PortManager.Models;

namespace PortManager.Services;

/// <summary>Reads open ports (TCP LISTEN + UDP) with owning process info.</summary>
public interface IPortScanner
{
    IReadOnlyList<PortEntry> Scan();
}
