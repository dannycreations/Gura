using System;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class NumberUtils
	{
		static NumberUtils()
		{
			Array.Sort<int>(NumberUtils.NumberTypes);
			Array.Sort<int>(NumberUtils.SignedIntegerTypes);
			Array.Sort<int>(NumberUtils.UnsignedIntegerTypes);
		}

		public static bool IsNumber(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return NumberUtils.IsNumber(Type.GetTypeCode(type));
		}

		public static bool IsNumber(TypeCode type)
		{
			return Array.BinarySearch<int>(NumberUtils.NumberTypes, (int)type) >= 0;
		}

		public static bool IsSignedInteger(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return NumberUtils.IsSignedInteger(Type.GetTypeCode(type));
		}

		public static bool IsSignedInteger(TypeCode type)
		{
			return Array.BinarySearch<int>(NumberUtils.SignedIntegerTypes, (int)type) >= 0;
		}

		public static bool IsUnsignedInteger(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return NumberUtils.IsUnsignedInteger(Type.GetTypeCode(type));
		}

		public static bool IsUnsignedInteger(TypeCode type)
		{
			return Array.BinarySearch<int>(NumberUtils.UnsignedIntegerTypes, (int)type) >= 0;
		}

		private static readonly int[] SignedIntegerTypes = new int[] { 5, 7, 9, 11 };

		private static readonly int[] UnsignedIntegerTypes = new int[] { 6, 8, 10, 12 };

		private static readonly int[] NumberTypes = new int[]
		{
			5, 7, 9, 11, 6, 8, 10, 12, 13, 14,
			15
		};
	}
}
