using System;
using UnityEngine;

namespace Phy
{
	public class FeederPVABagTackleObject : FeederTackleObject
	{
		public FeederPVABagTackleObject(PhyObjectType type, float normalMass, float hangingMass, float buoyancy, ConnectedBodiesSystem sim, TackleBehaviour tackle)
			: base(type, normalMass, hangingMass, buoyancy, sim, tackle)
		{
		}

		public override void SyncTransform(Transform transform, Transform topLineAnchor, Transform hookAnchor, Transform root)
		{
			base.SyncTransform(transform, topLineAnchor, hookAnchor, root);
			if (this.bagObject != null)
			{
				this.bagObject.transform.rotation = this.RigidBody.Rotation;
				this.bagObject.transform.position += base.Tackle.Rod.PositionCorrection(this.RigidBody, true) - this.bagRoot.position;
			}
		}

		public override void OnSetFilled(bool isFilled)
		{
			base.OnSetFilled(isFilled);
			if (!isFilled && this.bagObject != null)
			{
				this.Sim.RemoveConnection(this.leaderToBagConnection);
				this.Sim.RemoveMass(this.RigidBody);
				(this.Sim as FishingRodSimulation).AddCarpClassic(this.tackleSize, this.hookSize, this.baitMass, this.baitBuoyancy, this.tackleInitDirection, this.controller, this);
				(this.controller.Behaviour as Feeder1stBehaviour).RefreshObjectsFromSim();
				(base.Tackle.RodSlot.Line as Line1stBehaviour).RefreshObjectsFromSim();
				this.leaderToBagConnection = null;
				this.bagRoot = null;
				this.bagObject = null;
				this.FeederDissolved = true;
			}
		}

		public GameObject bagObject;

		public MassToRigidBodySpring leaderToBagConnection;

		public Transform bagRoot;

		public float tackleSize;

		public float hookSize;

		public float baitMass;

		public float baitBuoyancy;

		public Vector3 tackleInitDirection;

		public FeederController controller;
	}
}
