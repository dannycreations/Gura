using System;
using UnityEngine;

namespace Phy
{
	public abstract class TackleObject : SpringDrivenObject
	{
		protected TackleObject(PhyObjectType type, ConnectedBodiesSystem sim, TackleBehaviour tackle)
			: base(type, sim, tackle)
		{
		}

		protected TackleObject(ConnectedBodiesSystem sim, TackleObject source)
			: base(sim, source)
		{
		}

		public abstract Mass HookMass { get; }

		public abstract Vector3 DirectionVector { get; }

		public virtual void SyncTransform(Transform transform, Transform topLineAnchor, Transform hookAnchor, Transform root)
		{
			base.SyncSwivel();
		}

		public virtual void SyncTransformFrozen(Transform transform, Transform topLineAnchor, Transform hookAnchor, Transform root, Mass targetMass, Vector3 right)
		{
			Vector3 vector = base.Tackle.Rod.PositionCorrection(targetMass, base.Tackle.IsShowingComplete);
			Vector3 vector2 = base.Tackle.Rod.PositionCorrection(base.Tackle.KinematicHandLineMass, base.Tackle.IsShowingComplete);
			Vector3 normalized = (vector2 - vector).normalized;
			transform.rotation = Quaternion.LookRotation(normalized, right);
			transform.rotation = Quaternion.FromToRotation((topLineAnchor.position - hookAnchor.position).normalized, normalized) * transform.rotation;
			Vector3 vector3 = vector - hookAnchor.position;
			transform.position += vector3;
			base.SyncSwivel();
		}

		public abstract void RealingMasses();

		public abstract Vector3 GetLineAnchorPoint(Transform topLineAnchor);

		public abstract void Hitch(Vector3 position);

		public abstract void Unhitch();

		public virtual void HookFish(float fishMass, float fishBuoyancy)
		{
		}

		public virtual void EscapeFish()
		{
		}

		public virtual void OnEnterHangingState()
		{
		}

		public virtual void OnExitHangingState()
		{
		}

		public virtual void OnEnterWater()
		{
		}

		public virtual void OnLeaveWater()
		{
		}
	}
}
