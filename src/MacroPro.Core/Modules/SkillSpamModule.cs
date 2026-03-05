using MacroPro.Core.Engine;
using MacroPro.Core.Input;

namespace MacroPro.Core.Modules;

public sealed class SkillSpamOptions
{
    public bool Enabled { get; set; } = true;
    public VirtualKey Key { get; set; } = VirtualKey.F1;
    public int IntervalMs { get; set; } = 120;
    public int JitterMs { get; set; } = 0;
}

public sealed class SkillSpamModule : IActionModule
{
    private readonly Random _random = new();

    public SkillSpamModule(SkillSpamOptions options)
    {
        Options = options;
    }

    public string Name => "SkillSpam";
    public bool IsEnabled
    {
        get => Options.Enabled;
        set => Options.Enabled = value;
    }

    public SkillSpamOptions Options { get; }

    public async Task RunAsync(ModuleRuntimeContext context, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (Options.Key != VirtualKey.None)
            {
                context.Input.SendKeyTap(context.Target, Options.Key);
            }

            var baseDelay = Math.Max(40, Options.IntervalMs);
            var jitter = Math.Clamp(Options.JitterMs, 0, 2000);
            var offset = jitter == 0 ? 0 : _random.Next(-jitter, jitter + 1);
            var delay = Math.Max(40, baseDelay + offset);

            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }
}
