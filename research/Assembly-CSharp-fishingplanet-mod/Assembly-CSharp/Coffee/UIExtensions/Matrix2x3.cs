using System;
using UnityEngine;

namespace Coffee.UIExtensions
{
	internal struct Matrix2x3
	{
		public Matrix2x3(Rect rect, float cos, float sin)
		{
			float num = -rect.xMin / rect.width - 0.5f;
			float num2 = -rect.yMin / rect.height - 0.5f;
			this.m00 = cos / rect.width;
			this.m01 = -sin / rect.height;
			this.m02 = num * cos - num2 * sin + 0.5f;
			this.m10 = sin / rect.width;
			this.m11 = cos / rect.height;
			this.m12 = num * sin + num2 * cos + 0.5f;
		}

		public static Vector2 operator *(Matrix2x3 m, Vector2 v)
		{
			return new Vector2(m.m00 * v.x + m.m01 * v.y + m.m02, m.m10 * v.x + m.m11 * v.y + m.m12);
		}

		public float m00;

		public float m01;

		public float m02;

		public float m10;

		public float m11;

		public float m12;
	}
}
