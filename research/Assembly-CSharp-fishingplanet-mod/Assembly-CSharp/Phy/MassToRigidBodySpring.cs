using System;
using Mono.Simd;
using Mono.Simd.Math;
using Phy.Verlet;
using UnityEngine;

namespace Phy
{
	public class MassToRigidBodySpring : Spring
	{
		public MassToRigidBodySpring(Mass mass, RigidBody rigidbody, float torsionalFrictionFactor, float springLength, float frictionConstant, Vector3 attachmentPoint)
			: base(mass, rigidbody, 0f, springLength, frictionConstant)
		{
			this.RigidBodyAttachmentLocalPoint = attachmentPoint;
			this.RigidBodyMass = rigidbody;
			this.TopWaterLureMass = rigidbody as TopWaterLure;
			this.torsionalFrictionFactor = torsionalFrictionFactor;
		}

		public MassToRigidBodySpring(Simulation sim, MassToRigidBodySpring source)
			: base(sim, source)
		{
			this.RigidBodyAttachmentLocalPoint = source.RigidBodyAttachmentLocalPoint;
			this.RigidBodyMass = source.RigidBodyMass;
			this.TopWaterLureMass = source.TopWaterLureMass;
			this.torsionalFrictionFactor = source.torsionalFrictionFactor;
		}

		public float ImpulseVerticalFactor
		{
			get
			{
				return this._impulseVerticalFactor;
			}
			set
			{
				this._impulseVerticalFactor = value;
				base.ConnectionNeedSyncMark();
			}
		}

		public RigidBody RigidBody
		{
			get
			{
				return this.RigidBodyMass;
			}
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			this._impulseVerticalFactor = (source as MassToRigidBodySpring)._impulseVerticalFactor;
		}

		public override void UpdateReferences()
		{
			base.UpdateReferences();
			this.RigidBodyMass = base.Mass1.Sim.DictMasses[this.RigidBodyMass.UID] as RigidBody;
			this.TopWaterLureMass = this.RigidBodyMass as TopWaterLure;
		}

		public Vector3 AttachmentPointWorld()
		{
			return this.RigidBodyMass.LocalToWorld(this.RigidBodyAttachmentLocalPoint);
		}

