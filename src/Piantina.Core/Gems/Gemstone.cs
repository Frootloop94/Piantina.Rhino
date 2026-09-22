namespace Piantina.Core.Gems;

public class Gemstone
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public GemCategory Category { get; set; }

    public bool IsActive { get; set; } = true;

    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Controls display order within a category (ascending; ties break on
    /// Name). Defaults to last-place for anything that doesn't set it
    /// explicitly (e.g. a gem the user adds by hand).
    /// </summary>
    public int SortOrder { get; set; } = int.MaxValue;

    /// <summary>
    /// Base viewport appearance colour, used to draw the gem's swatch and to
    /// build the Rhino render material applied to selected geometry. Kept as
    /// plain RGB bytes rather than a UI colour type so this project stays
    /// free of any UI or RhinoCommon dependency.
    /// </summary>
    public byte ColorR { get; set; } = 200;

    public byte ColorG { get; set; } = 200;

    public byte ColorB { get; set; } = 200;

    /// <summary>
    /// 0 (opaque) to 1 (fully clear). Unlike a metal, a gem's appearance is
    /// dominated by how much light passes through it rather than how much it
    /// reflects off the surface.
    /// </summary>
    public double Transparency { get; set; } = 0.7;

    /// <summary>
    /// How much light bends passing through the gem, which is what actually
    /// drives sparkle/brilliance in a raytraced or physically-shaded render.
    /// Real gems range roughly 1.4 (opal) to 2.7 (moissanite); diamond is
    /// about 2.42. Scaled directly onto Rhino's Material.IndexOfRefraction.
    /// </summary>
    public double RefractiveIndex { get; set; } = 1.5;

    /// <summary>
    /// 0 (matte) to 1 (mirror-like), matching Rhino's Material.Reflectivity range.
    /// </summary>
    public double Reflectivity { get; set; } = 0.3;

    /// <summary>
    /// 0 (no highlight) to 1 (sharp highlight). Scaled up to Rhino's
    /// Material.Shine range (0-255) when the appearance is applied.
    /// </summary>
    public double Shine { get; set; } = 0.9;
}
