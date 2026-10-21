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

    // Manual volume mode
    private readonly NumericStepper _waxVolumeStepper;

    // From Rhino selection mode
    private readonly PrimaryButton _getVolumeButton;
    private readonly Label _volumeValueLabel;
    private decimal? _selectedVolumeCm3;

    private readonly InfoRow _metalWeightRow;
    private readonly InfoRow _costRow;

    // Quick Quote - Design Fee is the only field entered directly here; Wax
    // print and Casting fee are the business's own standing rates (see
    // CastingCalculator) applied to the volume/weight already worked out
    // above, and Metal cost just mirrors the Result card's own cost, so nothing
    // has to be re-entered or can drift out of sync with the calculator above it.
    private readonly NumericStepper _designFeeStepper;
    private readonly InfoRow _waxPrintRow;
    private readonly InfoRow _castingFeeRow;
    private readonly InfoRow _quoteMetalCostRow;
    private readonly InfoRow _totalQuoteRow;

    private decimal? _lastVolumeMm3;
    private decimal? _lastMetalWeight;
    private decimal? _lastMetalCost;

    private List<Material> _materials = new();

    public CalculatorView(MaterialService materialService)
    {
        Padding = 20;

        _materialService = materialService;

        _materialDropDown = new DropDown { Font = AppFonts.Body };

        // Vertical, not horizontal - "Manual volume (mm³)" and "From Rhino
        // selection" side by side need more width than a narrow docked
        // sidebar has, which was forcing the whole tab to scroll
        // horizontally and clip content at both edges.
        _modeSelector = new RadioButtonList
        {
            Orientation = Orientation.Vertical,
            Spacing = new Size(0, 6),
            Items = { "Manual volume (mm³)", "From Rhino selection" }
        };
        _modeSelector.SelectedIndex = 0;

        _waxVolumeStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 1,
            MinValue = 0,
            MaxValue = 1_000_000,
            Increment = 10,
            Value = 0
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

        _designFeeStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 0,
            MaxValue = 1_000_000,
            Increment = 50,
            Value = 0
        };

        _waxPrintRow = new InfoRow("Wax print", "-");
        _castingFeeRow = new InfoRow("Casting fee", "-");
        _quoteMetalCostRow = new InfoRow("Metal cost", "-");
        _totalQuoteRow = new InfoRow("Total Quote", "-");

        var quoteCard = new Card("Quick Quote")
            .WithContent(
                FieldLabel("Design Fee (R)"),
                _designFeeStepper,
                _waxPrintRow,
                _castingFeeRow,
                _quoteMetalCostRow,
                _totalQuoteRow);

        // Stacked vertically rather than side by side: this panel is meant to
        // dock as a narrow sidebar, where there isn't room for the cards to
        // sit next to each other.
        var layout = new DynamicLayout
        {
            Spacing = new Size(0, 20)
        };

        layout.AddRow(inputCard);
        layout.AddRow(resultCard);
        layout.AddRow(quoteCard);

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
        _waxVolumeStepper.ValueChanged += (_, _) => Recalculate();
        _designFeeStepper.ValueChanged += (_, _) => UpdateQuote();

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
                    // mm³, not grams - the business sometimes can't get a
                    // reliable volume straight out of Rhino and instead
                    // exports to Materialise to get it there, so this takes
                    // the volume directly rather than a weight that would
                    // need converting back through an assumed wax density.
                    FieldLabel("Wax volume (mm³)"),
                    _waxVolumeStepper
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
            SetResult(null, null, null);
            return;
        }

        // Both modes resolve to a volume in cm³ first - "manual" just starts
        // from mm³ (what the business actually has on hand day to day) and
        // converts, rather than the old gram-plus-assumed-wax-density path.
        decimal volumeCm3;

        if (_modeSelector.SelectedIndex == 1)
        {
            if (_selectedVolumeCm3 is null || _selectedVolumeCm3 <= 0)
            {
                SetResult(null, null, null);
                return;
            }

            volumeCm3 = _selectedVolumeCm3.Value;
        }
        else
        {
            var waxVolumeMm3 = (decimal)_waxVolumeStepper.Value;

            if (waxVolumeMm3 <= 0)
            {
                SetResult(null, null, null);
                return;
            }

            volumeCm3 = waxVolumeMm3 / 1000m;
        }

        var metalWeight = CastingCalculator.CalculateMetalWeightFromVolume(volumeCm3, material.Density);
        var cost = CastingCalculator.CalculateCost(metalWeight, material.PricePerGram);

        SetResult(volumeCm3 * 1000m, metalWeight, cost);
    }

    private void SetResult(decimal? volumeMm3, decimal? metalWeight, decimal? cost)
    {
        _metalWeightRow.Value = metalWeight is null ? "-" : $"{metalWeight:0.00} g";
        _costRow.Value = cost is null ? "-" : $"R {cost:N2}";

        _lastVolumeMm3 = volumeMm3;
        _lastMetalWeight = metalWeight;
        _lastMetalCost = cost;

        UpdateQuote();
    }

    /// <summary>
    /// Recomputes the Quick Quote card from the business's standing rates
    /// (CastingCalculator.DefaultWaxPrintRatePerMm3/DefaultCastingFeeRatePerGram)
    /// applied to the Calculator's own last result, plus whatever Design Fee
    /// is currently entered. Called after every Recalculate() (so it tracks
    /// the Calculator above it) and on its own when just the Design Fee
    /// changes.
    /// </summary>
    private void UpdateQuote()
    {
        var designFee = (decimal)_designFeeStepper.Value;

        var waxPrintCost = _lastVolumeMm3 is { } volumeMm3
            ? CastingCalculator.CalculateWaxPrintCost(volumeMm3)
            : (decimal?)null;

        var castingFee = _lastMetalWeight is { } metalWeight
            ? CastingCalculator.CalculateCastingFee(metalWeight)
            : (decimal?)null;

        _waxPrintRow.Value = waxPrintCost is null ? "-" : $"R {waxPrintCost:N2}";
        _castingFeeRow.Value = castingFee is null ? "-" : $"R {castingFee:N2}";
        _quoteMetalCostRow.Value = _lastMetalCost is null ? "-" : $"R {_lastMetalCost:N2}";

        var total = designFee + (waxPrintCost ?? 0) + (castingFee ?? 0) + (_lastMetalCost ?? 0);
        _totalQuoteRow.Value = $"R {total:N2}";
    }
}
