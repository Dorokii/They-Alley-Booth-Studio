using MacroPro.Core.Engine;
using MacroPro.Core.Input;
using MacroPro.Core.Targeting;

namespace MacroPro.Core.Modules;

public sealed class HunterTurretOptions
{
    public bool Enabled { get; set; }
    public string MapName { get; set; } = "gl_prison";
    public VirtualKey AttackSkillKey { get; set; } = VirtualKey.F1;
    public VirtualKey TargetCycleKey { get; set; } = VirtualKey.Tab;
    public int AttackIntervalMs { get; set; } = 145;
    public int AcquireIntervalMs { get; set; } = 110;
    public int LostTargetCooldownMs { get; set; } = 180;
    public TargetIdentifierOptions Identifier { get; set; } = new();
}

public sealed class HunterTurretModule : IActionModule
{
    private readonly ITargetIdentifier _targetIdentifier;
    private DateTime _lastIdentifierWarningAtUtc = DateTime.MinValue;
    private string? _lastLoggedTargetName;
    private DateTime _lastTargetNameLogAtUtc = DateTime.MinValue;

    public HunterTurretModule(HunterTurretOptions options, ITargetIdentifier targetIdentifier)
    {
        Options = options;
        _targetIdentifier = targetIdentifier;
    }

    public string Name => "HunterTurret";
    public HunterTurretOptions Options { get; }

    public bool IsEnabled
    {
        get => Options.Enabled;
        set => Options.Enabled = value;
    }

    public async Task RunAsync(ModuleRuntimeContext context, CancellationToken cancellationToken)
    {
        context.Log($"Hunter turret active for map: {Options.MapName}");

        while (!cancellationToken.IsCancellationRequested)
        {
            var snapshot = await _targetIdentifier.IdentifyAsync(context.Target, cancellationToken).ConfigureAwait(false);

            if (snapshot.HasTarget && snapshot.IsAllowedTarget)
            {
                TryLogTarget(context, snapshot.TargetName);

                context.Input.SendKeyTap(context.Target, Options.AttackSkillKey);
                var delay = Math.Max(40, Options.AttackIntervalMs);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (snapshot.HasTarget && !snapshot.IsAllowedTarget)
            {
                context.Input.SendKeyTap(context.Target, Options.TargetCycleKey);
                await Task.Delay(Math.Max(60, Options.AcquireIntervalMs), cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (!snapshot.HasTarget)
            {
                if (ShouldLogIdentifierWarning())
                {
                    context.Log("No valid target detected. Cycling target key.");
                }

                context.Input.SendKeyTap(context.Target, Options.TargetCycleKey);
                await Task.Delay(Math.Max(60, Options.AcquireIntervalMs), cancellationToken).ConfigureAwait(false);
                await Task.Delay(Math.Max(80, Options.LostTargetCooldownMs), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private bool ShouldLogIdentifierWarning()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastIdentifierWarningAtUtc).TotalSeconds < 7)
        {
            return false;
        }

        _lastIdentifierWarningAtUtc = now;
        return true;
    }

    private void TryLogTarget(ModuleRuntimeContext context, string? targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
        {
            return;
        }

        var normalized = TargetNameMatcher.Normalize(targetName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (string.Equals(_lastLoggedTargetName, normalized, StringComparison.OrdinalIgnoreCase)
            && (now - _lastTargetNameLogAtUtc).TotalSeconds < 5)
        {
            return;
        }

        _lastLoggedTargetName = normalized;
        _lastTargetNameLogAtUtc = now;
        context.Log($"Target locked: {targetName}");
    }
}
