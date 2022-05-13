using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using SharpDX;


namespace glTFViewer.Utils;


public static class ScreenUtil {

    public static (double X, double Y) WpfToScreen(double x, double y) {
        var X = x * Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.WorkArea.Width;
        var Y = y * Screen.PrimaryScreen.WorkingArea.Height / SystemParameters.WorkArea.Height;
        return (X, Y);
    }


    public static Vector2 WpfToScreen(System.Windows.Point point) {
        var (x, y) = WpfToScreen(point.X, point.Y);
        return new Vector2((float) x, (float) y);
    }


    public static Size2 WpfToScreen(Size size) {
        var (width, height) = WpfToScreen(size.Width, size.Height);
        return new Size2((int) Math.Ceiling(width), (int) Math.Ceiling(height));
    }


    public static (double X, double Y) Dpi() {
        using var src = new HwndSource(new HwndSourceParameters());
        var m = src.CompositionTarget.TransformToDevice;
        return (m.M11 * 96, m.M22 * 96);
    }
    
}