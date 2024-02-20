using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PopupAchivmentsInfo : PopupInfo
{
	protected override void SetValueToPanel(GameObject popupPanel)
	{
		this._popupPanel = popupPanel;
		Text component = popupPanel.transform.Find("Name").GetComponent<Text>();
		component.text = this.PopupValue;
		Text component2 = popupPanel.transform.Find("Description").GetComponent<Text>();
		component2.text = this.PopupValueDescription;
		Text component3 = popupPanel.transform.Find("Items/Rewards").GetComponent<Text>();
		if (this.RewardValue != string.Empty)
		{
			component3.text = this.RewardValue;
		}
		else
		{
			component3.gameObject.SetActive(false);
		}
		if (this.Rewards != null)
		{
			this.AskForRewards();
		}
		else if (this.ProductRewards != null)
		{
			this._popupPanel.GetComponent<ContentSizeFitter>().enabled = false;
			this.AddProductItems();
			this.EnableContentSizeFitter();
		}
	}

	private void OnDestroy()
	{
		if (this.waitingForRewardsData)
		{
			this.UnsubscribeFromRewardsData();
		}
	}

	private void AskForRewards()
	{
		CacheLibrary.ItemsCache.OnGotItems += this.ItemsCache_OnGotItems;
		CacheLibrary.ItemsCache.GetItems(this.Rewards, this.RewardID);
		this.waitingForRewardsData = true;
	}

	private void UnsubscribeFromRewardsData()
	{
		CacheLibrary.ItemsCache.OnGotItems -= this.ItemsCache_OnGotItems;
		this.waitingForRewardsData = false;
	}

	private void AddProductItems()
	{
		GameObject gameObject = this._popupPanel.transform.Find("Items").gameObject;
		List<StoreProduct> list = new List<StoreProduct>();
		int i;
		for (i = 0; i < this.ProductRewards.Length; i++)
		{
			StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == this.ProductRewards[i]);
			if (storeProduct != null)
			{
				list.Add(storeProduct);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			GameObject gameObject2 = GUITools.AddChild(gameObject, this.RewardItem.gameObject);
			gameObject2.GetComponent<TournamentRewardItemInit>().Init(list[j]);
			gameObject2.GetComponent<TournamentRewardItemInit>().TitleText.color = Color.black;
		}
	}

	private void ItemsCache_OnGotItems(List<InventoryItem> items, int sunscriberId)
	{
		if (sunscriberId == this.RewardID && this._popupPanel != null)
		{
			this.UnsubscribeFromRewardsData();
			GameObject gameObject = this._popupPanel.transform.Find("Items").gameObject;
			this._popupPanel.GetComponent<ContentSizeFitter>().enabled = false;
			for (int i = 0; i < items.Count; i++)
			{
				GameObject gameObject2 = GUITools.AddChild(gameObject, this.RewardItem.gameObject);
				gameObject2.GetComponent<TournamentRewardItemInit>().Init(items[i]);
				gameObject2.GetComponent<TournamentRewardItemInit>().TitleText.color = Color.black;
			}
			if (this.ProductRewards != null)
			{
				this.AddProductItems();
			}
			this.EnableContentSizeFitter();
		}
	}

	private void EnableContentSizeFitter()
	{
		if (this._popupPanel != null)
		{
			this._popupPanel.GetComponent<ContentSizeFitter>().enabled = true;
		}
	}

	public string PopupValueDescription = string.Empty;

	public string RewardValue = string.Empty;

	public int[] Rewards;

	public TournamentRewardItemInit RewardItem;

	public int RewardID;

	private GameObject _popupPanel;

	public int[] ProductRewards;

	private bool waitingForRewardsData;
}
