using System;
using UnityEngine;

namespace Phy
{
	public static class GpuExtensions
	{
		public static Vector4 ToVector4(this Quaternion q)
		{
			return new Vector4(q.x, q.y, q.z, q.w);
		}

		public static Vector3 ToVector3(this Color c)
		{
			return new Vector3(c.r, c.g, c.b);
		}

		public static Quaternion ToQuaternion(this Color c)
		{
			return new Quaternion(c.r, c.g, c.b, c.a);
		}

		public static readonly Color Down = new Color(0f, -1f, 0f);

		public static readonly Color Up = new Color(0f, 1f, 0f);

		public static readonly Color Zero = new Color(0f, 0f, 0f);
	}
}
