using System;
using UnityEngine;

namespace Phy
{
	public class BasicLureTackleObject : TackleObject
	{
		public BasicLureTackleObject(PhyObjectType type, ConnectedBodiesSystem sim, TackleBehaviour tackle)
			: base(type, sim, tackle)
		{
		}

		public BasicLureTackleObject(ConnectedBodiesSystem sim, BasicLureTackleObject source)
			: base(sim, source)
		{
		}

		public override Mass HookMass
		{
			get
			{
				return base.Masses[2];
			}
		}

		public override Vector3 DirectionVector
		{
			get
			{
				return base.Masses[0].Position - base.Masses[1].Position;
			}
		}

		public override void RealingMasses()
		{
			Mass mass = base.Masses[0];
			Mass mass2 = base.Masses[1];
			Mass mass3 = base.Masses[2];
			Spring spring = base.Springs[1];
			Spring spring2 = base.Springs[2];
			mass2.Position = mass.Position + Vector3.down * spring.SpringLength;
			mass3.Position = mass2.Position + Vector3.down * spring2.SpringLength;
		}

		public override void SyncTransform(Transform transform, Transform topLineAnchor, Transform hookAnchor, Transform root)
		{
			base.SyncTransform(transform, topLineAnchor, hookAnchor, root);
			transform.rotation = Quaternion.FromToRotation(Vector3.forward, this.DirectionVector);
			transform.position += base.Tackle.Rod.PositionCorrection(base.Masses[0], true) - topLineAnchor.position;
		}

		public override void SyncTransformFrozen(Transform transform, Transform topLineAnchor, Transform hookAnchor, Transform root, Mass targetMass, Vector3 right)
		{
			base.SyncTransformFrozen(transform, topLineAnchor, hookAnchor, root, targetMass, right);
			LureBehaviour lureBehaviour = base.Tackle as LureBehaviour;
			if (lureBehaviour != null && lureBehaviour.BaitObject != null && base.Tackle.Fish != null)
			{
				lureBehaviour.SetForward(lureBehaviour.BaitObject, lureBehaviour.BaitTopAnchor.position + lureBehaviour.BaitShift, lureBehaviour.BaitTopAnchor.position - base.Tackle.Fish.ThroatPosition);
			}
		}

		public override Vector3 GetLineAnchorPoint(Transform topLineAnchor)
		{
			return topLineAnchor.position;
		}

		public override void Hitch(Vector3 position)
		{
			this.HookMass.IsKinematic = true;
			this.HookMass.StopMass();
			this.HookMass.Position = position;
		}

		public override void Unhitch()
		{
			this.HookMass.IsKinematic = false;
			this.HookMass.StopMass();
		}
	}
}
