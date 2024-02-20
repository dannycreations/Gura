using System;
using System.IO;
using ObjectModel;

namespace TPM
{
	[Serializable]
	public class TPMBait : IBait
	{
		public TPMBait(IBait bait)
		{
			this._itemId = bait.ItemId;
		}

		public TPMBait(int itemId)
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
				if (itemAssetPath != null)
				{
					return itemAssetPath.Asset;
				}
				return "Baits/Pea/pPeas";
			}
		}

		public void ReplaceData(IBait bait)
		{
			this._itemId = bait.ItemId;
		}

		public static void WriteToStream(Stream stream, TPMBait bait)
		{
			Serializer.WriteBool(stream, bait != null);
			if (bait != null)
			{
				Serializer.WriteInt(stream, bait._itemId);
			}
		}

		public static TPMBait ReadFromStream(Stream stream)
		{
			return (!Serializer.ReadBool(stream)) ? null : new TPMBait(Serializer.ReadInt(stream));
		}

		private int _itemId;
	}
}
