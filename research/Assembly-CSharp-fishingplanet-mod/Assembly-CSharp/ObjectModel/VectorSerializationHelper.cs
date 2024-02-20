using System;

namespace ObjectModel
{
	public static class VectorSerializationHelper
	{
		public static byte[] SerializePoint3(object customobject)
		{
			Point3 point = (Point3)customobject;
			int num = 0;
			byte[] array = new byte[12];
			VectorSerializationHelper.Serialize(point.X, array, ref num);
			VectorSerializationHelper.Serialize(point.Y, array, ref num);
			VectorSerializationHelper.Serialize(point.Z, array, ref num);
			return array;
		}

		public static object DeserializePoint3(byte[] bytes)
		{
			int num = 0;
			float num2;
			VectorSerializationHelper.Deserialize(out num2, bytes, ref num);
			float num3;
			VectorSerializationHelper.Deserialize(out num3, bytes, ref num);
			float num4;
			VectorSerializationHelper.Deserialize(out num4, bytes, ref num);
			return new Point3(num2, num3, num4);
		}

		public static void Serialize(float value, byte[] array, ref int offset)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			for (int i = 0; i < bytes.Length; i++)
			{
				array[offset + i] = bytes[i];
			}
			offset += bytes.Length;
		}

		public static void Deserialize(out float value, byte[] array, ref int offset)
		{
			value = BitConverter.ToSingle(array, offset);
			offset += 4;
		}
	}
}
