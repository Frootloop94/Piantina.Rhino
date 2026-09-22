using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Materials;

/// <summary>
/// A single clickable material tile - a colour swatch with its name
/// underneath, laid out in a grid alongside other tiles rather than as a
/// plain list row. Closer to RhinoGold's old material palette tiles.
/// </summary>
public class MaterialListItem : Panel
{
    public event EventHandler? Selected;

    public event EventHandler? DoubleClicked;

    public bool IsSelected { get; private set; }

    public Material Material { get; }

    private readonly Panel _swatch;
    private readonly Label _label;

    public void Select()
    {
        IsSelected = true;

        BackgroundColor = AppColors.Selected;

        _label.Font = AppFonts.SmallBold;
    }

    public void Deselect()
    {
        IsSelected = false;

        BackgroundColor = Colors.Transparent;

        _label.Font = AppFonts.Small;
    }

    public MaterialListItem(Material material)
    {
        Material = material;

        Width = 72;
        Height = 64;

        Padding = 4;

        BackgroundColor = Colors.Transparent;

        _swatch = new Panel
        {
            Width = 64,
            Height = 40,
            BackgroundColor = Color.FromArgb(material.ColorR, material.ColorG, material.ColorB)
        };

        _label = new Label
        {
            Text = material.Name,
            Font = AppFonts.Small,
            TextAlignment = TextAlignment.Center,
            TextColor = AppColors.Text
        };

        // TableLayout + TableRow.Cells rather than StackLayout - the same
        // grid mechanism CardGrid uses, since a plain StackLayout has been
        // unreliable for sizing small fixed-size panels like the swatch here.
        var tile = new TableLayout
        {
            Padding = 0,
            Spacing = new Size(0, 2)
        };

        var swatchRow = new TableRow();
        swatchRow.Cells.Add(_swatch);
        tile.Rows.Add(swatchRow);

        var labelRow = new TableRow();
        labelRow.Cells.Add(_label);
        tile.Rows.Add(labelRow);

        Content = tile;

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

        void RaiseSelected(object? sender, EventArgs e) => Selected?.Invoke(this, EventArgs.Empty);

        MouseDown += RaiseSelected;
        _swatch.MouseDown += RaiseSelected;
        _label.MouseDown += RaiseSelected;

        void RaiseDoubleClicked(object? sender, EventArgs e) => DoubleClicked?.Invoke(this, EventArgs.Empty);

        MouseDoubleClick += RaiseDoubleClicked;
        _swatch.MouseDoubleClick += RaiseDoubleClicked;
        _label.MouseDoubleClick += RaiseDoubleClicked;
    }
}
