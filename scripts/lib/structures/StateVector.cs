using Godot;
using System;
using System.Linq;

[Serializable]
public struct StateVector
{
    [Export] public Vector3 position;
    [Export] public Vector3 velocity;

    public static readonly StateVector Zero = new StateVector(Vector3.Zero, Vector3.Zero);

    public StateVector(Vector3 position, Vector3 velocity)
    {
        this.position = position;
        this.velocity = velocity;
    }

    public StateVector(RigidBody3D rb)
    {
        this.position = rb.Position;
        this.velocity = rb.LinearVelocity;
    }

    public static StateVector operator +(StateVector a, StateVector b)
    {
        return new StateVector(a.position + b.position, a.velocity + b.velocity);
    }

    public static StateVector operator -(StateVector a, StateVector b)
    {
        return new StateVector(a.position - b.position, a.velocity - b.velocity);
    }

    public override string ToString()
    {
        return $"Position: {position}, Velocity: {velocity}";
    }
}
