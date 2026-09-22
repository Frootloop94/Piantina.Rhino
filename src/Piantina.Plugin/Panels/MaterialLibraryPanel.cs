using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.Services;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Panels;

/// <summary>
/// Always-available dockable panel of material swatches, in the spirit of
/// RhinoGold's old material library: click a swatch to give the current Rhino
/// selection a decent shaded look in the viewport (Rendered/Arctic display
/// mode) without needing a raytraced render. Separate from PiantinaPanel's
/// sidebar-driven Materials CRUD screen, which is about editing the catalog
/// rather than quick viewport preview.
/// </summary>
[Guid("BB15ECAB-4D51-4033-BA23-96F4A2CFDA08")]
public class MaterialLibraryPanel : Panel
{
    private const int Columns = 3;

    private readonly MaterialService _materialService;
    private readonly Panel _swatchHost;
    private readonly Label _statusLabel;
    private readonly RadioButtonList _finishSelector;

    public MaterialLibraryPanel()
    {
        Padding = 16;

        _materialService = new MaterialService();

        var header = new Label
        {
            Text = "Material Library",
            Font = AppFonts.Heading,
            TextColor = AppColors.Text
        };

        var refreshButton = new Button { Text = "Refresh" };
        refreshButton.Click += (_, _) => LoadSwatches();

        var headerRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Items = { new StackLayoutItem(header, true), refreshButton }
        };

        _statusLabel = new Label
        {
            Font = AppFonts.Small,
            TextColor = AppColors.TextMuted,
            Text = "Select objects in Rhino, then click a swatch to apply it."
        };

        _swatchHost = new Panel();

        var scrollable = new Scrollable
        {
            Content = _swatchHost,
            Border = BorderType.None
        };

        _finishSelector = new RadioButtonList
        {
            Orientation = Orientation.Horizontal,
            Spacing = new Size(16, 0),
            Items = { "Polish", "Hammered", "Sand Blast" }
        };
        _finishSelector.SelectedIndex = 0;

        var finishRow = new StackLayout
        {
            Spacing = 6,
            Items =
            {
                new Label { Text = "Metal Finish", Font = AppFonts.Body, TextColor = AppColors.TextMuted },
                _finishSelector
            }
        };

        Content = new StackLayout
        {
            Spacing = 12,
            Items =
            {
                headerRow,
                _statusLabel,
                new StackLayoutItem(scrollable, true),
                finishRow
            }
        };

        LoadSwatches();
    }

    private MetalFinish SelectedFinish => _finishSelector.SelectedIndex switch
    {
        1 => MetalFinish.Hammered,
        2 => MetalFinish.SandBlast,
        _ => MetalFinish.Polished
    };

    /// <summary>
    /// Rebuilds the whole swatch grid from a fresh DynamicLayout rather than
    /// clearing and reusing one in place - same workaround MaterialList uses,
    /// since Eto's DynamicLayout doesn't reliably support Clear()+AddRow() reuse.
    /// </summary>
    private void LoadSwatches()
    {
        var layout = new DynamicLayout
        {
            Spacing = new Size(10, 10)
        };

        var groups = _materialService.GetMaterialsByCategory()
            .OrderBy(group => group.Key.ToString(), StringComparer.OrdinalIgnoreCase);

        var anyMaterials = false;

        foreach (var group in groups)
        {
            var materials = group
                .OrderBy(material => material.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (materials.Count == 0)
                continue;

            anyMaterials = true;

            layout.AddRow(new Label
            {
                Text = group.Key.ToString(),
                Font = AppFonts.Heading,
                TextColor = AppColors.Text
            });

            for (var i = 0; i < materials.Count; i += Columns)
            {
                var row = new Control[Columns];

                for (var column = 0; column < Columns; column++)
                {
                    if (i + column >= materials.Count)
                    {
                        row[column] = new Panel();
                        continue;
                    }

                    var swatch = new MaterialSwatch(materials[i + column]);
                    swatch.Clicked += Swatch_Clicked;
                    row[column] = swatch;
                }

                layout.AddRow(row);
            }
        }

        if (!anyMaterials)
        {
            layout.AddRow(new Label
            {
                Text = "No active materials yet. Add one from the Materials screen.",
                Font = AppFonts.Body,
                TextColor = AppColors.TextMuted
            });
        }

        _swatchHost.Content = layout;
    }

    private void Swatch_Clicked(object? sender, EventArgs e)
    {
        if (sender is not MaterialSwatch swatch)
            return;

        var result = MaterialAppearanceService.ApplyToSelection(swatch.Material, SelectedFinish);

        _statusLabel.Text = result switch
        {
            MaterialAppearanceService.ApplyResult.Applied =>
                $"Applied {swatch.Material.Name} ({SelectedFinish}) to the selection.",
            MaterialAppearanceService.ApplyResult.NothingSelected =>
                "Select objects in Rhino first, then click a swatch.",
            _ => "No active Rhino document."
        };
    }
}
