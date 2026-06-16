using System.Net;
using System.Runtime.InteropServices;

namespace PortManager.Interop;

/// <summary>
/// P/Invoke layer for iphlpapi.dll GetExtendedTcpTable / GetExtendedUdpTable.
/// Returns connection rows that already carry the owning PID, so no netstat
/// parsing is needed.
/// </summary>
internal static class NativeMethods
{
    private const int AF_INET = 2;

    // TCP_TABLE_CLASS
    private const int TCP_TABLE_OWNER_PID_ALL = 5;

    // UDP_TABLE_CLASS
    private const int UDP_TABLE_OWNER_PID = 1;

    private const int NO_ERROR = 0;
    private const int ERROR_INSUFFICIENT_BUFFER = 122;

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPROW_OWNER_PID
    {
        public uint state;
        public uint localAddr;
        public uint localPort;   // bytes 0-1 hold the port, network byte order
        public uint remoteAddr;
        public uint remotePort;
        public uint owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_UDPROW_OWNER_PID
    {
        public uint localAddr;
        public uint localPort;
        public uint owningPid;
    }

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(
        IntPtr pTcpTable,
        ref int dwOutBufLen,
        bool sort,
        int ipVersion,
        int tableClass,
        int reserved);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedUdpTable(
        IntPtr pUdpTable,
        ref int dwOutBufLen,
        bool sort,
        int ipVersion,
        int tableClass,
        int reserved);

    public readonly record struct TcpRow(string LocalAddress, int Port, int State, int Pid);
    public readonly record struct UdpRow(string LocalAddress, int Port, int Pid);

    public static List<TcpRow> GetTcpTable()
    {
        var rows = new List<TcpRow>();
        int size = 0;
        GetExtendedTcpTable(IntPtr.Zero, ref size, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);

        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            uint ret = GetExtendedTcpTable(buffer, ref size, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
            if (ret != NO_ERROR)
                throw new InvalidOperationException($"GetExtendedTcpTable failed: {ret}");

            int count = Marshal.ReadInt32(buffer);
            IntPtr rowPtr = buffer + 4;
            int rowSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();

            for (int i = 0; i < count; i++)
            {
                var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                rows.Add(new TcpRow(
                    FormatAddress(row.localAddr),
                    PortFromNetwork(row.localPort),
                    (int)row.state,
                    (int)row.owningPid));
                rowPtr += rowSize;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return rows;
    }

    public static List<UdpRow> GetUdpTable()
    {
        var rows = new List<UdpRow>();
        int size = 0;
        GetExtendedUdpTable(IntPtr.Zero, ref size, true, AF_INET, UDP_TABLE_OWNER_PID, 0);

        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            uint ret = GetExtendedUdpTable(buffer, ref size, true, AF_INET, UDP_TABLE_OWNER_PID, 0);
            if (ret != NO_ERROR)
                throw new InvalidOperationException($"GetExtendedUdpTable failed: {ret}");

            int count = Marshal.ReadInt32(buffer);
            IntPtr rowPtr = buffer + 4;
            int rowSize = Marshal.SizeOf<MIB_UDPROW_OWNER_PID>();

            for (int i = 0; i < count; i++)
            {
                var row = Marshal.PtrToStructure<MIB_UDPROW_OWNER_PID>(rowPtr);
                rows.Add(new UdpRow(
                    FormatAddress(row.localAddr),
                    PortFromNetwork(row.localPort),
                    (int)row.owningPid));
                rowPtr += rowSize;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return rows;
    }

    private static string FormatAddress(uint addr) => new IPAddress(addr).ToString();

    // localPort holds the port in network byte order in the low 2 bytes.
    private static int PortFromNetwork(uint port)
    {
        return ((int)(port & 0xFF) << 8) | (int)((port >> 8) & 0xFF);
    }
}
