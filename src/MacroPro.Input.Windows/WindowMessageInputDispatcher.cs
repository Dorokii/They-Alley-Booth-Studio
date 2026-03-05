using System.Runtime.InteropServices;
using MacroPro.Core.Input;
using MacroPro.Core.Targeting;

namespace MacroPro.Input.Windows;

public sealed class WindowMessageInputDispatcher : IInputDispatcher
{
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;

    public void SendKeyTap(TargetWindow target, VirtualKey key)
    {
        if (!target.IsValid || key == VirtualKey.None)
        {
            return;
        }

        PostMessage(target.MainWindowHandle, WM_KEYDOWN, (nint)(int)key, nint.Zero);
        PostMessage(target.MainWindowHandle, WM_KEYUP, (nint)(int)key, nint.Zero);
    }

    public void SendLeftClick(TargetWindow target)
    {
        if (!target.IsValid)
        {
            return;
        }

        PostMessage(target.MainWindowHandle, WM_LBUTTONDOWN, nint.Zero, nint.Zero);
        PostMessage(target.MainWindowHandle, WM_LBUTTONUP, nint.Zero, nint.Zero);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(nint hWnd, int message, nint wParam, nint lParam);
}
