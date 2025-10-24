namespace ResourceMonitor.Views;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

public sealed class StatControl : UserControl
{
    private static readonly Point MousePositonNone = new(-1, -1);

    private readonly SKElement skElement;

    private int mouseHoverIndex = -1;
    private Point lastMousePosition = MousePositonNone;

    private readonly Popup tooltipPopup;
    private readonly TextBlock tooltipText;
    private bool isTooltipVisible;

    public static readonly DependencyProperty GraphColorProperty = DependencyProperty.Register(
        nameof(GraphColor),
        typeof(Color),
        typeof(StatControl),
        new PropertyMetadata(Colors.Blue, OnPropertyChanged));

    public Color GraphColor
    {
        get => (Color)GetValue(GraphColorProperty);
        set => SetValue(GraphColorProperty, value);
    }

    public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
        nameof(HeaderHeight),
        typeof(int),
        typeof(StatControl),
        new PropertyMetadata(32, OnPropertyChanged));

    public int HeaderHeight
    {
        get => (int)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    public static readonly DependencyProperty HeaderPaddingProperty = DependencyProperty.Register(
        nameof(HeaderPadding),
        typeof(float),
        typeof(StatControl),
        new PropertyMetadata(4f, OnPropertyChanged));

    public float HeaderPadding
    {
        get => (float)GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register(
        nameof(LabelFontSize),
        typeof(float),
        typeof(StatControl),
        new PropertyMetadata(16f, OnPropertyChanged));

    public float LabelFontSize
    {
        get => (float)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public static readonly DependencyProperty ValueFontSizeProperty = DependencyProperty.Register(
        nameof(ValueFontSize),
        typeof(float),
        typeof(StatControl),
        new PropertyMetadata(24f, OnPropertyChanged));

    public float ValueFontSize
    {
        get => (float)GetValue(ValueFontSizeProperty);
        set => SetValue(ValueFontSizeProperty, value);
    }

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(StatControl),
        new PropertyMetadata("Usage", OnPropertyChanged));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
        nameof(Unit),
        typeof(string),
        typeof(StatControl),
        new PropertyMetadata("%", OnPropertyChanged));

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
        nameof(MaxValue),
        typeof(float),
        typeof(StatControl),
        new PropertyMetadata(100.0f, OnPropertyChanged));

    public float MaxValue
    {
        get => (float)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly DependencyProperty DataSetProperty = DependencyProperty.Register(
        nameof(DataSet),
        typeof(StatDataSet),
        typeof(StatControl),
        new PropertyMetadata(null, OnDataSetChanged));

    public StatDataSet DataSet
    {
        get => (StatDataSet)GetValue(DataSetProperty);
        set => SetValue(DataSetProperty, value);
    }

    public static readonly DependencyProperty UseTooltipProperty = DependencyProperty.Register(
        nameof(UseTooltip),
        typeof(bool),
        typeof(StatControl),
        new PropertyMetadata(true));

    public bool UseTooltip
    {
        get => (bool)GetValue(UseTooltipProperty);
        set => SetValue(UseTooltipProperty, value);
    }

    public static readonly DependencyProperty TooltipOffsetProperty = DependencyProperty.Register(
        nameof(TooltipOffset),
        typeof(Point),
        typeof(StatControl),
        new PropertyMetadata(new Point(8, -32)));

    public Point TooltipOffset
    {
        get => (Point)GetValue(TooltipOffsetProperty);
        set => SetValue(TooltipOffsetProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public StatControl()
    {
        skElement = new SKElement
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        skElement.PaintSurface += OnPaintSurface;
        skElement.MouseMove += OnMouseMove;
        skElement.MouseLeave += OnMouseLeave;
        Content = skElement;

        tooltipText = new TextBlock
        {
            Padding = new Thickness(4, 2, 4, 2),
            Background = new SolidColorBrush(Color.FromArgb(225, 32, 32, 32)),
            Foreground = Brushes.White,
            FontSize = 12
        };
        tooltipPopup = new Popup
        {
            Child = tooltipText,
            PlacementTarget = this,
            Placement = PlacementMode.Relative,
            IsOpen = false,
            AllowsTransparency = true
        };
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (StatControl)d;
        control.Dispatcher.InvokeAsync(() => control.skElement.InvalidateVisual(), DispatcherPriority.Render);
    }

    private static void OnDataSetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (StatControl)d;

        if (e.OldValue is StatDataSet oldDataSet)
        {
            oldDataSet.Updated -= control.OnDataSetUpdated;
        }

        if (e.NewValue is StatDataSet newDataSet)
        {
            newDataSet.Updated += control.OnDataSetUpdated;
        }
    }

    private void OnDataSetUpdated(object? sender, EventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
        {
            skElement.InvalidateVisual();
            if ((mouseHoverIndex >= 0) && (lastMousePosition.X >= 0))
            {
                UpdateTooltipPosition(lastMousePosition);
            }
        }, DispatcherPriority.Render);
    }

    //--------------------------------------------------------------------------------
    // Paint
    //--------------------------------------------------------------------------------

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var width = e.Info.Width;
        var height = e.Info.Height;
        var statHeight = height - HeaderHeight;
        var headerPadding = HeaderPadding;

        var values = DataSet;
        var pointWidth = (float)width / (values.Capacity - 1);
        var maxValueForScale = MaxValue > 0 ? MaxValue : 100f;
        var skColor = GraphColor.ToSKColor();

        canvas.Clear(SKColors.Transparent);

        // Background
        using var backgroundPaint = new SKPaint();
        backgroundPaint.Style = SKPaintStyle.Fill;
        backgroundPaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(width, 0),
            [skColor.WithAlpha(255), skColor.WithAlpha(192)],
            SKShaderTileMode.Clamp);
        canvas.DrawRect(0, 0, width, height, backgroundPaint);

        // Path
        using var wavePath = new SKPath();
        wavePath.MoveTo(0, height);

        for (var i = 0; i < values.Capacity; i++)
        {
            var x = i * pointWidth;
            var normalizedValue = values.GetValue(i) / maxValueForScale;
            var y = height - (normalizedValue * statHeight);
            wavePath.LineTo(x, y);
        }

        wavePath.LineTo(width, height);
        wavePath.Close();

        // Gradation
        using var wavePaint = new SKPaint();
        wavePaint.Style = SKPaintStyle.Fill;
        wavePaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, height),
            [SKColors.White.WithAlpha(192), SKColors.White.WithAlpha(64)],
            SKShaderTileMode.Clamp);
        canvas.DrawPath(wavePath, wavePaint);

        // Line
        using var linePath = new SKPath();
        linePath.MoveTo(0, height - (values.GetValue(0) / maxValueForScale * statHeight));

        for (var i = 1; i < values.Capacity; i++)
        {
            var x = i * pointWidth;
            var y = height - (values.GetValue(i) / maxValueForScale * statHeight);
            linePath.LineTo(x, y);
        }

        using var linePaint = new SKPaint();
        linePaint.Style = SKPaintStyle.Stroke;
        linePaint.Color = SKColors.White;
        linePaint.StrokeWidth = 1.5f;
        linePaint.IsAntialias = true;
        canvas.DrawPath(linePath, linePaint);

        // Mouse point
        if (mouseHoverIndex >= 0)
        {
            var x = mouseHoverIndex * pointWidth;
            var y = height - (values.GetValue(mouseHoverIndex) / maxValueForScale * statHeight);

            using var pointPaint = new SKPaint();
            pointPaint.Style = SKPaintStyle.Fill;
            pointPaint.Color = SKColors.White;
            pointPaint.IsAntialias = true;
            canvas.DrawCircle(x, y, 3, pointPaint);
        }

        // Label
        using var labelPaint = new SKPaint();
        labelPaint.Color = SKColors.White;
        labelPaint.IsAntialias = true;
        using var labelFont = new SKFont(SKTypeface.Default, LabelFontSize);
        canvas.DrawText(Label, headerPadding, Math.Abs(labelFont.Metrics.Ascent) - labelFont.Metrics.Descent + headerPadding, labelFont, labelPaint);

        // Value
        var currentValue = values.LastValue;
        var unit = Unit;
        var valueText = String.IsNullOrEmpty(unit) ? $"{currentValue:F1}" : $"{currentValue:F1} {unit}";

        using var valuePaint = new SKPaint();
        valuePaint.Color = SKColors.White;
        valuePaint.IsAntialias = true;
        using var valueFont = new SKFont(SKTypeface.Default, ValueFontSize);
        canvas.DrawText(valueText, width - headerPadding, Math.Abs(valueFont.Metrics.Ascent) - valueFont.Metrics.Descent + headerPadding, SKTextAlign.Right, valueFont, valuePaint);
    }

    //--------------------------------------------------------------------------------
    // Mouse
    //--------------------------------------------------------------------------------

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!UseTooltip)
        {
            return;
        }

        lastMousePosition = e.GetPosition(skElement);
        UpdateTooltipPosition(lastMousePosition);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        HideTooltip();
        mouseHoverIndex = -1;
        lastMousePosition = MousePositonNone;
        skElement.InvalidateVisual();
    }

    //--------------------------------------------------------------------------------
    // Tooltip
    //--------------------------------------------------------------------------------

    private void UpdateTooltipPosition(Point position)
    {
        var values = DataSet;
        var width = (float)skElement.ActualWidth;
        var index = (int)(position.X / width * values.Capacity);
        if (index >= 0 && index < values.Capacity)
        {
            mouseHoverIndex = index;
            ShowTooltip(position, values.GetValue(index));
        }
        else
        {
            HideTooltip();
        }

        skElement.InvalidateVisual();
    }

    private void ShowTooltip(Point position, float value)
    {
        var unit = Unit;
        tooltipText.Text = String.IsNullOrEmpty(unit) ? $"{value:F1}" : $"{value:F1}{unit}";

        var offset = TooltipOffset;
        tooltipPopup.HorizontalOffset = position.X + offset.X;
        tooltipPopup.VerticalOffset = position.Y + offset.Y;

        if (!isTooltipVisible)
        {
            isTooltipVisible = true;
            tooltipPopup.IsOpen = true;
        }
    }

    private void HideTooltip()
    {
        if (isTooltipVisible)
        {
            isTooltipVisible = false;
            tooltipPopup.IsOpen = false;
        }
    }
}
