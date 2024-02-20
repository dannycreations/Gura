using System;

namespace Phy.Verlet
{
	public class HingeVerletSpring : VerletSpring
	{
		public HingeVerletSpring(VerletMass mass1, VerletMass mass2, float length, float friction, bool compressible = false, float xLimit = 0f, float yLimit = 0f, float zLimit = 0f, Mass headMass = null)
			: base(mass1, mass2, length, friction, compressible)
		{
		}

		public HingeVerletSpring(Simulation sim, VerletSpring source)
			: base(sim, source)
		{
		}

		public override string ToString()
		{
			return string.Format("HingeVerletSpring {0} Len:{1}, Fric:{2}, Compressible:{3})", new object[]
			{
				base.ToString(),
				this.length,
				this.friction,
				(!this.compressible) ? "N" : "Y"
			});
		}
	}
}
