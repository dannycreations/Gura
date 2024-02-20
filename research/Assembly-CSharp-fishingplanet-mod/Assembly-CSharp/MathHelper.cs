using System;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
	public static Vector3 GetBezQuadPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		Vector3 vector = Vector3.Lerp(p0, p1, t);
		Vector3 vector2 = Vector3.Lerp(p1, p2, t);
		return Vector3.Lerp(vector, vector2, t);
	}

	public static float CorrectFloat(float value)
	{
		if (float.IsNaN(value))
		{
			return 0f;
		}
		if (float.IsPositiveInfinity(value))
		{
			return float.MaxValue;
		}
		if (float.IsNegativeInfinity(value))
		{
			return float.MinValue;
		}
		return value;
	}

	public static float Square(float value)
	{
		return value * value;
	}

	public static float Hypot(float x, float y)
	{
		return Mathf.Sqrt(x * x + y * y);
	}

	public static float Hypot(float x, float y, float z)
	{
		return Mathf.Sqrt(x * x + y * y + z * z);
	}

	public static int Fact(int n)
	{
		int num = 1;
		for (int i = 2; i <= n; i++)
		{
			num *= i;
		}
		return num;
	}

	public static float FloatArrayLerp(IList<float> array, float pos)
	{
		if (array.Count == 0)
		{
			return 0f;
		}
		if (pos <= 0f || array.Count == 1)
		{
			return array[0];
		}
		if (pos < 1f)
		{
			float num = pos * (float)(array.Count - 1);
			float num2 = Mathf.Floor(num);
			int num3 = (int)num;
			return Mathf.Lerp(array[num3], array[num3 + 1], num - num2);
		}
		return array[array.Count - 1];
	}

	public static float PiecewiseLinear(Vector2[] points, float x)
	{
		if (x < points[0].x)
		{
			return points[0].y;
		}
		int num = points.Length;
		if (x > points[num - 1].x)
		{
			return points[num - 1].y;
		}
		for (int i = 1; i < num; i++)
		{
			if (x < points[i].x)
			{
				return Mathf.Lerp(points[i - 1].y, points[i].y, (x - points[i - 1].x) / (points[i].x - points[i - 1].x));
			}
		}
		throw new ArgumentException("Points list of piecewise linear function must be ordered by ascending X coordinate.");
	}

	public static float InterpolateTriangular(float nxny, float pxny, float pxpy, float nxpy, float dx, float dy)
	{
		if (dx < dy)
		{
			return nxny + (pxpy - nxpy) * dx + (nxpy - nxny) * dy;
		}
		return nxny + (pxny - nxny) * dx + (pxpy - pxny) * dy;
	}
}
