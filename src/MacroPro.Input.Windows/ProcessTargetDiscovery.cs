using System.Diagnostics;
using MacroPro.Core.Targeting;

namespace MacroPro.Input.Windows;

public static class ProcessTargetDiscovery
{
    public static IReadOnlyList<TargetWindow> GetRunningTargets()
    {
        var list = new List<TargetWindow>();

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                if (process.MainWindowHandle == IntPtr.Zero)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(process.MainWindowTitle))
                {
                    continue;
                }

                var executable = process.MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(executable))
                {
                    continue;
                }

                list.Add(new TargetWindow(
                    process.Id,
                    process.ProcessName,
                    process.MainWindowHandle,
                    executable));
            }
            catch
            {
                // Ignore system/inaccessible processes.
            }
            finally
            {
                process.Dispose();
            }
        }

        return list
            .OrderBy(static target => target.ProcessName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
