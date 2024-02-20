using System;
using UnityEngine;

namespace Phy
{
	[Obsolete]
	public class BendConstraint : ConstraintBase
	{
		public BendConstraint(Mass mass, float distance)
			: base(mass)
		{
			this.FixedDistance = distance;
		}

		public override void Apply(Mass mass)
		{
			Vector3 vector = this.ReferenceMass.Position - mass.Position;
			float magnitude = vector.magnitude;
			if (magnitude != this.FixedDistance)
			{
				mass.Position += vector.normalized * (magnitude - this.FixedDistance);
			}
		}

		public readonly float FixedDistance;
	}
}
