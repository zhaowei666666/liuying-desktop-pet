namespace LiuYingPet;

internal readonly record struct MotionSample(
    double X,
    double Y,
    double Rotation,
    double ScaleX,
    double ScaleY)
{
    public static MotionSample Neutral { get; } = new(0, 0, 0, 1, 1);

    public static MotionSample Lerp(MotionSample from, MotionSample to, double amount)
    {
        amount = Math.Clamp(amount, 0, 1);
        return new MotionSample(
            Lerp(from.X, to.X, amount),
            Lerp(from.Y, to.Y, amount),
            Lerp(from.Rotation, to.Rotation, amount),
            Lerp(from.ScaleX, to.ScaleX, amount),
            Lerp(from.ScaleY, to.ScaleY, amount));
    }

    private static double Lerp(double from, double to, double amount) => from + (to - from) * amount;
}

internal readonly record struct MotionFrame(
    double At,
    double X,
    double Y,
    double Rotation,
    double ScaleX,
    double ScaleY)
{
    public MotionSample ToSample() => new(X, Y, Rotation, ScaleX, ScaleY);
}

internal sealed class MotionProfile
{
    private readonly MotionFrame[] _frames;

    public MotionProfile(double duration, params MotionFrame[] frames)
    {
        Duration = Math.Max(0.1, duration);
        _frames = frames.Length == 0
            ? [new MotionFrame(0, 0, 0, 0, 1, 1)]
            : frames.OrderBy(frame => frame.At).ToArray();
    }

    public double Duration { get; }

    public MotionSample Sample(double elapsedSeconds)
    {
        if (_frames.Length == 1)
        {
            return _frames[0].ToSample();
        }

        var time = elapsedSeconds % Duration;
        if (time < 0)
        {
            time += Duration;
        }

        var previous = _frames[0];
        var next = _frames[^1];

        for (var i = 0; i < _frames.Length; i++)
        {
            if (_frames[i].At > time)
            {
                next = _frames[i];
                previous = i == 0 ? _frames[^1] : _frames[i - 1];
                break;
            }
        }

        var segmentDuration = next.At - previous.At;
        var segmentElapsed = time - previous.At;
        if (segmentDuration <= 0)
        {
            segmentDuration = Duration - previous.At + next.At;
            segmentElapsed = time >= previous.At ? time - previous.At : Duration - previous.At + time;
        }

        var amount = SmoothStep(segmentElapsed / segmentDuration);
        return MotionSample.Lerp(previous.ToSample(), next.ToSample(), amount);
    }

    private static double SmoothStep(double value)
    {
        value = Math.Clamp(value, 0, 1);
        return value * value * (3 - 2 * value);
    }
}

