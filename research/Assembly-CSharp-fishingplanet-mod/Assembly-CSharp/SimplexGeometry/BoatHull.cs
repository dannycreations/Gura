using System;
using Mono.Simd;

namespace SimplexGeometry
{
	public abstract class BoatHull : SimplexComposite
	{
		public BoatHull()
		{
		}

		public BoatHull(float massValue)
			: base(massValue)
		{
		}

		public Vector4f BowPoint { get; protected set; }

		public Vector4f SternPoint { get; protected set; }
	}
}
