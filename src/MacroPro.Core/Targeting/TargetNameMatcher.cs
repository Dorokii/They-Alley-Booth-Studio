using System.Text;
using System.Text.RegularExpressions;

namespace MacroPro.Core.Targeting;

public static partial class TargetNameMatcher
{
    public static bool IsMatch(string? rawTargetName, IReadOnlyCollection<string> allowedNames, bool requireNameMatch, out bool matchedByName)
    {
        matchedByName = false;

        if (allowedNames.Count == 0)
        {
            return true;
        }

        var normalizedTarget = Normalize(rawTargetName);
        if (string.IsNullOrWhiteSpace(normalizedTarget))
        {
            return !requireNameMatch;
        }

        foreach (var allowed in allowedNames)
        {
            var normalizedAllowed = Normalize(allowed);
            if (string.IsNullOrWhiteSpace(normalizedAllowed))
            {
                continue;
            }

            if (normalizedTarget.Contains(normalizedAllowed, StringComparison.OrdinalIgnoreCase))
            {
                matchedByName = true;
                return true;
            }
        }

        return false;
    }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var lowered = value.Trim().ToLowerInvariant();
        var stripped = NonAlphaNumericRegex().Replace(lowered, " ");
        var sb = new StringBuilder(stripped.Length);

        var wasSpace = false;
        foreach (var ch in stripped)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!wasSpace)
                {
                    sb.Append(' ');
                    wasSpace = true;
                }
            }
            else
            {
                sb.Append(ch);
                wasSpace = false;
            }
        }

        return sb.ToString().Trim();
    }

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumericRegex();
}
