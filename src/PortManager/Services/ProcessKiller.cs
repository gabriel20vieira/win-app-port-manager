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
            return new KillResult(true, $"Processo {pid} terminado.");
        }
        catch (ArgumentException)
        {
            // Already gone.
            return new KillResult(true, $"Processo {pid} já não existe.");
        }
        catch (Win32Exception ex)
        {
            return new KillResult(false, $"Acesso negado ao matar PID {pid}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new KillResult(false, $"Falha ao matar PID {pid}: {ex.Message}");
        }
    }
}
