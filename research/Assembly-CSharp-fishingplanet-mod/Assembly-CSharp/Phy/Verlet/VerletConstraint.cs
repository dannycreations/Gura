using System;

namespace Phy.Verlet
{
	public abstract class VerletConstraint : ConnectionBase
	{
		public VerletConstraint(VerletMass mass1, VerletMass mass2, int forceUID = -1)
			: base(mass1, mass2, forceUID)
		{
		}

		public virtual int SatisfyIterations
		{
			get
			{
				return 1;
			}
		}

		public abstract void Satisfy();
	}
}
