namespace MacroPro.Core.Targeting;

public sealed record TargetSnapshot(
    bool HasTarget,
    bool IsAllowedTarget,
    string? TargetName,
    bool NameMatched);
