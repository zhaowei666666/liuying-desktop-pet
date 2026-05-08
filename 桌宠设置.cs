namespace LiuYingPet;

internal sealed class PetSettings
{
    public bool TopMost { get; set; } = true;

    public double Scale { get; set; } = 1.0;

    public double? Left { get; set; }

    public double? Top { get; set; }

    public bool StartWithWindows { get; set; }
}
