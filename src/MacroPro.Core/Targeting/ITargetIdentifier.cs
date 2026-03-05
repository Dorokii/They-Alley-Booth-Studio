namespace MacroPro.Core.Targeting;

public interface ITargetIdentifier
{
    ValueTask<TargetSnapshot> IdentifyAsync(TargetWindow target, CancellationToken cancellationToken);
}
