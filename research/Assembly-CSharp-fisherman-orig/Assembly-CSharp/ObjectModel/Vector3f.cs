using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ObjectModel
{
	public struct Vector3f
	{
		public Vector3f(float x0, float y0, float z0)
		{
			this.x = x0;
			this.y = y0;
			this.z = z0;
		}

		public Vector3f(Vector3 v)
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
		}

		[JsonIgnore]
		public static Vector3f Zero
		{
			get
			{
				return new Vector3f(0f, 0f, 0f);
			}
		}

		public static Vector3f operator +(Vector3f left, Vector3f right)
		{
			return new Vector3f(left.x + right.x, left.y + right.y, left.z + right.z);
		}

		public static Vector3f operator -(Vector3f left, Vector3f right)
		{
			return new Vector3f(left.x - right.x, left.y - right.y, left.z - right.z);
		}

		public static Vector3f operator *(float k, Vector3f right)
		{
			return new Vector3f(k * right.x, k * right.y, k * right.z);
		}

		public static Vector3f operator *(Vector3f left, float k)
		{
			return new Vector3f(k * left.x, k * left.y, k * left.z);
		}

		public static bool operator ==(Vector3f left, Vector3f right)
		{
			return (double)(left.x - right.x) < 1E-06 && (double)(left.y - right.y) < 1E-06 && (double)(left.z - right.z) < 1E-06;
		}

		public static bool operator !=(Vector3f left, Vector3f right)
		{
			return !(left == right);
		}

		[JsonIgnore]
		public float Magnitude
		{
			get
			{
				return (float)Math.Sqrt((double)(this.x * this.x + this.z * this.z + this.y * this.y));
			}
		}

		[JsonIgnore]
		public float SqrMagnitude
		{
			get
			{
				return this.x * this.x + this.z * this.z + this.y * this.y;
			}
		}

		public override string ToString()
		{
			return string.Format("({0:f3}, {1:f3}, {2:f3})", this.x, this.y, this.z);
		}

		public Vector3 ToVector3()
		{
			return new Vector3(this.x, this.y, this.z);
		}

		public float x;

		public float y;

		public float z;
	}
}
