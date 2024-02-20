using System;
using UnityEngine;

namespace ObjectModel
{
	public class Box
	{
		public Box(Point3 position, Point3 scale, Point3 rotation)
		{
			this.Position = position;
			this.Scale = scale;
			this.Rotation = rotation;
		}

		public Point3 Position { get; set; }

		public Point3 Scale { get; set; }

		public Point3 Rotation
		{
			get
			{
				return this.rotation;
			}
			set
			{
				this.rotation = value;
				this.rotationMatrix = null;
			}
		}

		protected void EnsureRotationMatrix()
		{
			if (this.rotationMatrix == null)
			{
				this.CalculateRotationMatrix();
			}
		}

		private void CalculateRotationMatrix()
		{
			if (this.Rotation == null)
			{
				return;
			}
			float num = Box.DegreesToRadians(-this.Rotation.X);
			float num2 = Box.DegreesToRadians(-this.Rotation.Y);
			float num3 = Box.DegreesToRadians(-this.Rotation.Z);
			float[,] array = new float[3, 3];
			array[0, 0] = 1f;
			array[0, 1] = 0f;
			array[0, 2] = 0f;
			array[1, 0] = 0f;
			array[1, 1] = Mathf.Cos(num);
			array[1, 2] = -Mathf.Sin(num);
			array[2, 0] = 0f;
			array[2, 1] = Mathf.Sin(num);
			array[2, 2] = Mathf.Cos(num);
			float[,] array2 = new float[3, 3];
			array2[0, 0] = Mathf.Cos(num2);
			array2[0, 1] = 0f;
			array2[0, 2] = Mathf.Sin(num2);
			array2[1, 0] = 0f;
			array2[1, 1] = 1f;
			array2[1, 2] = 0f;
			array2[2, 0] = -Mathf.Sin(num2);
			array2[2, 1] = 0f;
			array2[2, 2] = Mathf.Cos(num2);
			float[,] array3 = new float[3, 3];
			array3[0, 0] = Mathf.Cos(num3);
			array3[0, 1] = -Mathf.Sin(num3);
			array3[0, 2] = 0f;
			array3[1, 0] = Mathf.Sin(num3);
			array3[1, 1] = Mathf.Cos(num3);
			array3[1, 2] = 0f;
			array3[2, 0] = 0f;
			array3[2, 1] = 0f;
			array3[2, 2] = 1f;
			this.rotationMatrix = Box.MxMultiply(Box.MxMultiply(array, array2), array3);
		}

		public Point3 TransformCoordinates(Point3 point)
		{
			Point3 point2 = new Point3(point.X - this.Position.X, point.Y - this.Position.Y, point.Z - this.Position.Z);
			if (this.Rotation != null && (Math.Abs(this.Rotation.X) > 0.0001f || Math.Abs(this.Rotation.Y) > 0.0001f || Math.Abs(this.Rotation.Z) > 0.0001f))
			{
				return this.RotationTransform(point2);
			}
			return point2;
		}

		private Point3 RotationTransform(Point3 point)
		{
			return new Point3(point.X * this.rotationMatrix[0, 0] + point.Y * this.rotationMatrix[0, 1] + point.Z * this.rotationMatrix[0, 2], point.X * this.rotationMatrix[1, 0] + point.Y * this.rotationMatrix[1, 1] + point.Z * this.rotationMatrix[1, 2], point.X * this.rotationMatrix[2, 0] + point.Y * this.rotationMatrix[2, 1] + point.Z * this.rotationMatrix[2, 2]);
		}

		public bool Contains(Point3 point)
		{
			if (this.Position == null || this.Scale == null || this.Rotation == null)
			{
				return false;
			}
			this.EnsureRotationMatrix();
			Point3 point2 = this.TransformCoordinates(point);
			return Math.Abs(point2.X) <= this.Scale.X / 2f && Math.Abs(point2.Y) <= this.Scale.Y / 2f && Math.Abs(point2.Z) <= this.Scale.Z / 2f;
		}

		public float Distance(Point3 point)
		{
			if (this.Position == null || this.Scale == null || this.Rotation == null)
			{
				return 0f;
			}
			this.EnsureRotationMatrix();
			Point3 point2 = this.TransformCoordinates(point);
			Point3 point3 = new Point3(Mathf.Max(Mathf.Abs(point2.X) - this.Scale.X / 2f, 0f), Mathf.Max(Mathf.Abs(point2.Y) - this.Scale.Y / 2f, 0f), Mathf.Max(Mathf.Abs(point2.Z) - this.Scale.Z / 2f, 0f));
			return point3.Magnitude;
		}

		private static float DegreesToRadians(float angle)
		{
			return 0.017453292f * angle;
		}

		private static float[,] MxMultiply(float[,] m1, float[,] m2)
		{
			float[,] array = new float[3, 3];
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					array[i, j] = 0f;
					for (int k = 0; k < 3; k++)
					{
						array[i, j] += m1[i, k] * m2[k, j];
					}
				}
			}
			return array;
		}

		private const float ZeroTollerance = 0.0001f;

		private Point3 rotation;

		private float[,] rotationMatrix;
	}
}