		public static float SatisfyImpulseConstraintRigidBody(MassToRigidBodySpring s, Mass mass1, RigidBody rigidbody, float springLength, Vector4f friction, float torsionalFriction, float impulseThreshold2, bool isHitch = false)
		{
			float num = 0f;
			Vector4f vector4f = rigidbody.InvMassValue4f;
			if (isHitch)
			{
				vector4f = Vector4f.Zero;
			}
			Vector4f vector4f2 = mass1.Position4f + mass1.Velocity4f * Simulation.TimeQuant4f;
			Vector4f vector4f3 = rigidbody.LocalToWorld4f(s.RigidBodyAttachmentLocalPoint);
			Vector4f velocity4f = mass1.Velocity4f;
			Vector4f vector4f4 = rigidbody.PointWorldVelocity4f(s.RigidBodyAttachmentLocalPoint);
			Vector4f vector4f5 = vector4f3 + vector4f4 * Simulation.TimeQuant4f;
			Vector4f vector4f6 = vector4f5 - vector4f2;
			float num2 = vector4f6.Magnitude();
			Vector4f vector4f7;
			vector4f7..ctor(num2);
			Vector4f vector4f8 = vector4f6 / vector4f7;
			Vector4f vector4f9 = vector4f8.Negative();
			float num3 = rigidbody.AngularVelocity.Magnitude();
			if (!Mathf.Approximately(num3, 0f) && !Mathf.Approximately(num2, 0f))
			{
				Vector4f vector4f10 = new Vector4f(-Mathf.Abs(Vector4fExtensions.Dot(rigidbody.AngularVelocity, vector4f8)) / num3 * torsionalFriction) * rigidbody.AngularVelocity;
				vector4f10.W = 0f;
				rigidbody.ApplyTorque(vector4f10);
			}
			Vector4f vector4f11;
			vector4f11..ctor(Vector4fExtensions.Dot(vector4f8, Vector4fExtensions.Cross(Vector4fExtensions.Cross(vector4f3 - rigidbody.Position4f, vector4f8) / rigidbody.GlobalInertiaTensor, vector4f3 - rigidbody.Position4f)));
			Vector4f vector4f12 = Vector4f.Zero;
			if (num2 > springLength)
			{
				Vector4f vector4f13 = vector4f8 * new Vector4f(num2 - springLength);
				Vector4f vector4f14 = vector4f13 / (mass1.InvMassValue4f + vector4f + vector4f11);
				Vector4f vector4f15 = vector4f14 / Simulation.TimeQuant4f;
				float num4 = vector4f15.Magnitude();
				num = num4;
				vector4f15.W = 0f;
				vector4f12 += vector4f15 * mass1.InvMassValue4f;
				if (num4 > impulseThreshold2 || rigidbody.Position4f.Y > 0.3f)
				{
					Vector4f vector4f16 = vector4f15.Negative() + vector4f8 * new Vector4f(impulseThreshold2);
					if (s.TopWaterLureMass != null)
					{
						s.TopWaterLureMass.SpringAppliedImpulse = vector4f16;
						Vector3 vector = Quaternion.AngleAxis(s.TopWaterLureMass.RotateSpringImpulse, Vector3.up) * vector4f16.AsVector3();
						vector4f16 = vector.AsPhyVector(ref vector4f16);
						vector4f16.Y *= s.ImpulseVerticalFactor;
					}
					rigidbody.ApplyImpulse(vector4f16, s.RigidBodyAttachmentLocalPoint);
				}
			}
			Vector4f vector4f17 = mass1.Position4f - vector4f3;
			float num5 = vector4f17.Magnitude();
			if ((double)num5 > 1E-06)
			{
				Vector4f vector4f18 = vector4f17 / new Vector4f(num5);
				Vector4f vector4f19 = (velocity4f - vector4f4) / (mass1.InvMassValue4f + vector4f);
				vector4f19 *= friction;
				vector4f12 -= vector4f19 * mass1.InvMassValue4f;
				if (s.TopWaterLureMass != null)
				{
					vector4f19.Y *= s.ImpulseVerticalFactor;
				}
				rigidbody.ApplyImpulse(vector4f19, s.RigidBodyAttachmentLocalPoint);
			}
			if (mass1 is VerletMass)
			{
				(mass1 as VerletMass).ApplyImpulse(vector4f12 * Simulation.TimeQuant4f, false);
			}
			else
			{
				mass1.Velocity4f += vector4f12;
			}
			return num;
		}

