using System;
using System.IO;

namespace TPM
{
	[Serializable]
	public struct TPMReel : IReel
	{
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

		public ReelTypes ReelType
		{
			get
			{
				return this._reelType;
			}
		}

		public void ReplaceData(IReel reel)
		{
			this._itemId = reel.ItemId;
			this._reelType = reel.ReelType;
		}

		public void WriteToStream(Stream stream)
		{
			Serializer.WriteInt(stream, this._itemId);
			Serializer.WriteAsByte(stream, (int)this._reelType);
		}

		public void ReadFromStream(Stream stream)
		{
			this._itemId = Serializer.ReadInt(stream);
			this._reelType = (ReelTypes)Serializer.ReadByte(stream);
		}

		private int _itemId;

		public ReelTypes _reelType;
	}
}
