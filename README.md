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

## Pré-requisito: .NET 8 Desktop Runtime

O exe em `dist/PortManager.exe` é **framework-dependent** (pequeno, ~0.2 MB),
por isso precisa do **.NET 8 Desktop Runtime** instalado na máquina que o corre.

Instalar via **winget** (recomendado):

```powershell
winget install Microsoft.DotNet.DesktopRuntime.8
```

Alternativas:

- **Download direto:** https://dotnet.microsoft.com/download/dotnet/8.0
  → secção *Desktop Runtime* (`windowsdesktop-runtime-8.0.x-win-x64.exe`).
- **Chocolatey:** `choco install dotnet-8.0-desktopruntime`
- Se já tens o **.NET 8 SDK** instalado, o runtime já vem incluído — nada a fazer.

> Não precisas disto se usares uma build **self-contained** (`--self-contained
> true`), mas essa fica ~150 MB.

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
