using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using MacroPro.Core.Targeting;

namespace MacroPro.Input.Windows;

internal static class WindowCapture
{
    public static bool TryGetClientBoundsOnScreen(TargetWindow target, out Rectangle bounds)
    {
        bounds = Rectangle.Empty;
        if (target.MainWindowHandle == IntPtr.Zero)
        {
            return false;
        }

        if (!GetClientRect(target.MainWindowHandle, out var clientRect))
        {
            return false;
        }

        var topLeft = new POINT { X = 0, Y = 0 };
        if (!ClientToScreen(target.MainWindowHandle, ref topLeft))
        {
            return false;
        }

        var width = clientRect.Right - clientRect.Left;
        var height = clientRect.Bottom - clientRect.Top;
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        bounds = new Rectangle(topLeft.X, topLeft.Y, width, height);
        return true;
    }

    public static Rectangle ToAbsoluteRectangle(Rectangle clientBounds, RelativeRegion region)
    {
        var x = clientBounds.Left + (int)Math.Round(clientBounds.Width * Clamp01(region.X));
        var y = clientBounds.Top + (int)Math.Round(clientBounds.Height * Clamp01(region.Y));
        var width = (int)Math.Round(clientBounds.Width * Clamp01(region.Width));
        var height = (int)Math.Round(clientBounds.Height * Clamp01(region.Height));

        width = Math.Max(1, width);
        height = Math.Max(1, height);

        var rect = new Rectangle(x, y, width, height);
        return Rectangle.Intersect(rect, clientBounds);
    }

    public static Bitmap CaptureScreenRegion(Rectangle screenRect)
    {
        var bitmap = new Bitmap(screenRect.Width, screenRect.Height, PixelFormat.Format24bppRgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(
            sourceX: screenRect.X,
            sourceY: screenRect.Y,
            destinationX: 0,
            destinationY: 0,
            blockRegionSize: screenRect.Size,
            copyPixelOperation: CopyPixelOperation.SourceCopy);
        return bitmap;
    }

    private static double Clamp01(double value) => Math.Clamp(value, 0d, 1d);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(nint hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(nint hWnd, ref POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}
