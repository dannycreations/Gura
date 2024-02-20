using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class PointOnRigidBody : Mass
	{
		public PointOnRigidBody(Simulation sim, RigidBody rigidBody, Vector3 localPosition, Mass.MassType type)
			: base(sim, 0f, rigidBody.LocalToWorld(localPosition), type)
		{
			this.RigidBody = rigidBody;
			this.LocalPosition = localPosition;
		}

		public PointOnRigidBody(Simulation sim, PointOnRigidBody source)
			: base(sim, source)
		{
			this.RigidBody = source.RigidBody;
			this.LocalPosition = source.LocalPosition;
		}

		public Vector3 LocalPosition
		{
			get
			{
				return this._localPosition;
			}
			set
			{
				this._localPosition = value;
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.LocalPositionChanged(base.UID, value);
				}
			}
		}

		public override void UpdateReferences()
		{
			this.RigidBody = this.Sim.DictMasses[this.RigidBody.UID] as RigidBody;
		}

		public override void ApplyForce(Vector3 force, bool capture = false)
		{
			this.ApplyForce(force.AsPhyVector(ref this._force4f), capture);
		}

		public override void ApplyForce(Vector4f force, bool capture = false)
		{
			this.RigidBody.ApplyForce(force, this.LocalPosition, capture);
			this._force4f += force;
			if (capture)
			{
				base.UpdateAvgForce(force);
			}
		}

		public override void Simulate()
		{
			this.Position4f = this.RigidBody.LocalToWorld4f(this.LocalPosition);
			this.Velocity4f = this.RigidBody.PointWorldVelocity4f(this.LocalPosition);
			this.groundPoint = this.RigidBody.GroundPoint4f;
			this.groundNormal = this.RigidBody.GroundNormal4f;
		}

		public RigidBody RigidBody;

		private Vector3 _localPosition;
	}
}
