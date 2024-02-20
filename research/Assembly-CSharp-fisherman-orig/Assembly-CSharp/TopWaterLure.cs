using System;
using Mono.Simd;
using Mono.Simd.Math;
using Phy;
using UnityEngine;

public class TopWaterLure : RigidBody
{
	public TopWaterLure(Simulation sim, float mass, Vector3 position, float length, float width)
		: base(sim, mass, position, Mass.MassType.TopWaterLure, 0.0125f)
	{
		this.Length = length;
		this.Width = width;
		this.CalculateInertiaTensor();
		this.rodSim = this.Sim as FishingRodSimulation;
	}

	public TopWaterLure(Simulation sim, TopWaterLure source)
		: base(sim, source)
	{
		this.Length = source.Length;
		this.Width = source.Width;
		this.BuoyancyVolumetric = source.BuoyancyVolumetric;
		this.BuoyancyMaxDisplacement = source.BuoyancyMaxDisplacement;
		this.LateralWaterResistance = source.LateralWaterResistance;
		this.LongitudalWaterResistance = source.LongitudalWaterResistance;
		this.FactorLevel = source.FactorLevel;
		this.MaxForceFactor = source.MaxForceFactor;
		this.ImbalanceRatio = source.ImbalanceRatio;
		this.WalkingFactor = source.WalkingFactor;
		this.SinkYLevel = source.SinkYLevel;
		this.SinkRate = source.SinkRate;
		this.WobblingEnabled = source.WobblingEnabled;
		this.IsFloatUp = source.IsFloatUp;
		this.waterResisntace = new Vector3(this.LateralWaterResistance, this.LateralWaterResistance, this.LongitudalWaterResistance);
		this.CalculateInertiaTensor();
		this.rodSim = this.Sim as FishingRodSimulation;
	}

	public bool Strike
	{
		get
		{
			return this._strike;
		}
		set
		{
			this._strike = value;
			if (this.Sim.PhyActionsListener != null)
			{
				this.Sim.PhyActionsListener.TopWaterStrikeSignal(base.UID);
			}
		}
	}

	public override void CalculateInertiaTensor()
	{
		float num = base.MassValue / 5f;
		float num2 = this.Width * 0.5f;
		float num3 = this.Length * 0.5f;
		base.InertiaTensor = new Vector4f(num * (num2 * num2 + num3 * num3), num * (num2 * num2 + num3 * num3), num * (2f * num2 * num2), 1f);
	}

	private Vector2 SegmentZeroYIntersection(Vector2 a, Vector2 b)
	{
		float num = -a.y / (b.y - a.y);
		return Vector2.Lerp(a, b, num);
	}

	private Vector2 SegmentLineIntersection(Vector2 a, Vector2 b, Vector2 lineNormal, Vector2 linePoint)
	{
		Vector2 vector = b - a;
		float magnitude = vector.magnitude;
		vector /= magnitude;
		float num = Vector2.Dot(linePoint - a, lineNormal);
		float num2 = Vector2.Dot(vector, lineNormal);
		if (Mathf.Approximately(num2, 0f))
		{
			return Vector2.zero;
		}
		float num3 = num / num2;
		if (num3 > magnitude)
		{
			return b;
		}
		Vector2 vector2 = vector * num3;
		return a + vector2;
	}

	private Vector2 TriangleCenterAndArea(Vector2 a, Vector2 b, Vector2 c, out float area)
	{
		area = Mathf.Abs(0.5f * Vector3.Cross(b - a, c - a).magnitude);
		return (a + b + c) / 3f;
	}

