using UniRx;

public class SimPrefs
{
    public static IntReactiveProperty FieldSize { get; private set; } = new(64);
    public static IntReactiveProperty UnitsCount { get; private set; } = new(512);
    public static FloatReactiveProperty SimulationSpeed { get; private set; } = new(1f);
}