using System.Drawing;
using MacroPro.Core.Targeting;

namespace MacroPro.Input.Windows;

public sealed class PixelTargetIdentifier : ITargetIdentifier
{
    private readonly TargetIdentifierOptions _options;
    private readonly ITargetNameReader _targetNameReader;
    private readonly Color _targetBarColor;

    public PixelTargetIdentifier(TargetIdentifierOptions options, ITargetNameReader targetNameReader)
    {
        _options = options;
        _targetNameReader = targetNameReader;
        _targetBarColor = ParseColor(options.TargetBarColorHex, Color.FromArgb(91, 194, 54));
    }

    public async ValueTask<TargetSnapshot> IdentifyAsync(TargetWindow target, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return new TargetSnapshot(
                HasTarget: true,
                IsAllowedTarget: true,
                TargetName: null,
                NameMatched: false);
        }

        if (!WindowCapture.TryGetClientBoundsOnScreen(target, out var clientBounds))
        {
            return new TargetSnapshot(false, false, null, false);
        }

        var targetBarRegion = WindowCapture.ToAbsoluteRectangle(clientBounds, _options.TargetBarSearchRegion);
        if (targetBarRegion.Width <= 0 || targetBarRegion.Height <= 0)
        {
            return new TargetSnapshot(false, false, null, false);
        }

        bool hasTarget;
        using (var bitmap = WindowCapture.CaptureScreenRegion(targetBarRegion))
        {
            hasTarget = ContainsColor(bitmap, _targetBarColor, _options.ColorTolerance);
        }

        if (!hasTarget)
        {
            return new TargetSnapshot(false, false, null, false);
        }

        var hasAllowList = _options.AllowedMonsterNames.Count > 0;
        if (!hasAllowList)
        {
            return new TargetSnapshot(true, true, null, false);
        }

        string? targetName = null;
        if (_options.UseNameOcr)
        {
            targetName = await _targetNameReader.TryReadTargetNameAsync(target, cancellationToken).ConfigureAwait(false);
        }

        var allowed = TargetNameMatcher.IsMatch(
            targetName,
            _options.AllowedMonsterNames,
            _options.RequireNameMatch,
            out var matchedByName);

        return new TargetSnapshot(
            HasTarget: true,
            IsAllowedTarget: allowed,
            TargetName: targetName,
            NameMatched: matchedByName);
    }

    private static Color ParseColor(string? raw, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return fallback;
        }

        var hex = raw.Trim().TrimStart('#');
        if (hex.Length != 6)
        {
            return fallback;
        }

        try
        {
            var r = Convert.ToInt32(hex.Substring(0, 2), 16);
            var g = Convert.ToInt32(hex.Substring(2, 2), 16);
            var b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return Color.FromArgb(r, g, b);
        }
        catch
        {
            return fallback;
        }
    }

    private static bool ContainsColor(Bitmap bitmap, Color targetColor, int tolerance)
    {
        var clampedTolerance = Math.Clamp(tolerance, 0, 255);
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var c = bitmap.GetPixel(x, y);
                if (Math.Abs(c.R - targetColor.R) <= clampedTolerance
                    && Math.Abs(c.G - targetColor.G) <= clampedTolerance
                    && Math.Abs(c.B - targetColor.B) <= clampedTolerance)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
