using System;
using System.Runtime.InteropServices;

namespace BiteEditor
{
	public class Converter
	{
		private static byte GetNByte(int value, byte bytePosition)
		{
			return (byte)((value >> (int)(bytePosition * 8)) & 255);
		}

		public static byte[] FloatToBytes(float value)
		{
			Converter.IntFloat intFloat = default(Converter.IntFloat);
			intFloat.FloatValue = value;
			int intValue = intFloat.IntValue;
			return new byte[]
			{
				Converter.GetNByte(intValue, 0),
				Converter.GetNByte(intValue, 1),
				Converter.GetNByte(intValue, 2),
				Converter.GetNByte(intValue, 3)
			};
		}

		public static float BytesToFloat(byte[] bytes)
		{
			return BitConverter.ToSingle(bytes, 0);
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct IntFloat
		{
			[FieldOffset(0)]
			public float FloatValue;

			[FieldOffset(0)]
			public int IntValue;
		}
	}
}
