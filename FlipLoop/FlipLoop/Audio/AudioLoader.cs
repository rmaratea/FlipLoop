using System.IO;
using NAudio.Wave;

namespace FlipLoop.Audio;

public static class AudioLoader
{
    public static AudioBuffer Load(string path)
    {
        using var reader = new AudioFileReader(path);

        var left = new List<float>();
        var right = new List<float>();

        var buffer = new float[4096];

        int read;

        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (reader.WaveFormat.Channels == 1)
            {
                for (int i = 0; i < read; i++)
                    left.Add(buffer[i]);
            }
            else
            {
                for (int i = 0; i < read; i += 2)
                {
                    left.Add(buffer[i]);

                    if (i + 1 < read)
                        right.Add(buffer[i + 1]);
                }
            }
        }

        if (reader.WaveFormat.Channels == 1)
            right.AddRange(left);

        return new AudioBuffer
        {
            FileName = Path.GetFileName(path),
            Left = left.ToArray(),
            Right = right.ToArray(),
            SampleRate = reader.WaveFormat.SampleRate,
            Channels = reader.WaveFormat.Channels
        };
    }
}
