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

## Build & correr

```powershell
dotnet build src/PortManager/PortManager.csproj -c Debug
dotnet run --project src/PortManager/PortManager.csproj
```

> Ao correr, o Windows pede elevação (UAC). Sem admin a app não arranca, por
> design (manifest `requireAdministrator`).

## Testes

A camada de serviços e o ViewModel são testáveis via as interfaces. Projeto de
testes (xUnit) a adicionar em `tests/`.
