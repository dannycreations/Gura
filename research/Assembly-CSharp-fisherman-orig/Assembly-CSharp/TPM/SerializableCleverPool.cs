using System;
using System.IO;

namespace TPM
{
	public class SerializableCleverPool<T> : CleverPool<T> where T : class, SerializableCleverPool<T>.ISerializableItem, new()
	{
		public SerializableCleverPool(int size, int maxId = 32000)
			: base(size, maxId)
		{
		}

		public void Serialize(Stream stream)
		{
			Serializer.WriteAsByte(stream, this._curCount);
			for (int i = 0; i < this._curCount; i++)
			{
				this._pool[i].WriteToStream(stream);
			}
		}

		public void Deserialize(Stream stream)
		{
			this._curCount = (int)Serializer.ReadByte(stream);
			for (int i = 0; i < this._curCount; i++)
			{
				this._pool[i].ReadFromStream(stream);
			}
		}

		public interface ISerializableItem : CleverPool<T>.IItem
		{
			void WriteToStream(Stream stream);

			void ReadFromStream(Stream stream);
		}
	}
}
