using System;
using Godot;

public class NodeHelper
{
    public static void RemoveChildren(Node node)
    {
        foreach(Node child in node.GetChildren())
        {
            node.RemoveChild(child);
            child.QueueFree();
        }
    }
}
