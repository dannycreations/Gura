using System;

public class TPMUnderwaterItem : IUnderwaterItem
{
	public TPMUnderwaterItem(int itemId)
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
			return CacheLibrary.AssetsCache.GetFishAssetPath(this._itemId);
		}
	}

	private int _itemId;
}
