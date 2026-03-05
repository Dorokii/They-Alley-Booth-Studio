namespace MacroPro.Core.Targeting;

public interface ITargetNameReader
{
    ValueTask<string?> TryReadTargetNameAsync(TargetWindow target, CancellationToken cancellationToken);
}
