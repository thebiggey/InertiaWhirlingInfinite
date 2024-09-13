using System;
using Godot;

public partial class OrbitCamera : Node3D
{
    [ExportCategory("Settings")]
    [Export] double sensitivityX = 0.01f;
    [Export] double sensitivityY = 0.01f;
    [Export] double sensitivityZoom = 5;


    [ExportCategory("Attributes")]
    [Export] double angleX = 0;
    [Export] double angleY = 0;
    [Export] double zoom = 0;


    Node3D rotator;
    Node3D camera;

    public override void _Ready()
    {
        rotator = GetChild<Node3D>(0);
        camera = rotator.GetChild<Node3D>(0);
    }

    public override void _Process(double delta)
    {

        Vector2 mouseVelocity = Input.GetLastMouseVelocity();

        angleX += -mouseVelocity.X * sensitivityX * delta;
        angleY = Mathf.Clamp(angleY + mouseVelocity.Y * sensitivityY * delta, -nMath.pi * 0.5d, nMath.pi * 0.5d);

        double scroll = 0;
        if(Input.IsMouseButtonPressed(MouseButton.Left)) scroll = 1;
        else if(Input.IsMouseButtonPressed(MouseButton.Right)) scroll = -1;

        zoom += scroll * sensitivityZoom * delta;


        Basis basis = Basis.Identity;

        basis = basis.Rotated(Vector3.Up, angleX);
        basis = basis.Rotated(basis.Z, angleY);

        Transform3D transform = rotator.Transform;
        transform.Basis = basis;
        rotator.Transform = transform;


        camera.Position = Vector3.Right * Math.Exp(-zoom);
    }
}
