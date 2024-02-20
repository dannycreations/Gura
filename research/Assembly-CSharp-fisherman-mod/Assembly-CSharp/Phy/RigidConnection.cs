using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class RigidConnection : ConnectionBase
	{
		public RigidConnection(Mass mass1, Mass mass2)
			: base(mass1, mass2, -1)
		{
			this.body1 = mass1 as RigidBody;
			this.body2 = mass2 as RigidBody;
			if (this.body1 == null || this.body2 == null)
			{
				Debug.LogError("Both masses should be rigid bodies for rigid connection.");
			}
			this.restRelativeRotation = this.RelativeRotation;
			this.restRelativeRotationInverse = Quaternion.Inverse(this.restRelativeRotation);
			this.restRelativePosition = this.RelativePosition;
		}

		public Quaternion RelativeRotation
		{
			get
			{
				return this.body2._rotation * Quaternion.Inverse(this.body1._rotation);
			}
		}

		public Vector3 RelativePosition
		{
			get
			{
				return this.body2.LocalToWorld(this.Body2AttachmentLocalPoint) - this.body1.LocalToWorld(this.Body1AttachmentLocalPoint);
			}
		}

		public override void Solve()
		{
			Vector3 eulerAngles = (this.RelativeRotation * this.restRelativeRotationInverse).eulerAngles;
			Vector3 vector = this.RelativePosition - this.restRelativePosition;
			Vector4f vector4f = vector.AsPhyVector(ref this.body1.Torque) * new Vector4f(this.PositionalStiffness);
			vector4f -= (this.body1.PointWorldVelocity4f(this.Body1AttachmentLocalPoint) - this.body2.PointWorldVelocity4f(this.Body2AttachmentLocalPoint)) * new Vector4f(this.FrictionConstant);
			this.body1.ApplyForce(vector4f, this.Body1AttachmentLocalPoint, base.Mass1.IsRef);
			this.body2.ApplyForce(vector4f.Negative(), this.Body2AttachmentLocalPoint, false);
		}

		public float StraightLength;

		public float RotationalStiffness;

		public float PositionalStiffness;

		public Vector3 Body1AttachmentLocalPoint;

		public Vector3 Body2AttachmentLocalPoint;

		public float FrictionConstant;

		private RigidBody body1;

		private RigidBody body2;

		private Quaternion restRelativeRotation;

		private Quaternion restRelativeRotationInverse;

		private Vector3 restRelativePosition;
	}
}
