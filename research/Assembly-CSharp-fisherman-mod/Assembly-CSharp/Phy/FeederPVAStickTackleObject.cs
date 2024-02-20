using System;
using UnityEngine;

namespace Phy
{
	public class FeederPVAStickTackleObject : FeederTackleObject
	{
		public FeederPVAStickTackleObject(PhyObjectType type, float normalMass, float hangingMass, float buoyancy, ConnectedBodiesSystem sim, TackleBehaviour tackle)
			: base(type, normalMass, hangingMass, buoyancy, sim, tackle)
		{
		}

		public override void HookFish(float fishMass, float fishBuoyancy)
		{
		}

		public override void EscapeFish()
		{
		}

		public override void SyncTransform(Transform transform, Transform topLineAnchor, Transform hookAnchor, Transform root)
		{
			base.SyncTransform(transform, topLineAnchor, hookAnchor, root);
		}

		public override void OnSetFilled(bool isFilled)
		{
			base.OnSetFilled(isFilled);
			if (!isFilled && this.stickBody != null)
			{
				this.Sim.RemoveConnection(this.leaderToStickConnection);
				this.Sim.RemoveConnection(this.stickToHookConnection);
				this.Sim.RemoveMass(this.stickBody);
				Spring spring = new Spring(this.leaderToStickConnection.Mass1, this.stickToHookConnection.Mass1, 0f, this.leaderToStickConnection.SpringLength + this.stickToHookConnection.SpringLength + Mathf.Abs(this.leaderToStickConnection.RigidBodyAttachmentLocalPoint.z - this.stickToHookConnection.RigidBodyAttachmentLocalPoint.z), 0.002f);
				this.Sim.Connections.Add(spring);
				this.leaderToStickConnection = null;
				this.stickToHookConnection = null;
				this.stickBody = null;
				this.stickObject = null;
				this.FeederDissolved = true;
			}
		}

		public MassToRigidBodySpring leaderToStickConnection;

		public MassToRigidBodySpring stickToHookConnection;

		public RigidBody stickBody;

		public GameObject stickObject;

		public Transform stickRoot;
	}
}
