using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace LiuYingPet;

internal sealed class AssetCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly Dictionary<PetPose, string> _stateFiles;
    private readonly Dictionary<PetPose, BitmapImage?> _cache = new();

    public AssetCatalog()
    {
        AssetsDirectory = Path.Combine(AppContext.BaseDirectory, "assets");
        ManifestPath = Path.Combine(AssetsDirectory, "manifest.json");
        _stateFiles = GetDefaultFiles();
        LoadManifest();
    }

    public string AssetsDirectory { get; }

    public string ManifestPath { get; }

    public string GetExpectedFile(PetPose pose) => _stateFiles.TryGetValue(pose, out var file)
        ? file
        : _stateFiles[PetPose.Idle];

    public BitmapImage? TryLoad(PetPose pose, out string problem)
    {
        if (_cache.TryGetValue(pose, out var cached))
        {
            problem = cached is null ? $"缺少素材：{GetExpectedFile(pose)}" : "";
            return cached;
        }

        var relativeFile = GetExpectedFile(pose);
        var fullPath = Path.Combine(AssetsDirectory, relativeFile);

        if (!File.Exists(fullPath))
        {
            problem = $"缺少素材：{relativeFile}";
            _cache[pose] = null;
            return null;
        }

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(fullPath, UriKind.Absolute);
            image.EndInit();
            image.Freeze();

            problem = "";
            _cache[pose] = image;
            return image;
        }
        catch (Exception ex)
        {
            problem = $"素材无法读取：{relativeFile}";
            _cache[pose] = null;
            ErrorLogService.Append(new InvalidOperationException(
                $"Failed to load asset '{fullPath}' for pose '{pose}'.",
                ex));
            return null;
        }
    }

    public void ClearCache() => _cache.Clear();

    private void LoadManifest()
    {
        try
        {
            if (!File.Exists(ManifestPath))
            {
                return;
            }

            var json = File.ReadAllText(ManifestPath);
            var manifest = JsonSerializer.Deserialize<ManifestDocument>(json, JsonOptions);
            if (manifest?.States is null)
            {
                return;
            }

            foreach (var item in manifest.States)
            {
                if (Enum.TryParse(item.Key, ignoreCase: true, out PetPose pose) &&
                    !string.IsNullOrWhiteSpace(item.Value))
                {
                    _stateFiles[pose] = item.Value.Trim();
                }
            }
        }
        catch (Exception ex)
        {
            ErrorLogService.Append(new InvalidOperationException(
                $"Failed to parse manifest '{ManifestPath}'.",
                ex));
            // Keep defaults when the manifest is temporarily malformed.
        }
    }

    private static Dictionary<PetPose, string> GetDefaultFiles() => new()
    {
        [PetPose.Idle] = "待机_正面.png",
        [PetPose.Blink] = "待机_眨眼.png",
        [PetPose.Wave] = "打招呼.png",
        [PetPose.Happy] = "靠近_开心.png",
        [PetPose.Startled] = "快速经过_受惊.png",
        [PetPose.Dragged] = "拖拽_抱起.png",
        [PetPose.RunRight1] = "小跑_右_1.png",
        [PetPose.RunRight2] = "小跑_右_2.png",
        [PetPose.RunLeft1] = "小跑_左_1.png",
        [PetPose.RunLeft2] = "小跑_左_2.png",
        [PetPose.Morning] = "早晨_伸懒腰.png",
        [PetPose.Noon] = "午间_鼓劲.png",
        [PetPose.Evening] = "傍晚_放松.png",
        [PetPose.Night] = "夜晚_犯困.png",
        [PetPose.DeepNight] = "深夜_睡觉.png",
        [PetPose.Clicked] = "点击_害羞.png",
        [PetPose.ReleaseBounce] = "待机_正面.png"
    };

    private sealed class ManifestDocument
    {
        public int Version { get; set; }

        public Dictionary<string, string>? States { get; set; }
    }
}
