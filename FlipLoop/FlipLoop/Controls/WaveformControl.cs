using FlipLoop.Audio;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

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

        dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(30,30,30)), null,
            new Rect(0,0,ActualWidth,ActualHeight));

        if (ActualWidth < 2 || ActualHeight < 2)
            return;

        if (AudioBuffer is null)
        {
            var ft = new FormattedText(
                "Apri o trascina un file audio",
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                18,
                Brushes.Gray,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(ft,new Point((ActualWidth-ft.Width)/2,(ActualHeight-ft.Height)/2));
            return;
        }

        var samples = AudioBuffer.Left;
        double spp = Math.Max(1.0, samples.Length / ActualWidth);
        double cy = ActualHeight / 2;
        var pen = new Pen(Brushes.Lime,1);

        for(int x=0;x<(int)ActualWidth;x++)
        {
            int start=(int)(x*spp);
            int end=Math.Min(samples.Length,(int)((x+1)*spp));

            float min=1,max=-1;

            for(int i=start;i<end;i++)
            {
                float s=samples[i];
                if(s<min) min=s;
                if(s>max) max=s;
            }

            dc.DrawLine(
                pen,
                new Point(x, cy-max*cy),
                new Point(x, cy-min*cy));
        }
    }
}
