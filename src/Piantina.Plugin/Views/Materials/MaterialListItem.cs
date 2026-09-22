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

    private readonly Label _label;

    public void Select()
    {
        IsSelected = true;

        BackgroundColor = AppColors.Selected;

        _label.Font = AppFonts.BodyBold;
    }

    public void Deselect()
    {
        IsSelected = false;

        BackgroundColor = Colors.Transparent;

        _label.Font = AppFonts.Body;
    }

    public MaterialListItem(Material material)
    {
        Material = material;

        Height = 32;

        Padding = new Padding(10, 6);

        BackgroundColor = Colors.Transparent;

        var swatch = new Panel
        {
            Width = 18,
            Height = 18,
            BackgroundColor = Color.FromArgb(material.ColorR, material.ColorG, material.ColorB)
        };

        _label = new Label
        {
            Text = material.Name,
            Font = AppFonts.Body,
            TextColor = AppColors.Text
        };

        // TableLayout + TableRow.Cells rather than StackLayout - the same
        // grid mechanism CardGrid uses, since a plain StackLayout has been
        // unreliable for sizing small fixed-size panels like the swatch here.
        var row = new TableLayout
        {
            Padding = 0,
            Spacing = new Size(8, 0)
        };

        var tableRow = new TableRow();
        tableRow.Cells.Add(new TableCell(swatch, false));
        tableRow.Cells.Add(new TableCell(_label, true));
        row.Rows.Add(tableRow);

        Content = row;

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
        swatch.MouseDown += RaiseSelected;
        _label.MouseDown += RaiseSelected;
    }
}
