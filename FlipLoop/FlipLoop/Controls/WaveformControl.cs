using System.Windows;
using System.Windows.Media;
using FlipLoop.Audio;

namespace FlipLoop.Controls;

public class WaveformControl : FrameworkElement
{
    public AudioBuffer? AudioBuffer
    {
        get => (AudioBuffer?)GetValue(AudioBufferProperty);
        set => SetValue(AudioBufferProperty, value);
    }

    public static readonly DependencyProperty AudioBufferProperty =
        DependencyProperty.Register(
            nameof(AudioBuffer),
            typeof(AudioBuffer),
            typeof(WaveformControl),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(
            Brushes.Black,
            null,
            new Rect(0, 0, ActualWidth, ActualHeight));

        if (AudioBuffer == null)
        {
            DrawCenteredText(dc, "Trascina un file audio");
            return;
        }

        DrawWaveform(dc);
    }

    private void DrawWaveform(DrawingContext dc)
    {
        double centerY = ActualHeight / 2;

        Pen pen = new Pen(Brushes.Lime, 1);

        float[] samples = AudioBuffer!.Left;

        double samplesPerPixel =
            (double)samples.Length / ActualWidth;

        for (int x = 0; x < ActualWidth; x++)
        {
            int start = (int)(x * samplesPerPixel);
            int end = (int)((x + 1) * samplesPerPixel);

            if (end > samples.Length)
                end = samples.Length;

            float min = 1f;
            float max = -1f;

            for (int i = start; i < end; i++)
            {
                float s = samples[i];

                if (s < min)
                    min = s;

                if (s > max)
                    max = s;
            }

            double y1 = centerY - max * centerY;
            double y2 = centerY - min * centerY;

            dc.DrawLine(
                pen,
                new Point(x, y1),
                new Point(x, y2));
        }
    }

    private void DrawCenteredText(DrawingContext dc, string text)
    {
        FormattedText ft =
            new(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                20,
                Brushes.Gray,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

        dc.DrawText(
            ft,
            new Point(
                (ActualWidth - ft.Width) / 2,
                (ActualHeight - ft.Height) / 2));
    }
}