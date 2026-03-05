using MacroPro.Core.Targeting;

namespace MacroPro.Input.Windows;

public sealed class NoopTargetNameReader : ITargetNameReader
{
    public ValueTask<string?> TryReadTargetNameAsync(TargetWindow target, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<string?>(null);
    }
}
