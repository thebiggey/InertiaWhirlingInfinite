using Godot;
using System;

public partial class TimeController : Node
{
    [Export] Godot.Collections.Array<int> scales;
    int current = 0;

    bool raise, lower, reset;
    bool w_raise, w_lower, w_reset;

    public override void _Ready()
    {
        current = 0;
        raise = false; lower = false; reset = false;
        w_raise = false; w_lower = false; w_reset = false;
    }

    public override void _Process(double delta)
    {
        bool p_reset = Input.IsActionPressed("reset_time_scale");
        bool p_lower = Input.IsActionPressed("lower_time_scale");
        bool p_raise = Input.IsActionPressed("raise_time_scale");

        if(p_reset)
        {
            if(!w_reset)
            {
                reset = true;
            }

            w_reset = true;
        }
        else
        {
            w_reset = false;
        }

        if(p_lower)
        {
            if(!w_lower)
            {
                lower = true;
            }

            w_lower = true;
        }
        else
        {
            w_lower = false;
        }

        if(p_raise)
        {
            if(!w_raise)
            {
                raise = true;
            }

            w_raise = true;
        }
        else
        {
            w_raise = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if(reset)
        {
            current = 0;
            Clock.SetTimeScale(scales[current]);
        }
        else if(lower && current > 0)
        {
            current--;
            Clock.SetTimeScale(scales[current]);
        }
        else if(raise && current < scales.Count)
        {
            current++;
            Clock.SetTimeScale(scales[current]);
        }


        reset = false; lower = false; raise = false;
    }
}
