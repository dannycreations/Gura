using System;
using System.IO;
using ObjectModel;

namespace TPM
{
	[Serializable]
	public struct TPMRod : IRod
	{
		public int ItemId
		{
			get
			{
				return this._itemId;
			}
		}

		public float Action
		{
			get
			{
				return this._action;
			}
		}

		public int Slot
		{
			get
			{
				return 0;
			}
		}

		public string Asset
		{
			get
			{
				ItemAssetInfo itemAssetPath = CacheLibrary.AssetsCache.GetItemAssetPath(this._itemId);
				if (itemAssetPath == null)
				{
					return null;
				}
				return itemAssetPath.Asset;
			}
		}

		public void ReplaceData(IRod rod)
		{
			this._itemId = rod.ItemId;
			this._action = rod.Action;
		}

		public void WriteToStream(Stream stream)
		{
			Serializer.WriteInt(stream, this._itemId);
			Serializer.WriteAsByte(stream, (int)((byte)(this._action * 10f)));
		}

		public void ReadFromStream(Stream stream)
		{
			this._itemId = Serializer.ReadInt(stream);
			this._action = (float)Serializer.ReadByte(stream) / 10f;
		}

		private int _itemId;

		private float _action;
	}
}
