using System.Windows;
using System.Windows.Threading;
using PortManager.Services;
using PortManager.ViewModels;

namespace PortManager;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();

        var resolver = new ProcessResolver();
        _vm = new MainViewModel(
            new PortScanner(resolver),
            new ProcessKiller(),
            new DialogService());
        DataContext = _vm;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _timer.Tick += (_, _) => _vm.Refresh();
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.AutoRefresh))
                ToggleTimer();
        };

        Loaded += (_, _) => _vm.Refresh();
    }

    private void ToggleTimer()
    {
        if (_vm.AutoRefresh)
            _timer.Start();
        else
            _timer.Stop();
    }
}