internal static class KeyframeMotionLibrary
{
    private static readonly Dictionary<PetPose, MotionProfile> Profiles = new()
    {
        [PetPose.Idle] = new MotionProfile(
            2.8,
            Frame(0.00, 0, 0, 0, 1.000, 1.000),
            Frame(0.70, 0, -1.7, 0.8, 1.006, 0.995),
            Frame(1.40, 0, -2.6, 0, 0.996, 1.012),
            Frame(2.10, 0, -1.1, -0.8, 1.004, 0.998),
            Frame(2.80, 0, 0, 0, 1.000, 1.000)),

        [PetPose.Blink] = new MotionProfile(
            1.25,
            Frame(0.00, 0, 0, 0, 1.000, 1.000),
            Frame(0.18, 0, 0.7, 0, 1.010, 0.965),
            Frame(0.32, 0, -1.2, 0, 0.996, 1.018),
            Frame(0.80, 0, -1.8, 0.4, 1.003, 1.004),
            Frame(1.25, 0, 0, 0, 1.000, 1.000)),

        [PetPose.LookLeft] = new MotionProfile(
            1.8,
            Frame(0.00, -2.0, -0.4, -4.8, 1.000, 1.000),
            Frame(0.55, -3.0, -1.4, -6.2, 1.004, 1.002),
            Frame(1.20, -2.2, -0.7, -5.2, 0.998, 1.005),
            Frame(1.80, -2.0, -0.4, -4.8, 1.000, 1.000)),

        [PetPose.LookRight] = new MotionProfile(
            1.8,
            Frame(0.00, 2.0, -0.4, 4.8, 1.000, 1.000),
            Frame(0.55, 3.0, -1.4, 6.2, 1.004, 1.002),
            Frame(1.20, 2.2, -0.7, 5.2, 0.998, 1.005),
            Frame(1.80, 2.0, -0.4, 4.8, 1.000, 1.000)),

        [PetPose.Wave] = new MotionProfile(
            1.2,
            Frame(0.00, 0, 0, 0, 1.000, 1.000),
            Frame(0.18, 0, -5.0, -3.0, 1.030, 0.985),
            Frame(0.38, 1.0, -2.0, 4.0, 0.985, 1.030),
            Frame(0.62, -1.0, -4.5, -3.5, 1.020, 0.995),
            Frame(0.86, 1.0, -1.0, 3.0, 0.995, 1.015),
            Frame(1.20, 0, 0, 0, 1.000, 1.000)),

        [PetPose.Happy] = new MotionProfile(
            0.92,
            Frame(0.00, 0, -1.0, 0, 1.000, 1.000),
            Frame(0.16, 0, -9.0, 0, 0.970, 1.075),
            Frame(0.34, 0, -2.0, 0, 1.055, 0.955),
            Frame(0.56, 0, -6.5, 0, 0.985, 1.040),
            Frame(0.78, 0, -2.0, 0, 1.020, 0.985),
            Frame(0.92, 0, -1.0, 0, 1.000, 1.000)),

        [PetPose.Startled] = new MotionProfile(
            0.58,
            Frame(0.00, 0, -1.0, 0, 1.000, 1.000),
            Frame(0.08, -3.5, -8.0, -6.0, 1.065, 0.930),
            Frame(0.17, 4.0, -6.0, 5.5, 0.955, 1.060),
            Frame(0.28, -2.5, -7.0, -4.0, 1.040, 0.965),
            Frame(0.42, 2.0, -3.0, 2.0, 0.995, 1.018),
            Frame(0.58, 0, -1.0, 0, 1.000, 1.000)),

        [PetPose.Dragged] = new MotionProfile(
            1.45,
            Frame(0.00, 0, -11.0, -3.0, 1.000, 1.000),
            Frame(0.35, 2.0, -17.0, 6.5, 1.012, 0.990),
            Frame(0.72, -2.0, -13.0, -7.0, 0.992, 1.012),
            Frame(1.10, 1.5, -18.0, 5.0, 1.008, 0.996),
            Frame(1.45, 0, -11.0, -3.0, 1.000, 1.000)),

        [PetPose.RunRight1] = RunProfile(4.2, 3),
        [PetPose.RunRight2] = RunProfile(4.2, 3),
        [PetPose.RunLeft1] = RunProfile(-4.2, -3),
        [PetPose.RunLeft2] = RunProfile(-4.2, -3),

        [PetPose.Morning] = new MotionProfile(
            1.85,
            Frame(0.00, 0, 0, 0, 1.000, 1.000),
            Frame(0.32, 0, -9.0, -3.2, 0.975, 1.075),
            Frame(0.72, 0, -5.0, 3.0, 1.020, 0.990),
            Frame(1.20, 0, -2.0, -1.0, 1.006, 1.008),
            Frame(1.85, 0, 0, 0, 1.000, 1.000)),

        [PetPose.Noon] = new MotionProfile(
            1.25,
            Frame(0.00, 0, -1.0, 0, 1.000, 1.000),
            Frame(0.20, 0, -7.0, 0, 0.980, 1.060),
            Frame(0.42, 0, -2.0, 0, 1.050, 0.965),
            Frame(0.74, 0, -5.0, 0, 0.995, 1.028),
            Frame(1.25, 0, -1.0, 0, 1.000, 1.000)),

        [PetPose.Evening] = new MotionProfile(
            3.2,
            Frame(0.00, 0, 0, 0, 1.000, 1.000),
            Frame(0.85, -0.7, -1.0, -1.4, 1.004, 0.998),
            Frame(1.65, 0.4, -2.0, 0.6, 0.997, 1.008),
            Frame(2.45, 0.8, -0.8, 1.2, 1.003, 0.999),
            Frame(3.20, 0, 0, 0, 1.000, 1.000)),

        [PetPose.Night] = new MotionProfile(
            2.6,
            Frame(0.00, 0, 0, -1.0, 1.000, 0.985),
            Frame(0.70, -0.6, 1.0, -3.2, 1.010, 0.960),
            Frame(1.35, 0.4, -0.2, 0.8, 0.992, 1.005),
            Frame(2.00, 0.2, 0.7, 2.2, 1.006, 0.970),
            Frame(2.60, 0, 0, -1.0, 1.000, 0.985)),

        [PetPose.DeepNight] = new MotionProfile(
            3.8,
            Frame(0.00, 0, 0, 0, 1.000, 0.940),
            Frame(1.15, 0, -0.8, -0.8, 1.006, 0.955),
            Frame(2.10, 0, -1.4, 0.4, 0.996, 0.965),
            Frame(3.00, 0, -0.7, 0.8, 1.004, 0.950),
            Frame(3.80, 0, 0, 0, 1.000, 0.940)),

        [PetPose.Clicked] = new MotionProfile(
            0.95,
            Frame(0.00, 0, 0, 0, 1.000, 1.000),
            Frame(0.12, 0, -7.5, 0, 0.965, 1.080),
            Frame(0.28, 0, -1.2, 0, 1.070, 0.940),
            Frame(0.50, 0, -4.2, -2.0, 0.990, 1.035),
            Frame(0.74, 0, -1.0, 1.5, 1.018, 0.990),
            Frame(0.95, 0, 0, 0, 1.000, 1.000))
    };

    public static MotionSample Sample(PetPose pose, double elapsedSeconds) =>
        Profiles.TryGetValue(pose, out var profile)
            ? profile.Sample(elapsedSeconds)
            : Profiles[PetPose.Idle].Sample(elapsedSeconds);

    private static MotionProfile RunProfile(double rotation, double x) => new(
        0.62,
        Frame(0.00, x, -1.0, rotation, 1.020, 0.985),
        Frame(0.16, x, -7.5, rotation, 0.980, 1.055),
        Frame(0.31, x, -2.0, rotation, 1.045, 0.960),
        Frame(0.47, x, -6.0, rotation, 0.990, 1.030),
        Frame(0.62, x, -1.0, rotation, 1.020, 0.985));

    private static MotionFrame Frame(double at, double x, double y, double rotation, double scaleX, double scaleY) =>
        new(at, x, y, rotation, scaleX, scaleY);
}
