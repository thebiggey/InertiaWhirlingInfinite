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

    public static void SetParent(Node node, Node parent)
    {
        Node oldParent = node.GetParent();

        if(oldParent != null)
            oldParent.RemoveChild(node);

        parent.AddChild(node);
    }
}
