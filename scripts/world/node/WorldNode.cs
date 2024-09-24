using System;
using Godot;

[Tool]
public partial class WorldNode : Node3D
{
    [ExportGroup("Parameters")]
    [Export] double loadingRadius = 0;

    Node3D body;

    private bool loaded;
    private bool wasLoaded;

    public bool isLoaded => loaded;

    internal virtual bool LoadCheck(Node3D player)
    {
        if(loadingRadius > 0)
            return GlobalPosition.DistanceSquaredTo(player.GlobalPosition) > loadingRadius * loadingRadius;
        return true;
    }

    internal virtual void Load()
    {
        body = GetChild<Node3D>(0);

        body.Visible = true;
    }

    internal virtual void FreeNode()
    {
        body.Visible = false;
    }

    internal virtual void OnWorldLoad() { }

    internal virtual void Update(double delta) { }

    public override void _Ready()
    {
        OnWorldLoad();
    }

    public override void _PhysicsProcess(double delta)
    {
        if(LoadCheck(WorldManager.player))
        {
            loaded = true;

            Load();
        }
        else if(wasLoaded)
        {
            FreeNode();
        }

        if(loaded)
            Update(delta);

        wasLoaded = loaded;
    }
}
