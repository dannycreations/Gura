using System;
using System.IO;

namespace TPM
{
	[Serializable]
	public class TPMChum : IChum
	{
		public TPMChum(IChum chum)
		{
			this._itemId = chum.ItemId;
			this.InstanceId = chum.InstanceId;
		}

		public TPMChum(int itemId)
		{
			this._itemId = itemId;
		}

		public Guid? InstanceId { get; private set; }

		public int ItemId
		{
			get
			{
				return this._itemId;
			}
		}

		public bool IsExpired { get; private set; }

		public bool WasThrown { get; set; }

		public string Asset
		{
			get
			{
				return CacheLibrary.AssetsCache.GetItemAssetPath(this._itemId).Asset;
			}
		}

		public void ReplaceData(IChum chum)
		{
			this._itemId = chum.ItemId;
		}

		public static void WriteToStream(Stream stream, TPMChum chum)
		{
			Serializer.WriteBool(stream, chum != null);
			if (chum != null)
			{
				Serializer.WriteInt(stream, chum._itemId);
			}
		}

		public static TPMChum ReadFromStream(Stream stream)
		{
			return (!Serializer.ReadBool(stream)) ? null : new TPMChum(Serializer.ReadInt(stream));
		}

		private int _itemId;
	}
}
