using System;
using Godot;

[Tool]
public partial class Clock : Node
{
    public static double globalTime;

    public static double delta;

    public static double timeScale = 1d;

    public static double t => globalTime;

    public static void SetTimeScale(double timeScale)
    {
        Clock.timeScale = timeScale;

        GD.Print($"ClOCK: set to timescale {timeScale}");
    }

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
