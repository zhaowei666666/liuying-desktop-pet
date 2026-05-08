using System.IO;
using System.Text.Json;

namespace LiuYingPet;

internal sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string SettingsDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "流萤桌宠");

    public string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    public PetSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new PetSettings();
            }

            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<PetSettings>(json, JsonOptions) ?? new PetSettings();
        }
        catch
        {
            return new PetSettings();
        }
    }

    public void Save(PetSettings settings)
    {
        Directory.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }
}
