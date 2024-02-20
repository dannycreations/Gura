using System;
using System.IO;

namespace TPM
{
	[Serializable]
	public class TPMSinker : ISinker
	{
		public TPMSinker(ISinker sinker)
		{
			this._itemId = sinker.ItemId;
		}

		public TPMSinker(int itemId)
		{
			this._itemId = itemId;
		}

		public int ItemId
		{
			get
			{
				return this._itemId;
			}
		}

		public string Asset
		{
			get
			{
				return CacheLibrary.AssetsCache.GetItemAssetPath(this._itemId).Asset;
			}
		}

		public void ReplaceData(ISinker sinker)
		{
			this._itemId = sinker.ItemId;
		}

		public static void WriteToStream(Stream stream, TPMSinker sinker)
		{
			Serializer.WriteBool(stream, sinker != null);
			if (sinker != null)
			{
				Serializer.WriteInt(stream, sinker._itemId);
			}
		}

		public static TPMSinker ReadFromStream(Stream stream)
		{
			return (!Serializer.ReadBool(stream)) ? null : new TPMSinker(Serializer.ReadInt(stream));
		}

		private int _itemId;
	}
}
