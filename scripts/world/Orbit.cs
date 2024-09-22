using Godot;
using System;

[Tool]
public partial class Orbit : Resource
{
	public PlanetarySystem planetarySystem;

	[Export] public double semiMajorAxis;
	[Export] public double eccentricity;
	[Export] public double inclination;
	[Export] public double ascendingNodeLongitude;
	[Export] public double argumentOfPeriapsis;

	// I have no fucking idea why but if n isn't marked as export the objects start orbiting much faster
	// the amount of pain this has caused me is unimaginable
	[Export] double n;

	enum Type { Elliptical, Hyperbolic }
	[Export] Type type = Type.Elliptical;

	// The matrix for converting from local to world space is cached, as it is needed whenever evaluating the orbit
	// and the resulting performance improvement outweighs the additional memory cost.
	// Since it's dimensions are constant it can be stored as a one-dimensional array
	private readonly double[] LocalToWorldMatrix;


	public double a => semiMajorAxis;
	public double e => eccentricity;
	public double i => inclination;
	public double o => ascendingNodeLongitude;
	public double w => argumentOfPeriapsis;

	public double b => a * axisRatio;

	public double axisRatio {
		get {
			if(isElliptical)
				return Math.Sqrt(1 - e * e);
			return Math.Sqrt(e * e - 1);
		}
	}

	double mu => planetarySystem.mu;

	public double T => nMath.tau / n;

	public bool isElliptical => type == Type.Elliptical;

	public double Periapsis {
		get {
			return a * (1 - e);
		}
	}

	public double Apoapsis {
		get {
			return a * (1 + e);
		}
	}

	public Vector3 Normal {
		get {
			return LocalToWorldSpace(Vector2.Right).Cross(LocalToWorldSpace(Vector2.Up)).Normalized();
		}
	}

	public static readonly Vector3 referenceVector = new Vector3(0, 0, 1);


	private double[] ComputeLocalToWorldMatrix(double i, double om, double w)
	{
		double[] matrix = new double[6];

		double cosI = Math.Cos(i);
		double sinI = Math.Sin(i);
		double cosOmega = Math.Cos(om);
		double sinOmega = Math.Sin(om);
		double cosW = Math.Cos(w);
		double sinW = Math.Sin(w);

		matrix[0] = -(cosI * sinOmega * sinW - cosOmega * cosW);
		matrix[1] = (cosI * sinOmega * cosW + cosOmega * sinW);
		matrix[2] = sinI * sinW;
		matrix[3] = -sinI * cosW;
		matrix[4] = -(cosI * cosOmega * sinW + sinOmega * cosW);
		matrix[5] = (cosI * cosOmega * cosW - sinOmega * sinW);

		return matrix;
	}

	public Orbit()
	{
		this.planetarySystem = null;
		this.semiMajorAxis = 1;
		this.eccentricity = 0;
		this.inclination = 0;
		this.ascendingNodeLongitude = 0;
		this.argumentOfPeriapsis = 0;
		this.type = Type.Elliptical;

		this.n = 1;

		this.LocalToWorldMatrix = ComputeLocalToWorldMatrix(0, 0, 0);
	}

	public Orbit(PlanetarySystem planetarySystem, double semiMajorAxis, double eccentricity, double inclination, double ascendingNodeLongitude, double argumentOfPeriapsis)
	{
		this.planetarySystem = planetarySystem;
		this.semiMajorAxis = semiMajorAxis;
		this.eccentricity = eccentricity;
		this.inclination = inclination;
		this.ascendingNodeLongitude = ascendingNodeLongitude;
		this.argumentOfPeriapsis = argumentOfPeriapsis;

		this.n = Math.Sqrt(mu / Math.Abs(this.semiMajorAxis * this.semiMajorAxis * this.semiMajorAxis));

		this.LocalToWorldMatrix = ComputeLocalToWorldMatrix(inclination, ascendingNodeLongitude, argumentOfPeriapsis);
	}

	public Orbit(PlanetarySystem planetarySystem, StateVector cartesian) : this(planetarySystem, cartesian, out _) {  }

