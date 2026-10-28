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

    // From Rhino selection mode - shown at the bottom of the card (see
    // _selectionFieldsHost), separate from the mode selector/Material
    // fields above it, since one click here can populate everything above
    // it (both metals, not just this one).
    private readonly PrimaryButton _getVolumeButton;
    private readonly Label _volumeValueLabel;
    private readonly Panel _selectionFieldsHost;
    private decimal? _selectedVolumeCm3;

    // Second metal - fixed to exactly one extra metal (not an open-ended
    // list) since that covers the business's actual case ("sometimes two
    // metals part of the quote") without the extra UI an arbitrary-length
    // list would need in a narrow sidebar. Manual volume entry only, no
    // "From Rhino selection" mode of its own - picking which of two
    // selected solids is which metal adds real complexity for a case that's
    // already the exception rather than the rule.
    private readonly CheckBox _secondMetalCheckBox;
    private readonly DropDown _material2DropDown;
    private readonly NumericStepper _volume2Stepper;
    private readonly Panel _secondMetalInputHost;

    private readonly InfoRow _metalWeightRow;
    private readonly InfoRow _costRow;

    // Only populated (via _secondMetalResultHost.Content) while
    // _secondMetalCheckBox is checked and resolves to a real result - see
    // SetResult().
    private readonly Panel _secondMetalResultHost;
    private readonly InfoRow _metal2WeightRow;
    private readonly InfoRow _metal2CostRow;
    private readonly InfoRow _totalWeightRow;
    private readonly InfoRow _totalMetalCostRow;

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
        _selectionFieldsHost = new Panel();

        _secondMetalCheckBox = new CheckBox { Text = "Add second metal" };

        _material2DropDown = new DropDown { Font = AppFonts.Body };

        _volume2Stepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 1,
            MinValue = 0,
            MaxValue = 1_000_000,
            Increment = 10,
            Value = 0
        };

        _secondMetalInputHost = new Panel();

        // Mode selector first, then Material 1 and (if second metal is
        // added) Material 2's own fields, and the "Use Selected Object"
        // button/result last - it can populate everything above it in one
        // click (see UseSelectedObjectVolume), so it reads as the final step
        // rather than being buried in the middle of the other fields.
        var inputCard = new Card("Casting Calculator")
            .WithContent(
                _modeSelector,
                FieldLabel("Material"),
                _materialDropDown,
                _dynamicFieldsHost,
                _secondMetalCheckBox,
                _secondMetalInputHost,
                _selectionFieldsHost);

        _metalWeightRow = new InfoRow("Metal weight", "-");
        _costRow = new InfoRow("Estimated cost", "-");

        _metal2WeightRow = new InfoRow("Metal 2 weight", "-");
        _metal2CostRow = new InfoRow("Metal 2 cost", "-");
        _totalWeightRow = new InfoRow("Total weight", "-");
        _totalMetalCostRow = new InfoRow("Total metal cost", "-");
        _secondMetalResultHost = new Panel();

        var resultCard = new Card("Result")
            .WithContent(
                _metalWeightRow,
                _costRow,
                _secondMetalResultHost);

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
        _secondMetalCheckBox.CheckedChanged += (_, _) => RebuildSecondMetalFields();
        _material2DropDown.SelectedKeyChanged += (_, _) => Recalculate();
        _volume2Stepper.ValueChanged += (_, _) => Recalculate();
        _designFeeStepper.ValueChanged += (_, _) => UpdateQuote();

        LoadMaterials();
        RebuildDynamicFields();
        RebuildSecondMetalFields();
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
        _material2DropDown.Items.Clear();

        foreach (var material in _materials)
        {
            _materialDropDown.Items.Add(new ListItem { Text = material.Name, Key = material.Id.ToString() });
            _material2DropDown.Items.Add(new ListItem { Text = material.Name, Key = material.Id.ToString() });
        }

        if (_materials.Count > 0)
        {
            _materialDropDown.SelectedKey = _materials[0].Id.ToString();
            _material2DropDown.SelectedKey = _materials[0].Id.ToString();
        }
    }

    /// <summary>
    /// Swaps _dynamicFieldsHost/_selectionFieldsHost's content for a fresh
    /// layout depending on the selected mode. The individual controls
    /// (steppers, button, label) are created once in the constructor and
    /// keep their values across a swap. Manual volume entry sits right under
    /// Material (where it's needed); the Rhino-selection button/result sits
    /// in _selectionFieldsHost instead, at the very bottom of the card (see
    /// its placement in the constructor) rather than here, since it can also
    /// populate Material 2 further up the card.
    /// </summary>
    private void RebuildDynamicFields()
    {
        var usingSelection = _modeSelector.SelectedIndex == 1;

        _dynamicFieldsHost.Content = usingSelection
            ? null
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

        _selectionFieldsHost.Content = usingSelection
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
            : null;

        Recalculate();
    }

    /// <summary>
    /// Swaps _secondMetalInputHost's content between empty and the
    /// Material 2/Volume 2 fields, following _secondMetalCheckBox - same
    /// wholesale-Content-replace pattern as RebuildDynamicFields() above.
    /// </summary>
    private void RebuildSecondMetalFields()
    {
        _secondMetalInputHost.Content = _secondMetalCheckBox.Checked == true
            ? new StackLayout
            {
                Spacing = 8,
                Items =
                {
                    FieldLabel("Material 2"),
                    _material2DropDown,
                    FieldLabel("Volume 2 (mm³)"),
                    _volume2Stepper
                }
            }
            : null;

        Recalculate();
    }

    /// <summary>
    /// Splits the current selection's volume by whichever Piantina material
    /// each object is wearing (see RhinoGeometryService.GetSelectedVolumesByMaterial),
    /// so one click handles both a plain single-metal selection and a
    /// two-tone piece whose parts were already materialed via Apply to
    /// Selection - populating Material 1 (and, if a second distinct material
    /// is found, ticking "Add second metal" and populating Material 2 too)
    /// rather than requiring the material to also be picked by hand.
    ///
    /// Only matches objects wearing a Rhino material this plugin itself
    /// created; anything else (materialed by hand, imported already-
    /// textured, left at the layer default) is skipped and called out in the
    /// result label rather than silently mixed into whichever metal it's
    /// closest to.
    /// </summary>
    private void UseSelectedObjectVolume()
    {
        var (groups, unmatchedCount) = RhinoGeometryService.GetSelectedVolumesByMaterial();

        // Only the two largest groups are kept - this calculator only
        // supports a fixed two metals, so a selection with more than that
        // (unusual) still populates the two biggest rather than refusing
        // outright, and says so in the label.
        var matchedGroups = groups
            .Select(group => (Group: group, Material: _materials.FirstOrDefault(m =>
                string.Equals(m.Name, group.MaterialName, StringComparison.OrdinalIgnoreCase))))
            .Where(pair => pair.Material is not null)
            .Take(2)
            .ToList();

        var skippedCount = unmatchedCount + (groups.Count - matchedGroups.Count);

        if (matchedGroups.Count == 0)
        {
            _selectedVolumeCm3 = null;

            _volumeValueLabel.Text = groups.Count == 0 && unmatchedCount == 0
                ? "No solid object selected in Rhino"
                : "No selected object's material matches a catalog material. Apply a material from the Materials tab first, or set Material/Volume by hand above.";
            _volumeValueLabel.TextColor = Colors.Crimson;

            Recalculate();
            return;
        }

        var (firstGroup, firstMaterial) = matchedGroups[0];

        _materialDropDown.SelectedKey = firstMaterial!.Id.ToString();
        _selectedVolumeCm3 = firstGroup.VolumeCm3;

        if (matchedGroups.Count > 1)
        {
            var (secondGroup, secondMaterial) = matchedGroups[1];

            _material2DropDown.SelectedKey = secondMaterial!.Id.ToString();
            _volume2Stepper.Value = (double)(secondGroup.VolumeCm3 * 1000m);

            if (_secondMetalCheckBox.Checked != true)
            {
                _secondMetalCheckBox.Checked = true;
                RebuildSecondMetalFields();
            }

            _volumeValueLabel.Text =
                $"{firstMaterial.Name}: {firstGroup.VolumeCm3:0.000} cm³, {secondMaterial!.Name}: {secondGroup.VolumeCm3:0.000} cm³";
        }
        else
        {
            _volumeValueLabel.Text = $"{firstGroup.VolumeCm3:0.000} cm³";
        }

        if (skippedCount > 0)
        {
            _volumeValueLabel.Text += skippedCount == 1
                ? " (1 object skipped - no matching material.)"
                : $" ({skippedCount} objects skipped - no matching material.)";
        }

        _volumeValueLabel.TextColor = AppColors.Text;

        Recalculate();
    }

    private record MetalResult(decimal VolumeMm3, decimal Weight, decimal Cost);

    /// <summary>
    /// Resolves one metal's volume (cm³) against a material to a full
    /// result, or null if either input isn't usable yet - shared by the
    /// primary material/mode and the optional second metal.
    /// </summary>
    private MetalResult? ComputeMetalResult(string? materialKey, decimal? volumeCm3)
    {
        if (volumeCm3 is not { } v || v <= 0)
        {
            return null;
        }

        var material = _materials.FirstOrDefault(m => m.Id.ToString() == materialKey);

        if (material is null)
        {
            return null;
        }

        var weight = CastingCalculator.CalculateMetalWeightFromVolume(v, material.Density);
        var cost = CastingCalculator.CalculateCost(weight, material.PricePerGram);

        return new MetalResult(v * 1000m, weight, cost);
    }

    private void Recalculate()
    {
        // Both modes resolve to a volume in cm³ first - "manual" just starts
        // from mm³ (what the business actually has on hand day to day) and
        // converts, rather than the old gram-plus-assumed-wax-density path.
        decimal? primaryVolumeCm3;

        if (_modeSelector.SelectedIndex == 1)
        {
            primaryVolumeCm3 = _selectedVolumeCm3;
        }
        else
        {
            var waxVolumeMm3 = (decimal)_waxVolumeStepper.Value;
            primaryVolumeCm3 = waxVolumeMm3 > 0 ? waxVolumeMm3 / 1000m : null;
        }

        var metal1 = ComputeMetalResult(_materialDropDown.SelectedKey, primaryVolumeCm3);

        MetalResult? metal2 = null;

        if (_secondMetalCheckBox.Checked == true)
        {
            var volume2Mm3 = (decimal)_volume2Stepper.Value;
            var volume2Cm3 = volume2Mm3 > 0 ? volume2Mm3 / 1000m : (decimal?)null;

            metal2 = ComputeMetalResult(_material2DropDown.SelectedKey, volume2Cm3);
        }

        SetResult(metal1, metal2);
    }

    private void SetResult(MetalResult? metal1, MetalResult? metal2)
    {
        _metalWeightRow.Value = metal1 is null ? "-" : $"{metal1.Weight:0.00} g";
        _costRow.Value = metal1 is null ? "-" : $"R {metal1.Cost:N2}";

        if (metal2 is not null)
        {
            _metal2WeightRow.Value = $"{metal2.Weight:0.00} g";
            _metal2CostRow.Value = $"R {metal2.Cost:N2}";
            _totalWeightRow.Value = $"{(metal1?.Weight ?? 0) + metal2.Weight:0.00} g";
            _totalMetalCostRow.Value = $"R {(metal1?.Cost ?? 0) + metal2.Cost:N2}";

            _secondMetalResultHost.Content = new StackLayout
            {
                Spacing = 4,
                Items = { _metal2WeightRow, _metal2CostRow, _totalWeightRow, _totalMetalCostRow }
            };
        }
        else
        {
            _secondMetalResultHost.Content = null;
        }

        decimal? totalVolumeMm3 = null;
        decimal? totalWeight = null;
        decimal? totalCost = null;

        foreach (var metal in new[] { metal1, metal2 })
        {
            if (metal is null)
            {
                continue;
            }

            totalVolumeMm3 = (totalVolumeMm3 ?? 0) + metal.VolumeMm3;
            totalWeight = (totalWeight ?? 0) + metal.Weight;
            totalCost = (totalCost ?? 0) + metal.Cost;
        }

        _lastVolumeMm3 = totalVolumeMm3;
        _lastMetalWeight = totalWeight;
        _lastMetalCost = totalCost;

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
