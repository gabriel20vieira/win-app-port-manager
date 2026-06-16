namespace PortManager.Services;

public readonly record struct KillResult(bool Success, string Message);

/// <summary>Terminates a process by PID.</summary>
public interface IProcessKiller
{
    KillResult Kill(int pid);
}
