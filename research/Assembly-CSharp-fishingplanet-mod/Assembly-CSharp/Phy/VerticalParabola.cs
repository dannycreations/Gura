using System;
using UnityEngine;

namespace Phy
{
	public class VerticalParabola
	{
		public VerticalParabola(Vector3 StartPoint, Vector3 FinishPoint, float midPointY)
		{
			this.StartPoint = StartPoint;
			this.FinishPoint = FinishPoint;
			this.MidPointY = midPointY;
			this.yaxis = Vector3.up;
			this.xaxis = (FinishPoint - StartPoint).normalized;
			this.xaxis -= this.yaxis * Vector3.Dot(this.xaxis, this.yaxis);
			this.xaxis.Normalize();
			this.normal = Vector3.Cross(this.xaxis, this.yaxis);
			this.normal.Normalize();
			this.pivot = StartPoint;
			Vector3 vector = (StartPoint + FinishPoint) * 0.5f;
			vector += this.yaxis * (midPointY - Vector3.Dot(vector, this.yaxis));
			Vector2 vector2 = this.SpaceToPlane(vector);
			Vector2 vector3 = this.SpaceToPlane(FinishPoint);
			this.x3 = vector3.x;
			float num = vector2.x * vector2.x;
			float x = vector2.x;
			float y = vector2.y;
			float num2 = -vector2.x * vector2.x + vector3.x * vector3.x;
			float num3 = -vector2.x + vector3.x;
			float num4 = -vector2.y + vector3.y;
			float num5 = -num3 / x;
			float num6 = num5 * num + num2;
			float num7 = num5 * y + num4;
			this.a = num7 / num6;
			this.b = (y - num * this.a) / x;
		}

		public VerticalParabola(VerticalParabola source)
		{
			this.Sync(source);
		}

		public Vector3 StartPoint { get; private set; }

		public Vector3 FinishPoint { get; private set; }

		public float MidPointY { get; private set; }

		public void Sync(VerticalParabola source)
		{
			this.a = source.a;
			this.b = source.b;
			this.x3 = source.x3;
			this.xaxis = source.xaxis;
			this.yaxis = source.yaxis;
			this.normal = source.normal;
			this.pivot = source.pivot;
		}

		public Vector3 GetPoint(float t)
		{
			float num = t * this.x3;
			float num2 = this.a * num * num + this.b * num;
			return this.PlaneToSpace(new Vector2(num, num2));
		}

		public float GetDerivative(float t)
		{
			float num = t * this.x3;
			return this.a * 2f * num + this.b;
		}

		private Vector2 SpaceToPlane(Vector3 point)
		{
			Vector3 vector = point - this.pivot;
			return new Vector2(Vector3.Dot(this.xaxis, vector), Vector3.Dot(this.yaxis, vector));
		}

		private Vector3 PlaneToSpace(Vector2 point)
		{
			return this.pivot + this.xaxis * point.x + this.yaxis * point.y;
		}

		public void DebugDraw(Color color)
		{
			Vector3 vector = this.GetPoint(0f);
			for (float num = 0.01f; num < 1f; num += 0.01f)
			{
				Vector3 point = this.GetPoint(num);
				Debug.DrawLine(vector, point, color, 10f);
				vector = point;
			}
		}

		private float a;

		private float b;

		private float x3;

		private Vector3 xaxis;

		private Vector3 yaxis;

		private Vector3 normal;

		private Vector3 pivot;
	}
}
