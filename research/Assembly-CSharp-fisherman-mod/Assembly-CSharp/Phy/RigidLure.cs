using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class RigidLure : RigidBody
	{
		public RigidLure(Simulation sim, float mass, Vector3 position, Vector3 dimensions, Mass.MassType type)
			: base(sim, mass, position, type, 0.0125f)
		{
			this.Dimensions = dimensions;
			this.BuoyancyCenter = Vector3.zero;
			base.InertiaTensor = RigidBody.SolidBoxInertiaTensor(mass, dimensions.x, dimensions.y, dimensions.z);
			this.WorkingDepth = 1f;
		}

		public RigidLure(Simulation sim, RigidLure source)
			: base(sim, source)
		{
			this.Dimensions = source.Dimensions;
			this.BuoyancyCenter = source.BuoyancyCenter;
			this.WingletLift = source.WingletLift;
			this.WingletAttack = source.WingletAttack;
			this.WingletPosition = source.WingletPosition;
			this.BuoyancyVolumetric = source.BuoyancyVolumetric;
			this.WorkingDepth = source.WorkingDepth;
			this.depthSigmoidSign = ((this.BuoyancyVolumetric < 1f) ? (-1f) : 1f);
		}

		public new Vector3 Dimensions { get; protected set; }

		public override void CalculateInertiaTensor()
		{
			base.InertiaTensor = RigidBody.SolidBoxInertiaTensor(base.MassValue, this.Dimensions.x, this.Dimensions.y, this.Dimensions.z);
		}

		public override void Simulate()
		{
			float num = Mathf.Sqrt(this.Velocity4f.X * this.Velocity4f.X + this.Velocity4f.Z * this.Velocity4f.Z);
			this.path += num * 0.0004f;
			if (this.path > 100f)
			{
				this.path = 0f;
			}
			if ((this.Position + this.Rotation * this.BuoyancyCenter).y < 0f)
			{
				base.ApplyForce(new Vector4f(0f, this.BuoyancyVolumetric * base.MassValue * 9.81f, 0f, 0f), this.BuoyancyCenter, false);
				if (this.BuoyancyVolumetric < 0f)
				{
					float num2 = this.BuoyancyVolumetric * base.MassValue * 9.81f;
				}
				else
				{
					float num3 = this.BuoyancyVolumetric * base.MassValue * 9.81f;
				}
			}
			Vector3 vector = this.Position + this.Rotation * this.WingletPosition;
			if (vector.y < 0f && vector.y > this.groundPoint.Y + 0.1f && num > 0.25f)
			{
				Vector4f vector4f = base.PointWorldVelocity4f(this.WingletPosition) - this.FlowVelocity;
				Vector4f vector4f2 = (this.Rotation * this.WingletAttack).AsPhyVector(ref vector4f);
				Vector4f up = Vector4fExtensions.up;
				Vector4f vector4f3 = (this.Rotation * this.WingletLift).AsPhyVector(ref vector4f);
				float num4 = Vector4fExtensions.Dot(vector4f, vector4f2);
				num4 = Mathf.Max(0f, num4);
				if (num4 > 1f)
				{
					num4 *= num4;
				}
				Vector4f vector4f4 = vector4f2 * new Vector4f(-num4);
				float num5 = 0f;
				if (vector.y > -this.WorkingDepth + 0.1f)
				{
					num5 = 1f;
				}
				else if (vector.y < -this.WorkingDepth - 0.1f)
				{
					num5 = -1f;
				}
				Vector4f vector4f5 = vector4f3 * new Vector4f(num4 * num5);
				base.ApplyForce(vector4f4 + vector4f5, this.WingletPosition, false);
				if (this.Position4f.Y < -0.1f)
				{
					base.ApplyTorque(up * new Vector4f(base.MassValue * 1f * num * Mathf.Sin(this.path * 16f)));
				}
			}
			base.Simulate();
		}

		public Vector3 BuoyancyCenter;

		public float BuoyancyVolumetric;

		public Vector3 WingletLift;

		public Vector3 WingletAttack;

		public Vector3 WingletPosition;

		public float WorkingDepth;

		private float path;

		private float depthSigmoidSign;

		private const float depthSigmoidSlope = 20f;

		private const float wobbleMinSpeed = 0.25f;

		private const float wobbleMinDepth = 0.1f;

		private const float wobbleFrequency = 16f;

		private const float wobbleAmplitude = 1f;
	}
}