	public Vector2 RectDisplacement2D(Vector2[] vertices, out float area, Vector2 clipNormal, Vector2 clipPoint)
	{
		int[] array = new int[4];
		int[] array2 = new int[4];
		int num = 0;
		int num2 = 0;
		Vector2 vector = Vector2.zero;
		area = 0f;
		for (int i = 0; i < 4; i++)
		{
			if (Vector2.Dot(vertices[i] - clipPoint, clipNormal) <= 0f)
			{
				array[num] = i;
				num++;
			}
			else
			{
				array2[num2] = i;
				num2++;
			}
		}
		if (num == 4)
		{
			area = (vertices[0] - vertices[1]).magnitude * (vertices[0] - vertices[2]).magnitude;
			return (vertices[0] + vertices[1] + vertices[2] + vertices[3]) * 0.25f;
		}
		if (num == 3)
		{
			int num3 = (array2[0] + 3) % 4;
			int num4 = (array2[0] + 1) % 4;
			int num5 = (array2[0] + 2) % 4;
			Vector2 vector2 = this.SegmentLineIntersection(vertices[num3], vertices[array2[0]], clipNormal, clipPoint);
			Vector2 vector3 = this.SegmentLineIntersection(vertices[num4], vertices[array2[0]], clipNormal, clipPoint);
			float num6 = 0f;
			Vector2 vector4 = this.TriangleCenterAndArea(vertices[num3], vector2, vertices[num5], out num6);
			vector += vector4 * num6;
			area += num6;
			vector4 = this.TriangleCenterAndArea(vertices[num5], vector3, vertices[num4], out num6);
			vector += vector4 * num6;
			area += num6;
			vector4 = this.TriangleCenterAndArea(vector2, vector3, vertices[num5], out num6);
			area += num6;
			vector += vector4 * num6;
			vector /= area;
		}
		else if (num == 2)
		{
			int num7 = array2[0];
			int num8 = array2[1];
			int num9;
			int num10;
			if ((num7 + 1) % 4 == num8)
			{
				num9 = (num7 + 3) % 4;
				num10 = (num8 + 1) % 4;
			}
			else
			{
				num9 = (num7 + 1) % 4;
				num10 = (num8 + 3) % 4;
			}
			Vector2 vector5 = this.SegmentLineIntersection(vertices[num7], vertices[num9], clipNormal, clipPoint);
			Vector2 vector6 = this.SegmentLineIntersection(vertices[num8], vertices[num10], clipNormal, clipPoint);
			float num11 = 0f;
			Vector2 vector7 = this.TriangleCenterAndArea(vector5, vertices[num9], vertices[num10], out num11);
			vector += vector7 * num11;
			area += num11;
			vector7 = this.TriangleCenterAndArea(vector6, vector5, vertices[num10], out num11);
			vector += vector7 * num11;
			area += num11;
			vector /= area;
		}
		else if (num == 1)
		{
			int num12 = (array[0] + 3) % 4;
			int num13 = (array[0] + 1) % 4;
			Vector2 vector8 = this.SegmentLineIntersection(vertices[num12], vertices[array[0]], clipNormal, clipPoint);
			Vector2 vector9 = this.SegmentLineIntersection(vertices[num13], vertices[array[0]], clipNormal, clipPoint);
			vector = this.TriangleCenterAndArea(vertices[array[0]], vector8, vector9, out area);
		}
		return vector;
	}

