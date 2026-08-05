using FlipLoop.Audio;
using FlipLoop.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FlipLoop.Controls;

public class WaveformControl : FrameworkElement
{
    private static readonly Brush BackgroundBrush;
    private static readonly Pen WavePen;
    private static readonly Brush SelectionBrush;
    private static readonly Pen SelectionPen;

    private bool _dragging;
    private Point _mouseDown;
    private Point _mouseCurrent;

    static WaveformControl()
    {
        var bg = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        bg.Freeze();
        BackgroundBrush = bg;

        var wavePen = new Pen(Brushes.Lime, 1);
        wavePen.Freeze();
        WavePen = wavePen;

        var selectionBrush = new SolidColorBrush(Color.FromArgb(70, 0, 120, 255));
        selectionBrush.Freeze();
        SelectionBrush = selectionBrush;

        var selectionPen = new Pen(Brushes.DodgerBlue, 1);
        selectionPen.Freeze();
        SelectionPen = selectionPen;
    }

    public WaveformControl()
    {
        Focusable = true;

        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
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

    public LoopRegion Loop { get; } = new();

    public bool HasSelection => Loop.IsValid;

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
                SampleToPixel(Loop.StartSample),
                SampleToPixel(Loop.EndSample));
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (AudioBuffer == null)
            return;

        Focus();
        CaptureMouse();

        _dragging = true;
        _mouseDown = e.GetPosition(this);
        _mouseCurrent = _mouseDown;

        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragging)
            return;

        _mouseCurrent = e.GetPosition(this);

        InvalidateVisual();

        e.Handled = true;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging)
            return;

        try
        {
            _mouseCurrent = e.GetPosition(this);

            long s1 = PixelToSample(_mouseDown.X);
            long s2 = PixelToSample(_mouseCurrent.X);

            Loop.StartSample = Math.Min(s1, s2);
            Loop.EndSample = Math.Max(s1, s2);

            InvalidateVisual();
        }
        finally
        {
            _dragging = false;
            ReleaseMouseCapture();
        }

        e.Handled = true;
    }

    private long PixelToSample(double x)
    {
        if (AudioBuffer == null)
            return 0;

        x = Math.Clamp(x, 0, ActualWidth);

        double ratio = x / ActualWidth;

        return (long)(ratio * AudioBuffer.SampleCount);
    }

    private double SampleToPixel(long sample)
    {
        if (AudioBuffer == null || AudioBuffer.SampleCount == 0)
            return 0;

        return sample * ActualWidth / AudioBuffer.SampleCount;
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
        double samplesPerPixel = Math.Max(1.0, (double)samples.Length / ActualWidth);

        int width = (int)ActualWidth;

        for (int x = 0; x < width; x++)
        {
            int start = (int)(x * samplesPerPixel);
            int end = Math.Min(samples.Length, (int)((x + 1) * samplesPerPixel));

            float min = float.MaxValue;
            float max = float.MinValue;

            for (int i = start; i < end; i++)
            {
                float sample = samples[i];

                if (sample < min)
                    min = sample;

                if (sample > max)
                    max = sample;
            }

            double y1 = centerY - max * centerY;
            double y2 = centerY - min * centerY;

            dc.DrawLine(
                WavePen,
                new Point(x, y1),
                new Point(x, y2));
        }
    }

    private void DrawSelection(
        DrawingContext dc,
        double x1,
        double x2)
    {
        double left = Math.Min(x1, x2);
        double right = Math.Max(x1, x2);

        dc.DrawRectangle(
            SelectionBrush,
            SelectionPen,
            new Rect(
                left,
                0,
                right - left,
                ActualHeight));
    }
}