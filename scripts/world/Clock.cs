using System;
using Godot;

[Tool]
public partial class Clock : Node
{
    public static double globalTime;
    [Export] double timeScaler {
        get {
            return timeScale;
        }
        set {
            timeScale = value;
        }
    }

    public static double delta;

    public static double timeScale = 1d;

    public static double t => globalTime;

    // TEMPORARY
    public override void _Ready()
    {
        globalTime = 0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        globalTime += delta * timeScale;
        Clock.delta = delta;

        //GD.Print($"CLOCK: {globalTime}");
    }
}
