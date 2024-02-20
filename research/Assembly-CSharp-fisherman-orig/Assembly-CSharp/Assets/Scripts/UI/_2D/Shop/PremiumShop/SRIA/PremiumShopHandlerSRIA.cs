using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.Models;
using Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.ViewHolders;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA
{
	public class PremiumShopHandlerSRIA : SRIA<PremiumShopParams, PremiumShopBaseVH>
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnInited = delegate
		{
		};

		protected override void Start()
		{
			base.Start();
			this.OnInited();
		}

		public void UpdateData(List<PremiumShopBaseModel> models)
		{
			this._Params.data.AddRange(models);
			this.ResetItems(this._Params.data.Count, false, false);
		}

		protected override PremiumShopBaseVH CreateViewsHolder(int itemIndex)
		{
			Type cachedType = this._Params.data[itemIndex].CachedType;
			if (cachedType == typeof(HeaderPremiumShopGroupModel))
			{
				HeaderPremiumShopGroupVH headerPremiumShopGroupVH = new HeaderPremiumShopGroupVH();
				headerPremiumShopGroupVH.Init(this._Params.headerPrefab, itemIndex, true, true);
				return headerPremiumShopGroupVH;
			}
			if (cachedType == typeof(PremiumShopItemModel))
			{
				PremiumShopItemVH premiumShopItemVH = new PremiumShopItemVH();
				premiumShopItemVH.Init(this._Params.itemPrefab, itemIndex, true, true);
				return premiumShopItemVH;
			}
			if (cachedType == typeof(PremiumShopItemModelIcoSmall))
			{
				PremiumShopItemIcoSmallVH premiumShopItemIcoSmallVH = new PremiumShopItemIcoSmallVH();
				premiumShopItemIcoSmallVH.Init(this._Params.itemIcoSmallPrefab, itemIndex, true, true);
				return premiumShopItemIcoSmallVH;
			}
			throw new InvalidOperationException(string.Format("PremiumShopHandlerSRIA - Unrecognized model type:{0}", cachedType.Name));
		}

		protected override void UpdateViewsHolder(PremiumShopBaseVH newOrRecycled)
		{
			PremiumShopBaseModel premiumShopBaseModel = this._Params.data[newOrRecycled.ItemIndex];
			newOrRecycled.root.gameObject.SetActive(true);
			newOrRecycled.UpdateViews(premiumShopBaseModel);
		}

		protected override void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
		{
			base.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, itemsDesc);
			if (changeMode == ItemCountChangeMode.REMOVE || count == 0)
			{
				return;
			}
			int num;
			if (changeMode == ItemCountChangeMode.RESET)
			{
				num = 0;
			}
			else
			{
				num = indexIfInsertingOrRemoving;
			}
			int num2 = num + count;
			itemsDesc.BeginChangingItemsSizes(num);
			for (int i = num; i < num2; i++)
			{
				itemsDesc[i] = (float)this._Params.data[i].GetHeight();
			}
			itemsDesc.EndChangingItemsSizes();
		}

		protected override bool IsRecyclable(PremiumShopBaseVH potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float heightOfItemThatWillBecomeVisible)
		{
			return EventSystem.current != null && potentiallyRecyclable != null && potentiallyRecyclable.root != null && EventSystem.current.currentSelectedGameObject != potentiallyRecyclable.root.gameObject && this._Params.data != null && indexOfItemThatWillBecomeVisible < this._Params.data.Count && potentiallyRecyclable.CanPresentModelType(this._Params.data[indexOfItemThatWillBecomeVisible].CachedType);
		}
	}
}
