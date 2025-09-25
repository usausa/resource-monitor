using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Sandbox;

// TODO 同じ値の時に変化がない！

public class ResourceGraphControl : UserControl
{
    private readonly List<float> _dataPoints = new List<float>();
    private readonly int _maxDataPoints = 100;
    private float _currentValue;
    private readonly SKElement _skElement;
    private readonly Popup _tooltipPopup;
    private readonly TextBlock _tooltipText;
    private bool _isTooltipVisible;
    private int _mouseHoverIndex = -1;
    private Point _lastMousePosition;

    #region 依存関係プロパティ

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(ResourceGraphControl),
        new PropertyMetadata("Resource", OnVisualPropertyChanged));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(float), typeof(ResourceGraphControl),
        new PropertyMetadata(0.0f, OnValueChanged));

    public static readonly DependencyProperty GraphColorProperty = DependencyProperty.Register(
        nameof(GraphColor), typeof(Color), typeof(ResourceGraphControl),
        new PropertyMetadata(Colors.Blue, OnVisualPropertyChanged));

    public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
        nameof(MaxValue), typeof(float), typeof(ResourceGraphControl),
        new PropertyMetadata(100.0f, OnVisualPropertyChanged));

    public static readonly DependencyProperty ShowTooltipsProperty = DependencyProperty.Register(
        nameof(ShowTooltips), typeof(bool), typeof(ResourceGraphControl),
        new PropertyMetadata(true));

    #endregion

    #region プロパティ

    [Description("グラフのラベル")]
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    [Description("グラフに表示する値")]
    public float Value
    {
        get => (float)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    [Description("グラフの色")]
    public Color GraphColor
    {
        get => (Color)GetValue(GraphColorProperty);
        set => SetValue(GraphColorProperty, value);
    }

    [Description("グラフの最大値")]
    public float MaxValue
    {
        get => (float)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    [Description("ツールチップを表示するかどうか")]
    public bool ShowTooltips
    {
        get => (bool)GetValue(ShowTooltipsProperty);
        set => SetValue(ShowTooltipsProperty, value);
    }

    #endregion

    public ResourceGraphControl()
    {
        // 初期データを追加
        for (int i = 0; i < _maxDataPoints; i++)
        {
            _dataPoints.Add(0);
        }

        // SKElementの作成
        _skElement = new SKElement
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        _skElement.PaintSurface += OnPaintSurface;
        _skElement.MouseMove += OnMouseMove;
        _skElement.MouseLeave += OnMouseLeave;

        // ツールチップの準備
        _tooltipText = new TextBlock
        {
            Padding = new Thickness(5),
            Background = new SolidColorBrush(Color.FromArgb(225, 32, 32, 32)),
            Foreground = Brushes.White,
            FontSize = 12
        };

        _tooltipPopup = new Popup
        {
            Child = _tooltipText,
            PlacementTarget = this,
            Placement = PlacementMode.Relative,
            IsOpen = false,
            AllowsTransparency = true
        };

        // コンテンツの設定
        Content = _skElement;

        // デザイン時のデータ
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            for (int i = 0; i < _dataPoints.Count; i++)
            {
                _dataPoints[i] = (float)(25 + 10 * Math.Sin(i / 10.0));
            }
            _currentValue = 30.5f;
        }
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var width = e.Info.Width;
        var height = e.Info.Height;

        // キャンバスをクリア
        canvas.Clear(SKColors.Transparent);

        // SKColorに変換
        var skColor = new SKColor(
            GraphColor.R,
            GraphColor.G,
            GraphColor.B,
            GraphColor.A);

        // 強化した背景グラデーションを描画
        using (var backgroundPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, 0),
                new[] {
                    skColor.WithAlpha(255),
                    skColor.WithAlpha(192)
                },
                new float[] { 0.2f, 1.0f },  // グラデーションの配置位置
                SKShaderTileMode.Clamp)
        })
        {
            canvas.DrawRect(0, 0, width, height, backgroundPaint);
        }

        // データがない場合は描画しない
        if (_dataPoints.Count == 0)
            return;

        float pointWidth = (float)width / (_maxDataPoints - 1);
        float maxValueForScale = MaxValue > 0 ? MaxValue : 100f;

        // グラフのパスを作成
        using (var wavePath = new SKPath())
        {
            // 開始点を設定
            wavePath.MoveTo(0, height);

            // 各データポイントに対してパスを描画
            for (int i = 0; i < _dataPoints.Count; i++)
            {
                float x = i * pointWidth;
                float normalizedValue = _dataPoints[i] / maxValueForScale;
                float y = height - (normalizedValue * height * 0.7f);
                wavePath.LineTo(x, y);
            }

            // パスを下部に閉じる
            wavePath.LineTo(width, height);
            wavePath.Close();

            // より白みのあるグラデーションで塗りつぶし
            using (var wavePaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(0, height),
                    new[] {
                        SKColors.White.WithAlpha(192),  // より強い白色
                        SKColors.White.WithAlpha(64)           // 下部は半透明の元の色
                    },
                    new float[] { 0.0f, 1.0f },
                    SKShaderTileMode.Clamp)
            })
            {
                canvas.DrawPath(wavePath, wavePaint);
            }

            // グラフの線を描画
            using (var linePath = new SKPath())
            {
                linePath.MoveTo(0, height - (_dataPoints[0] / maxValueForScale * height * 0.7f));

                for (int i = 1; i < _dataPoints.Count; i++)
                {
                    float x = i * pointWidth;
                    float y = height - (_dataPoints[i] / maxValueForScale * height * 0.7f);
                    linePath.LineTo(x, y);
                }

                using (var linePaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.White,
                    StrokeWidth = 1.5f,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(linePath, linePaint);
                }
            }

            // マウスホバー位置にポイントを描画
            if (_mouseHoverIndex >= 0 && _mouseHoverIndex < _dataPoints.Count)
            {
                float x = _mouseHoverIndex * pointWidth;
                float y = height - (_dataPoints[_mouseHoverIndex] / maxValueForScale * height * 0.7f);

                using (var pointPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.White,
                    IsAntialias = true
                })
                {
                    //canvas.DrawCircle(x, y, 5, pointPaint);
                    canvas.DrawCircle(x, y, 3, pointPaint);
                }
            }
        }

        // ラベルを表示
        using (var labelPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 16,
            IsAntialias = true
        })
        {
            canvas.DrawText(Label, 10, 20, labelPaint);
        }

        // 現在の値を表示
        using (var valuePaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 24,
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        })
        {
            string valueText = _currentValue.ToString("F1");
            canvas.DrawText(valueText, width - 10, 30, valuePaint);
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!ShowTooltips)
            return;

        _lastMousePosition = e.GetPosition(_skElement);
        UpdateTooltipForPosition(_lastMousePosition);
    }

    private void UpdateTooltipForPosition(Point position)
    {
        float width = (float)_skElement.ActualWidth;

        // マウス位置からインデックスを計算
        int index = (int)(position.X / width * _dataPoints.Count);
        if (index >= 0 && index < _dataPoints.Count)
        {
            _mouseHoverIndex = index;
            ShowTooltip(position, _dataPoints[index]);
        }
        else
        {
            HideTooltip();
        }

        // グラフを再描画（ホバー効果を表示するため）
        _skElement.InvalidateVisual();
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        HideTooltip();
        _mouseHoverIndex = -1;
        _lastMousePosition = new Point(-1, -1);
        _skElement.InvalidateVisual();
    }

    private void ShowTooltip(Point position, float value)
    {
        _tooltipText.Text = $"{value:F1}%";

        // ツールチップの位置を調整
        _tooltipPopup.HorizontalOffset = position.X + 10;
        _tooltipPopup.VerticalOffset = position.Y - 30;

        if (!_isTooltipVisible)
        {
            _isTooltipVisible = true;
            _tooltipPopup.IsOpen = true;
        }
    }

    private void HideTooltip()
    {
        if (_isTooltipVisible)
        {
            _isTooltipVisible = false;
            _tooltipPopup.IsOpen = false;
        }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ResourceGraphControl)d;
        var newValue = (float)e.NewValue;

        control._currentValue = newValue;
        control.AddDataPoint(newValue);

        // SKElementの再描画を強制（UI thread dispatch）
        control.Dispatcher.InvokeAsync(() => {
            if (control._skElement != null)
            {
                // グラフを再描画
                control._skElement.InvalidateVisual();

                // マウスがグラフ上にある場合は、ツールチップも更新
                if (control._mouseHoverIndex >= 0 && control._lastMousePosition.X >= 0)
                {
                    control.UpdateTooltipForPosition(control._lastMousePosition);
                }
            }
        }, DispatcherPriority.Render);
    }

    private void AddDataPoint(float value)
    {
        _dataPoints.Add(value);
        if (_dataPoints.Count > _maxDataPoints)
        {
            _dataPoints.RemoveAt(0);
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ResourceGraphControl)d;

        // SKElementの再描画を強制（UI thread dispatch）
        control.Dispatcher.InvokeAsync(() => {
            if (control._skElement != null)
            {
                control._skElement.InvalidateVisual();
            }
        }, DispatcherPriority.Render);
    }
}
