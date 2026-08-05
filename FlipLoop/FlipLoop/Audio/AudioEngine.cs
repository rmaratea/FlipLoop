using System;

namespace FlipLoop.Audio;

public sealed class AudioEngine
{
    public AudioBuffer? CurrentBuffer { get; private set; }

    public bool IsLoaded => CurrentBuffer != null;

    public void Load(string fileName)
    {
        CurrentBuffer = AudioLoader.Load(fileName);
    }

    public void Unload()
    {
        CurrentBuffer = null;
    }
}