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
        catch (Exception ex)
        {
            ErrorLogService.Append(new InvalidOperationException(
                $"Failed to load settings from '{SettingsPath}'.",
                ex));
            return new PetSettings();
        }
    }

    public void Save(PetSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            var tempPath = SettingsPath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, SettingsPath, overwrite: true);
        }
        catch (Exception ex)
        {
            try
            {
                var tempPath = SettingsPath + ".tmp";
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch
            {
                // Best effort cleanup only.
            }

            ErrorLogService.Append(new InvalidOperationException(
                $"Failed to save settings to '{SettingsPath}'.",
                ex));
        }
    }
}
