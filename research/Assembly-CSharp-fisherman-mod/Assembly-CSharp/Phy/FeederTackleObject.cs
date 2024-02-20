using System;
using UnityEngine;

namespace Phy
{
	public class FeederTackleObject : RigidBodyTackleObject
	{
		public FeederTackleObject(PhyObjectType type, float normalMass, float hangingMass, float buoyancy, ConnectedBodiesSystem sim, TackleBehaviour tackle)
			: base(type, normalMass, hangingMass, buoyancy, sim, tackle)
		{
			this.hookMass = null;
			this.rigidBody = null;
		}

		public FeederTackleObject(ConnectedBodiesSystem sim, FeederTackleObject source)
			: base(sim, source)
		{
			this.hookMass = source.hookMass;
			this.rigidBody = source.rigidBody;
		}

		public override Mass HookMass
		{
			get
			{
				return this.hookMass;
			}
		}

		public override RigidBody RigidBody
		{
			get
			{
				RigidBody rigidBody;
				if ((rigidBody = this.rigidBody) == null)
				{
					rigidBody = (base.Masses[1] as RigidBody) ?? this.cachedRigidBody;
				}
				return rigidBody;
			}
		}

		public virtual bool FeederDissolved { get; protected set; }

		public override void SyncTransform(Transform transform, Transform topLineAnchor, Transform hookAnchor, Transform root)
		{
			transform.rotation = this.RigidBody.Rotation;
			transform.position += base.Tackle.Rod.PositionCorrection(this.RigidBody, true) - root.position;
			base.SyncSwivel();
		}

		public override void Sync(PhyObject source)
		{
			base.Sync(source);
		}

		public virtual void OnSetFilled(bool isFilled)
		{
		}

		public Mass hookMass;

		public RigidBody rigidBody;
	}
}
