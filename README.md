# Port Manager

Aplicação Windows (WPF, .NET 8) que mostra os portos abertos e os processos
associados, e permite matar esses processos. Corre **sempre como administrador**
(UAC pedido no arranque via manifest) para conseguir ler todos os PIDs e
terminar processos de qualquer dono.

## Funcionalidades

- Lista portos **TCP em LISTEN** e **UDP** com o processo dono.
- Colunas: Protocolo · Porto · Endereço local · Estado · PID · Nome · Caminho.
- **Atualizar** manual + checkbox **Auto (5s)**.
- **Filtro** de texto live (porto, PID, nome, caminho, endereço, protocolo).
- **Matar processo** com diálogo de confirmação.
- Lê os portos via P/Invoke `iphlpapi.dll`
  (`GetExtendedTcpTable` / `GetExtendedUdpTable`) — PID por linha, sem parsing
  de `netstat`.

## Arquitetura

| Unidade | Faz |
|---------|-----|
| `Interop/NativeMethods` | P/Invoke das tabelas TCP/UDP com PID |
| `Services/PortScanner` (`IPortScanner`) | Lê TCP LISTEN + UDP → `PortEntry` |
| `Services/ProcessResolver` (`IProcessResolver`) | PID → nome + caminho, com cache por scan |
| `Services/ProcessKiller` (`IProcessKiller`) | Mata PID, devolve ok/erro |
| `Services/DialogService` (`IDialogService`) | Confirmar / info / erro |
| `ViewModels/MainViewModel` | Lista, filtro, comandos, estado |
| `MainWindow` | DataGrid + toolbar + status bar |

As interfaces (`IPortScanner`, `IProcessKiller`, `IProcessResolver`,
`IDialogService`) permitem testar o `MainViewModel` com fakes, sem tocar no
win32.

## Correr o exe

O `dist/PortManager.exe` é **self-contained** (~154 MB): traz o próprio runtime
.NET 8, por isso **não precisa de nada instalado**. Copia para qualquer Windows
x64 e corre — aceita o UAC.

> Só funciona em **Windows x64**. Ver secção *Linux?* abaixo.

### (Opcional) build framework-dependent mais pequena

Se preferes um exe minúsculo (~0.2 MB) à custa de exigir runtime instalado:

```powershell
dotnet publish src/PortManager/PortManager.csproj -c Release -r win-x64 `
  --self-contained false -p:PublishSingleFile=true -o dist
```

Nesse caso a máquina precisa do **.NET 8 Desktop Runtime**:

- **winget:** `winget install Microsoft.DotNet.DesktopRuntime.8`
- **Chocolatey:** `choco install dotnet-8.0-desktopruntime`
- **Download:** https://dotnet.microsoft.com/download/dotnet/8.0 (*Desktop Runtime*)

## Linux?

**Não.** A app é Windows-only por três razões de fundo:

- **WPF** só corre em Windows (sem port para Linux).
- Lê os portos via **`iphlpapi.dll`** (API exclusiva do Windows).
- Elevação via manifest **`requireAdministrator`** é mecanismo Windows.

Para Linux seria preciso reescrever: outra UI (ex. Avalonia) e outra fonte de
dados (parsing de `/proc/net/tcp` + `/proc/net/udp`, ou `ss`/`lsof`). A camada
de serviços/ViewModel (atrás de interfaces) reaproveitava-se; o `NativeMethods`
e a UI não.

## Build & correr (a partir do código)

```powershell
dotnet build src/PortManager/PortManager.csproj -c Debug
dotnet run --project src/PortManager/PortManager.csproj
```

> Ao correr, o Windows pede elevação (UAC). Sem admin a app não arranca, por
> design (manifest `requireAdministrator`).

## Testes

A camada de serviços e o ViewModel são testáveis via as interfaces. Projeto de
testes (xUnit) a adicionar em `tests/`.
