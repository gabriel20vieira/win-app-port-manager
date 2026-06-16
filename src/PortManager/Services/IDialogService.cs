namespace PortManager.Services;

/// <summary>
/// Abstracts user prompts so the ViewModel stays testable (no direct MessageBox).
/// </summary>
public interface IDialogService
{
    bool Confirm(string title, string message);
    void Info(string title, string message);
    void Error(string title, string message);
}
