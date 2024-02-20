using System;
using System.IO;
using ObjectModel;

namespace TPM
{
	[Serializable]
	public class TPMHook : IHook
	{
		public TPMHook(IHook hook)
		{
			this._itemId = hook.ItemId;
			this.HookSize = hook.HookSize;
		}

		public TPMHook(int itemId, float hookSize)
		{
			this._itemId = itemId;
			this.HookSize = hookSize;
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
				return "Tackle/Hooks/DefaultHook/pDefaultHook";
			}
		}

		public float HookSize { get; set; }

		public void ReplaceData(IHook hook)
		{
			this._itemId = hook.ItemId;
			this.HookSize = hook.HookSize;
		}

		public static void WriteToStream(Stream stream, TPMHook hook)
		{
			Serializer.WriteBool(stream, hook != null);
			if (hook != null)
			{
				Serializer.WriteInt(stream, hook._itemId);
				Serializer.WriteAsByte(stream, (int)((byte)hook.HookSize));
			}
		}

		public static TPMHook ReadFromStream(Stream stream)
		{
			return (!Serializer.ReadBool(stream)) ? null : new TPMHook(Serializer.ReadInt(stream), (float)Serializer.ReadByte(stream));
		}

		private int _itemId;
	}
}
