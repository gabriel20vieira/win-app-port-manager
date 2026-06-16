using System.ComponentModel;
using System.Diagnostics;

namespace PortManager.Services;

/// <summary>Kills a process via Process.Kill, returning a clear ok/error result.</summary>
public sealed class ProcessKiller : IProcessKiller
{
    public KillResult Kill(int pid)
    {
        try
        {
            using var p = Process.GetProcessById(pid);
            p.Kill();
            p.WaitForExit(5000);
            return new KillResult(true, $"Process {pid} terminated.");
        }
        catch (ArgumentException)
        {
            // Already gone.
            return new KillResult(true, $"Process {pid} no longer exists.");
        }
        catch (Win32Exception ex)
        {
            return new KillResult(false, $"Access denied killing PID {pid}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new KillResult(false, $"Failed to kill PID {pid}: {ex.Message}");
        }
    }
}
