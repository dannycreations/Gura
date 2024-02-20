using System;
using System.IO;

namespace TPM
{
	public struct TPMFish : IFish
	{
		public TPMFish(IFish fish)
		{
			this._itemId = fish.FishId;
			this.Length = fish.Length;
		}

		public TPMFish(int itemId, float length)
		{
			this._itemId = itemId;
			this.Length = length;
		}

		public int FishId
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
				return CacheLibrary.AssetsCache.GetFishAssetPath(this._itemId);
			}
		}

		public string ItemAsset
		{
			get
			{
				return CacheLibrary.AssetsCache.GetItemAssetPath(this._itemId).Asset;
			}
		}

		public float Length { get; set; }

		public void ReplaceFishId(int itemId)
		{
			this._itemId = itemId;
		}

		public void WriteToStream(Stream stream)
		{
			Serializer.WriteInt(stream, this._itemId);
			Serializer.WriteAsByte(stream, (int)((byte)(this.Length * 100f)));
		}

		public static TPMFish ReadFromStream(Stream stream)
		{
			int num = Serializer.ReadInt(stream);
			float num2 = (float)Serializer.ReadByte(stream) / 100f;
			return new TPMFish(num, num2);
		}

		private int _itemId;
	}
}
