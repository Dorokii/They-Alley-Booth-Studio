using MacroPro.Core.Engine;
using MacroPro.Core.Input;

namespace MacroPro.Core.Modules;

public enum MacroStepAction
{
    KeyTap,
    LeftClick,
    Wait
}

public sealed class MacroStep
{
    public MacroStepAction Action { get; set; } = MacroStepAction.KeyTap;
    public VirtualKey Key { get; set; } = VirtualKey.None;
    public int DelayAfterMs { get; set; } = 80;
}

public sealed class MacroChainOptions
{
    public bool Enabled { get; set; }
    public int CycleDelayMs { get; set; } = 300;
    public List<MacroStep> Steps { get; set; } = new();
}

public sealed class MacroChainModule : IActionModule
{
    public MacroChainModule(MacroChainOptions options)
    {
        Options = options;
    }

    public string Name => "MacroChain";
    public bool IsEnabled
    {
        get => Options.Enabled;
        set => Options.Enabled = value;
    }

    public MacroChainOptions Options { get; }

    public async Task RunAsync(ModuleRuntimeContext context, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var step in Options.Steps)
            {
                switch (step.Action)
                {
                    case MacroStepAction.KeyTap when step.Key != VirtualKey.None:
                        context.Input.SendKeyTap(context.Target, step.Key);
                        break;
                    case MacroStepAction.LeftClick:
                        context.Input.SendLeftClick(context.Target);
                        break;
                    case MacroStepAction.Wait:
                        break;
                }

                var delayPerStep = Math.Max(30, step.DelayAfterMs);
                await Task.Delay(delayPerStep, cancellationToken).ConfigureAwait(false);
            }

            var cycleDelay = Math.Max(80, Options.CycleDelayMs);
            await Task.Delay(cycleDelay, cancellationToken).ConfigureAwait(false);
        }
    }
}
