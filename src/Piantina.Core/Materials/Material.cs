namespace Piantina.Core.Materials;

public class Material
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public MaterialCategory Category { get; set; }

    public decimal Density { get; set; }

    public decimal PricePerGram { get; set; }

    public bool IsActive { get; set; } = true;

    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Base viewport appearance colour, used to draw the material's swatch and to
    /// build the Rhino render material applied to selected geometry. Kept as
    /// plain RGB bytes rather than a UI colour type so this project stays free
    /// of any UI or RhinoCommon dependency.
    /// </summary>
    public byte ColorR { get; set; } = 200;

    public byte ColorG { get; set; } = 200;

    public byte ColorB { get; set; } = 200;

    /// <summary>
    /// 0 (matte) to 1 (mirror-like), matching Rhino's Material.Reflectivity range.
    /// </summary>
    public double Reflectivity { get; set; } = 0.5;

    /// <summary>
    /// 0 (no highlight) to 1 (sharp highlight). Scaled up to Rhino's
    /// Material.Shine range (0-255) when the appearance is applied.
    /// </summary>
    public double Shine { get; set; } = 0.6;
}
