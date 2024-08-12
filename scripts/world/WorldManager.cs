using Godot;
using System;

[Tool]
public partial class WorldManager : Node
{
	[Export] Node3D _player;

	public static Node3D player;
	public override void _Ready()
	{
		player = _player;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
