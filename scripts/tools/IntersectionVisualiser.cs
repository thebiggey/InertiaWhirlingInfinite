using Godot;
using System;

public partial class IntersectionVisualiser : Node
{
    [Export] Node3D template;
    [Export] Godot.Collections.Array<Node3D> nodes;

    public void Clear()
    {
        foreach(Node3D node in nodes)
        {
            node.QueueFree();
        }

        nodes.Clear();
    }

    public void Visualise(PlanetarySystem system, Godot.Collections.Array<Vector3> vals)
    {
        Clear();

        int n = vals.Count;

        for(int i = 0; i < n; i++)
        {
            Node3D node = (Node3D)template.Duplicate();

            node.Position = vals[i];

            system.AddWorldChild(node);
        }
    }
}
