using System;
using UnityEngine;

namespace Mono.Simd.Math
{
	public static class Vector4fExtensions
	{
		public static void CheckNaN(Vector4f v, Vector4f v_src)
		{
			if (float.IsNaN(v.X) || float.IsNaN(v.X) || float.IsNaN(v.X) || float.IsNaN(v.X))
			{
				Debug.LogError("CheckNan " + v_src);
			}
		}

		public static bool CheckInvalid(Vector4f v)
		{
			return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.X) || float.IsNaN(v.X) || float.IsInfinity(v.X) || float.IsInfinity(v.Y) || float.IsInfinity(v.X) || float.IsInfinity(v.X);
		}

		public static bool CheckInvalid(Vector3 v)
		{
			return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);
		}

		public static bool CheckInvalid(float x)
		{
			return float.IsNaN(x) || float.IsInfinity(x);
		}

		public static Vector4f FromVector3(Vector3 v)
		{
			return new Vector4f(v.x, v.y, v.z, 0f);
		}

		public static Vector3 AsVector3(this Vector4f vec)
		{
			return new Vector3(vec.X, vec.Y, vec.Z);
		}

		public static void Negate(this Vector4f vec)
		{
			vec *= Vector4f.MinusOne;
		}

		public static Vector4f Negative(this Vector4f vec)
		{
			return vec * Vector4f.MinusOne;
		}

		public static void Normalize(this Vector4f vec)
		{
			if (vec.SqrMagnitude() <= 1E-10f)
			{
				vec = Vector4f.Zero;
			}
			Vector4f vector4f = vec * vec;
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			vector4f = VectorOperations.InvSqrt(vector4f);
			vec *= vector4f;
		}

		public static Vector4f Normalized(this Vector4f vec)
		{
			if (vec.SqrMagnitude() <= 1E-10f)
			{
				return Vector4f.Zero;
			}
			Vector4f vector4f = vec * vec;
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			vector4f = VectorOperations.InvSqrt(vector4f);
			return vec * vector4f;
		}

		public static float Magnitude(this Vector4f vec)
		{
			Vector4f vector4f = vec * vec;
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			return VectorOperations.Sqrt(vector4f).X;
		}

		public static float SqrMagnitude(this Vector4f vec)
		{
			Vector4f vector4f = vec * vec;
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			return VectorOperations.HorizontalAdd(vector4f, vector4f).X;
		}

		public static void Decompose(this Vector4f vec, out Vector4f normalized, out float magnitude)
		{
			Vector4f vector4f = vec * vec;
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			magnitude = VectorOperations.Sqrt(vector4f).X;
			normalized = vec * new Vector4f(1f / magnitude);
		}

		public static float Decompose(this Vector4f vec)
		{
			Vector4f vector4f = vec * vec;
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			float x = VectorOperations.Sqrt(vector4f).X;
			vec *= new Vector4f(1f / x);
			return x;
		}

		public static float Distance(this Vector4f vec, Vector4f other)
		{
			return (vec - other).Magnitude();
		}

		public static float LengthSquared(this Vector4f vec)
		{
			Vector4f vector4f = vec * vec;
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			return VectorOperations.HorizontalAdd(vector4f, vector4f).X;
		}

		public static bool ApproxEquals(this Vector4f vec, Vector4f vector, float tolerance)
		{
			Vector4f vector4f = vec - vector;
			return vector4f.Magnitude() <= tolerance;
		}

		public static bool Approximately(this Vector4f vec, Vector4f other)
		{
			return Mathf.Approximately(vec.X, other.X) && Mathf.Approximately(vec.Y, other.Y) && Mathf.Approximately(vec.Z, other.Z) && Mathf.Approximately(vec.W, other.W);
		}

		public static bool IsFinite(this Vector4f vec)
		{
			return Utils.IsFinite(vec.X) && Utils.IsFinite(vec.Y) && Utils.IsFinite(vec.Z) && Utils.IsFinite(vec.W);
		}

		public static bool IsZero(this Vector4f vec)
		{
			return vec.X == 0f && vec.Y == 0f && vec.Z == 0f && vec.W == 0f;
		}

		public static float Dot(Vector4f a, Vector4f b)
		{
			Vector4f vector4f = a * b;
			vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
			return VectorOperations.HorizontalAdd(vector4f, vector4f).X;
		}

		public static Vector4f Cross(Vector4f a, Vector4f b)
		{
			Vector4f vector4f = a;
			Vector4f vector4f2 = b;
			vector4f.W = vector4f.X;
			vector4f = VectorOperations.Shuffle(vector4f, 57);
			vector4f2.W = vector4f2.Z;
			vector4f2 = VectorOperations.Shuffle(vector4f2, 147);
			Vector4f vector4f3 = a;
			Vector4f vector4f4 = b;
			vector4f3.W = vector4f3.Z;
			vector4f3 = VectorOperations.Shuffle(vector4f3, 147);
			vector4f4.W = vector4f4.X;
			vector4f4 = VectorOperations.Shuffle(vector4f4, 57);
			Vector4f vector4f5 = vector4f * vector4f2 - vector4f3 * vector4f4;
			vector4f5.W = 0f;
			return vector4f5;
		}

		public static Vector4f Lerp(Vector4f a, Vector4f b, float t)
		{
			Vector4f vector4f;
			vector4f..ctor(t);
			return a * (Vector4f.One - vector4f) + b * vector4f;
		}

		public static Vector4f Lerp(Vector4f a, Vector4f b, Vector4f t)
		{
			return a * (Vector4f.One - t) + b * t;
		}

		public static Vector4f ProjectPointOnPlane(Vector4f planeNormal, Vector4f planePoint, Vector4f point)
		{
			Vector4f vector4f;
			vector4f..ctor(-Vector4fExtensions.Dot(planeNormal, point - planePoint));
			return point + planeNormal * vector4f;
		}

		public static Quaternion NormalizeQuaternion(Quaternion q)
		{
			Vector4f vector4f;
			vector4f..ctor(q.x, q.y, q.z, q.w);
			Vector4f vector4f2 = vector4f * vector4f;
			vector4f2 = VectorOperations.HorizontalAdd(vector4f2, vector4f2);
			vector4f2 = VectorOperations.HorizontalAdd(vector4f2, vector4f2);
			vector4f *= new Vector4f(1f / Mathf.Sqrt(vector4f2.X));
			return new Quaternion(vector4f.X, vector4f.Y, vector4f.Z, vector4f.W);
		}

		public static readonly Vector4f up = new Vector4f(0f, 1f, 0f, 0f);

		public static readonly Vector4f down = new Vector4f(0f, -1f, 0f, 0f);

		public static readonly Vector4f right = new Vector4f(1f, 0f, 0f, 0f);

		public static readonly Vector4f left = new Vector4f(-1f, 0f, 0f, 0f);

		public static readonly Vector4f forward = new Vector4f(0f, 0f, 1f, 0f);

		public static readonly Vector4f back = new Vector4f(0f, 0f, -1f, 0f);

		public static readonly Vector4f Zero = new Vector4f(0f);

		public static readonly Vector4f one3 = new Vector4f(1f, 1f, 1f, 0f);

		public static readonly Vector4f two3 = new Vector4f(2f, 2f, 2f, 0f);

		public static readonly Vector4f half3 = new Vector4f(0.5f, 0.5f, 0.5f, 0f);

		public static readonly Vector4f onethird3 = new Vector4f(0.33333334f, 0.33333334f, 0.33333334f, 0f);

		public static readonly Vector4f quarter3 = new Vector4f(0.25f, 0.25f, 0.25f, 0f);

		public static readonly Vector4f one = new Vector4f(1f, 1f, 1f, 1f);

		public static readonly Vector4f two = new Vector4f(2f, 2f, 2f, 2f);

		public static readonly Vector4f half = new Vector4f(0.5f, 0.5f, 0.5f, 0.5f);

		public static readonly Vector4f onethird = new Vector4f(0.33333334f, 0.33333334f, 0.33333334f, 0.33333334f);

		public static readonly Vector4f quarter = new Vector4f(0.25f, 0.25f, 0.25f, 0.25f);
	}
}
