using System;

namespace Phy
{
	public abstract class ConstraintBase
	{
		protected ConstraintBase(Mass mass)
		{
			this.ReferenceMass = mass;
		}

		public abstract void Apply(Mass mass);

		public Mass ReferenceMass;
	}
}
