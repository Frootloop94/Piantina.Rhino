using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;

/// <summary>
/// A single clickable colour swatch in the Material Library panel, showing a
/// Material's base appearance colour with its name underneath - the same
/// swatch-per-alloy idea RhinoGold's material palette used.
/// </summary>
public class MaterialSwatch : Panel
{
    public Material Material { get; }

    public event EventHandler? Clicked;

    public MaterialSwatch(Material material)
    {
        Material = material;

        Width = 72;
        Height = 64;

        var colorPanel = new Panel
        {
            Height = 44,
            BackgroundColor = Color.FromArgb(material.ColorR, material.ColorG, material.ColorB)
        };

        var label = new Label
        {
            Text = material.Name,
            Font = AppFonts.Small,
            TextAlignment = TextAlignment.Center,
            TextColor = AppColors.Text
        };

        Content = new StackLayout
        {
            Spacing = 2,
            Items = { colorPanel, label }
        };

        MouseDown += (_, _) => Clicked?.Invoke(this, EventArgs.Empty);
    }
}
