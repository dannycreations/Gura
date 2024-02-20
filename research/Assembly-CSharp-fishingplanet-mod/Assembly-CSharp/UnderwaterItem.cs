using System;

public class UnderwaterItem : IUnderwaterItem
{
	public UnderwaterItem(int itemId, string a)
	{
		this._itemId = itemId;
		this.Asset = a;
	}

	public string Asset { get; set; }

	private int _itemId;
}
