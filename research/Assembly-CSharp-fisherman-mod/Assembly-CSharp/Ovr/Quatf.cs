using System;

namespace Ovr
{
	public struct Quatf
	{
		public Quatf(float _x, float _y, float _z, float _w)
		{
			this.x = _x;
			this.y = _y;
			this.z = _z;
			this.w = _w;
		}

		public float x;

		public float y;

		public float z;

		public float w;
	}
}
