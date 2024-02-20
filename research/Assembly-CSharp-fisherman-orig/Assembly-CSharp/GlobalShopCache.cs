using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Common;
using ObjectModel;
using UnityEngine;

public class GlobalShopCache : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<GlobalCacheResponceEventArgs> OnGetItems;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<GlobalCacheResponceEventArgs> OnUpdatedItems;

	internal void Start()
	{
		this.SubscribeEvents();
	}

	internal void OnDestroy()
	{
		this.UnsubscribeEvents();
	}

	protected virtual void Update()
	{
		if (this.WaitOp != null)
		{
			this.WaitOp.Update();
		}
	}

	public void SubscribeEvents()
	{
		PhotonConnectionFactory.Instance.OnGettingItemsForCategoryFailed += this.GettingItemsForCategoryFailed;
		PhotonConnectionFactory.Instance.OnGotItemsForCategory += this.OnGotItemsForCategory;
		PhotonConnectionFactory.Instance.OnPondUnlocked += this.OnGotPondUnlocked;
		PhotonConnectionFactory.Instance.OnPondLevelInvalidated += this.OnPondLevelInvalidated;
		PhotonConnectionFactory.Instance.OnPondStayFinish += this.OnPondStayFinish;
	}

	public void UnsubscribeEvents()
	{
		PhotonConnectionFactory.Instance.OnGettingItemsForCategoryFailed -= this.GettingItemsForCategoryFailed;
		PhotonConnectionFactory.Instance.OnGotItemsForCategory -= this.OnGotItemsForCategory;
		PhotonConnectionFactory.Instance.OnPondUnlocked -= this.OnGotPondUnlocked;
		PhotonConnectionFactory.Instance.OnPondLevelInvalidated -= this.OnPondLevelInvalidated;
		PhotonConnectionFactory.Instance.OnPondStayFinish -= this.OnPondStayFinish;
	}

	private int GetUnlockLevel()
	{
		int num = -1;
		if (PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.LevelLockRemovals != null)
		{
			for (int i = 0; i < PhotonConnectionFactory.Instance.Profile.LevelLockRemovals.Count; i++)
			{
				LevelLockRemoval levelLockRemoval = PhotonConnectionFactory.Instance.Profile.LevelLockRemovals[i];
				if (levelLockRemoval.Ponds != null)
				{
					for (int j = 0; j < levelLockRemoval.Ponds.Length; j++)
					{
						if (levelLockRemoval.AccessibleLevel != null && num < levelLockRemoval.AccessibleLevel)
						{
							num = levelLockRemoval.AccessibleLevel.Value;
						}
					}
				}
			}
		}
		return num;
	}

	private void OnPondLevelInvalidated()
	{
		int unlockLevel = this.GetUnlockLevel();
		if (unlockLevel != -1)
		{
			this.OnGotPondUnlocked(0, unlockLevel);
		}
	}

	private void OnPondStayFinish(PondStayFinish finish)
	{
		int unlockLevel = this.GetUnlockLevel();
		foreach (KeyValuePair<int, List<InventoryItem>> keyValuePair in this._globalShopChache)
		{
			List<InventoryItem> value = keyValuePair.Value;
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].MinLevel <= unlockLevel && PhotonConnectionFactory.Instance.Profile.Level < unlockLevel && value[i].PriceGold > 0.0)
				{
					value[i].MinLevel = 1;
				}
				if (value[i].OriginalMinLevel > unlockLevel && PhotonConnectionFactory.Instance.Profile.Level < value[i].OriginalMinLevel && value[i].PriceGold > 0.0)
				{
					value[i].MinLevel = value[i].OriginalMinLevel;
				}
			}
		}
		if (this.OnUpdatedItems != null && this._requestedCategories != null)
		{
			EventHandler<GlobalCacheResponceEventArgs> onUpdatedItems = this.OnUpdatedItems;
			GlobalCacheResponceEventArgs globalCacheResponceEventArgs = new GlobalCacheResponceEventArgs();
			globalCacheResponceEventArgs.Items = this._globalShopChache.Where((KeyValuePair<int, List<InventoryItem>> x) => this._requestedCategories.Contains(x.Key)).SelectMany((KeyValuePair<int, List<InventoryItem>> x) => x.Value).ToList<InventoryItem>();
			onUpdatedItems(this, globalCacheResponceEventArgs);
		}
	}

	private void OnGotPondUnlocked(int pondId, int accesibleLevel)
	{
		foreach (KeyValuePair<int, List<InventoryItem>> keyValuePair in this._globalShopChache)
		{
			List<InventoryItem> value = keyValuePair.Value;
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].MinLevel <= accesibleLevel && PhotonConnectionFactory.Instance.Profile.Level < accesibleLevel && value[i].PriceGold > 0.0)
				{
					value[i].MinLevel = 1;
				}
			}
		}
		if (this.OnUpdatedItems != null && this._requestedCategories != null)
		{
			EventHandler<GlobalCacheResponceEventArgs> onUpdatedItems = this.OnUpdatedItems;
			GlobalCacheResponceEventArgs globalCacheResponceEventArgs = new GlobalCacheResponceEventArgs();
			globalCacheResponceEventArgs.Items = this._globalShopChache.Where((KeyValuePair<int, List<InventoryItem>> x) => this._requestedCategories.Contains(x.Key)).SelectMany((KeyValuePair<int, List<InventoryItem>> x) => x.Value).ToList<InventoryItem>();
			onUpdatedItems(this, globalCacheResponceEventArgs);
		}
	}

	public virtual void GetItemsFromCategory(int[] categoryIds)
	{
		this.CheckItemsInCache(categoryIds, delegate(int[] unrequestedCat)
		{
			PhotonConnectionFactory.Instance.GetGlobalItemsFromCategory(unrequestedCat, false);
		});
	}

	protected void OnGotItemsForCategory(List<InventoryItem> items)
	{
		if (this._requestedCategories == null)
		{
			return;
		}
		string text = "Response {0} items from category {1}";
		object[] array = new object[2];
		array[0] = items.Count;
		array[1] = string.Join(",", this._requestedCategories.Select((int p) => p.ToString()).ToArray<string>());
		Debug.LogFormat(text, array);
		IEnumerable<IGrouping<ItemSubTypes, InventoryItem>> enumerable = from x in items
			group x by x.ItemSubType;
		foreach (IGrouping<ItemSubTypes, InventoryItem> grouping in enumerable)
		{
			if (!this._globalShopChache.ContainsKey((int)grouping.Key))
			{
				this._globalShopChache.Add((int)grouping.Key, grouping.ToList<InventoryItem>());
			}
		}
		IEnumerable<int> enumerable2 = this._requestedCategories.Where((int x) => !this._globalShopChache.ContainsKey(x));
		foreach (int num in enumerable2)
		{
			this._globalShopChache.Add(num, new List<InventoryItem>());
		}
		this.GetItems();
		if (this.WaitOp != null)
		{
			this.WaitOp.StopWaiting(true);
		}
	}

	protected void GetItems()
	{
		if (this.OnGetItems != null)
		{
			EventHandler<GlobalCacheResponceEventArgs> onGetItems = this.OnGetItems;
			GlobalCacheResponceEventArgs globalCacheResponceEventArgs = new GlobalCacheResponceEventArgs();
			globalCacheResponceEventArgs.Items = this._globalShopChache.Where((KeyValuePair<int, List<InventoryItem>> x) => this._requestedCategories.Contains(x.Key)).SelectMany((KeyValuePair<int, List<InventoryItem>> x) => x.Value).ToList<InventoryItem>();
			onGetItems(this, globalCacheResponceEventArgs);
		}
		this._requestedCategories = null;
	}

	public List<InventoryItem> GetItemsFromCategoryImmediate(List<int> categories)
	{
		return this._globalShopChache.Where((KeyValuePair<int, List<InventoryItem>> x) => categories.Contains(x.Key) || x.Value.All((InventoryItem y) => categories.Contains((int)y.ItemType))).SelectMany((KeyValuePair<int, List<InventoryItem>> x) => x.Value).ToList<InventoryItem>();
	}

	public List<InventoryItem> GetAllItemsImmediate()
	{
		return this._globalShopChache.SelectMany((KeyValuePair<int, List<InventoryItem>> x) => x.Value).ToList<InventoryItem>();
	}

	protected void CheckItemsInCache(int[] categoryIds, Action<int[]> getFromServer)
	{
		this._requestedCategories = categoryIds.ToList<int>();
		int[] array = this._requestedCategories.Where((int x) => !this._globalShopChache.ContainsKey(x)).ToArray<int>();
		if (array.Length > 0)
		{
			this.WaitOp = new WaitingOperation(0f, 10f);
			this.WaitOp.OnStopWaiting += this.StopWaiting;
			Debug.Log("Request items from category: " + string.Join(",", array.Select((int p) => p.ToString()).ToArray<string>()));
			getFromServer(array);
		}
		else
		{
			this.GetItems();
		}
	}

	protected void GettingItemsForCategoryFailed(Failure f)
	{
		Debug.LogErrorFormat("GettingItemsForCategoryFailed FullErrorInfo:{0}", new object[] { f.FullErrorInfo });
		if (this.WaitOp != null)
		{
			this.WaitOp.StopWaiting(true);
		}
	}

	protected void StopWaiting()
	{
		if (this.WaitOp != null)
		{
			this.WaitOp.OnStopWaiting -= this.StopWaiting;
			this.WaitOp = null;
		}
	}

	protected Dictionary<int, List<InventoryItem>> _globalShopChache = new Dictionary<int, List<InventoryItem>>();

	protected List<int> _requestedCategories;

	protected WaitingOperation WaitOp;
}
