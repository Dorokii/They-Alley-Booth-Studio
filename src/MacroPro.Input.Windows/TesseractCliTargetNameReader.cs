using System.Diagnostics;
using System.Drawing.Imaging;
using MacroPro.Core.Targeting;

namespace MacroPro.Input.Windows;

public sealed class TesseractCliTargetNameReader : ITargetNameReader
{
    private readonly RelativeRegion _nameRegion;
    private readonly string? _binaryPath;

    public TesseractCliTargetNameReader(RelativeRegion nameRegion)
    {
        _nameRegion = nameRegion;
        _binaryPath = ResolveBinaryPath();
    }

    public async ValueTask<string?> TryReadTargetNameAsync(TargetWindow target, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_binaryPath))
        {
            return null;
        }

        if (!WindowCapture.TryGetClientBoundsOnScreen(target, out var clientBounds))
        {
            return null;
        }

        var region = WindowCapture.ToAbsoluteRectangle(clientBounds, _nameRegion);
        if (region.Width < 4 || region.Height < 4)
        {
            return null;
        }

        var filePath = Path.Combine(Path.GetTempPath(), $"macropro-target-{Guid.NewGuid():N}.png");

        try
        {
            using (var bitmap = WindowCapture.CaptureScreenRegion(region))
            {
                bitmap.Save(filePath, ImageFormat.Png);
            }

            var psi = new ProcessStartInfo
            {
                FileName = _binaryPath,
                Arguments = $"\"{filePath}\" stdout --psm 7",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                return null;
            }

            var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(output))
            {
                return null;
            }

            var line = output
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(line) ? null : line.Trim();
        }
        catch
        {
            return null;
        }
        finally
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
            }
        }
    }

    private static string? ResolveBinaryPath()
    {
        var env = Environment.GetEnvironmentVariable("TESSERACT_PATH");
        if (!string.IsNullOrWhiteSpace(env))
        {
            return env;
        }

        return "tesseract";
    }
}