	public override void Simulate()
	{
		Vector3 vector = this._rotation * Vector3.forward;
		float num = Mathf.Atan2(vector.y, Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z));
		Vector3 vector2;
		vector2..ctor(0f, Mathf.Sin(num), Mathf.Cos(num));
		Vector3 normalized = Vector3.Cross(vector2, Vector3.right).normalized;
		Vector2 vector3;
		vector3..ctor(Mathf.Sin(-num), Mathf.Cos(-num));
		Vector2 right = Vector2.right;
		Vector2 up = Vector2.up;
		Vector2 vector4;
		vector4..ctor(0f, this.Position4f.Y);
		Vector2[] array = new Vector2[]
		{
			right * this.Length * 0.5f + up * this.Width * 0.5f,
			right * this.Length * 0.5f - up * this.Width * 0.5f,
			-right * this.Length * 0.5f - up * this.Width * 0.5f,
			-right * this.Length * 0.5f + up * this.Width * 0.5f
		};
		float num2;
		Vector2 vector5 = this.RectDisplacement2D(array, out num2, vector3, -vector3 * this.Position4f.Y);
		float num3 = num2 / (this.Length * this.Width);
		this.imbalance = Mathf.Lerp(this.imbalance, Mathf.Exp(-10f * (this.Velocity4f - this.FlowVelocity).Magnitude()), 0.05f);
		vector5..ctor(vector5.x * this.Length, (vector5.y * 0.5f + 0.5f) * this.Width);
		Vector3 vector6;
		vector6..ctor(vector5.y, 0f, -vector5.x + this.imbalance * this.Length * this.ImbalanceRatio);
		this.debug_DisplacementCenter = base.LocalToWorld(vector6);
		this.debug_DisplacementRelVolume = num2 / (this.Length * this.Width);
		if (this.IsFloatUp)
		{
			base.ApplyForce(new Vector4f(0f, this.BuoyancyVolumetric * num3, 0f, 0f), vector6, false);
		}
		else
		{
			base.ApplyForce(new Vector4f(0f, num2 * this.BuoyancyVolumetric * this.Width * 1000f, 0f, 0f), vector6, false);
		}
		float num4 = Mathf.Clamp01(1f - (this.Position4f.Y - 0.05f) / 0.1f);
		Vector3 vector7;
		vector7..ctor(vector5.y, 0f, -vector5.x);
		Vector4f vector4f = base.PointWorldVelocity4f(vector7) - this.FlowVelocity;
		float num5 = vector4f.Magnitude();
		float num6 = num5;
		if (num6 < 1f)
		{
			num6 = 1f;
		}
		if (!Mathf.Approximately(num5, 0f) && num3 > 0f)
		{
			Quaternion quaternion = Quaternion.Inverse(this._rotation);
			Vector3 vector8 = quaternion * vector4f.AsVector3();
			float num7 = Mathf.Sqrt(vector4f.X * vector4f.X + vector4f.Z * vector4f.Z);
			Vector3 vector9 = vector8 / num5;
			Vector4f vector4f2;
			vector4f2..ctor(Mathf.Min(num6, 2f) * num4 * ((Mathf.Abs(vector9.x) + Mathf.Abs(vector9.y)) * this.LateralWaterResistance + Mathf.Abs(vector9.z) * this.LongitudalWaterResistance));
			this.ApplyForce(vector4f.Negative() * vector4f2, false);
			Vector4f vector4f3 = (this._rotation * Vector3.forward).AsPhyVector(ref this.Velocity4f);
			Vector4f vector4f4 = (this._rotation * Vector3.right).AsPhyVector(ref this.Velocity4f);
			Vector4f vector4f5 = (this._rotation * Vector3.up).AsPhyVector(ref this.Velocity4f);
			this.ApplyForce(vector4f4 * new Vector4f(-num4 * num6 * this.LateralWaterResistance * Vector4fExtensions.Dot(vector4f, vector4f4)), false);
			this.ApplyForce(vector4f5 * new Vector4f(-num4 * num6 * this.LateralWaterResistance * Vector4fExtensions.Dot(vector4f, vector4f5)), false);
			if (num7 > 0.3f)
			{
				float num8 = base.GroundHeight - 0.1f;
				bool flag = this.Position4f.Y < num8;
				if (num8 < this.SinkYLevel)
				{
					num8 = this.SinkYLevel;
				}
				float num9 = Mathf.Abs(this.Position4f.Y - this.SinkYLevel);
				bool flag2 = this.Position4f.Y < num8;
				float num10;
				if (flag)
				{
					num10 = 10f * (1f - (this.Position4f.Y - base.GroundHeight) / 0.1f);
				}
				else
				{
					num10 = this.FactorLevel * num9;
					if (num10 > this.MaxForceFactor)
					{
						num10 = this.MaxForceFactor;
					}
					if (this.rodSim != null)
					{
						float num11 = this.rodSim.FinalLineLength - 5f;
						if (num11 <= 0f)
						{
							num10 = 0f;
						}
						else if (num11 < 1f)
						{
							num10 *= num11;
						}
					}
				}
				if (num10 > 0f)
				{
					Vector4f vector4f6;
					vector4f6..ctor(num10 * this.SinkRate * num3 * 0.25f);
					this.ApplyForce(vector4f * new Vector4f(-this.LongitudalWaterResistance * num7 / num5) * (vector4f6 + Vector4fExtensions.half3), false);
					this.ApplyForce(new Vector4f(num7 * ((!flag2 && !flag) ? (-0.5f) : 0.5f)) * vector4f6 * Vector4fExtensions.up, false);
				}
			}
			if (this._strike && this.Sim.InternalTime > this.strikeTimestamp + 0.5f)
			{
				this.strikeTimestamp = this.Sim.InternalTime;
				this.liftSign = -this.liftSign;
				this._strike = false;
			}
			float num12 = this.Sim.InternalTime - this.strikeTimestamp;
			float num13 = this.liftSign * Mathf.Cos(num12 * 3.1415927f * 2f) * Mathf.Exp(-num12);
			this.RotateSpringImpulse = 75f * num13 * Mathf.Pow(num3, 0.1f) * this.WalkingFactor;
			if (this.WobblingEnabled)
			{
				this.RotateSpringImpulse += Mathf.Clamp(num5 * 20f * 2f * (Mathf.PerlinNoise(this.Sim.InternalTime * 5f, 0f) - 0.5f), -45f, 45f);
			}
			base.ApplyTorque(this.AngularVelocity.Negative() * new Vector4f(num3 * 0.0002f));
		}
		else
		{
			this.RotateSpringImpulse = 0f;
		}
		base.Simulate();
	}

	public const float LiftFactor = 2f;

	public const float ResistanceMaxVelocity = 2f;

	public const float ShakingIntensity = 20f;

	public const float ShakingFrequency = 5f;

	public const float SpeedLaminar = 1f;

	public const float MinOverGround = 0.1f;

	public const float GroundFactor = 10f;

	public const float SmallFinalLineLength = 5f;

	public float Length;

	public float Width;

	public float BuoyancyVolumetric;

	public float BuoyancyMaxDisplacement;

	public float LateralWaterResistance;

	public float LongitudalWaterResistance;

	public float FactorLevel;

	public float MaxForceFactor;

	public float WalkingFactor;

	public float ImbalanceRatio;

	public float SinkYLevel = 1f;

	public float SinkRate;

	public bool WobblingEnabled;

	public bool IsFloatUp;

	private bool _strike;

	public Vector4f SpringAppliedImpulse;

	public float RotateSpringImpulse;

	public Vector3 debug_DisplacementCenter;

	public float debug_DisplacementRelVolume;

	public Vector3 debug_Lift;

	private Vector3 waterResisntace;

	private FishingRodSimulation rodSim;

	private float imbalance;

	private float imbalanceVel;

	private float strikeTimestamp;

	private float liftSign = 1f;

	private float prevLiftSign = 1f;
}
