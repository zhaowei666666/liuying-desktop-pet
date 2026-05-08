namespace LiuYingPet;

internal static class CommandLineTime
{
    public static TimeOnly? Parse(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], "--time", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (i + 1 < args.Length && TimeOnly.TryParse(args[i + 1], out var time))
            {
                return time;
            }
        }

        return null;
    }
}
