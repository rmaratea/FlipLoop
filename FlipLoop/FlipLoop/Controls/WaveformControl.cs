
using FlipLoop.Audio;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FlipLoop.Controls;

public class WaveformControl : FrameworkElement
{
    private static readonly Brush BackgroundBrush;
    
    private static readonly Pen WavePen;
    
    private bool _dragging;

    private Point _mouseDown;

    private Point _mouseCurrent;



    public long LoopStartSample { get; private set; }

    public long LoopEndSample { get; private set; }

    public bool HasSelection =>
        LoopEndSample > LoopStartSample;

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

    public WaveformControl()
    {
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    private void OnMouseLeftButtonDown(object sender,
                                   MouseButtonEventArgs e)
    {
        if (AudioBuffer == null)
            return;

        CaptureMouse();

        _dragging = true;

        _mouseDown = e.GetPosition(this);

        _mouseCurrent = _mouseDown;
    }

    private void OnMouseMove(object sender,
                         MouseEventArgs e)
    {
        if (!_dragging)
            return;

        _mouseCurrent = e.GetPosition(this);

        InvalidateVisual();
    }

    private void OnMouseLeftButtonUp(object sender,
                                 MouseButtonEventArgs e)
    {
        if (!_dragging)
            return;

        ReleaseMouseCapture();

        _dragging = false;

        _mouseCurrent = e.GetPosition(this);

        long s1 = PixelToSample(_mouseDown.X);

        long s2 = PixelToSample(_mouseCurrent.X);

        LoopStartSample = Math.Min(s1, s2);

        LoopEndSample = Math.Max(s1, s2);

        InvalidateVisual();
    }

    private long PixelToSample(double x)
    {
        if (AudioBuffer == null)
            return 0;

        x = Math.Clamp(x, 0, ActualWidth);

        double ratio = x / ActualWidth;

        return (long)(ratio * AudioBuffer.SampleCount);
    }

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

        if (_dragging)
        {
            DrawSelection(
                dc,
                _mouseDown.X,
                _mouseCurrent.X);
        }
        else if (HasSelection)
        {
            DrawSelection(
                dc,
                SampleToPixel(LoopStartSample),
                SampleToPixel(LoopEndSample));
        }
    }

    private double SampleToPixel(long sample)
    {
        if (AudioBuffer == null)
            return 0;

        return sample *
               ActualWidth /
               AudioBuffer.SampleCount;
    }

    private void DrawSelection(
    DrawingContext dc,
    double x1,
    double x2)
    {
        double left = Math.Min(x1, x2);

        double right = Math.Max(x1, x2);

        dc.DrawRectangle(

            new SolidColorBrush(
                Color.FromArgb(70, 0, 120, 255)),

            new Pen(
                Brushes.DodgerBlue,
                1),

            new Rect(
                left,
                0,
                right - left,
                ActualHeight));
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