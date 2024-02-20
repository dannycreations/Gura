using System;
using Mono.Simd;

namespace Phy
{
	public class SpringConstraintVelocity : ConstraintBase
	{
		public SpringConstraintVelocity(Mass mass, float distance, bool enabledDebugPlotter = false)
			: base(mass)
		{
			this.MaxDistance = distance;
			this.maxDistance4f = new Vector4f(this.MaxDistance);
		}

		public override void Apply(Mass mass)
		{
		}

		public readonly float MaxDistance;

		private Vector4f maxDistance4f;
	}
}
