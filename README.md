# Port Manager

A Windows desktop app (WPF, .NET 8) that lists open ports and their owning
processes, and lets you kill those processes. It **always runs as administrator**
(UAC is requested at startup via the manifest) so it can read every PID and
terminate processes owned by any user.

## Features

- Lists **TCP ports in LISTEN** state and **UDP** ports with the owning process.
- Columns: Protocol · Port · Local address · State · PID · Name · Path.
- Manual **Refresh** plus an **Auto (5s)** checkbox.
- Live text **filter** (port, PID, name, path, address, protocol).
- **Kill process** with a confirmation dialog.
- Reads ports via P/Invoke into `iphlpapi.dll`
  (`GetExtendedTcpTable` / `GetExtendedUdpTable`) — PID per row, no `netstat`
  parsing.

## Architecture

| Unit | Responsibility |
|------|----------------|
| `Interop/NativeMethods` | P/Invoke for the TCP/UDP tables with owning PID |
| `Services/PortScanner` (`IPortScanner`) | Reads TCP LISTEN + UDP → `PortEntry` |
| `Services/ProcessResolver` (`IProcessResolver`) | PID → name + path, cached per scan |
| `Services/ProcessKiller` (`IProcessKiller`) | Kills a PID, returns ok/error |
| `Services/DialogService` (`IDialogService`) | Confirm / info / error prompts |
| `ViewModels/MainViewModel` | List, filter, commands, state |
| `MainWindow` | DataGrid + toolbar + status bar |

The interfaces (`IPortScanner`, `IProcessKiller`, `IProcessResolver`,
`IDialogService`) let `MainViewModel` be tested with fakes, without touching
win32.

## Running the exe

`dist/PortManager.exe` is **self-contained** (~154 MB): it bundles the .NET 8
runtime, so it **needs nothing installed**. Copy it to any Windows x64 machine
and run — accept the UAC prompt.

> Windows x64 only. See the *Linux?* section below.

### (Optional) smaller framework-dependent build

If you prefer a tiny exe (~0.2 MB) at the cost of requiring the runtime to be
installed:

```powershell
dotnet publish src/PortManager/PortManager.csproj -c Release -r win-x64 `
  --self-contained false -p:PublishSingleFile=true -o dist
```

That build needs the **.NET 8 Desktop Runtime** on the machine:

- **winget:** `winget install Microsoft.DotNet.DesktopRuntime.8`
- **Chocolatey:** `choco install dotnet-8.0-desktopruntime`
- **Download:** https://dotnet.microsoft.com/download/dotnet/8.0 (*Desktop Runtime*)

## Linux?

**No.** The app is Windows-only for three fundamental reasons:

- **WPF** runs on Windows only (no Linux port).
- Ports are read via **`iphlpapi.dll`**, a Windows-exclusive API.
- Elevation through the **`requireAdministrator`** manifest is a Windows
  mechanism.

A Linux version would require a rewrite: a different UI (e.g. Avalonia) and a
different data source (parsing `/proc/net/tcp` + `/proc/net/udp`, or `ss`/`lsof`).
The services/ViewModel layer (behind interfaces) would be reusable; the
`NativeMethods` and UI would not.

## Build & run (from source)

```powershell
dotnet build src/PortManager/PortManager.csproj -c Debug
dotnet run --project src/PortManager/PortManager.csproj
```

> On launch, Windows prompts for elevation (UAC). Without admin the app does not
> start, by design (`requireAdministrator` manifest).

## Tests

The services and the ViewModel are testable through their interfaces. An xUnit
test project will live under `tests/`.
