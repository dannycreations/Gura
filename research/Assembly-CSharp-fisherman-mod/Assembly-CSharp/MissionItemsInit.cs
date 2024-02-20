using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class MissionItemsInit : ActivityStateControlled
{
	private void Update()
	{
		if (this._itemsToLoad.Count > 0)
		{
			int num = ((!this._isLoadingBarEnabled) ? this._itemsToLoad.Count : Math.Min(this._itemsToLoad.Count, 3));
			for (int i = 0; i < num; i++)
			{
				MissionItem missionItem = this._itemsToLoad.Dequeue();
				MissionItemComponent missionItemComponent = this.AddItem(this.ScrollContent, missionItem);
				if (this._fullListWasGenerated)
				{
					this.SetItemToSortedPos(this.ScrollContent, missionItemComponent);
				}
			}
			if (this._isLoadingBarEnabled)
			{
				this.Loading.UpdateLoading(this._itemsToLoadCount, this._itemsToLoadCount - this._itemsToLoad.Count);
			}
			if (this._itemsToLoad.Count == 0)
			{
				this._fullListWasGenerated = true;
				Scrollbar verticalScrollbar = base.GetComponent<ScrollRect>().verticalScrollbar;
				if (verticalScrollbar != null)
				{
					verticalScrollbar.value = (float)((verticalScrollbar.direction != 3) ? 1 : 0);
				}
				base.GetComponent<CanvasGroup>().alpha = 1f;
				this.Loading.Deactivate();
			}
		}
	}

	private void SetItemToSortedPos(GameObject contentsPanel, MissionItemComponent itemComponent)
	{
		MissionItem inventoryItem = itemComponent.InventoryItem;
		for (int i = 0; i < contentsPanel.transform.childCount; i++)
		{
			MissionItem inventoryItem2 = contentsPanel.transform.GetChild(i).GetComponent<MissionItemComponent>().InventoryItem;
			if (inventoryItem.ItemSubType < inventoryItem2.ItemSubType || (inventoryItem.ItemSubType == inventoryItem2.ItemSubType && inventoryItem.Name.CompareTo(inventoryItem2.Name) < 0))
			{
				itemComponent.transform.SetSiblingIndex(i);
				break;
			}
		}
	}

	protected override void SetHelp()
	{
		UIStatsCollector.ChangeGameScreen(GameScreenType.Specials, GameScreenTabType.Undefined, null, null, null, null, null);
		this.RefreshItems();
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.RefreshItems;
	}

	protected override void HideHelp()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.RefreshItems;
	}

	private MissionItemComponent AddItem(GameObject contentsPanel, MissionItem item2Add)
	{
		GameObject gameObject = GUITools.AddChild(contentsPanel, this.ItemPrefab);
		MissionItemComponent component = gameObject.GetComponent<MissionItemComponent>();
		component.Init(item2Add);
		component.parentMaskRect = this.Mask;
		ScrollForwarding scrollForwarding = gameObject.AddComponent<ScrollForwarding>();
		return component;
	}

	private void RefreshItems()
	{
		if (this._itemsToLoad.Count > 0)
		{
			return;
		}
		MissionItem[] array = PhotonConnectionFactory.Instance.Profile.Inventory.MissionItems.ToArray();
		HashSet<Guid> hashSet = new HashSet<Guid>(this._curItems.Keys, MissionItemsInit._guidComparer);
		List<MissionItem> list = new List<MissionItem>();
		this.ListIsEmptyText.SetActive(array.Length == 0);
		foreach (MissionItem missionItem in array)
		{
			if (missionItem.InstanceId != null)
			{
				if (this._curItems.ContainsKey(missionItem.InstanceId.Value))
				{
					MissionItemsInit.PresentItem presentItem = this._curItems[missionItem.InstanceId.Value];
					Transform itemUIObject = this.GetItemUIObject(presentItem);
					if (this._curItems[missionItem.InstanceId.Value].UpdateItem(missionItem))
					{
						if (itemUIObject == null)
						{
							LogHelper.Warning("Can't find item {0} to update", new object[] { presentItem });
						}
						else
						{
							itemUIObject.GetComponent<MissionItemComponent>().Init(missionItem);
						}
					}
					hashSet.Remove(missionItem.InstanceId.Value);
				}
				else
				{
					this._curItems[missionItem.InstanceId.Value] = new MissionItemsInit.PresentItem
					{
						InstanceId = missionItem.InstanceId.Value,
						Count = missionItem.Count,
						Durability = missionItem.Durability,
						ItemSubType = missionItem.ItemSubType
					};
					list.Add(missionItem);
				}
			}
		}
		for (int j = 0; j < hashSet.Count; j++)
		{
			MissionItemsInit.PresentItem presentItem2 = this._curItems[hashSet.ElementAt(j)];
			Transform itemUIObject2 = this.GetItemUIObject(presentItem2);
			if (itemUIObject2 == null)
			{
				LogHelper.Error("Can't find item {0} to delete", new object[] { presentItem2 });
			}
			else
			{
				Object.Destroy(itemUIObject2.gameObject);
				this._curItems.Remove(hashSet.ElementAt(j));
			}
		}
		this._itemsToLoad = new Queue<MissionItem>(from x in list
			orderby x.ItemSubType, x.Name
			select x);
		this._itemsToLoadCount = this._itemsToLoad.Count;
		if (this._itemsToLoadCount >= 10)
		{
			this._isLoadingBarEnabled = true;
			this.Loading.GetComponent<CanvasGroup>().alpha = 1f;
			this.Loading.Activate();
			this.ScrollContent.GetComponent<ChildrenChangedListener>().enabled = false;
		}
		if (this._itemsToLoadCount == 0)
		{
			base.GetComponent<CanvasGroup>().alpha = 1f;
			this.Loading.Deactivate();
		}
	}

	private Transform GetItemUIObject(MissionItemsInit.PresentItem item)
	{
		for (int i = 0; i < this.ScrollContent.transform.childCount; i++)
		{
			Transform child = this.ScrollContent.transform.GetChild(i);
			if (child.GetComponent<MissionItemComponent>().InventoryItem.InstanceId == item.InstanceId)
			{
				return child;
			}
		}
		return null;
	}

	public GameObject ItemPrefab;

	public GameObject ScrollContent;

	public GameObject ListIsEmptyText;

	public RectTransform Mask;

	private static GuidComparer _guidComparer = new GuidComparer();

	private const int MIN_ITEMS_COUNT_FOR_LOADING_BAR = 10;

	private const int ITEMS_TO_LOAD_PER_FRAME = 3;

	private readonly Dictionary<Guid, MissionItemsInit.PresentItem> _curItems = new Dictionary<Guid, MissionItemsInit.PresentItem>(MissionItemsInit._guidComparer);

	private Queue<MissionItem> _itemsToLoad = new Queue<MissionItem>();

	private int _itemsToLoadCount;

	private bool _isLoadingBarEnabled;

	private bool _fullListWasGenerated;

	public LoadingController Loading;

	private class PresentItem
	{
		public override string ToString()
		{
			return string.Format("({0}: {1} and {2})", this.InstanceId, this.Count, this.Durability);
		}

		public bool UpdateItem(MissionItem newItem)
		{
			if (newItem.Durability != this.Durability || newItem.Count != this.Count)
			{
				this.Count = newItem.Count;
				this.Durability = newItem.Durability;
				return true;
			}
			return false;
		}

		public Guid InstanceId;

		public int Count;

		public int Durability;

		public ItemSubTypes ItemSubType;
	}
}
