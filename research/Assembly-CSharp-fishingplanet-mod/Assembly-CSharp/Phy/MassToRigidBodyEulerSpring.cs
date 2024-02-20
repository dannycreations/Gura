using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class MassToRigidBodyEulerSpring : Spring
	{
		public MassToRigidBodyEulerSpring(Mass mass, RigidBody rigidbody, float springConstant, float springLength, float frictionConstant, Vector3 attachmentPoint)
			: base(mass, rigidbody, springConstant, springLength, frictionConstant)
		{
			this.RigidBodyAttachmentLocalPoint = attachmentPoint;
			this.RigidBodyMass = rigidbody;
		}

		public MassToRigidBodyEulerSpring(Simulation sim, MassToRigidBodyEulerSpring source)
			: base(sim, source)
		{
			this.RigidBodyAttachmentLocalPoint = source.RigidBodyAttachmentLocalPoint;
			this.RigidBodyMass = source.RigidBodyMass;
		}

		public RigidBody RigidBody
		{
			get
			{
				return this.RigidBodyMass;
			}
		}

		public override void UpdateReferences()
		{
			base.UpdateReferences();
			this.RigidBodyMass = base.Mass1.Sim.DictMasses[this.RigidBodyMass.UID] as RigidBody;
		}

		public Vector3 AttachmentPointWorld()
		{
			return this.RigidBodyMass.LocalToWorld(this.RigidBodyAttachmentLocalPoint);
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
			Vector4f vector4f = base.Mass1.Position4f - this.RigidBodyMass.LocalToWorld4f(this.RigidBodyAttachmentLocalPoint);
			float num = vector4f.Magnitude();
			float num2 = num - base.SpringLength;
			this.DeltaLength = num2;
			if (num < base.SpringLength || num < 0.001f)
			{
				this.CurrentForceMagnitude = 0f;
				return;
			}
			Vector4f vector4f2 = Vector4f.Zero;
			vector4f2 += vector4f.Normalized() * new Vector4f(this.DeltaLength * -base.SpringConstant);
			vector4f2 -= (base.Mass1.Velocity4f - this.RigidBodyMass.PointWorldVelocity4f(this.RigidBodyAttachmentLocalPoint)) * this._frictionConstant4f;
			base.Mass1.ApplyForce(vector4f2, base.Mass1.IsRef);
			this.RigidBodyMass.ApplyForce(vector4f2.Negative(), this.RigidBodyAttachmentLocalPoint, false);
			this.CurrentForceMagnitude = vector4f2.Magnitude();
			if (base.MaxForce < this.CurrentForceMagnitude)
			{
				base.MaxForce = this.CurrentForceMagnitude;
			}
		}

		public Vector3 RigidBodyAttachmentLocalPoint;

		protected RigidBody RigidBodyMass;
	}
}