	// this shit is an absolute mess and needs to be rewritten at some point
	public Orbit(PlanetarySystem planetarySystem, StateVector cartesian, out double trueAnomaly)
	{
		this.planetarySystem = planetarySystem;

		this.type = Type.Elliptical;

		if(cartesian.position.Cross(cartesian.velocity).Length() == 0)
		{
			const double velocityEpsilon = 0.001d;
			cartesian.velocity += cartesian.position.Cross(nMath.GenerateNorthVector(cartesian.position)).Normalized() * velocityEpsilon;
		}

		// adjust position and velocity vectors to reference frame. This is a bad way to impliment this, but I can't be fucked to figure out the proper math
		Vector3 position = new Vector3(cartesian.position.X, -cartesian.position.Z, cartesian.position.Y);
		Vector3 velocity = new Vector3(cartesian.velocity.X, -cartesian.velocity.Z, cartesian.velocity.Y);

		Vector3 h = position.Cross(velocity);
		Vector3 n = referenceVector.Cross(h);

		double nMag = n.Length();
		double hMag = h.Length();

		double a = -0.5d * (mu / ((velocity.LengthSquared() * 0.5d) - (mu / position.Length())));

		Vector3 eccentricityVector = (velocity.Cross(h) / mu) - position.Normalized();
		double e = eccentricityVector.Length();

		// Instead of accounting for parabolic or circular orbits, we just adjust them slightly, so that we don't have to account with them for complicated code
		// This is a hack fix, but it works and the actual impact this has on the orbit should be negligible, since the error is incredibly tiny at 1d
		// If it causes problems down the line I'll fix it then
		const double eccentricityEpsilon = 0.0000001d;
		if(e == 1)
		{
			e -= eccentricityEpsilon;
		}
		else if(e == 0)
		{
			e += eccentricityEpsilon;
		}
		else if (e > 1)
		{
			type = Type.Hyperbolic;
		}


		double i = Math.Acos(h.Z / hMag);

		double omega = Math.Acos(n.X / nMag);
		if (n.Y < 0)
			omega = nMath.tau - omega;

		double arg = Math.Acos(eccentricityVector.Dot(n) / (e * nMag));
		if (eccentricityVector.Z < 0)
		{
			arg = nMath.tau - arg;
		}

		double v = Math.Acos(eccentricityVector.Dot(position) / (e * position.Length()));
		if(position.Dot(velocity) < 0)
		{
			v = nMath.tau - v;
		}

		// This code catches the edge cases which cause divisions by 0 in the calculations.
		// in this case the orbit is equatorial
		if(nMag == 0)
		{
			omega = 0;

			// not sure why this is needed, but it is
			if(eccentricityVector.Y < 0)
			{
				i = 0;
			}
			else
			{
				i = nMath.pi;
			}

			arg = nMath.tau - Math.Acos(eccentricityVector.X / e);
		}

		// Another critical edge case would be the orbit being perfectly circular, but since we account for that with the eccentricity, this isn't needed.
		// If this is ever changed, we need to account for this here.

		this.semiMajorAxis = a;
		this.eccentricity = e;
		this.inclination = i;
		this.ascendingNodeLongitude = omega;
		this.argumentOfPeriapsis = arg;

		trueAnomaly = v;

		this.n = Math.Sqrt(mu / Math.Abs(this.semiMajorAxis * this.semiMajorAxis * this.semiMajorAxis));

		this.LocalToWorldMatrix = ComputeLocalToWorldMatrix(i, omega, arg);
	}

	public void SetPlanetarySystem(PlanetarySystem planetarySystem)
	{
		this.planetarySystem = planetarySystem;
	}

	private double EccentricAnomalyToTrueAnomaly(double E)
	{
		return 2 * Math.Atan(Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(E * 0.5d));
	}

	private double TrueAnomalyToEccentricAnomaly(double v)
	{
		return 2 * Math.Atan(Math.Sqrt((1 - e) / (1 + e)) * Math.Tan(v * 0.5d));
	}

	private double HyperbolicAnomalyToTrueAnomaly(double H)
	{
		return 2 * Math.Atan(Math.Sqrt((e + 1) / (e - 1)) * Math.Tanh(H * 0.5d));
	}

