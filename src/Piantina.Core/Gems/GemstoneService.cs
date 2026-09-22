namespace Piantina.Core.Gems;

public class GemstoneService
{
    private readonly GemstoneRepository _repository;
    private readonly List<Gemstone> _gemstones;

    public GemstoneService(GemstoneRepository? repository = null)
    {
        _repository = repository ?? new GemstoneRepository();

        _gemstones = _repository.Load();

        if (_gemstones.Count == 0)
        {
            _gemstones.AddRange(GetDefaultGemstones());
            Persist();
        }
    }

    /// <summary>
    /// A starting set of common jewellery gemstones. Colour, transparency and
    /// refractive index are typical values for each gem type - not a
    /// certified gemmological spec sheet, just enough for a plausible
    /// viewport swatch. Deliberately no pricing: unlike a metal, a gem's
    /// value depends heavily on its own cut, clarity and grade rather than
    /// just its type, so a single price on the catalog entry would be
    /// misleading rather than useful.
    /// </summary>
    private static IEnumerable<Gemstone> GetDefaultGemstones()
    {
        var gemstones = new List<Gemstone>
        {
            NewGemstone("Diamond", GemCategory.Diamond, 245, 250, 255, 0.95, 2.42, 0.50, 0.95),
            NewGemstone("Ruby", GemCategory.Ruby, 180, 20, 45, 0.75, 1.77, 0.40, 0.85),
            NewGemstone("Sapphire", GemCategory.Sapphire, 20, 70, 160, 0.75, 1.77, 0.40, 0.85),
            NewGemstone("Emerald", GemCategory.Emerald, 10, 130, 80, 0.70, 1.58, 0.35, 0.80),
            NewGemstone("Amethyst", GemCategory.Amethyst, 120, 60, 170, 0.80, 1.55, 0.35, 0.80),
            NewGemstone("Blue Topaz", GemCategory.Topaz, 140, 200, 220, 0.85, 1.62, 0.35, 0.82),
            NewGemstone("Aquamarine", GemCategory.Aquamarine, 150, 210, 210, 0.85, 1.57, 0.35, 0.80),
            NewGemstone("Garnet", GemCategory.Garnet, 110, 20, 30, 0.70, 1.76, 0.40, 0.82),
            NewGemstone("Peridot", GemCategory.Peridot, 150, 190, 60, 0.80, 1.67, 0.35, 0.80),
            NewGemstone("Tanzanite", GemCategory.Tanzanite, 75, 65, 160, 0.80, 1.70, 0.35, 0.82)
        };

        for (var i = 0; i < gemstones.Count; i++)
        {
            gemstones[i].SortOrder = i;
        }

        return gemstones;
    }

    private static Gemstone NewGemstone(
        string name,
        GemCategory category,
        byte colorR,
        byte colorG,
        byte colorB,
        double transparency,
        double refractiveIndex,
        double reflectivity,
        double shine) => new()
    {
        Name = name,
        Category = category,
        ColorR = colorR,
        ColorG = colorG,
        ColorB = colorB,
        Transparency = transparency,
        RefractiveIndex = refractiveIndex,
        Reflectivity = reflectivity,
        Shine = shine
    };

    /// <summary>
    /// Adds any built-in default gem not already present (matched by name)
    /// without touching existing gemstones. Mirrors
    /// MaterialService.AddMissingDefaults().
    /// </summary>
    public int AddMissingDefaults()
    {
        var existingNames = _gemstones
            .Select(gemstone => gemstone.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = GetDefaultGemstones()
            .Where(gemstone => !existingNames.Contains(gemstone.Name))
            .ToList();

        if (missing.Count == 0)
        {
            return 0;
        }

        _gemstones.AddRange(missing);
        Persist();

        return missing.Count;
    }

    public IReadOnlyList<Gemstone> GetGemstones(bool includeInactive = false)
    {
        return _gemstones
            .Where(gemstone => includeInactive || gemstone.IsActive)
            .ToList();
    }

    public void AddGemstone(Gemstone gemstone)
    {
        _gemstones.Add(gemstone);
        Persist();
    }

    public void SetActive(Guid gemstoneId, bool isActive)
    {
        var gemstone = _gemstones.FirstOrDefault(g => g.Id == gemstoneId);

        if (gemstone is not null)
        {
            gemstone.IsActive = isActive;
            Persist();
        }
    }

    /// <summary>
    /// Call after an in-place edit (e.g. via GemEditorDialog, which mutates the
    /// existing Gemstone instance directly) so the change is saved to disk.
    /// </summary>
    public void NotifyGemstoneUpdated()
    {
        Persist();
    }

    private void Persist()
    {
        _repository.Save(_gemstones);
    }
}
