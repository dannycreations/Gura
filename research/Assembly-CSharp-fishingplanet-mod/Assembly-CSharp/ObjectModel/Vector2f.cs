using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ObjectModel
{
	[Serializable]
	public struct Vector2f
	{
		public Vector2f(float x0, float y0)
		{
			this.x = x0;
			this.y = y0;
		}

		public Vector2f(Vector3 v, bool ignoreHeight = true)
		{
			this.x = v.x;
			this.y = ((!ignoreHeight) ? v.y : v.z);
		}

		public static Vector2f operator *(float left, Vector2f right)
		{
			return new Vector2f(left * right.x, left * right.y);
		}

		public static Vector2f operator *(Vector2f left, float right)
		{
			return new Vector2f(left.x * right, left.y * right);
		}

		public static Vector2f operator +(Vector2f left, Vector2f right)
		{
			return new Vector2f(left.x + right.x, left.y + right.y);
		}

		public static Vector2f operator -(Vector2f left, Vector2f right)
		{
			return new Vector2f(left.x - right.x, left.y - right.y);
		}

		public static bool operator ==(Vector2f left, Vector2f right)
		{
			return (double)(left.x - right.x) < 1E-06 && (double)(left.y - right.y) < 1E-06;
		}

		public static bool operator !=(Vector2f left, Vector2f right)
		{
			return !(left == right);
		}

		[JsonIgnore]
		public float Magnitude
		{
			get
			{
				return (float)Math.Sqrt((double)(this.x * this.x + this.y * this.y));
			}
		}

		[JsonIgnore]
		public float SqrMagnitude
		{
			get
			{
				return this.x * this.x + this.y * this.y;
			}
		}

		public Vector2 ToVector2()
		{
			return new Vector2(this.x, this.y);
		}

		[JsonProperty]
		public float x;

		[JsonProperty]
		public float y;
	}
}