	private double TrueAnomalyToHyperbolicAnomaly(double v)
	{
		return 2 * Math.Atanh(Math.Sqrt((e - 1) / (e + 1)) * Math.Tan(v * 0.5d));
	}

	public double TrueAnomalyToTime(double v)
	{
		double M;

		if(isElliptical)
		{
			double E = TrueAnomalyToEccentricAnomaly(v);
			M = E - e * Math.Sin(E);
		}
		else
		{
			double H = TrueAnomalyToHyperbolicAnomaly(v);
			M = e * Math.Sinh(H) - H;
		}

		return M / n;
	}


	public double TimeToTrueAnomaly(double t, int bound = anomalyBound)
	{
		double M = (n * t);

		if(isElliptical)
		{
			double E = ComputeEccentricAnomaly(M, bound);
			return EccentricAnomalyToTrueAnomaly(E);
		}
		else
		{
			double H = ComputeHyperbolicAnomaly(M, bound);
			return HyperbolicAnomalyToTrueAnomaly(H);
		}
	}

	private const int anomalyBound = 7;

	private double ComputeEccentricAnomaly(double M, int bound)
	{
		double E = M;

		for (int i = 0; i < bound; i++)
		{
			E -= (E - e * Math.Sin(E) - M) / (1 - e * Math.Cos(E));
		}

		return E;
	}

	private double ComputeHyperbolicAnomaly(double M, int bound)
	{
		double H = M;

		for(int i = 0; i < bound; i++)
		{
			H -= (e * Math.Sinh(H) - H - M) / (e * Math.Cosh(H) - 1);
		}

		return H;
	}

	public Vector3 LocalToWorldSpace(Vector2 v)
	{
		double x = v.X * LocalToWorldMatrix[0] + v.Y * LocalToWorldMatrix[1];
		double y = v.X * LocalToWorldMatrix[2] + v.Y * LocalToWorldMatrix[3];
		double z = v.X * LocalToWorldMatrix[4] + v.Y * LocalToWorldMatrix[5];

		return new Vector3(x, y, z);
	}

	private StateVector EllipticalStateVector(double E, double v)
	{
		double r = a * (1 - e * Math.Cos(E));
		Vector2 localPosition = new Vector2(Math.Cos(v), Math.Sin(v));

		double speed = Math.Sqrt(mu * ((2 / r) - (1 / a)));
		Vector2 localVelocity = new Vector2(-Math.Sin(E), axisRatio * Math.Cos(E));

		return new StateVector(r * LocalToWorldSpace(localPosition.Normalized()), -speed * LocalToWorldSpace(localVelocity.Normalized()));
	}

	private StateVector HyperbolicStateVector(double H, double v)
	{
		double r = a * (1 - e * Math.Cosh(H));
		Vector2 localPosition = new Vector2(Math.Cos(v), Math.Sin(v));

		double speed = Math.Sqrt(mu * ((2 / r) - (1 / a)));
		Vector2 localVelocity = new Vector2(-Math.Sinh(H), axisRatio * Math.Cosh(H));

		return new StateVector(r * LocalToWorldSpace(localPosition.Normalized()), -speed * LocalToWorldSpace(localVelocity.Normalized()));
	}

	public StateVector GetStateVector(double t, int bound = anomalyBound)
	{
		// Not sure why, but every angle seems to be flipped with the new cartesian implementation. There's probably a proper fix for this, but until it
		// actually causes issues, this works too.
		t *= -1;

		double M = (n * t);

		if(isElliptical)
		{
			double E = ComputeEccentricAnomaly(M, bound);
			double v = EccentricAnomalyToTrueAnomaly(E);

			return EllipticalStateVector(E, v);
		}
		else
		{
			double H = ComputeHyperbolicAnomaly(M, bound);
			double v = HyperbolicAnomalyToTrueAnomaly(H);

			return HyperbolicStateVector(H, v);
		}
	}

	public StateVector GetStateVectorFromTruenomaly(double v)
	{
		v *= -1;

		if(isElliptical)
		{
			double E = TrueAnomalyToEccentricAnomaly(v);

			return EllipticalStateVector(E, v);
		}
		else
		{
			double H = TrueAnomalyToHyperbolicAnomaly(v);

			return HyperbolicStateVector(H, v);
		}
	}

