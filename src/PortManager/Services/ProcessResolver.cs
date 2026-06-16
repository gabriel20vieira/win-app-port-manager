using System.Diagnostics;

namespace PortManager.Services;

/// <summary>
/// Resolves PID -> (name, path) via System.Diagnostics. Caches per scan pass
/// and degrades gracefully when access is denied or the process is gone.
/// </summary>
public sealed class ProcessResolver : IProcessResolver
{
    private readonly Dictionary<int, ProcessInfo> _cache = new();

    public ProcessInfo Resolve(int pid)
    {
        if (_cache.TryGetValue(pid, out var cached))
            return cached;

        var info = Lookup(pid);
        _cache[pid] = info;
        return info;
    }

    /// <summary>Drop cached entries so the next scan sees fresh process state.</summary>
    public void Reset() => _cache.Clear();

    private static ProcessInfo Lookup(int pid)
    {
        if (pid == 0)
            return new ProcessInfo("System Idle Process", "");
        if (pid == 4)
            return new ProcessInfo("System", "");

        try
        {
            using var p = Process.GetProcessById(pid);
            string name = p.ProcessName;
            string path = "";
            try
            {
                path = p.MainModule?.FileName ?? "";
            }
            catch
            {
                // Access denied to module path is common for protected/system
                // processes even when elevated. Keep the name, skip the path.
                path = "(access denied)";
            }
            return new ProcessInfo(name, path);
        }
        catch (ArgumentException)
        {
            return new ProcessInfo("(process exited)", "");
        }
        catch (Exception)
        {
            return new ProcessInfo("(access denied)", "");
        }
    }
}
