using Eto.Drawing;

namespace Piantina.Plugin.UI;

public static class AppFonts
{
    public static readonly Font Title =
        new Font(SystemFont.Bold, 18);

    public static readonly Font Heading =
        new Font(SystemFont.Bold, 14);

    public static readonly Font Body =
        SystemFonts.Default();

    public static readonly Font BodyBold =
        new Font(SystemFont.Bold, Body.Size);

    public static readonly Font Small =
        new Font(SystemFont.Default, 8);

    public static readonly Font SmallBold =
        new Font(SystemFont.Bold, 8);
}