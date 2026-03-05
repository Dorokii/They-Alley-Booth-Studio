using MacroPro.Core.Input;
using MacroPro.Core.Targeting;

namespace MacroPro.Core.Engine;

public sealed class MacroEngine
{
    private readonly object _sync = new();
    private readonly List<IActionModule> _modules = new();
    private CancellationTokenSource? _cts;
    private Task[] _runningTasks = Array.Empty<Task>();

    public bool IsRunning { get; private set; }
    public IReadOnlyList<IActionModule> Modules => _modules;

    public void ConfigureModules(IEnumerable<IActionModule> modules)
    {
        lock (_sync)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Cannot configure modules while engine is running.");
            }

            _modules.Clear();
            _modules.AddRange(modules);
        }
    }

    public void Start(TargetWindow target, IInputDispatcher input, Action<string>? log = null)
    {
        lock (_sync)
        {
            if (IsRunning)
            {
                return;
            }

            if (!target.IsValid)
            {
                throw new InvalidOperationException("Target process is invalid.");
            }

            _cts = new CancellationTokenSource();
            var context = new ModuleRuntimeContext(target, input, log);
            var token = _cts.Token;

            _runningTasks = _modules
                .Where(static module => module.IsEnabled)
                .Select(module => Task.Run(async () =>
                {
                    try
                    {
                        context.Log($"[{module.Name}] started.");
                        await module.RunAsync(context, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        context.Log($"[{module.Name}] crashed: {ex.Message}");
                    }
                    finally
                    {
                        context.Log($"[{module.Name}] stopped.");
                    }
                }, token))
                .ToArray();

            IsRunning = true;
        }
    }

    public async Task StopAsync()
    {
        Task[] tasks;
        CancellationTokenSource? cts;

        lock (_sync)
        {
            if (!IsRunning)
            {
                return;
            }

            tasks = _runningTasks;
            _runningTasks = Array.Empty<Task>();

            cts = _cts;
            _cts = null;

            IsRunning = false;
        }

        if (cts is null)
        {
            return;
        }

        cts.Cancel();
        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            cts.Dispose();
        }
    }
}
