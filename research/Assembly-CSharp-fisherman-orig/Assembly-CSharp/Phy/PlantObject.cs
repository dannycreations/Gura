using System;
using UnityEngine;

namespace Phy
{
	public class PlantObject : UnderwaterItemObject
	{
		public PlantObject(ConnectedBodiesSystem sim = null)
			: base(PhyObjectType.Plant, sim)
		{
		}

		public PlantObject(ConnectedBodiesSystem sim, PlantObject source)
			: base(sim, source)
		{
		}

		public override void Simulate(float dt)
		{
			if (this.ForHookMass != null && this.RodTipMass != null)
			{
				Vector3 vector = this.ForHookMass._rotation * this.RootDirection;
				Vector3 normalized = (this.ForHookMass.Position - this.RodTipMass.Position).normalized;
				this.ForHookMass._rotation = Quaternion.FromToRotation(vector, normalized) * this.ForHookMass._rotation;
			}
		}
	}
}
