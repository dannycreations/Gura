using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class ItemsCache : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotItemsSubscribed OnGotItems;

	private void OnDestroy()
	{
		this.UnsubscribeEvents();
	}

	public void SubscribeEvents()
	{
	}

	public void UnsubscribeEvents()
	{
		PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItemsHandler;
	}

	public InventoryItem GetItemFromCache(int itemId)
	{
		return this._itemsChache.Values.FirstOrDefault((InventoryItem i) => i.ItemId == itemId);
	}

	private void OnGotItemsHandler(List<InventoryItem> items, int subscriberId)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (!this._itemsChache.ContainsKey(items[i].ItemId))
			{
				this._itemsChache.Add(items[i].ItemId, items[i]);
			}
		}
		int[] itemsRequsted = null;
		if (this._subsciberIdToItemsRequested.TryGetValue(subscriberId, out itemsRequsted))
		{
			PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItemsHandler;
			this._subsciberIdToItemsRequested.Remove(subscriberId);
			if (this.OnGotItems != null)
			{
				this.OnGotItems(this._itemsChache.Values.Where((InventoryItem item) => itemsRequsted.Contains(item.ItemId)).ToList<InventoryItem>(), subscriberId);
			}
		}
	}

	public void GetItems(int[] items, int subscriberId)
	{
		int[] array = items.Except(this._itemsChache.Keys).ToArray<int>();
		if (array.Length != 0)
		{
			PhotonConnectionFactory.Instance.GetItemsByIds(array, subscriberId, true);
			PhotonConnectionFactory.Instance.OnGotItems += this.OnGotItemsHandler;
			if (!this._subsciberIdToItemsRequested.ContainsKey(subscriberId))
			{
				this._subsciberIdToItemsRequested.Add(subscriberId, items);
			}
		}
		else if (this.OnGotItems != null)
		{
			this.OnGotItems(this._itemsChache.Values.Where((InventoryItem item) => items.Contains(item.ItemId)).ToList<InventoryItem>(), subscriberId);
		}
	}

	private Dictionary<int, InventoryItem> _itemsChache = new Dictionary<int, InventoryItem>();

	private Dictionary<int, int[]> _subsciberIdToItemsRequested = new Dictionary<int, int[]>();
}
