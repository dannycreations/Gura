using System;
using Phy;

namespace Boats
{
	public interface IBoatWithEngine
	{
		bool GlidingBoat { get; }

		float GlidingOnSpeed { get; }

		float GlidingOffSpeed { get; }

		float GlidingAcceleration { get; }

		float BoatVelocity { get; }

		FloatingSimplexComposite Phyboat { get; }
	}
}
