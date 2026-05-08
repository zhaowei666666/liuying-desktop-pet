using System.IO;

namespace LiuYingPet;

internal static class ErrorLogService
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "流萤桌宠");

    private static readonly string LogPath = Path.Combine(LogDirectory, "错误日志.txt");

    public static void Append(Exception exception)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            File.AppendAllText(
                LogPath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\n{exception}\n\n");
        }
        catch
        {
            // Logging must never crash the pet.
        }
    }
}
