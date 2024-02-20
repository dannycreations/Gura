using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy.Verlet
{
	public class VerletBend : VerletConstraint
	{
		public VerletBend(VerletMass mass1, VerletMass mass2, float friction, float rotationalStiffness, float displacementStiffness, float normalStiffness)
			: base(mass1, mass2, -1)
		{
			this.vmass1 = mass1;
			this.vmass2 = mass2;
			this.FrictionConstant = friction;
			this.RotationalStiffness = rotationalStiffness;
			this.DisplacementStiffness = displacementStiffness;
			this.NormalStiffness = normalStiffness;
			this.p21 = Quaternion.Inverse(base.Mass2.Rotation) * (base.Mass1.Position4f - base.Mass2.Position4f).AsVector3();
			this.p12 = Quaternion.Inverse(base.Mass1.Rotation) * (base.Mass2.Position4f - base.Mass1.Position4f).AsVector3();
			this.r12sqr = this.p12.sqrMagnitude;
			this.r12 = Mathf.Sqrt(this.r12sqr);
			this.q12 = base.Mass2.Rotation * Quaternion.Inverse(base.Mass1.Rotation);
			this.q21 = base.Mass1.Rotation * Quaternion.Inverse(base.Mass2.Rotation);
		}

		public float FrictionConstant
		{
			get
			{
				return this._frictionConstant4f.X;
			}
			set
			{
				this._frictionConstant4f = new Vector4f(value);
			}
		}

		public float DisplacementStiffness
		{
			get
			{
				return this._displacementStiffness4f.X;
			}
			set
			{
				this._displacementStiffness4f = new Vector4f(value);
			}
		}

		public float NormalStiffness
		{
			get
			{
				return this._normalStiffness4f.X;
			}
			set
			{
				this._normalStiffness4f = new Vector4f(value);
			}
		}

		public override void SetMasses(Mass mass1, Mass mass2 = null)
		{
			if (mass1 != null)
			{
				base.Mass1 = mass1;
				this.vmass1 = mass1 as VerletMass;
			}
			if (mass2 != null)
			{
				base.Mass2 = mass2;
				this.vmass2 = mass2 as VerletMass;
			}
		}

		public override void Satisfy()
		{
			Vector3 vector = base.Mass2.Rotation * this.p21;
			Vector3 vector2 = base.Mass1.Rotation * this.p12;
			Vector3 vector3 = (base.Mass1.Position4f - base.Mass2.Position4f).AsVector3();
			Vector3 vector4 = (base.Mass2.Position4f - base.Mass1.Position4f).AsVector3();
			Quaternion quaternion = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(vector, vector3), 0.5f);
			Quaternion quaternion2 = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(vector2, vector4), 0.5f);
			Quaternion rotation = base.Mass1.Rotation;
			if (!base.Mass2._isKinematic)
			{
				base.Mass2.Rotation = Quaternion.Lerp(base.Mass2.Rotation, quaternion2 * (this.q12 * base.Mass1.Rotation), this.RotationalStiffness);
			}
			Vector4f position4f = base.Mass1.Position4f;
			if (!base.Mass2._isKinematic)
			{
				base.Mass2.Position4f = Vector4fExtensions.Lerp(base.Mass2.Position4f, position4f + (base.Mass1.Rotation * this.p12).AsPhyVector(ref base.Mass2.Position4f), this._displacementStiffness4f.X);
			}
			Vector4f vector4f = base.Mass2.Position4f - base.Mass1.Position4f;
			float num = Vector4fExtensions.Dot(vector4f, vector4f);
			float num2 = (this.r12sqr / (num + this.r12sqr) - 0.5f) * this.NormalStiffness;
			if (!base.Mass1._isKinematic)
			{
				base.Mass1.Position4f -= vector4f * new Vector4f(num2);
			}
			if (!base.Mass2._isKinematic)
			{
				base.Mass2.Position4f += vector4f * new Vector4f(num2);
			}
		}

		public override void Solve()
		{
			Vector4f vector4f = (this.vmass1.PositionDelta4f - this.vmass2.PositionDelta4f) * this._frictionConstant4f;
			if (!base.Mass1._isKinematic)
			{
				base.Mass1.Position4f -= vector4f;
			}
			if (!base.Mass2._isKinematic)
			{
				base.Mass2.Position4f += vector4f;
			}
		}

		private Vector4f _frictionConstant4f;

		public float RotationalStiffness;

		private Vector4f _displacementStiffness4f;

		private Vector4f _normalStiffness4f;

		private Vector3 p12;

		private Vector3 p21;

		private float r12;

		private float r12sqr;

		private Quaternion q12;

		private Quaternion q21;

		protected VerletMass vmass1;

		protected VerletMass vmass2;
	}
}
