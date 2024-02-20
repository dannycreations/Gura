using System;
using UnityEngine;

namespace frame8.Logic.Misc.Other.Extensions
{
	public static class StructsInfinityValuesExtensions
	{
		public static Vector2 SetToInfinity(this Vector2 val)
		{
			val.x = (val.y = float.PositiveInfinity);
			return val;
		}

		public static Vector2 SetToNegativeInfinity(this Vector2 val)
		{
			val.x = (val.y = float.NegativeInfinity);
			return val;
		}

		public static Vector3 SetToInfinity(this Vector3 val)
		{
			val.x = (val.y = (val.z = float.PositiveInfinity));
			return val;
		}

		public static Vector3 SetToNegativeInfinity(this Vector3 val)
		{
			val.x = (val.y = (val.z = float.NegativeInfinity));
			return val;
		}

		public static Vector4 SetToInfinity(this Vector4 val)
		{
			val.x = (val.y = (val.z = (val.w = float.PositiveInfinity)));
			return val;
		}

		public static Vector4 SetToNegativeInfinity(this Vector4 val)
		{
			val.x = (val.y = (val.z = (val.w = float.NegativeInfinity)));
			return val;
		}

		public static Bounds SetToInfinity(this Bounds val)
		{
			Vector3 vector = default(Vector3).SetToInfinity();
			val.size = vector;
			val.center = vector;
			return val;
		}

		public static Bounds SetToNegativeInfinity(this Bounds val)
		{
			Vector3 vector = default(Vector3).SetToNegativeInfinity();
			val.size = vector;
			val.center = vector;
			return val;
		}

		public static Rect SetToInfinity(this Rect val)
		{
			float num = float.PositiveInfinity;
			val.height = num;
			num = num;
			val.width = num;
			num = num;
			val.y = num;
			val.x = num;
			return val;
		}

		public static Rect SetToNegativeInfinity(this Rect val)
		{
			float num = float.NegativeInfinity;
			val.height = num;
			num = num;
			val.width = num;
			num = num;
			val.y = num;
			val.x = num;
			return val;
		}
	}
}