	public StateVector GetStateVectorFromEccentricAnomaly(double E)
	{
		E *= -1;

		if(isElliptical)
		{
			double v = EccentricAnomalyToTrueAnomaly(E);

			return EllipticalStateVector(E, v);
		}
		else
		{
			double v = HyperbolicAnomalyToTrueAnomaly(E);

			return HyperbolicStateVector(E, v);
		}
	}

	// this is a reduced version of GetStateVector, which only returns the position
	// this should be mainly used for orbit rendering, not for orbital mechanics
	public Vector3 Sample(double v)
	{
		v *= -1;
		double r;

		if(isElliptical)
		{
			double E = TrueAnomalyToEccentricAnomaly(v);
			r = a * (1 - e * Math.Cos(E));
		}
		else
		{
			double H = TrueAnomalyToHyperbolicAnomaly(v);
			r = a * (1 - e * Math.Cosh(H));
		}

		return r * LocalToWorldSpace(new Vector2(Math.Cos(v), Math.Sin(v)));
	}

	// This function returns the true anomaly at which the orbit exits the SOI (NaN if the orbit doesn't exit the SOI)
	// Since the orbit's focal point must lie at the centre of the planetary system, we know that the orbital plane always perfectly bisects the SOI.
	// Therefore we can treat both of these shapes as two-dimensional within the orbital plane.
	// This function is the result of setting the 2D ellipse equation (rearranged for finding x) equal to the equation for the SOI, which is simply a circle with the system radius.
	// In this setup the origin is the centre of the system, which means we also have to shift the ellipse so that the closer focal point is at the origin.
	private double EllipticalSphereIntersection()
	{

		double r = planetarySystem.systemRadius;

		if(Apoapsis < r) return double.NaN; // if the apoapsis is lower than the SOI radius, we can't be intersecting a sphere and we can return NaN

		double b = a * axisRatio; // semi-minor axis
		double c = a * e; // shifting factor added to ellipse

		// squares are cached
		double sqA = a * a;
		double sqB = b * b;
		double sqC = c * c;
		double sqR = r * r;

		// part of the equation, but saved as it's own variable for readability purposes
		double root = -(sqA * sqA * sqB) + (sqA * sqB * sqB) + (sqA * sqA * sqR) + (sqA * sqB * sqC) - (sqA * sqB * sqR);

		double x = (((sqB * c) - Math.Sqrt(root)) / (sqA - sqB)); // this is the actual equation

		return Math.Acos(x / r); // calculates true anomaly from x and returns
	}

	// This is a version of the function above adjusted for the case of a hyperbolic orbit.
	// For a hyperbolic orbit the equation and shifting factor are different.
	// This version shouldn't return NaN, since a hyperbolic orbit must always exit the SOI.
	private double HyperbolicSphereIntersection()
	{
		double r = planetarySystem.systemRadius;

		double b = a * axisRatio;
		double c = Math.Sqrt(a * a + b * b);

		double sqA = a * a;
		double sqB = b * b;
		double sqC = c * c;
		double sqR = r * r;

		double root = (sqA * sqA * sqB) + (sqA * sqB * sqB) - (sqA * sqB * sqC) + (sqA * sqA * sqR) + (sqA * sqB * sqR);

		double x = (-(sqB * c) + Math.Sqrt(root)) / (sqA + sqB);

		return Math.Acos(-x / r); // hyperbola is flipped, therefore must use -x
	}

	public double SphereIntersection()
	{
		return isElliptical ? EllipticalSphereIntersection() : HyperbolicSphereIntersection();
	}

	public override string ToString()
	{
		return $"Semi-Major Axis: {this.semiMajorAxis}, Eccentricity: {this.eccentricity}, Inclination: {Mathf.RadToDeg(this.inclination)}, Longitude of Ascending Node: {Mathf.RadToDeg(this.ascendingNodeLongitude)}, Argument of Periapsis: {Mathf.RadToDeg(this.argumentOfPeriapsis)};";
	}
}
