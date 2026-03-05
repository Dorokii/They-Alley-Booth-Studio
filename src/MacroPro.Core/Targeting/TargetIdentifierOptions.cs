namespace MacroPro.Core.Targeting;

public sealed class RelativeRegion
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}

public sealed class TargetIdentifierOptions
{
    public bool Enabled { get; set; } = true;
    public string TargetBarColorHex { get; set; } = "#5BC236";
    public int ColorTolerance { get; set; } = 35;
    public RelativeRegion TargetBarSearchRegion { get; set; } = new()
    {
        X = 0.34,
        Y = 0.02,
        Width = 0.32,
        Height = 0.08
    };

    public bool UseNameOcr { get; set; }
    public bool RequireNameMatch { get; set; }
    public RelativeRegion TargetNameRegion { get; set; } = new()
    {
        X = 0.38,
        Y = 0.01,
        Width = 0.24,
        Height = 0.04
    };

    public List<string> AllowedMonsterNames { get; set; } = new();
}
