
using FlipLoop.Audio;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlipLoop.Controls;

public class WaveformControl : FrameworkElement
{
    private static readonly Brush BackgroundBrush;
    private static readonly Pen WavePen;

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(
            double.IsInfinity(availableSize.Width) ? 100 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 100 : availableSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return finalSize;
    }


    static WaveformControl()
    {
        var bg = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        bg.Freeze();
        BackgroundBrush = bg;

        var pen = new Pen(Brushes.Lime, 1);
        pen.Freeze();
        WavePen = pen;
    }

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

        if (ActualWidth <= 0 || ActualHeight <= 0)
            return;

        DrawBackground(dc);

        if (AudioBuffer == null || AudioBuffer.Left.Length == 0)
        {
            DrawPlaceholder(dc);
            return;
        }

        DrawWaveform(dc);
    }

    private void DrawBackground(DrawingContext dc)
    {
        dc.DrawRectangle(
            BackgroundBrush,
            null,
            new Rect(0, 0, ActualWidth, ActualHeight));
    }

    private void DrawPlaceholder(DrawingContext dc)
    {
        var ft = new FormattedText(
            "Apri o trascina un file audio",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            18,
            Brushes.Gray,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        dc.DrawText(
            ft,
            new Point(
                (ActualWidth - ft.Width) / 2,
                (ActualHeight - ft.Height) / 2));
    }

    private void DrawWaveform(DrawingContext dc)
    {
        float[] samples = AudioBuffer!.Left;

        double centerY = ActualHeight / 2.0;

        double samplesPerPixel =
            Math.Max(1.0, (double)samples.Length / ActualWidth);

        int width = (int)ActualWidth;

        for (int x = 0; x < width; x++)
        {
            int start = (int)(x * samplesPerPixel);
            int end = Math.Min(
                samples.Length,
                (int)((x + 1) * samplesPerPixel));

            float min = float.MaxValue;
            float max = float.MinValue;

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
                WavePen,
                new Point(x, y1),
                new Point(x, y2));
        }
    }
}