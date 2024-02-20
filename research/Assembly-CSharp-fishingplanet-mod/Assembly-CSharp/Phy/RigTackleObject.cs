using System;
using ObjectModel;
using UnityEngine;

namespace Phy
{
	public class RigTackleObject : RigidBodyTackleObject
	{
		public RigTackleObject(PhyObjectType type, float normalMass, float hangingMass, float buoyancy, ConnectedBodiesSystem sim, TackleBehaviour tackle)
			: base(type, normalMass, hangingMass, buoyancy, sim, tackle)
		{
			this.hookMass = null;
			this.rigidBody = null;
			this.smallSinker = null;
		}

		public RigTackleObject(ConnectedBodiesSystem sim, RigTackleObject source)
			: base(sim, source)
		{
			this.hookMass = source.hookMass;
			this.rigidBody = source.rigidBody;
			this.smallSinker = source.smallSinker;
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
				return this.rigidBody ?? this.cachedRigidBody;
			}
		}

		public override Vector3 DirectionVector
		{
			get
			{
				return base.Masses[0].Position - base.Masses[1].Position;
			}
		}

		public override Vector3 GetLineAnchorPoint(Transform topLineAnchor)
		{
			return topLineAnchor.position;
		}

		public override void SyncTransform(Transform transform, Transform topLineAnchor, Transform hookAnchor, Transform root)
		{
			transform.rotation = Quaternion.FromToRotation(Vector3.forward, this.DirectionVector);
			transform.position += base.Tackle.Rod.PositionCorrection(base.Masses[0], true) - topLineAnchor.position;
			LureBehaviour lureBehaviour = base.Tackle as LureBehaviour;
			if (lureBehaviour != null)
			{
				if (lureBehaviour.RodTemplate != RodTemplate.TexasRig)
				{
					lureBehaviour.SinkerObject.transform.rotation = this.rigidBody.Rotation;
					lureBehaviour.SinkerObject.transform.position += base.Tackle.Rod.PositionCorrection(this.rigidBody, true) - base.Tackle.Sinker.center.position;
				}
				if (this.smallSinker != null)
				{
					lureBehaviour.SmallSinkerObject.transform.position = base.Tackle.Rod.PositionCorrection(this.smallSinker, true);
				}
			}
		}

		public override void SyncTransformFrozen(Transform transform, Transform topLineAnchor, Transform hookAnchor, Transform root, Mass targetMass, Vector3 right)
		{
			base.SyncTransformFrozen(transform, topLineAnchor, hookAnchor, root, targetMass, right);
			LureBehaviour lureBehaviour = base.Tackle as LureBehaviour;
			if (lureBehaviour != null)
			{
				if (lureBehaviour.RodTemplate != RodTemplate.TexasRig)
				{
					lureBehaviour.SinkerObject.transform.rotation = this.rigidBody.Rotation;
					lureBehaviour.SinkerObject.transform.position += base.Tackle.Rod.PositionCorrection(this.rigidBody, true) - base.Tackle.Sinker.center.position;
				}
				if (this.smallSinker != null)
				{
					lureBehaviour.SmallSinkerObject.transform.position = base.Tackle.Rod.PositionCorrection(this.smallSinker, true);
				}
				if (lureBehaviour.BaitObject != null && base.Tackle.Fish != null)
				{
					lureBehaviour.SetForward(lureBehaviour.BaitObject, lureBehaviour.BaitTopAnchor.position + lureBehaviour.BaitShift, lureBehaviour.BaitTopAnchor.position - base.Tackle.Fish.ThroatPosition);
				}
			}
		}

		public override void Hitch(Vector3 position)
		{
			Mass mass = this.rigidBody ?? this.hookMass;
			if (mass != null)
			{
				mass.IsKinematic = true;
				mass.StopMass();
				mass.Position = position;
			}
		}

		public override void Unhitch()
		{
			Mass mass = this.rigidBody ?? this.hookMass;
			if (mass != null)
			{
				mass.IsKinematic = false;
				mass.StopMass();
			}
		}

		public override void HookFish(float fishMass, float fishBuoyancy)
		{
		}

		public override void EscapeFish()
		{
		}

		public override void OnEnterHangingState()
		{
		}

		public override void OnExitHangingState()
		{
		}

		public Mass hookMass;

		public RigidBody rigidBody;

		public Mass smallSinker;
	}
}
