namespace FlipLoop.Models;

public sealed class LoopRegion
{
    public long StartSample { get; set; }

    public long EndSample { get; set; }

    public bool IsValid => EndSample > StartSample;
}