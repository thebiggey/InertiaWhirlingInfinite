using Godot;
using System;
using System.Linq;

[Tool]
public class nMath
{
    /// <summary>
    /// Circular constant pi. Defined as the ratio between the circumference and diameter of a circle. Is equal to tau / 2. (Double)
    /// </summary>
    public const double pi = 3.14159265358979323846264338327950288419716939937510582097494459230781640d;

    /// <summary>
    /// Circular constant tao. Defined as the ratio between the circumference and radius of a circle. Is equal to 2 * pi. (Double)
    /// </summary>
    public const double tau = 6.2831853071795864769252867665590057683943387987502116419498891846156328d;

    /// <summary>
    /// Euler's Constant. Defined as the sum of 1 / n! over 0 to infinity. (Double)
    /// </summary>
    public const double e = 2.7182818284590452353602874713526624977572470936999595749669676277240766d;

    /// <summary>
    /// Maps a value from one set of boundaries to another.
    /// </summary>
    /// <param name="num"></param>
    /// <param name="fromlower"></param>
    /// <param name="fromupper"></param>
    /// <param name="tolower"></param>
    /// <param name="toupper"></param>
    /// <returns></returns>
    public static double Map(double num, double fromlower, double fromupper, double tolower, double toupper)
    {
        return (num - fromlower) / (fromupper - fromlower) * (toupper - tolower) + tolower;
    }

    /// <summary>
    /// Maps a value from a set of boundaries to a number between 0 and 1.
    /// </summary>
    /// <param name="num"></param>
    /// <param name="fromlower"></param>
    /// <param name="fromupper"></param>
    /// <returns></returns>
    public static double Map01(double num, double fromlower, double fromupper)
    {
        return (num - fromlower) / (fromupper - fromlower);
    }

    /// <summary>
    /// Clamps a value between two other values.
    /// </summary>
    /// <param name="num"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static double Clamp (double num, double min, double max)
    {
        if (num < min)
            return min;
        else if (num > max)
            return max;
        return num;
    }

    /// <summary>
    /// Returns the smoothed maximum between two values given a value k (Default: 1).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public static double SmoothMax(double x, double y, double k = 1f)
    {
        return Mathf.Log(Mathf.Exp(k * x) + Mathf.Exp(k * y)) / k;
    }

    /// <summary>
    /// Returns the smoothed minimum between two values given a value k (Default: 1).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public static double SmoothMin(double x, double y, double k = 1f)
    {
        return - Mathf.Log(Mathf.Exp(-k * x) + Mathf.Exp(-k * y)) / k;
    }

    /// <summary>
    /// Returns the angle derived from the cosine theorem of 3 sides.
    /// </summary>
    /// <param name="adjacent1"></param>
    /// <param name="adjacent2"></param>
    /// <param name="opposite"></param>
    /// <returns></returns>
    public static double CosineTheoremAngle(double adjacent1, double adjacent2, double opposite)
    {
        return Mathf.Acos((sq(adjacent1) + sq(adjacent2) - sq(opposite)) / (2 * adjacent1 * adjacent2));
    }

    public static double ScaleAngle(double angle, double scale)
    {
        return (angle + (scale * tau)) % tau;
    }

    /// <summary>
    /// Checks if 2 points are within a certain distance of each other (2D).
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static bool CheckDistance(Vector2 pos1, Vector2 pos2, double distance)
    {
        return (pos1 - pos2).LengthSquared() <= distance * distance;
    }

    /// <summary>
    /// Checks if 2 points are within a certain distance of each other (3D).
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static bool CheckDistance(Vector3 pos1, Vector3 pos2, float distance)
    {
        return (pos1 - pos2).LengthSquared() <= distance * distance;
    }

    /// <summary>
    /// Returns nth pyramid number (1 + 2 + 3 + ... + n).
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static int Pyramid(int n)
    {
        return ((n * n) + n) / 2;
    }

    /// <summary>
    /// Returns the component of a vector a over another vector b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static double VectorComponent(Vector3 a, Vector3 b)
    {
        return a.Dot(b) / b.Length();
    }

    public static double AngleInPlane(Vector3 a, Vector3 b, Vector3 n)
    {
        double th = Mathf.Atan2(n.Dot(a.Cross(b)), a.Dot(b));
        if (th < 0)
            th += tau;
        return th;
    }

    /// <summary>
    /// Rotates a vector a around another vector b by an angle theta.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="theta"></param>
    /// <returns></returns>
    public static Vector3 RotateVectorAround(Vector3 a, Vector3 b, float theta)
    {
        return a * Mathf.Cos(theta) + b.Cross(a) * Mathf.Sin(theta) + b * b.Dot(a) * (1 - Mathf.Cos(theta));
    }

    public static Vector3 GenerateNorthVector(Vector3 r)
    {
        if(r == Vector3.Zero) return Vector3.Zero;

        r = r.Normalized();

        double theta = r.Z == 0f ? 0 : Math.Atan2(r.Z, r.X);

        double x = -Mathf.Cos(theta) * r.Y;
        double y = Mathf.Sqrt(1 - r.Y * r.Y);
        double z = -Mathf.Sin(theta) * r.Y;

        return new Vector3(x, y, z).Normalized();
    }

    public static Vector3 ProjectOntoPlane(Vector3 v, Vector3 n, Vector3 o)
    {
        return v - n.Dot(v - o) * n;
    }

    /// <summary>
    /// Returns the number's square. Useful for readability.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static float sq(float num)
    {
        return num * num;
    }

    /// <summary>
    /// Returns the number's square. Useful for readability.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static double sq(double num)
    {
        return num * num;
    }

    /// <summary>
    /// Returns the number's square. Useful for readability.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static int sq(int num)
    {
        return num * num;
    }

    public static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * t;
    }

    public static Vector3 ExpDecay(Vector3 a, Vector3 b, double delta, double decay = 1d)
    {
        return b + (a - b) * Math.Exp(-decay * delta);
    }
}
