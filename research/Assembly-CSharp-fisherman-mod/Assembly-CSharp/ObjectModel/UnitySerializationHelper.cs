using System;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectModel
{
	public static class UnitySerializationHelper
	{
		public static byte[] SerializeVector3(Vector3 v)
		{
			int num = 0;
			byte[] array = new byte[UnitySerializationHelper.VECTOR3_SIZE];
			UnitySerializationHelper.Serialize(v.x, array, ref num);
			UnitySerializationHelper.Serialize(v.y, array, ref num);
			UnitySerializationHelper.Serialize(v.z, array, ref num);
			return array;
		}

		public static byte[] SerializeQuaternion(Quaternion q)
		{
			int num = 0;
			byte[] array = new byte[UnitySerializationHelper.QUATERNION_SIZE];
			UnitySerializationHelper.Serialize(q.w, array, ref num);
			UnitySerializationHelper.Serialize(q.x, array, ref num);
			UnitySerializationHelper.Serialize(q.y, array, ref num);
			UnitySerializationHelper.Serialize(q.z, array, ref num);
			return array;
		}

		private static void Serialize(float value, byte[] array, ref int offset)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			for (int i = 0; i < bytes.Length; i++)
			{
				array[offset + i] = bytes[i];
			}
			offset += bytes.Length;
		}

		private static void Deserialize(out float value, byte[] array, ref int offset)
		{
			value = BitConverter.ToSingle(array, offset);
			offset += 4;
		}

		public static Vector3 DeserializeVector3(byte[] bytes)
		{
			int num = 0;
			return UnitySerializationHelper.DeserializeVector3(bytes, ref num);
		}

		public static Vector3 DeserializeVector3(byte[] bytes, ref int offset)
		{
			float num;
			UnitySerializationHelper.Deserialize(out num, bytes, ref offset);
			float num2;
			UnitySerializationHelper.Deserialize(out num2, bytes, ref offset);
			float num3;
			UnitySerializationHelper.Deserialize(out num3, bytes, ref offset);
			return new Vector3(num, num2, num3);
		}

		public static Vector3[] DeserializeVector3Array(byte[] bytes)
		{
			if (bytes.Length == 0)
			{
				return new Vector3[0];
			}
			if (bytes.Length % UnitySerializationHelper.VECTOR3_SIZE != 0)
			{
				LogHelper.Error("DeserializeVector3Array error - inconsistence data, invalid data size {0} bytes", new object[] { bytes.Length });
				return null;
			}
			int num = bytes.Length / UnitySerializationHelper.VECTOR3_SIZE;
			Vector3[] array = new Vector3[num];
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				array[i] = UnitySerializationHelper.DeserializeVector3(bytes, ref num2);
			}
			return array;
		}

		public static List<Vector3> DeserializeVector3List(byte[] bytes)
		{
			if (bytes.Length == 0)
			{
				return new List<Vector3>();
			}
			if (bytes.Length % UnitySerializationHelper.VECTOR3_SIZE != 0)
			{
				LogHelper.Error("DeserializeVector3List error - inconsistence data, invalid data size {0} bytes", new object[] { bytes.Length });
				return null;
			}
			int num = bytes.Length / UnitySerializationHelper.VECTOR3_SIZE;
			List<Vector3> list = new List<Vector3>(num);
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				list.Add(UnitySerializationHelper.DeserializeVector3(bytes, ref num2));
			}
			return list;
		}

		public static Quaternion DeserializeQuaternion(byte[] bytes)
		{
			int num = 0;
			return UnitySerializationHelper.DeserializeQuaternion(bytes, ref num);
		}

		public static Quaternion DeserializeQuaternion(byte[] array, ref int offset)
		{
			float num;
			UnitySerializationHelper.Deserialize(out num, array, ref offset);
			float num2;
			UnitySerializationHelper.Deserialize(out num2, array, ref offset);
			float num3;
			UnitySerializationHelper.Deserialize(out num3, array, ref offset);
			float num4;
			UnitySerializationHelper.Deserialize(out num4, array, ref offset);
			return new Quaternion(num2, num3, num4, num);
		}

		public static T[] DeserializeTArray<T>(byte[] bytes, UnitySerializationHelper.DeserializerFunctionDelegate<T> f, int elementSize) where T : struct
		{
			if (bytes.Length == 0)
			{
				return new T[0];
			}
			if (bytes.Length % elementSize != 0)
			{
				LogHelper.Error("DeserializeIntArray error - inconsistence data, invalid data size {0} bytes", new object[] { bytes.Length });
				return null;
			}
			int num = bytes.Length / elementSize;
			T[] array = new T[num];
			int i = 0;
			int num2 = 0;
			while (i < num)
			{
				array[i] = f(bytes, num2);
				i++;
				num2 += elementSize;
			}
			return array;
		}

		public static short[] DeserializeShortArray(byte[] bytes)
		{
			return UnitySerializationHelper.DeserializeTArray<short>(bytes, new UnitySerializationHelper.DeserializerFunctionDelegate<short>(BitConverter.ToInt16), 2);
		}

		public static int[] DeserializeIntArray(byte[] bytes)
		{
			return UnitySerializationHelper.DeserializeTArray<int>(bytes, new UnitySerializationHelper.DeserializerFunctionDelegate<int>(BitConverter.ToInt32), 4);
		}

		public static float[] DeserializeFloatArray(byte[] bytes)
		{
			return UnitySerializationHelper.DeserializeTArray<float>(bytes, new UnitySerializationHelper.DeserializerFunctionDelegate<float>(BitConverter.ToSingle), 4);
		}

		public static bool[] DeserializeBoolArray(byte[] bytes)
		{
			return UnitySerializationHelper.DeserializeTArray<bool>(bytes, new UnitySerializationHelper.DeserializerFunctionDelegate<bool>(BitConverter.ToBoolean), 1);
		}

		public static int VECTOR3_SIZE = 12;

		public static int QUATERNION_SIZE = 16;

		public delegate T DeserializerFunctionDelegate<T>(byte[] bytes, int offset) where T : struct;
	}
}
