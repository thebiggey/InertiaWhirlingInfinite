using System;
using Godot;

[Tool]
public partial class Trajectory : Resource
{
    [Export] Godot.Collections.Array<TrajectorySection> trajectorySections;

    public int Length => trajectorySections.Count;

    public Trajectory()
    {
        trajectorySections = new Godot.Collections.Array<TrajectorySection>();
    }

    public Trajectory(TrajectorySection trajectorySection)
    {
        trajectorySections = new Godot.Collections.Array<TrajectorySection>
        {
            trajectorySection
        };
    }

    public Trajectory(Godot.Collections.Array<TrajectorySection> trajectorySections)
    {
        this.trajectorySections = trajectorySections;
    }

    public void Add(TrajectorySection trajectorySection)
    {
        trajectorySections.Add(trajectorySection);
    }

    public TrajectorySection Get(int i)
    {
        if(i >= trajectorySections.Count) return null;
        return trajectorySections[i];
    }

    public TrajectorySection GetLast()
    {
        if(trajectorySections.Count < 1) return null;
        return trajectorySections[trajectorySections.Count - 1];
    }

    public void Skip()
    {
        if(Length == 0)
        {
            GD.Print("WARNING: Trajectory cannot be skipped because it is empty!");
            return;
        }

        trajectorySections.RemoveAt(0);
    }

    public double GetTotalDuration()
    {
        double totalDuration = 0;
        foreach(TrajectorySection section in trajectorySections)
        {
            if(section.isFull)
            {
                break;
            }

            totalDuration += section.duration;
        }

        return totalDuration;
    }

    public StateVector Evaluate(double t)
    {
        int k = 0;
        while(k < Length && t > trajectorySections[k].duration)
        {
            t -= trajectorySections[k].duration;
            k++;
        }

        return trajectorySections[k].Evaluate(t);
    }
}
