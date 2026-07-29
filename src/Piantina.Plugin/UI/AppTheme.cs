using Eto.Drawing;


namespace Piantina.Plugin.UI;

public static class AppTheme
{
    // Fonts
    public static Font TitleFont => AppFonts.Title;
    public static Font HeadingFont => AppFonts.Heading;
    public static Font BodyFont => AppFonts.Body;

    // Sizes
    public static int SidebarWidth => Metrics.SidebarWidth;
    public static int Padding => Metrics.Padding;
    public static int Spacing => Metrics.Spacing;

    // Colours
    public static Color Background => AppColors.Background;
    public static Color SidebarBackground => AppColors.Sidebar;
    public static Color Accent => AppColors.Accent;
    public static Color Text => AppColors.Text;
    public static Color TextMuted => AppColors.TextMuted;
    public static Color Hover => AppColors.Hover;
    public static Color Selected => AppColors.Selected;
}