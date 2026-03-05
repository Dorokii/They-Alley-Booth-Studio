using System.IO;

namespace MacroPro.Core.Targeting;

public sealed class ProcessTargetRules
{
    private readonly IReadOnlyList<string> _allowedRoots;

    public ProcessTargetRules(IEnumerable<string> allowedRoots)
    {
        _allowedRoots = allowedRoots
            .Select(NormalizeRoot)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<string> AllowedRoots => _allowedRoots;

    public bool IsAllowed(TargetWindow target, out string reason)
    {
        if (!target.IsValid)
        {
            reason = "Invalid process target.";
            return false;
        }

        if (_allowedRoots.Count == 0)
        {
            reason = "No allowed root path configured.";
            return false;
        }

        var executable = NormalizePath(target.ExecutablePath);
        if (string.IsNullOrWhiteSpace(executable))
        {
            reason = "Target executable path is unavailable.";
            return false;
        }

        foreach (var root in _allowedRoots)
        {
            if (executable.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                reason = string.Empty;
                return true;
            }
        }

        reason = $"Target path is outside allowed roots. Path: {target.ExecutablePath}";
        return false;
    }

    private static string NormalizeRoot(string path)
    {
        var normalized = NormalizePath(path);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        return normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
               + Path.DirectorySeparatorChar;
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        try
        {
            var full = Path.GetFullPath(path.Trim());
            return full;
        }
        catch
        {
            return string.Empty;
        }
    }
}
