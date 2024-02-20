using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.Models;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;

namespace Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA
{
	[Serializable]
	public class PremiumShopParams : BaseParams
	{
		public RectTransform headerPrefab;

		public RectTransform itemPrefab;

		public RectTransform itemIcoSmallPrefab;

		[NonSerialized]
		public List<PremiumShopBaseModel> data = new List<PremiumShopBaseModel>();
	}
}
