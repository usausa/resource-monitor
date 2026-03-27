namespace ResourceMonitor.Views;

public sealed class StatDataSet
{
    public event EventHandler<EventArgs>? Updated;

    private readonly float[] buffer;

    private int head;

    public float LastValue
    {
        get
        {
            var actualIndex = head == 0 ? Capacity - 1 : head - 1;
            return buffer[actualIndex];
        }
    }

    public int Capacity { get; }

    public StatDataSet(int capacity)
    {
        Capacity = capacity;
        buffer = new float[capacity];
    }

    public void Add(float value)
    {
        var index = head;
        head = (head + 1) % Capacity;
        buffer[index] = value;

        Updated?.Invoke(this, EventArgs.Empty);
    }

    public float GetValue(int index)
    {
        var actualIndex = (head + index) % Capacity;
        return buffer[actualIndex];
    }
}
