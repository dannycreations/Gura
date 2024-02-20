using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class RewardSponsoredItem : MonoBehaviour
{
	public float Height
	{
		get
		{
			return (float)this._items.ToList<TournamentDetailsMessageBaseNew.RewardItem>().Count((TournamentDetailsMessageBaseNew.RewardItem p) => p.Item.activeSelf) * this._items[0].Item.GetComponent<RectTransform>().rect.height;
		}
	}

	public void Init(List<StoreProduct> products, List<ShopLicense> licenses, List<RewardInventoryItemBrief> iiItems)
	{
		List<ResourcesHelpers.AsyncLoadableData> list = new List<ResourcesHelpers.AsyncLoadableData>();
		int num = 0;
		TournamentDetailsMessageBaseNew.AddRewardAsyncList<StoreProduct>(products, ref num, ref list, (StoreProduct brief) => brief.Name, (StoreProduct brief) => brief.ImageBID, this._items);
		if (num < this._items.Length)
		{
			TournamentDetailsMessageBaseNew.AddRewardAsyncList<ShopLicense>(licenses, ref num, ref list, (ShopLicense brief) => brief.Name, (ShopLicense brief) => brief.LogoBID, this._items);
		}
		if (num < this._items.Length)
		{
			TournamentDetailsMessageBaseNew.AddRewardAsyncList<RewardInventoryItemBrief>(iiItems, ref num, ref list, (RewardInventoryItemBrief brief) => brief.Name, (RewardInventoryItemBrief brief) => brief.ThumbnailBID, this._items);
		}
		this.RewardItemLoader.Load(list, "Textures/Inventory/{0}");
		this._le.preferredHeight = this.Height;
	}

	[SerializeField]
	private TournamentDetailsMessageBaseNew.RewardItem[] _items;

	[SerializeField]
	private LayoutElement _le;

	private ResourcesHelpers.AsyncLoadableImage RewardItemLoader = new ResourcesHelpers.AsyncLoadableImage();
}
