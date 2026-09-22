using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Calculator;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.Services;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Calculator;

public class CalculatorView : Panel
{
    private readonly MaterialService _materialService;

    private readonly DropDown _materialDropDown;
    private readonly RadioButtonList _modeSelector;

    // Hosts whichever field set is active; its Content is replaced wholesale on
    // mode change rather than clearing/rebuilding the Card's own content area,
    // since Eto's DynamicLayout (which Card uses internally) doesn't reliably
    // support Clear()+AddRow() reuse. Same pattern MaterialList already uses
    // for its rebuildable list via _materialHost.
    private readonly Panel _dynamicFieldsHost;

    // Manual wax weight mode
    private readonly NumericStepper _waxWeightStepper;
    private readonly NumericStepper _waxDensityStepper;

    // From Rhino selection mode
    private readonly PrimaryButton _getVolumeButton;
    private readonly Label _volumeValueLabel;
    private decimal? _selectedVolumeCm3;

    private readonly InfoRow _metalWeightRow;
    private readonly InfoRow _costRow;

    private List<Material> _materials = new();

    public CalculatorView()
    {
        Padding = 20;

        _materialService = new MaterialService();

        _materialDropDown = new DropDown { Font = AppFonts.Body };

        _modeSelector = new RadioButtonList
        {
            Orientation = Orientation.Horizontal,
            Spacing = new Size(16, 0),
            Items = { "Manual wax weight", "From Rhino selection" }
        };
        _modeSelector.SelectedIndex = 0;

        _waxWeightStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 0,
            MaxValue = 10000,
            Increment = 0.1,
            Value = 0
        };

        _waxDensityStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 0.01,
            MaxValue = 5,
            Increment = 0.01,
            Value = (double)CastingCalculator.DefaultWaxDensity
        };

        _getVolumeButton = new PrimaryButton("Use Selected Object", UseSelectedObjectVolume);

        _volumeValueLabel = new Label
        {
            Font = AppFonts.Body,
            TextColor = AppColors.TextMuted,
            Text = "No object selected yet"
        };

        _dynamicFieldsHost = new Panel();

        var inputCard = new Card("Casting Calculator")
            .WithContent(
                FieldLabel("Material"),
                _materialDropDown,
                _modeSelector,
                _dynamicFieldsHost);

        _metalWeightRow = new InfoRow("Metal weight", "-");
        _costRow = new InfoRow("Estimated cost", "-");

        var resultCard = new Card("Result")
            .WithContent(
                _metalWeightRow,
                _costRow);

        // Stacked vertically rather than side by side: this panel is meant to
        // dock as a narrow sidebar, where there isn't room for the input and
        // result cards to sit next to each other.
        var layout = new DynamicLayout
        {
            Spacing = new Size(0, 20)
        };

        layout.AddRow(inputCard);
        layout.AddRow(resultCard);

        Content = new StackLayout
        {
            Spacing = 20,

            Items =
            {
                new SectionHeader("Calculator"),
                layout
            }
        };

        _materialDropDown.SelectedKeyChanged += (_, _) => Recalculate();
        _modeSelector.SelectedIndexChanged += (_, _) => RebuildDynamicFields();
        _waxWeightStepper.ValueChanged += (_, _) => Recalculate();
        _waxDensityStepper.ValueChanged += (_, _) => Recalculate();

        LoadMaterials();
        RebuildDynamicFields();
    }

    private static Label FieldLabel(string text) => new()
    {
        Text = text,
        Font = AppFonts.Body,
        TextColor = AppColors.TextMuted
    };

    private void LoadMaterials()
    {
        _materials = _materialService.GetMaterials().ToList();

        _materialDropDown.Items.Clear();

        foreach (var material in _materials)
        {
            _materialDropDown.Items.Add(new ListItem
            {
                Text = material.Name,
                Key = material.Id.ToString()
            });
        }

        if (_materials.Count > 0)
        {
            _materialDropDown.SelectedKey = _materials[0].Id.ToString();
        }
    }

    /// <summary>
    /// Swaps _dynamicFieldsHost.Content for a fresh layout depending on the
    /// selected mode. The individual controls (steppers, button, label) are
    /// created once in the constructor and keep their values across a swap.
    /// </summary>
    private void RebuildDynamicFields()
    {
        _dynamicFieldsHost.Content = _modeSelector.SelectedIndex == 1
            ? new StackLayout
            {
                Spacing = 8,
                Items =
                {
                    FieldLabel("Volume from selection"),
                    _getVolumeButton,
                    _volumeValueLabel
                }
            }
            : new StackLayout
            {
                Spacing = 8,
                Items =
                {
                    FieldLabel("Wax weight (g)"),
                    _waxWeightStepper,
                    FieldLabel("Wax density (g/cm³)"),
                    _waxDensityStepper
                }
            };

        Recalculate();
    }

    private void UseSelectedObjectVolume()
    {
        var volume = RhinoGeometryService.GetSelectedVolumeInCubicCentimetres();

        _selectedVolumeCm3 = volume;

        _volumeValueLabel.Text = volume is null
            ? "No solid object selected in Rhino"
            : $"{volume:0.000} cm³";

        _volumeValueLabel.TextColor = volume is null
            ? Colors.Crimson
            : AppColors.Text;

        Recalculate();
    }

    private void Recalculate()
    {
        var material = _materials.FirstOrDefault(m =>
            m.Id.ToString() == _materialDropDown.SelectedKey);

        if (material is null)
        {
            SetResult(null, null);
            return;
        }

        decimal metalWeight;

        if (_modeSelector.SelectedIndex == 1)
        {
            if (_selectedVolumeCm3 is null || _selectedVolumeCm3 <= 0)
            {
                SetResult(null, null);
                return;
            }

            metalWeight = CastingCalculator.CalculateMetalWeightFromVolume(
                _selectedVolumeCm3.Value,
                material.Density);
        }
        else
        {
            var waxWeight = (decimal)_waxWeightStepper.Value;
            var waxDensity = (decimal)_waxDensityStepper.Value;

            if (waxWeight <= 0 || waxDensity <= 0)
            {
                SetResult(null, null);
                return;
            }

            metalWeight = CastingCalculator.CalculateMetalWeight(waxWeight, material.Density, waxDensity);
        }

        var cost = CastingCalculator.CalculateCost(metalWeight, material.PricePerGram);

        SetResult(metalWeight, cost);
    }

    private void SetResult(decimal? metalWeight, decimal? cost)
    {
        _metalWeightRow.Value = metalWeight is null ? "-" : $"{metalWeight:0.00} g";
        _costRow.Value = cost is null ? "-" : $"R {cost:N2}";
    }
}
