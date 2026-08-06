using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Materials;

public class MaterialListItem : Panel
{
    public event EventHandler? Selected;

    public bool IsSelected { get; private set; }

    public Material Material { get; }

    public void Select()
    {
        IsSelected = true;

        BackgroundColor = AppColors.Selected;

        ((Label)Content).Font = AppFonts.BodyBold;
    }

    public void Deselect()
    {
        IsSelected = false;

        BackgroundColor = Colors.Transparent;

        ((Label)Content).Font = AppFonts.Body;
    }
    public MaterialListItem(Material material)
    {
        Material = material;

        Height = 32;

        Padding = new Padding(10, 6);

        BackgroundColor = Colors.Transparent;

        Content = new Label
        {
            Text = material.Name,
            Font = AppFonts.Body,
            TextColor = AppColors.Text
        };

        MouseEnter += (_, _) =>
        {
            if (!IsSelected)
                BackgroundColor = AppColors.Hover;
        };

        MouseLeave += (_, _) =>
        {
            if (!IsSelected)
                BackgroundColor = Colors.Transparent;
        };

        MouseDown += (_, _) =>
        {
            Selected?.Invoke(this, EventArgs.Empty);
        };


    }
}