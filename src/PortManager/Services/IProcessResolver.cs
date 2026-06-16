namespace PortManager.Services;

public readonly record struct ProcessInfo(string Name, string Path);

/// <summary>Maps a PID to its process name and executable path.</summary>
public interface IProcessResolver
{
    ProcessInfo Resolve(int pid);

    /// <summary>Clear any per-scan cache so the next scan sees fresh state.</summary>
    void Reset();
}
