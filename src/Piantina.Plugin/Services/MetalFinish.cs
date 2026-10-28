namespace Piantina.Plugin.Services;

/// <summary>
/// Surface finish applied on top of a Material's base appearance when assigning
/// it to selected geometry - affects how sharp the specular highlight is, not
/// the base colour. Purely a display concept with no bearing on costing, so it
/// lives in Piantina.Plugin rather than Piantina.Core.
/// </summary>
public enum MetalFinish
{
    Polished,
    Hammered,
    SandBlast
}
