namespace FlipLoop.Audio;

public sealed class AudioBuffer
{
    public string FileName { get; init; } = string.Empty;

    public float[] Left { get; init; } = [];

    public float[] Right { get; init; } = [];

    public int SampleRate { get; init; }

    public int Channels { get; init; }

    public long SampleCount => Left.LongLength;

    public TimeSpan Duration =>
        TimeSpan.FromSeconds((double)SampleCount / SampleRate);
}
