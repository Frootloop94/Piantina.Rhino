using System.Text.Json;
using System.Text.Json.Serialization;

namespace Piantina.Core.Materials;

/// <summary>
/// Reads/writes the material catalog as JSON under the user's local application
/// data folder, so it survives Rhino restarts and navigating away from the
/// Materials view. Kept separate from MaterialService so the storage mechanism
/// (a file today, maybe a database later) can change independently of the
/// business logic that uses it.
/// </summary>
public class MaterialRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;

    public MaterialRepository(string? filePath = null)
    {
        _filePath = filePath ?? GetDefaultFilePath();
    }

    private static string GetDefaultFilePath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Piantina");

        Directory.CreateDirectory(folder);

        return Path.Combine(folder, "materials.json");
    }

    public List<Material> Load()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Material>();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var materials = JsonSerializer.Deserialize<List<Material>>(json, Options);

            return materials ?? new List<Material>();
        }
        catch (Exception)
        {
            // Corrupt or unreadable file - start fresh rather than crashing the plugin.
            return new List<Material>();
        }
    }

    public void Save(IEnumerable<Material> materials)
    {
        var json = JsonSerializer.Serialize(materials, Options);

        // Write to a temp file first and swap it into place, rather than
        // overwriting materials.json directly. A crash, power loss, or antivirus
        // lock mid-write can otherwise leave the file half-written; Load() treats
        // an unreadable file as empty, which would look like every material the
        // user has added silently vanished the next time the plugin starts.
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
