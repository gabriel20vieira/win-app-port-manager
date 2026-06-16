using PortManager.Interop;
using PortManager.Models;

namespace PortManager.Services;

/// <summary>
/// Default scanner: TCP rows in LISTEN state plus all UDP rows, each resolved
/// to its owning process name and path.
/// </summary>
public sealed class PortScanner : IPortScanner
{
    private readonly IProcessResolver _resolver;

    public PortScanner(IProcessResolver resolver) => _resolver = resolver;

    public IReadOnlyList<PortEntry> Scan()
    {
        _resolver.Reset();
        var entries = new List<PortEntry>();

        foreach (var row in NativeMethods.GetTcpTable())
        {
            if (row.State != TcpStateNames.Listen)
                continue;

            var info = _resolver.Resolve(row.Pid);
            entries.Add(new PortEntry
            {
                Protocol = Protocol.Tcp,
                Port = row.Port,
                LocalAddress = row.LocalAddress,
                State = TcpStateNames.ToText(row.State),
                Pid = row.Pid,
                ProcessName = info.Name,
                ProcessPath = info.Path,
            });
        }

        foreach (var row in NativeMethods.GetUdpTable())
        {
            var info = _resolver.Resolve(row.Pid);
            entries.Add(new PortEntry
            {
                Protocol = Protocol.Udp,
                Port = row.Port,
                LocalAddress = row.LocalAddress,
                State = "",
                Pid = row.Pid,
                ProcessName = info.Name,
                ProcessPath = info.Path,
            });
        }

        return entries
            .OrderBy(e => e.Port)
            .ThenBy(e => e.ProtocolText)
            .ToList();
    }
}
