using System;
using System.Collections.Generic;

namespace ObjectModel
{
	public static class CollectionUtilities
	{
		public static double SumDouble<T>(this List<T> list, Func<T, double> getter)
		{
			double num = 0.0;
			foreach (T t in list)
			{
				num += getter(t);
			}
			return num;
		}

		public static float SumFloat<T>(this List<T> list, Func<T, float> getter)
		{
			float num = 0f;
			foreach (T t in list)
			{
				num += getter(t);
			}
			return num;
		}

		public static int SumInt<T>(this List<T> list, Func<T, int> getter)
		{
			int num = 0;
			foreach (T t in list)
			{
				num += getter(t);
			}
			return num;
		}

		public static float Max(List<float> list)
		{
			float num = float.MinValue;
			foreach (float num2 in list)
			{
				if (num2 > num)
				{
					num = num2;
				}
			}
			return num;
		}

		public static float RoundFloat(this double value)
		{
			return (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);
		}

		public static float RoundFloat(this float value)
		{
			return (float)Math.Round((double)value, 2, MidpointRounding.AwayFromZero);
		}
	}
}
