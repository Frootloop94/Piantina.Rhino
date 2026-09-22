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
            Width = 64,
            Height = 44,
            BackgroundColor = Color.FromArgb(material.ColorR, material.ColorG, material.ColorB)
        };

        var label = new Label
        {
            Text = material.Name,
            Width = 72,
            Font = AppFonts.Small,
            TextAlignment = TextAlignment.Center,
            TextColor = AppColors.Text
        };

        // TableLayout + TableRow.Cells, not StackLayout - the same grid
        // mechanism CardGrid already uses successfully. A plain StackLayout
        // left the colour panel with no reliable width in testing.
        var grid = new TableLayout
        {
            Padding = 0,
            Spacing = new Size(0, 2)
        };

        var colorRow = new TableRow();
        colorRow.Cells.Add(colorPanel);
        grid.Rows.Add(colorRow);

        var labelRow = new TableRow();
        labelRow.Cells.Add(label);
        grid.Rows.Add(labelRow);

        Content = grid;

        // Panel.MouseDown isn't guaranteed to bubble up from every child
        // control on every Eto backend, so the click is wired to each piece
        // of the swatch individually rather than relying on bubbling from a
        // single outer handler.
        void RaiseClicked(object? sender, EventArgs e) => Clicked?.Invoke(this, EventArgs.Empty);

        MouseDown += RaiseClicked;
        colorPanel.MouseDown += RaiseClicked;
        label.MouseDown += RaiseClicked;
    }
}