		public override void SatisfyImpulseConstraint()
		{
			Vector4f vector4f = base.Mass1.Position4f + base.Mass1.Velocity4f * Simulation.TimeQuant4f;
			Vector4f vector4f2 = this.RigidBodyMass.LocalToWorld4f(this.RigidBodyAttachmentLocalPoint);
			Vector4f velocity4f = base.Mass1.Velocity4f;
			Vector4f vector4f3 = this.RigidBodyMass.PointWorldVelocity4f(this.RigidBodyAttachmentLocalPoint);
			Vector4f vector4f4 = vector4f2 + vector4f3 * Simulation.TimeQuant4f;
			Vector4f vector4f5 = vector4f4 - vector4f;
			float num = vector4f5.Magnitude();
			Vector4f vector4f6;
			vector4f6..ctor(num);
			Vector4f vector4f7 = vector4f5 / vector4f6;
			Vector4f vector4f8 = vector4f7.Negative();
			float num2 = this.RigidBodyMass.AngularVelocity.Magnitude();
			if (!Mathf.Approximately(num2, 0f) && !Mathf.Approximately(num, 0f))
			{
				Vector4f vector4f9 = new Vector4f(-Mathf.Abs(Vector4fExtensions.Dot(this.RigidBodyMass.AngularVelocity, vector4f7)) / num2 * this.torsionalFrictionFactor) * this.RigidBodyMass.AngularVelocity;
				vector4f9.W = 0f;
				this.RigidBodyMass.ApplyTorque(vector4f9);
			}
			Vector4f vector4f10;
			vector4f10..ctor(Vector4fExtensions.Dot(vector4f7, Vector4fExtensions.Cross(Vector4fExtensions.Cross(vector4f2 - this.RigidBodyMass.Position4f, vector4f7) / this.RigidBodyMass.GlobalInertiaTensor, vector4f2 - this.RigidBodyMass.Position4f)));
			if (num > this._springLength)
			{
				Vector4f vector4f11 = vector4f7 * new Vector4f(num - this._springLength);
				Vector4f vector4f12 = vector4f11 / (base.Mass1.InvMassValue4f + base.Mass2.InvMassValue4f + vector4f10);
				Vector4f vector4f13 = vector4f12 / Simulation.TimeQuant4f;
				float num3 = vector4f13.Magnitude();
				vector4f13.W = 0f;
				base.Mass1.Velocity4f += vector4f13 * base.Mass1.InvMassValue4f;
				if (num3 > this._impulseThreshold2 || base.Mass2.Position4f.Y > 0.3f)
				{
					Vector4f vector4f14 = vector4f13.Negative() + vector4f7 * new Vector4f(this._impulseThreshold2);
					if (this.TopWaterLureMass != null)
					{
						this.TopWaterLureMass.SpringAppliedImpulse = vector4f14;
						Vector3 vector = Quaternion.AngleAxis(this.TopWaterLureMass.RotateSpringImpulse, Vector3.up) * vector4f14.AsVector3();
						vector4f14 = vector.AsPhyVector(ref vector4f14);
						vector4f14.Y *= this.ImpulseVerticalFactor;
					}
					this.RigidBodyMass.ApplyImpulse(vector4f14, this.RigidBodyAttachmentLocalPoint);
				}
			}
			Vector4f vector4f15 = this._mass1.Position4f - vector4f2;
			float num4 = vector4f15.Magnitude();
			if ((double)num4 > 1E-06)
			{
				Vector4f vector4f16 = vector4f15 / new Vector4f(num4);
				Vector4f vector4f17 = (velocity4f - vector4f3) / (base.Mass1.InvMassValue4f + base.Mass2.InvMassValue4f);
				vector4f17 *= this._frictionConstant4f;
				base.Mass1.Velocity4f -= vector4f17 * base.Mass1.InvMassValue4f;
				if (this.TopWaterLureMass != null)
				{
					vector4f17.Y *= this.ImpulseVerticalFactor;
				}
				this.RigidBodyMass.ApplyImpulse(vector4f17, this.RigidBodyAttachmentLocalPoint);
			}
		}

		public override void Solve()
		{
			if (base.Mass1.Sim.FrameIteration == 0)
			{
				this._oldSpringLength = this._springLength;
			}
			if (!Mathf.Approximately(this._targetSpringLength, this._springLength))
			{
				this._springLength = Mathf.Lerp(this._oldSpringLength, this._targetSpringLength, base.Mass1.Sim.FrameProgress);
			}
			MassToRigidBodySpring.SatisfyImpulseConstraintRigidBody(this, base.Mass1, this.RigidBody, this._springLength, this._frictionConstant4f, this.torsionalFrictionFactor, base.ImpulseThreshold2, false);
		}

		public Vector3 RigidBodyAttachmentLocalPoint;

		protected float _impulseVerticalFactor = 1f;

		protected RigidBody RigidBodyMass;

		protected TopWaterLure TopWaterLureMass;

		protected float torsionalFrictionFactor;
	}
}
