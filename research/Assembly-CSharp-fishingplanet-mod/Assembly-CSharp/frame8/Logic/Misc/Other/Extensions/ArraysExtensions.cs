using System;

namespace frame8.Logic.Misc.Other.Extensions
{
	public static class ArraysExtensions
	{
		public static bool IsBetween(this float[] arr, float[] minInclusive, float[] maxInclusive)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				if (arr[i] < minInclusive[i] || arr[i] > maxInclusive[i])
				{
					return false;
				}
			}
			return true;
		}

		public static T[] GetRotatedArray<T>(this T[] arr, int rotateAmount)
		{
			T[] array = new T[arr.Length];
			arr.GetRotatedArray(array, rotateAmount);
			return array;
		}

		public static void GetRotatedArray<T>(this T[] arr, T[] result, int rotateAmount)
		{
			int num = arr.Length;
			if (rotateAmount == 0)
			{
				Array.Copy(arr, result, num);
				return;
			}
			if (rotateAmount < 0)
			{
				rotateAmount += num;
			}
			if (Math.Abs(rotateAmount) >= num)
			{
				throw new InvalidOperationException();
			}
			Array.Copy(arr, 0, result, rotateAmount, num - rotateAmount);
			Array.Copy(arr, num - rotateAmount, result, 0, rotateAmount);
		}
	}
}
