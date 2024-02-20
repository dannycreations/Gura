using System;
using UnityEngine;

namespace ObjectModel
{
	public class Point3
	{
		public Point3()
		{
		}

		public Point3(float x, float y, float z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

		public Point3(Point3 point)
		{
			this.X = point.X;
			this.Y = point.Y;
			this.Z = point.Z;
		}

		[JsonConfig]
		public float X { get; set; }

		[JsonConfig]
		public float Y { get; set; }

		[JsonConfig]
		public float Z { get; set; }

		public Vector3f ToVector3f()
		{
			return new Vector3f(this.X, this.Y, this.Z);
		}

		public float Distance(Point3 point)
		{
			float num = point.X - this.X;
			float num2 = point.Y - this.Y;
			float num3 = point.Z - this.Z;
			return Mathf.Sqrt(num * num + num2 * num2 + num3 * num3);
		}

		public float Magnitude
		{
			get
			{
				return Mathf.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
			}
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (obj.GetType() == base.GetType() && this.Equals((Point3)obj)));
		}

		protected bool Equals(Point3 other)
		{
			return this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Z.Equals(other.Z);
		}

		public override int GetHashCode()
		{
			int num = this.X.GetHashCode();
			num = (num * 397) ^ this.Y.GetHashCode();
			return (num * 397) ^ this.Z.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("({0},{1},{2})", this.X, this.Y, this.Z);
		}

		public Point2 ToFlat()
		{
			return new Point2
			{
				X = this.X,
				Y = this.Z
			};
		}
	}
}
