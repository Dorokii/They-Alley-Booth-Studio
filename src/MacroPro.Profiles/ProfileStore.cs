using System.Text.Json;
using MacroPro.Core.Configuration;

namespace MacroPro.Profiles;

public sealed class ProfileStore
{
    private readonly string _rootDirectory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public ProfileStore(string rootDirectory)
    {
        _rootDirectory = rootDirectory;
    }

    public IReadOnlyList<string> ListProfiles()
    {
        if (!Directory.Exists(_rootDirectory))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(_rootDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
            .ToList()!;
    }

    public async Task<MacroProfile> LoadOrCreateAsync(string profileName, CancellationToken cancellationToken = default)
    {
        var path = GetProfilePath(profileName);
        if (!File.Exists(path))
        {
            var profile = new MacroProfile { ProfileName = profileName };
            await SaveAsync(profileName, profile, cancellationToken).ConfigureAwait(false);
            return profile;
        }

        await using var stream = File.OpenRead(path);
        var profileData = await JsonSerializer.DeserializeAsync<MacroProfile>(stream, _jsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (profileData is null)
        {
            throw new InvalidDataException($"Could not deserialize profile '{profileName}'.");
        }

        profileData.ProfileName = profileName;
        return profileData;
    }

    public async Task SaveAsync(string profileName, MacroProfile profile, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_rootDirectory);
        profile.ProfileName = profileName;
        var path = GetProfilePath(profileName);

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, profile, _jsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private string GetProfilePath(string profileName)
    {
        var safe = string.Concat(profileName.Select(static c =>
            Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        return Path.Combine(_rootDirectory, $"{safe}.json");
    }
}
