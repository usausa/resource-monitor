namespace ResourceMonitor.Behaviors;

using Smart.Windows.Interactivity;

using Microsoft.Xaml.Behaviors;

[TypeConstraint(typeof(Window))]
public sealed class WindowPlacementBehavior : Behavior<Window>
{
    public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
        nameof(Placement),
        typeof(WindowPlacement),
        typeof(WindowPlacementBehavior),
        new PropertyMetadata(WindowPlacement.TopLeft));

    public WindowPlacement Placement
    {
        get => (WindowPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public static readonly DependencyProperty DisplayIndexProperty = DependencyProperty.Register(
        nameof(DisplayIndex),
        typeof(int),
        typeof(WindowPlacementBehavior),
        new PropertyMetadata(0));

    public int DisplayIndex
    {
        get => (int)GetValue(DisplayIndexProperty);
        set => SetValue(DisplayIndexProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        var screens = Screen.AllScreens;
        if ((DisplayIndex < 0) || (DisplayIndex >= screens.Length))
        {
            DisplayIndex = Array.FindIndex(screens, static x => x.Primary);
            if (DisplayIndex < 0)
            {
                DisplayIndex = 0;
            }
        }

        var targetScreen = screens[DisplayIndex];
        var workingArea = targetScreen.WorkingArea;

        WindowPlacementHelper.UpdatePlacement(AssociatedObject, new Rect(workingArea.X, workingArea.Y, workingArea.Width, workingArea.Height), Placement);
    }
}
