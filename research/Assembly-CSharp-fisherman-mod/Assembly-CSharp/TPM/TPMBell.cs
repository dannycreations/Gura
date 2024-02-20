using System;
using System.IO;
using ObjectModel;

namespace TPM
{
	[Serializable]
	public class TPMBell : IBell
	{
		public TPMBell(IBell bell)
		{
			this._itemId = bell.ItemId;
		}

		public TPMBell(int itemId)
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
				ItemAssetInfo itemAssetPath = CacheLibrary.AssetsCache.GetItemAssetPath(this._itemId);
				return (itemAssetPath == null) ? null : itemAssetPath.Asset;
			}
		}

		public void ReplaceData(IBell bell)
		{
			this._itemId = bell.ItemId;
		}

		public static void WriteToStream(Stream stream, TPMBell bell)
		{
			Serializer.WriteBool(stream, bell != null);
			if (bell != null)
			{
				Serializer.WriteInt(stream, bell._itemId);
			}
		}

		public static TPMBell ReadFromStream(Stream stream)
		{
			return (!Serializer.ReadBool(stream)) ? null : new TPMBell(Serializer.ReadInt(stream));
		}

		private int _itemId;
	}
}
