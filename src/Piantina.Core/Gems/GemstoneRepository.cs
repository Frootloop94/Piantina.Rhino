using System.Text.Json;
using System.Text.Json.Serialization;

namespace Piantina.Core.Gems;

/// <summary>
/// Reads/writes the gemstone catalog as JSON under the user's local
/// application data folder, so it survives Rhino restarts. Separate file
/// from materials.json since gems are a distinct catalog with no pricing.
/// Mirrors MaterialRepository's atomic-save approach exactly.
/// </summary>
public class GemstoneRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;

    public GemstoneRepository(string? filePath = null)
    {
        _filePath = filePath ?? GetDefaultFilePath();
    }

    private static string GetDefaultFilePath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Piantina");

        Directory.CreateDirectory(folder);

        return Path.Combine(folder, "gemstones.json");
    }

    public List<Gemstone> Load()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Gemstone>();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var gemstones = JsonSerializer.Deserialize<List<Gemstone>>(json, Options);

            return gemstones ?? new List<Gemstone>();
        }
        catch (Exception)
        {
            // Corrupt or unreadable file - start fresh rather than crashing the plugin.
            return new List<Gemstone>();
        }
    }

    public void Save(IEnumerable<Gemstone> gemstones)
    {
        var json = JsonSerializer.Serialize(gemstones, Options);

        // Write to a temp file first and swap it into place - see
        // MaterialRepository.Save for why (avoids a half-written file on
        // crash/power loss looking like an empty catalog).
        var tempFilePath = _filePath + ".tmp";

        File.WriteAllText(tempFilePath, json);

        if (File.Exists(_filePath))
        {
            File.Replace(tempFilePath, _filePath, destinationBackupFileName: null);
        }
        else
        {
            File.Move(tempFilePath, _filePath);
        }
    }
}
