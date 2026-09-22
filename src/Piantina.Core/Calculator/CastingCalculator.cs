namespace Piantina.Core.Calculator;

/// <summary>
/// Pure calculation logic for converting a model's material volume into an
/// estimated metal weight and cost. No UI or RhinoCommon dependency here on
/// purpose, so this stays reusable from Manufacturing later and is easy to
/// unit test.
/// </summary>
public static class CastingCalculator
{
    /// <summary>
    /// Typical density of jewellery casting wax, in g/cm³. Editable per-calculation
    /// since exact wax blends vary, but this is a reasonable default.
    /// </summary>
    public const decimal DefaultWaxDensity = 0.96m;

    /// <summary>
    /// Estimates metal weight from a physically weighed wax model, converting
    /// through volume using the wax's known density: volume = wax weight ÷ wax
    /// density. Prefer CalculateMetalWeightFromVolume when the volume is already
    /// known exactly (e.g. computed from the 3D model itself) rather than
    /// estimated from a physical weighing.
    /// </summary>
    public static decimal CalculateMetalWeight(
        decimal waxWeightGrams,
        decimal materialDensity,
        decimal waxDensity = DefaultWaxDensity)
    {
        if (waxDensity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(waxDensity),
                "Wax density must be greater than zero.");
        }

        var volumeCm3 = waxWeightGrams / waxDensity;

        return CalculateMetalWeightFromVolume(volumeCm3, materialDensity);
    }

    /// <summary>
    /// Calculates metal weight directly from a known volume in cubic centimetres
    /// (e.g. computed from a Rhino model's VolumeMassProperties). This is the more
    /// accurate path when the model's actual volume is available, since it removes
    /// the wax-density estimation step entirely.
    /// </summary>
    public static decimal CalculateMetalWeightFromVolume(decimal volumeCm3, decimal materialDensity)
    {
        return volumeCm3 * materialDensity;
    }

    public static decimal CalculateCost(decimal metalWeightGrams, decimal pricePerGram)
    {
        return metalWeightGrams * pricePerGram;
    }
}
