using MacroPro.Core.Input;
using MacroPro.Core.Targeting;

namespace MacroPro.Core.Engine;

public sealed class ModuleRuntimeContext
{
    private readonly Action<string>? _log;

    public ModuleRuntimeContext(TargetWindow target, IInputDispatcher input, Action<string>? log = null)
    {
        Target = target;
        Input = input;
        _log = log;
    }

    public TargetWindow Target { get; }
    public IInputDispatcher Input { get; }

    public void Log(string message)
    {
        _log?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
