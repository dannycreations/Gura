using System;

namespace Ovr
{
	public struct Matrix4f
	{
		internal Matrix4f(Matrix4f_Raw raw)
		{
			float[,] array = new float[4, 4];
			array[0, 0] = raw.m00;
			array[0, 1] = raw.m01;
			array[0, 2] = raw.m02;
			array[0, 3] = raw.m03;
			array[1, 0] = raw.m10;
			array[1, 1] = raw.m11;
			array[1, 2] = raw.m12;
			array[1, 3] = raw.m13;
			array[2, 0] = raw.m20;
			array[2, 1] = raw.m21;
			array[2, 2] = raw.m22;
			array[2, 3] = raw.m23;
			array[3, 0] = raw.m30;
			array[3, 1] = raw.m31;
			array[3, 2] = raw.m32;
			array[3, 3] = raw.m33;
			this.m = array;
		}

		public float[,] m;
	}
}
