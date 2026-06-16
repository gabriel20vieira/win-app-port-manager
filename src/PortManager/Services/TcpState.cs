namespace PortManager.Services;

/// <summary>
/// MIB_TCP_STATE values to display text. We only surface LISTEN per scope,
/// but the map keeps other states readable if the filter changes.
/// </summary>
internal static class TcpStateNames
{
    public const int Listen = 2;

    private static readonly Dictionary<int, string> Names = new()
    {
        [1] = "CLOSED",
        [2] = "LISTEN",
        [3] = "SYN-SENT",
        [4] = "SYN-RCVD",
        [5] = "ESTABLISHED",
        [6] = "FIN-WAIT-1",
        [7] = "FIN-WAIT-2",
        [8] = "CLOSE-WAIT",
        [9] = "CLOSING",
        [10] = "LAST-ACK",
        [11] = "TIME-WAIT",
        [12] = "DELETE-TCB",
    };

    public static string ToText(int state) => Names.TryGetValue(state, out var n) ? n : state.ToString();
}
