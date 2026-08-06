using Eto.Drawing;
using Eto.Forms;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;

public class PrimaryButton : Button
{
    public PrimaryButton(string text)
    {
        Text = text;

        Width = -1;

        Height = 42;

        Font = AppFonts.Body;

        BackgroundColor = AppColors.Accent;

        TextColor = Colors.White;
    }

    public PrimaryButton(
    string text,
    Action onClick)
    : this(text)
    {
        Click += (_, _) => onClick();
    }

}