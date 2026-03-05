# 4RTools Review and Clean Starter Basis

## Scope
- Repository reviewed: `https://github.com/4RTools/4RTools`
- Local snapshot: `main` at commit `0cad2f6` (2025-05-10)
- Goal: extract architecture ideas without reusing risky update/network behavior.

## High-Risk Findings
1. **Unverified self-update replaces executable at startup**
   - `Program.cs` launches `AutoPatcher` first.
   - `AutoPatcher` downloads the first release asset and replaces `4RTools.exe` without signature/hash validation.
   - Risk: supply-chain compromise or malicious release asset execution.
   - Files:
     - `Program.cs`
     - `Forms/AutoPatcher.cs`

2. **Direct process memory read/write and allocation**
   - Uses `OpenProcess`, `ReadProcessMemory`, `WriteProcessMemory`, `VirtualAllocEx`.
   - Risk: anti-cheat detection, client crashes, undefined behavior across patches/servers.
   - Files:
     - `Utils/ProcessMemoryReader.cs`
     - `Model/Client.cs`

3. **Unsafe thread lifecycle management**
   - Worker loops run forever (`while (true)`), and stop is implemented with `Thread.Suspend()`.
   - Risk: deadlocks, frozen UI, leaked threads, non-deterministic module shutdown.
   - File:
     - `Utils/_4RThread.cs`

## Medium-Risk Findings
1. **Remote server offsets loaded at runtime with no integrity control**
   - Pulls `supported_servers.json` from remote storage and merges directly.
   - Risk: bad offsets can break memory reads/writes and behavior.
   - Files:
     - `Utils/AppConfig.cs`
     - `Forms/ClientUpdaterForm.cs`

2. **Telemetry and external calls enabled by default**
   - Sends analytics events and fetches ad/remote data endpoints.
   - Risk: unnecessary network trust surface for a local macro tool.
   - Files:
     - `Model/Tracker.cs`
     - `Model/Advertiser.cs`
     - `Utils/AppConfig.cs`

## Low-Risk/Code-Quality Findings
1. **High-frequency polling and very short sleeps**
   - Many modules poll continuously with short delays (1-10 ms).
   - Risk: avoidable CPU usage and unstable timings.
   - Files:
     - `Model/AHK.cs`
     - `Model/AutoBuff.cs`
     - `Model/Autopot.cs`
     - `Model/Macro.cs`

2. **Input dispatch mostly `WM_KEYDOWN` in several paths**
   - Not all flows send paired key-up events.
   - Risk: stuck-state behavior for some clients.
   - Files:
     - `Model/Macro.cs`
     - `Model/AutoRefreshSpammer.cs`
     - `Model/ATKDEFMode.cs`

## Reusable Ideas (Good Basis)
1. **Action abstraction**
   - `Start()`, `Stop()`, config serialization per module.
   - File: `Model/Action.cs`

2. **Profile-based modular config**
   - JSON profile stores module state independently.
   - File: `Model/Profile.cs`

3. **Central ON/OFF orchestration**
   - One global toggle controlling module lifecycle.
   - File: `Forms/ToggleApplicationStateForm.cs`

## Clean Architecture for Your Own Bot
Use this structure, but remove risky pieces:

```
RagnaMacro/
  Core/
    IActionModule.cs
    MacroEngine.cs
    CancellationHost.cs
  Input/
    IInputDispatcher.cs
    WindowMessageDispatcher.cs
  State/
    IGameStateProvider.cs
    OcrStateProvider.cs          # safer than memory write/read
    MemoryStateProvider.cs       # optional, disabled by default
  Profiles/
    ProfileStore.cs
    profile.schema.json
  UI/
    MainForm.cs
```

## Hard Rules for a Safer Build
1. No self-updater in-process.
2. No telemetry by default.
3. No remote offset list unless signed and verified.
4. Use `CancellationToken` for every worker; never use `Thread.Suspend`.
5. Default to input-only automation (`PostMessage`/`SendInput`) before memory access.
6. Add per-module max rate limits (e.g., min 40-80 ms action interval).
7. Keep an emergency stop hotkey that always works.

## Minimal Interface Starter
```csharp
public interface IActionModule
{
    string Name { get; }
    bool IsEnabled { get; set; }
    Task RunAsync(CancellationToken token);
}
```

```csharp
public sealed class MacroEngine
{
    private readonly List<IActionModule> _modules;
    private CancellationTokenSource? _cts;

    public MacroEngine(IEnumerable<IActionModule> modules) => _modules = modules.ToList();

    public void Start()
    {
        Stop();
        _cts = new CancellationTokenSource();
        foreach (var m in _modules.Where(m => m.IsEnabled))
            _ = Task.Run(() => m.RunAsync(_cts.Token), _cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
```

## Suggested Implementation Order
1. Build profile/config + module lifecycle first.
2. Implement one safe module (`SkillSpam`) with strict rate limit.
3. Add hotkey + emergency stop.
4. Add optional HP/SP trigger using OCR or pixel checks.
5. Add tests for config load/save and module cancellation behavior.
