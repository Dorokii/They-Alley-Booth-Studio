namespace MacroPro.Core.Targeting;

public sealed record TargetWindow(
    int ProcessId,
    string ProcessName,
    IntPtr MainWindowHandle,
    string ExecutablePath)
{
    public bool IsValid => ProcessId > 0 && MainWindowHandle != IntPtr.Zero && !string.IsNullOrWhiteSpace(ExecutablePath);

    public override string ToString()
    {
        return $"{ProcessName} (PID {ProcessId})";
    }
}
