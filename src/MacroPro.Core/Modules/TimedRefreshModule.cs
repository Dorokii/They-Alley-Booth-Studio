using MacroPro.Core.Engine;
using MacroPro.Core.Input;

namespace MacroPro.Core.Modules;

public sealed class TimedRefreshOptions
{
    public bool Enabled { get; set; }
    public VirtualKey Key { get; set; } = VirtualKey.F5;
    public int IntervalSeconds { get; set; } = 10;
}

public sealed class TimedRefreshModule : IActionModule
{
    public TimedRefreshModule(TimedRefreshOptions options)
    {
        Options = options;
    }

    public string Name => "TimedRefresh";
    public bool IsEnabled
    {
        get => Options.Enabled;
        set => Options.Enabled = value;
    }

    public TimedRefreshOptions Options { get; }

    public async Task RunAsync(ModuleRuntimeContext context, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (Options.Key != VirtualKey.None)
            {
                context.Input.SendKeyTap(context.Target, Options.Key);
            }

            var delay = Math.Max(1, Options.IntervalSeconds) * 1000;
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }
}
