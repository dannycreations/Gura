using System;
using System.IO;

namespace TPM
{
	[Serializable]
	public class TPMBobber : IBobber
	{
		public TPMBobber(IBobber bobber)
		{
			this._itemId = bobber.ItemId;
		}

		public TPMBobber(int itemId)
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

		public void ReplaceData(IBobber bobber)
		{
			this._itemId = bobber.ItemId;
		}

		public static void WriteToStream(Stream stream, TPMBobber bobber)
		{
			Serializer.WriteBool(stream, bobber != null);
			if (bobber != null)
			{
				Serializer.WriteInt(stream, bobber._itemId);
			}
		}

		public static TPMBobber ReadFromStream(Stream stream)
		{
			return (!Serializer.ReadBool(stream)) ? null : new TPMBobber(Serializer.ReadInt(stream));
		}

		private int _itemId;
	}
}
