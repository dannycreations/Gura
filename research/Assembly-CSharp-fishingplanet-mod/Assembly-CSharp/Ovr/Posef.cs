using System;

namespace Ovr
{
	public struct Posef
	{
		public Posef(Quatf q, Vector3f p)
		{
			this.Orientation = q;
			this.Position = p;
		}

		public Quatf Orientation;

		public Vector3f Position;
	}
}
