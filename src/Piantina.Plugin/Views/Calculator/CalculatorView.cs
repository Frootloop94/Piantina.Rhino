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
    // Shared by every InfoRow in Result/Quick Quote so their value columns
    // line up into one right-aligned table - see InfoRow.SetColumnWidths.
    // Wide enough for the longest label here ("Total metal cost") and a
    // six-figure Rand value.
    private const int ResultLabelWidth = 115;
    private const int ResultValueWidth = 90;

    private readonly MaterialService _materialService;

    private readonly DropDown _materialDropDown;
    private readonly NumericStepper _volume1Stepper;

    // Second metal - fixed to exactly one extra metal (not an open-ended
    // list) since that covers the business's actual case ("sometimes two
    // metals part of the quote") without the extra UI an arbitrary-length
    // list would need in a narrow sidebar. Mirrors Material 1's own
    // dropdown + volume stepper pair exactly, including that both are
    // always plain manual fields - "Use Selected Object" below just writes
    // into them, the same as a user typing a value in by hand.
    private readonly CheckBox _secondMetalCheckBox;
    private readonly DropDown _material2DropDown;
    private readonly NumericStepper _volume2Stepper;

    // Hosts whichever field set is active; its Content is replaced wholesale
    // rather than clearing/rebuilding the Card's own content area, since
    // Eto's DynamicLayout (which Card uses internally) doesn't reliably
    // support Clear()+AddRow() reuse. Same pattern MaterialList already uses
    // for its rebuildable list via _materialHost.
    private readonly Panel _secondMetalInputHost;

    private readonly PrimaryButton _getVolumeButton;
    private readonly Label _volumeValueLabel;

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
    // print, Casting fee, and the metal cost line(s) are all worked out from
    // the Calculator's own last result above, so nothing has to be
    // re-entered or can drift out of sync with it.
    private readonly NumericStepper _designFeeStepper;
    private readonly InfoRow _waxPrintRow;
    private readonly InfoRow _castingFeeRow;
    private readonly Panel _quoteMetalCostHost;
    private readonly InfoRow _totalQuoteRow;

    private MetalResult? _lastMetal1;
    private MetalResult? _lastMetal2;

    private List<Material> _materials = new();

    public CalculatorView(MaterialService materialService)
    {
        Padding = 20;

        _materialService = materialService;

        _materialDropDown = new DropDown { Font = AppFonts.Body };

        _volume1Stepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 1,
            MinValue = 0,
            MaxValue = 1_000_000,
            Increment = 10,
            Value = 0
        };

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

        _getVolumeButton = new PrimaryButton("Use Selected Object", UseSelectedObjectVolume);

        _volumeValueLabel = new Label
        {
            Font = AppFonts.Body,
            TextColor = AppColors.TextMuted,
            Text = "No object selected yet"
        };

        // Material 1's own fields are wrapped in a StackLayout the same way
        // Material 2's are (via _secondMetalInputHost below), rather than
        // added to the Card as individual rows directly - mixing the two
        // approaches was what made the Material dropdown render stretched
        // to the Card's full width while Material 2's own dropdown didn't.
        var material1Section = new StackLayout
        {
            Spacing = 8,
            Items =
            {
                FieldLabel("Material"),
                _materialDropDown,
                FieldLabel("Volume 1 (mm³)"),
                _volume1Stepper
            }
        };

        // "Use Selected Object" sits at the very bottom of the card, below
        // both metals' own fields - it can populate everything above it in
        // one click (see UseSelectedObjectVolume), so it reads as the final
        // step rather than being buried in the middle of the other fields.
        var selectionSection = new StackLayout
        {
            Spacing = 8,
            Items =
            {
                FieldLabel("Volume from selection"),
                _getVolumeButton,
                _volumeValueLabel
            }
        };

        var inputCard = new Card("Casting Calculator")
            .WithContent(
                material1Section,
                _secondMetalCheckBox,
                _secondMetalInputHost,
                selectionSection);

        _metalWeightRow = new InfoRow("Metal weight", "-");
        _costRow = new InfoRow("Estimated cost", "-");

        _metal2WeightRow = new InfoRow("Metal 2 weight", "-");
        _metal2CostRow = new InfoRow("Metal 2 cost", "-");
        _totalWeightRow = new InfoRow("Total weight", "-");
        _totalMetalCostRow = new InfoRow("Total metal cost", "-");
        _secondMetalResultHost = new Panel();

        // Fixed column widths rather than each InfoRow sizing itself to its
        // own text - otherwise every row in Result/Quick Quote lines up its
        // value right after that row's own (differently-sized) label
        // instead of into one shared right-aligned column. Wide enough for
        // the longest label here ("Total metal cost") and a six-figure Rand
        // value.
        foreach (var row in new[]
                 {
                     _metalWeightRow, _costRow,
                     _metal2WeightRow, _metal2CostRow, _totalWeightRow, _totalMetalCostRow
                 })
        {
            row.SetColumnWidths(labelWidth: ResultLabelWidth, valueWidth: ResultValueWidth);
        }

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
        _quoteMetalCostHost = new Panel();
        _totalQuoteRow = new InfoRow("Total Quote", "-");

        foreach (var row in new[] { _waxPrintRow, _castingFeeRow, _totalQuoteRow })
        {
            row.SetColumnWidths(labelWidth: ResultLabelWidth, valueWidth: ResultValueWidth);
        }

        var quoteCard = new Card("Quick Quote")
            .WithContent(
                FieldLabel("Design Fee (R)"),
                _designFeeStepper,
                _waxPrintRow,
                _castingFeeRow,
                _quoteMetalCostHost,
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
        _volume1Stepper.ValueChanged += (_, _) => Recalculate();
        _secondMetalCheckBox.CheckedChanged += (_, _) => RebuildSecondMetalFields();
        _material2DropDown.SelectedKeyChanged += (_, _) => Recalculate();
        _volume2Stepper.ValueChanged += (_, _) => Recalculate();
        _designFeeStepper.ValueChanged += (_, _) => UpdateQuote();

        LoadMaterials();
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

        Recalculate();
    }

    /// <summary>
    /// Swaps _secondMetalInputHost's content between empty and the
    /// Material 2/Volume 2 fields, following _secondMetalCheckBox.
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
    /// Selection - filling Volume 1 (and, if a second distinct material is
    /// found, ticking "Add second metal" and filling Material 2/Volume 2
    /// too) rather than requiring everything to be picked/typed by hand.
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
            _volumeValueLabel.Text = groups.Count == 0 && unmatchedCount == 0
                ? "No solid object selected in Rhino"
                : "No selected object's material matches a catalog material.\nApply a material from the Materials tab first, or set Material/Volume by hand above.";
            _volumeValueLabel.TextColor = Colors.Crimson;

            Recalculate();
            return;
        }

        var (firstGroup, firstMaterial) = matchedGroups[0];

        _materialDropDown.SelectedKey = firstMaterial!.Id.ToString();
        _volume1Stepper.Value = (double)(firstGroup.VolumeCm3 * 1000m);

        var lines = new List<string> { $"{firstMaterial.Name}: {firstGroup.VolumeCm3:0.000} cm³" };

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

            lines.Add($"{secondMaterial!.Name}: {secondGroup.VolumeCm3:0.000} cm³");
        }

        if (skippedCount > 0)
        {
            lines.Add(skippedCount == 1
                ? "1 object skipped - no matching material."
                : $"{skippedCount} objects skipped - no matching material.");
        }

        // One line per material/message rather than a single comma-joined
        // sentence, so a two-material result reads as a short list, not a
        // run-on.
        _volumeValueLabel.Text = string.Join("\n", lines);
        _volumeValueLabel.TextColor = AppColors.Text;

        Recalculate();
    }

    private record MetalResult(string MaterialName, decimal VolumeMm3, decimal Weight, decimal Cost);

    /// <summary>
    /// Resolves one metal's volume (cm³) against a material to a full
    /// result, or null if either input isn't usable yet - shared by both
    /// metals.
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

        return new MetalResult(material.Name, v * 1000m, weight, cost);
    }

    private void Recalculate()
    {
        var volume1Mm3 = (decimal)_volume1Stepper.Value;
        var volume1Cm3 = volume1Mm3 > 0 ? volume1Mm3 / 1000m : (decimal?)null;

        var metal1 = ComputeMetalResult(_materialDropDown.SelectedKey, volume1Cm3);

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
        var metal1Code = metal1 is null ? null : MaterialAbbreviation.Abbreviate(metal1.MaterialName);

        _metalWeightRow.Label = metal1Code is null ? "Metal weight" : $"{metal1Code} weight";
        _metalWeightRow.Value = metal1 is null ? "-" : $"{metal1.Weight:0.00} g";
        _costRow.Label = metal1Code is null ? "Estimated cost" : $"{metal1Code} cost";
        _costRow.Value = metal1 is null ? "-" : $"R {metal1.Cost:N2}";

        if (metal2 is not null)
        {
            var metal2Code = MaterialAbbreviation.Abbreviate(metal2.MaterialName);

            _metal2WeightRow.Label = $"{metal2Code} weight";
            _metal2WeightRow.Value = $"{metal2.Weight:0.00} g";
            _metal2CostRow.Label = $"{metal2Code} cost";
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

        _lastMetal1 = metal1;
        _lastMetal2 = metal2;

        UpdateQuote();
    }

    /// <summary>
    /// Recomputes the Quick Quote card from the business's standing rates
    /// (CastingCalculator.DefaultWaxPrintRatePerMm3/DefaultCastingFeeRatePerGram)
    /// applied to the Calculator's own last result, plus whatever Design Fee
    /// is currently entered. Called after every Recalculate() (so it tracks
    /// the Calculator above it) and on its own when just the Design Fee
    /// changes. The metal cost line(s) are named after whichever material(s)
    /// are actually in use - one row normally, two when a second metal is
    /// added - rather than a single generic "Metal cost" row.
    /// </summary>
    private void UpdateQuote()
    {
        var designFee = (decimal)_designFeeStepper.Value;

        decimal? totalVolumeMm3 = null;
        decimal? totalWeight = null;
        decimal totalMetalCost = 0;

        var metalCostRows = new List<Control>();

        foreach (var metal in new[] { _lastMetal1, _lastMetal2 })
        {
            if (metal is null)
            {
                continue;
            }

            totalVolumeMm3 = (totalVolumeMm3 ?? 0) + metal.VolumeMm3;
            totalWeight = (totalWeight ?? 0) + metal.Weight;
            totalMetalCost += metal.Cost;

            var metalCode = MaterialAbbreviation.Abbreviate(metal.MaterialName);

            var metalCostRow = new InfoRow($"{metalCode} cost", $"R {metal.Cost:N2}");
            metalCostRow.SetColumnWidths(labelWidth: ResultLabelWidth, valueWidth: ResultValueWidth);

            metalCostRows.Add(metalCostRow);
        }

        if (metalCostRows.Count == 0)
        {
            var emptyRow = new InfoRow("Metal cost", "-");
            emptyRow.SetColumnWidths(labelWidth: ResultLabelWidth, valueWidth: ResultValueWidth);

            _quoteMetalCostHost.Content = emptyRow;
        }
        else
        {
            var metalCostLayout = new StackLayout { Spacing = 4 };

            foreach (var row in metalCostRows)
            {
                metalCostLayout.Items.Add(row);
            }

            _quoteMetalCostHost.Content = metalCostLayout;
        }

        var waxPrintCost = totalVolumeMm3 is { } volumeMm3
            ? CastingCalculator.CalculateWaxPrintCost(volumeMm3)
            : (decimal?)null;

        var castingFee = totalWeight is { } weight
            ? CastingCalculator.CalculateCastingFee(weight)
            : (decimal?)null;

        _waxPrintRow.Value = waxPrintCost is null ? "-" : $"R {waxPrintCost:N2}";
        _castingFeeRow.Value = castingFee is null ? "-" : $"R {castingFee:N2}";

        var total = designFee + (waxPrintCost ?? 0) + (castingFee ?? 0) + totalMetalCost;
        _totalQuoteRow.Value = $"R {total:N2}";
    }
}
