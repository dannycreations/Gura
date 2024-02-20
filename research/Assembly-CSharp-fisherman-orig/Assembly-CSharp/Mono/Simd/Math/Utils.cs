using System;

namespace Mono.Simd.Math
{
	public static class Utils
	{
		public static bool MinimalSimdSupport()
		{
			return SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Addition") && SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Multiply") && SimdRuntime.IsMethodAccelerated(typeof(VectorOperations), "Shuffle", new Type[]
			{
				typeof(Vector4f),
				typeof(ShuffleSel)
			});
		}

		public static bool EnhancedSimdSupport()
		{
			return SimdRuntime.IsMethodAccelerated(typeof(VectorOperations), "HorizontalAdd", new Type[]
			{
				typeof(Vector4f),
				typeof(Vector4f)
			}) && SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Multiply");
		}

		public static float Barycentric(float value1, float value2, float value3, float amount1, float amount2)
		{
			return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
		}

		public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
		{
			double num = (double)(amount * amount);
			double num2 = num * (double)amount;
			return (float)(0.5 * (2.0 * (double)value2 + (double)((value3 - value1) * amount) + (2.0 * (double)value1 - 5.0 * (double)value2 + 4.0 * (double)value3 - (double)value4) * num + (3.0 * (double)value2 - (double)value1 - 3.0 * (double)value3 + (double)value4) * num2));
		}

		public static float Clamp(float value, float min, float max)
		{
			value = ((value <= max) ? value : max);
			value = ((value >= min) ? value : min);
			return value;
		}

		public static float Distance(float value1, float value2)
		{
			return Math.Abs(value1 - value2);
		}

		public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
		{
			double num = (double)value1;
			double num2 = (double)value2;
			double num3 = (double)tangent1;
			double num4 = (double)tangent2;
			double num5 = (double)amount;
			double num6 = num5 * num5 * num5;
			double num7 = num5 * num5;
			double num8;
			if (amount == 0f)
			{
				num8 = (double)value1;
			}
			else if (amount == 1f)
			{
				num8 = (double)value2;
			}
			else
			{
				num8 = (2.0 * num - 2.0 * num2 + num4 + num3) * num6 + (3.0 * num2 - 3.0 * num - 2.0 * num3 - num4) * num7 + num3 * num5 + num;
			}
			return (float)num8;
		}

		public static bool IsFinite(float value)
		{
			return !float.IsNaN(value) && !float.IsInfinity(value);
		}

		public static float Lerp(float value1, float value2, float amount)
		{
			return value1 + (value2 - value1) * amount;
		}

		public static float Max(float value1, float value2)
		{
			return Math.Max(value1, value2);
		}

		public static float Min(float value1, float value2)
		{
			return Math.Min(value1, value2);
		}

		public static float SmoothStep(float value1, float value2, float amount)
		{
			float num = Utils.Clamp(amount, 0f, 1f);
			return Utils.Hermite(value1, 0f, value2, 0f, num);
		}

		public static float ToDegrees(float radians)
		{
			return (float)((double)radians * 57.29577951308232);
		}

		public static float ToRadians(float degrees)
		{
			return (float)((double)degrees * 0.017453292519943295);
		}

		public const float E = 2.7182817f;

		public const float Log10E = 0.4342945f;

		public const float Log2E = 1.442695f;

		public const float Pi = 3.1415927f;

		public const float PiOver2 = 1.5707964f;

		public const float PiOver4 = 0.7853982f;

		public const float TwoPi = 6.2831855f;
	}
}
