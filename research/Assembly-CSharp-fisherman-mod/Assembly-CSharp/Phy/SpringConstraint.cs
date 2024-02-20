using System;
using UnityEngine;

namespace Phy
{
	public class SpringConstraint : ConstraintBase
	{
		public SpringConstraint(Mass mass, float distance)
			: base(mass)
		{
			this.MaxDistance = distance;
		}

		public override void Apply(Mass mass)
		{
			Vector3 vector = this.ReferenceMass.Position - mass.Position;
			float magnitude = vector.magnitude;
			if (magnitude > this.MaxDistance)
			{
				mass.Position += vector.normalized * (magnitude - this.MaxDistance);
			}
		}

		public readonly float MaxDistance;
	}
}
