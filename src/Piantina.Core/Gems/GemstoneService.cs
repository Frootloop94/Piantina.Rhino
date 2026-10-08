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
    /// A starting set of common jewellery gemstones. Colour and refractive
    /// index are typical values for each gem type - not a certified
    /// gemmological spec sheet, just enough for a plausible viewport swatch.
    /// Deliberately no pricing: unlike a metal, a gem's value depends
    /// heavily on its own cut, clarity and grade rather than just its type,
    /// so a single price on the catalog entry would be misleading rather
    /// than useful.
    ///
    /// Transparency here is tuned for how it actually looks, not physical
    /// accuracy: Rhino's non-raytraced Shaded/Rendered display modes don't
    /// do refraction or caustics, so a highly transparent material (a
    /// physically accurate diamond, say) just fades toward invisible against
    /// a light background instead of looking like glass. Lower transparency
    /// than the real gem would have reads as a visibly coloured, sparkly
    /// stone in that display mode; true refraction only shows up in Rhino's
    /// Raytraced mode, which RefractiveIndex is set accurately for anyway.
    /// </summary>
    private static IEnumerable<Gemstone> GetDefaultGemstones()
    {
        var gemstones = new List<Gemstone>
        {
            NewGemstone("Diamond", GemCategory.Diamond, 230, 240, 250, 0.30, 2.42, 0.65, 0.97),
            NewGemstone("Ruby", GemCategory.Ruby, 175, 15, 40, 0.45, 1.77, 0.50, 0.90),
            NewGemstone("Sapphire", GemCategory.Sapphire, 15, 60, 150, 0.45, 1.77, 0.50, 0.90),
            NewGemstone("Emerald", GemCategory.Emerald, 5, 120, 70, 0.40, 1.58, 0.45, 0.88),
            NewGemstone("Amethyst", GemCategory.Amethyst, 110, 50, 165, 0.45, 1.55, 0.45, 0.88),
            NewGemstone("Blue Topaz", GemCategory.Topaz, 120, 190, 215, 0.50, 1.62, 0.45, 0.88),
            NewGemstone("Aquamarine", GemCategory.Aquamarine, 130, 200, 200, 0.50, 1.57, 0.45, 0.88),
            NewGemstone("Garnet", GemCategory.Garnet, 100, 15, 25, 0.40, 1.76, 0.50, 0.88),
            NewGemstone("Peridot", GemCategory.Peridot, 140, 185, 55, 0.45, 1.67, 0.45, 0.86),
            NewGemstone("Tanzanite", GemCategory.Tanzanite, 65, 55, 150, 0.45, 1.70, 0.45, 0.88)
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

    /// <summary>
    /// Refreshes colour/transparency/refractive index/reflectivity/shine for
    /// any gem whose name matches a built-in default, from the current
    /// default catalog - lets a tuning change to the defaults (like
    /// transparency values that looked right in theory but rendered poorly
    /// in Rhino's non-raytraced display modes) reach a catalog that already
    /// seeded the old values.
    ///
    /// Unlike MaterialService.RepairMissingAppearance, this isn't gated on
    /// "still at an untouched placeholder" - gems are seeded with real
    /// appearance values from the start, so there's no such placeholder to
    /// detect safely. That means re-running this will overwrite a hand-tuned
    /// look on a gem that still has a default name (Diamond, Ruby, ...); a
    /// gem added under a different name is never touched. Returns how many
    /// were refreshed.
    /// </summary>
    public int RefreshDefaultAppearance()
    {
        var defaultsByName = GetDefaultGemstones()
            .ToDictionary(gemstone => gemstone.Name, StringComparer.OrdinalIgnoreCase);

        var refreshed = 0;

        foreach (var gemstone in _gemstones)
        {
            if (!defaultsByName.TryGetValue(gemstone.Name, out var defaultGemstone))
            {
                continue;
            }

            gemstone.ColorR = defaultGemstone.ColorR;
            gemstone.ColorG = defaultGemstone.ColorG;
            gemstone.ColorB = defaultGemstone.ColorB;
            gemstone.Transparency = defaultGemstone.Transparency;
            gemstone.RefractiveIndex = defaultGemstone.RefractiveIndex;
            gemstone.Reflectivity = defaultGemstone.Reflectivity;
            gemstone.Shine = defaultGemstone.Shine;

            refreshed++;
        }

        if (refreshed > 0)
        {
            Persist();
        }

        return refreshed;
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
