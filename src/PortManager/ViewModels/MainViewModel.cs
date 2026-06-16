using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using PortManager.Models;
using PortManager.Services;

namespace PortManager.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IPortScanner _scanner;
    private readonly IProcessKiller _killer;
    private readonly IDialogService _dialogs;

    private readonly ObservableCollection<PortEntry> _entries = new();
    private string _filterText = "";
    private string _status = "";
    private PortEntry? _selected;
    private bool _autoRefresh;

    public MainViewModel(IPortScanner scanner, IProcessKiller killer, IDialogService dialogs)
    {
        _scanner = scanner;
        _killer = killer;
        _dialogs = dialogs;

        Entries = CollectionViewSource.GetDefaultView(_entries);
        Entries.Filter = MatchesFilter;

        RefreshCommand = new RelayCommand(_ => Refresh());
        KillCommand = new RelayCommand(_ => KillSelected(), _ => _selected != null);
    }

    public ICollectionView Entries { get; }

    public ICommand RefreshCommand { get; }
    public ICommand KillCommand { get; }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetField(ref _filterText, value))
                Entries.Refresh();
        }
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public PortEntry? Selected
    {
        get => _selected;
        set => SetField(ref _selected, value);
    }

    /// <summary>Bound by the view to drive its auto-refresh timer.</summary>
    public bool AutoRefresh
    {
        get => _autoRefresh;
        set => SetField(ref _autoRefresh, value);
    }

    public void Refresh()
    {
        try
        {
            var rows = _scanner.Scan();
            _entries.Clear();
            foreach (var e in rows)
                _entries.Add(e);
            Status = $"{rows.Count} portos · atualizado {TimeOfDayNow()}";
        }
        catch (Exception ex)
        {
            Status = "Erro ao ler portos.";
            _dialogs.Error("Erro", ex.Message);
        }
    }

    private void KillSelected()
    {
        var target = _selected;
        if (target == null)
            return;

        string label = string.IsNullOrEmpty(target.ProcessName) ? "?" : target.ProcessName;
        bool ok = _dialogs.Confirm(
            "Confirmar",
            $"Matar PID {target.Pid} ({label})?\n\nPorto {target.ProtocolText} {target.Port}");
        if (!ok)
            return;

        var result = _killer.Kill(target.Pid);
        if (result.Success)
        {
            Status = result.Message;
            Refresh();
        }
        else
        {
            _dialogs.Error("Falha", result.Message);
        }
    }

    private bool MatchesFilter(object item)
    {
        if (string.IsNullOrWhiteSpace(_filterText))
            return true;
        if (item is not PortEntry e)
            return false;

        string q = _filterText.Trim();
        return e.Port.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
            || e.Pid.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
            || e.ProcessName.Contains(q, StringComparison.OrdinalIgnoreCase)
            || e.ProcessPath.Contains(q, StringComparison.OrdinalIgnoreCase)
            || e.ProtocolText.Contains(q, StringComparison.OrdinalIgnoreCase)
            || e.LocalAddress.Contains(q, StringComparison.OrdinalIgnoreCase);
    }

    // Wrapped so the time read stays in one place; the view owns the clock.
    private static string TimeOfDayNow() => DateTime.Now.ToString("HH:mm:ss");
}
