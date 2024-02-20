using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TPM
{
	public class Serializer
	{
		public static void WriteData(Stream stream, byte[] data)
		{
			stream.Write(data, 0, data.Length);
		}

		public static void WriteBool(Stream stream, bool value)
		{
			Serializer._boolBuffer[0] = ((!value) ? 0 : 1);
			Serializer.WriteData(stream, Serializer._boolBuffer);
		}

		public static void WriteAsByte(Stream stream, int value)
		{
			Serializer._byteBuffer[0] = (byte)value;
			Serializer.WriteData(stream, Serializer._byteBuffer);
		}

		public static void WriteAsUShort(Stream stream, ushort value)
		{
			Serializer.WriteAsByte(stream, (int)Serializer.GetNByte((int)value, 0));
			Serializer.WriteAsByte(stream, (int)Serializer.GetNByte((int)value, 1));
		}

		private static byte GetNByte(int value, byte bytePosition)
		{
			return (byte)((value >> (int)(bytePosition * 8)) & 255);
		}

		public static void WriteInt(Stream stream, int value)
		{
			Serializer.WriteAsByte(stream, (int)Serializer.GetNByte(value, 0));
			Serializer.WriteAsByte(stream, (int)Serializer.GetNByte(value, 1));
			Serializer.WriteAsByte(stream, (int)Serializer.GetNByte(value, 2));
			Serializer.WriteAsByte(stream, (int)Serializer.GetNByte(value, 3));
		}

		public static void WriteFloat(Stream stream, float value)
		{
			Serializer.IntFloat intFloat = default(Serializer.IntFloat);
			intFloat.FloatValue = value;
			int intValue = intFloat.IntValue;
			Serializer.WriteAsByte(stream, (int)Serializer.GetNByte(intValue, 0));
			Serializer.WriteAsByte(stream, (int)Serializer.GetNByte(intValue, 1));
			Serializer.WriteAsByte(stream, (int)Serializer.GetNByte(intValue, 2));
			Serializer.WriteAsByte(stream, (int)Serializer.GetNByte(intValue, 3));
		}

		public static void WriteVector3(Stream stream, Vector3 v)
		{
			Serializer.WriteFloat(stream, v.x);
			Serializer.WriteFloat(stream, v.y);
			Serializer.WriteFloat(stream, v.z);
		}

		public static void WriteQuaternion(Stream stream, Quaternion q)
		{
			Vector3 eulerAngles = q.eulerAngles;
			Serializer.WriteFloat(stream, eulerAngles.x);
			Serializer.WriteFloat(stream, eulerAngles.y);
			Serializer.WriteFloat(stream, eulerAngles.z);
		}

		public static void WriteXYQuaternion(Stream stream, Quaternion q)
		{
			Vector3 eulerAngles = q.eulerAngles;
			Serializer.WriteFloat(stream, eulerAngles.x);
			Serializer.WriteFloat(stream, eulerAngles.y);
		}

		public static void WriteVectors(Stream stream, Vector3[] vectors)
		{
			for (int i = 0; i < vectors.Length; i++)
			{
				Serializer.WriteVector3(stream, vectors[i]);
			}
		}

		public static void WriteVectors(Stream stream, List<Vector3> vectors)
		{
			if (vectors.Count > 255)
			{
				throw new ProtocolViolationException("List is to long. 255 element is maximum size!");
			}
			Serializer.WriteAsByte(stream, vectors.Count);
			for (int i = 0; i < vectors.Count; i++)
			{
				Serializer.WriteVector3(stream, vectors[i]);
			}
		}

		public static void Write01Floats(Stream stream, float[] floats)
		{
			for (int i = 0; i < floats.Length; i++)
			{
				float num = floats[i];
				if (Math.Abs(num) > 3f)
				{
					LogHelper.Error("Element at {0} index = {1} - too high value to pack into 2 bytes", new object[]
					{
						i,
						floats[i]
					});
					num = (float)(Math.Sign(num) * 3);
				}
				short num2 = (short)(num * 10000f);
				Serializer.WriteAsByte(stream, (int)Serializer.GetNByte((int)num2, 0));
				Serializer.WriteAsByte(stream, (int)Serializer.GetNByte((int)num2, 1));
			}
		}

		public static void WriteBools(Stream stream, params bool[] bools)
		{
			if (bools.Length > 8)
			{
				throw new ProtocolViolationException("Please use array of <=8 boolean elements to store it as single byte");
			}
			byte b = 0;
			for (int i = 0; i < bools.Length; i++)
			{
				b |= (byte)(((!bools[i]) ? 0 : 1) << (i & 31));
			}
			Serializer.WriteAsByte(stream, (int)b);
		}

		public static void WriteBoolsAsUShort(Stream stream, params bool[] bools)
		{
			if (bools.Length > 16)
			{
				throw new ProtocolViolationException("Please use array of <=16 boolean elements to store it into 2 bytes");
			}
			ushort num = 0;
			for (int i = 0; i < bools.Length; i++)
			{
				num |= (ushort)(((!bools[i]) ? 0 : 1) << (i & 31));
			}
			Serializer.WriteAsUShort(stream, num);
		}

		public static byte ReadByte(Stream stream)
		{
			if (stream.Read(Serializer._byteBuffer, 0, Serializer._byteBuffer.Length) == Serializer._byteBuffer.Length)
			{
				return Serializer._byteBuffer[0];
			}
			throw new ProtocolViolationException("Error reading byte value");
		}

		public static bool ReadBool(Stream stream)
		{
			if (stream.Read(Serializer._boolBuffer, 0, Serializer._boolBuffer.Length) == Serializer._boolBuffer.Length)
			{
				return BitConverter.ToBoolean(Serializer._boolBuffer, 0);
			}
			throw new ProtocolViolationException("Error reading boolean value");
		}

		public static ushort ReadUShort(Stream stream)
		{
			if (stream.Read(Serializer._ushortBuffer, 0, Serializer._shortBuffer.Length) == Serializer._ushortBuffer.Length)
			{
				return BitConverter.ToUInt16(Serializer._ushortBuffer, 0);
			}
			throw new ProtocolViolationException("Error reading ushort value");
		}

		public static int ReadInt(Stream stream)
		{
			if (stream.Read(Serializer._intBuffer, 0, Serializer._intBuffer.Length) == Serializer._intBuffer.Length)
			{
				return BitConverter.ToInt32(Serializer._intBuffer, 0);
			}
			throw new ProtocolViolationException("Error reading int value");
		}

		public static float ReadFloat(Stream stream)
		{
			if (stream.Read(Serializer._floatBuffer, 0, Serializer._floatBuffer.Length) == Serializer._floatBuffer.Length)
			{
				return BitConverter.ToSingle(Serializer._floatBuffer, 0);
			}
			throw new ProtocolViolationException("Error reading float value");
		}

		public static Vector3 ReadVector3(Stream stream)
		{
			if (stream.Position + 12L <= stream.Length)
			{
				return new Vector3(Serializer.ReadFloat(stream), Serializer.ReadFloat(stream), Serializer.ReadFloat(stream));
			}
			throw new ProtocolViolationException("Error reading Vector3 value");
		}

		public static Quaternion ReadQuaternion(Stream stream)
		{
			if (stream.Position + 12L <= stream.Length)
			{
				Vector3 vector = Serializer.ReadVector3(stream);
				return Quaternion.Euler(vector);
			}
			throw new ProtocolViolationException("Error reading Quaternion value");
		}

		public static Quaternion ReadXYQuaternion(Stream stream)
		{
			if (stream.Position + 8L <= stream.Length)
			{
				Vector3 vector;
				vector..ctor(Serializer.ReadFloat(stream), Serializer.ReadFloat(stream), 0f);
				return Quaternion.Euler(vector);
			}
			throw new ProtocolViolationException("Error reading Quaternion value");
		}

		public static void ReadVectorsArray(Stream stream, Vector3[] v)
		{
			for (int i = 0; i < v.Length; i++)
			{
				v[i] = Serializer.ReadVector3(stream);
			}
		}

		public static void ReadVectorsList(Stream stream, List<Vector3> buf)
		{
			byte b = Serializer.ReadByte(stream);
			buf.Clear();
			for (int i = 0; i < (int)b; i++)
			{
				buf.Add(Serializer.ReadVector3(stream));
			}
		}

		public static byte[] ReadBytes(Stream stream, int size)
		{
			byte[] array = new byte[size];
			if (stream.Read(array, 0, size) == size)
			{
				return array;
			}
			throw new ProtocolViolationException("Error reading byte value");
		}

		public static float[] Read01Floats(Stream stream, byte size)
		{
			float[] array = new float[(int)size];
			for (int i = 0; i < (int)size; i++)
			{
				if (stream.Read(Serializer._shortBuffer, 0, 2) != 2)
				{
					throw new ProtocolViolationException("Error reading float01 value");
				}
				array[i] = (float)BitConverter.ToInt16(Serializer._shortBuffer, 0) / 10000f;
			}
			return array;
		}

		public static bool[] ReadBools(Stream stream, byte size)
		{
			bool[] array = new bool[(int)size];
			byte b = Serializer.ReadByte(stream);
			int num = 0;
			while (b > 0)
			{
				array[num] = (b & 1) == 1;
				b = (byte)(b >> 1);
				num++;
			}
			return array;
		}

		public static bool[] ReadBoolsFromUShort(Stream stream, byte size)
		{
			bool[] array = new bool[(int)size];
			ushort num = Serializer.ReadUShort(stream);
			int num2 = 0;
			while (num > 0 && num2 < (int)size)
			{
				array[num2] = (num & 1) == 1;
				num = (ushort)(num >> 1);
				num2++;
			}
			return array;
		}

		private const float FLOAT01K = 10000f;

		private const int VECTOR3_SIZE = 12;

		private const int QUATERNION_SIZE = 12;

		private const int QUATERNION_XY_SIZE = 8;

		private static readonly byte[] _byteBuffer = new byte[1];

		private static readonly byte[] _boolBuffer = new byte[1];

		private static readonly byte[] _shortBuffer = new byte[2];

		private static readonly byte[] _ushortBuffer = new byte[2];

		private static readonly byte[] _intBuffer = new byte[4];

		private static readonly byte[] _floatBuffer = new byte[4];

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
