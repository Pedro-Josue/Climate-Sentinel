using System.Drawing;

namespace ClimateSentinel.Utils;

public static class UiPalette
{
    public static readonly Color Background = Color.FromArgb(7, 27, 52);
    public static readonly Color Sidebar = Color.FromArgb(10, 35, 66);
    public static readonly Color Surface = Color.FromArgb(19, 47, 82);
    public static readonly Color SurfaceLight = Color.FromArgb(27, 57, 95);
    public static readonly Color Accent = Color.FromArgb(80, 170, 255);
    public static readonly Color AccentLight = Color.FromArgb(138, 204, 255);
    public static readonly Color Border = Color.FromArgb(37, 78, 125);
    public static readonly Color TextPrimary = Color.White;
    public static readonly Color TextSecondary = Color.FromArgb(200, 220, 240);
    public static readonly Color Safe = Color.FromArgb(46, 204, 113);
    public static readonly Color Attention = Color.FromArgb(241, 196, 15);
    public static readonly Color HighRisk = Color.FromArgb(243, 156, 18);
    public static readonly Color Critical = Color.FromArgb(231, 76, 60);
    public static readonly Color Muted = Color.FromArgb(150, 177, 206);
}
