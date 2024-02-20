using System;
using UnityEngine;

namespace Phy
{
	public class BezierCurveWithTorsion : BezierCurve
	{
		public BezierCurveWithTorsion(int order = 2)
			: base(order)
		{
			this.RightAxis = new Vector3[base.Order];
		}

		public static float TriangleWindow(int c, float x)
		{
			return Mathf.Max(1f - Mathf.Abs(x - (float)c), 0f);
		}

		public override Vector3 CurvedCylinderTransform(Vector3 point)
		{
			Vector3 normalized = base.Derivative().normalized;
			Vector3 normalized2 = this.GetRightAxis().normalized;
			Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
			Vector3 vector = normalized2 * point.x * this.LateralScale.x + normalized3 * point.y * this.LateralScale.y;
			return base.Point() + vector;
		}

		public Quaternion CurvedCylinderRotation()
		{
			Vector3 normalized = base.Derivative().normalized;
			Vector3 normalized2 = this.GetRightAxis().normalized;
			Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
			float magnitude = normalized2.magnitude;
			float magnitude2 = normalized3.magnitude;
			if (!Mathf.Approximately(magnitude, 0f) && !Mathf.Approximately(magnitude2, 0f))
			{
				return Quaternion.LookRotation(normalized, normalized3);
			}
			return Quaternion.identity;
		}

		public Quaternion CurvedCylinderTransformRotation(Quaternion rotation)
		{
			Vector3 normalized = base.Derivative().normalized;
			Vector3 normalized2 = this.GetRightAxis().normalized;
			Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
			float magnitude = normalized2.magnitude;
			float magnitude2 = normalized3.magnitude;
			if (!Mathf.Approximately(magnitude, 0f) && !Mathf.Approximately(magnitude2, 0f))
			{
				return Quaternion.LookRotation(normalized, normalized3) * rotation;
			}
			return rotation;
		}

		public Vector3 GetRightAxis()
		{
			float num = this.tk[1] * (float)(base.Order - 1);
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < base.Order; i++)
			{
				vector += this.RightAxis[i] * BezierCurveWithTorsion.TriangleWindow(i, num);
			}
			return vector;
		}

		public Vector3[] RightAxis;

		public Vector2 LateralScale = Vector2.one;
	}
}
