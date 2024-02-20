using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class Wobbler : RigidBody
	{
		public Wobbler(Simulation sim, Wobbler source)
			: base(sim, source)
		{
			this.ApplyDiveForceLocalPoint = source.AxialWaterDragFactors;
			this.SinkYLevel = source.SinkYLevel;
			this.SinkRate = source.SinkRate;
			this.preventDiveTimer = source.preventDiveTimer;
			this.Size = source.Size;
			this.CalculateInertiaTensor();
		}

		public override void EnableMotionMonitor(string name = "MassMotion")
		{
			base.EnableMotionMonitor(name);
		}

		public override void CalculateInertiaTensor()
		{
			base.InertiaTensor = RigidBody.SolidBoxInertiaTensor(base.MassValue, this.Size * 0.3f, this.Size * 0.3f, this.Size);
		}

		public override void Simulate()
		{
			if (this.Position4f.Y <= this.WaterY + 0.05f)
			{
				this.preventDiveTimer -= 0.0004f;
				Vector4f vector4f = this.Velocity4f - this.FlowVelocity;
				float num = Mathf.Sqrt(Mathf.Max(vector4f.X * vector4f.X + vector4f.Z * vector4f.Z - vector4f.Y * vector4f.Y, 0f));
				float num2 = num / 4f;
				float num3 = -this.SinkYLevel;
				float num4 = 0.05f - this.Position4f.Y;
				if (num4 < num3 && this.Position4f.Y > base.GroundHeight + 0.05f && this.preventDiveTimer <= 0f)
				{
					float num5 = -(base.Buoyancy + 1f) * base.MassValue * 9.81f;
					float num6 = this.SinkRate * num * -num5;
					float num7 = (-base.MassValue * 9.81f - num5) * Mathf.Clamp01(num * 3f);
					float num8 = Mathf.Pow(num3, 0.5f);
					float num9 = (num7 - num6) / num8 * Mathf.Pow(num4, 0.5f) + num6;
					base.ApplyForce(Vector4fExtensions.down * new Vector4f(num9), this.ApplyDiveForceLocalPoint, false);
				}
				if (this.WobblingEnabled)
				{
					float num10 = 2f * (Mathf.PerlinNoise(this.Sim.InternalTime * 5f, 0f) - 0.5f);
					Vector3 vector = Vector3.Cross(this._rotation * Vector3.forward, Vector3.up);
					Vector4f vector4f2 = (vector * num * num10 * base.MassValue * 5f).AsPhyVector(ref vector4f);
					base.ApplyForce(vector4f2, Vector3.forward * this.Size * 0.5f, false);
					base.ApplyForce(vector4f2.Negative(), Vector3.back * this.Size * 0.5f, false);
				}
			}
			else
			{
				this.preventDiveTimer = 1f;
			}
			base.Simulate();
		}

		public const float ShakingIntensity = 5f;

		public const float ShakingFrequency = 5f;

		public const float MaxSpeed = 4f;

		public const float DepthForceCurvePower = 0.5f;

		public const float DiveMinGroundDistance = 0.05f;

		public float SinkYLevel;

		public float SinkRate;

		public Vector3 ApplyDiveForceLocalPoint;

		public float Size;

		public bool WobblingEnabled = true;

		public bool DivingEnabled = true;

		private float preventDiveTimer;
	}
}
