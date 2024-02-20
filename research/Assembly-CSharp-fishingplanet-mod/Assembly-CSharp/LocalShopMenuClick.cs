using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;

public class LocalShopMenuClick : MonoBehaviour
{
	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGettingItemsFailed -= this.Instance_OnGettingItemsFailed;
	}

	public string CategoryElementId
	{
		get
		{
			HintElementId hintElementId = base.GetComponent<HintElementId>();
			if (hintElementId == null)
			{
				hintElementId = base.gameObject.AddComponent<HintElementId>();
				hintElementId.SetElementId(string.Join(string.Empty, this.CategoryNames.ToArray()), null, null);
			}
			return hintElementId.GetElementId();
		}
	}

	public void OnClick()
	{
		if (ShopMainPageHandler.Instance != null)
		{
			ShopMainPageHandler.Instance.MenuClick(false);
		}
		LocalShopMenuClick.LastSelectedCategoryElementId = this.CategoryElementId;
		if (this.CategoryNames.Count > 0)
		{
			List<InventoryCategory> list = StaticUserData.AllCategories.Where(delegate(InventoryCategory x)
			{
				if (x.ItemSubType != null)
				{
					if (this.CategoryNames.ConvertAll<string>((string y) => y.ToUpper()).Contains(Enum.GetName(typeof(ItemSubTypes), x.ItemSubType).ToUpper()))
					{
						return true;
					}
				}
				return this.CategoryNames.ConvertAll<string>((string y) => y.ToUpper()).Contains(Enum.GetName(typeof(ItemTypes), x.ItemType).ToUpper());
			}).ToList<InventoryCategory>();
			IEnumerable<int> enumerable = list.Select((InventoryCategory y) => y.CategoryId);
			LocalShopMenuClick.LastSelectedFullCategoryIdsList = enumerable.ToArray<int>();
			List<string> list2 = new List<string>();
			foreach (int num in LocalShopMenuClick.LastSelectedFullCategoryIdsList)
			{
				List<string> list3 = HintSystem.GenerateShopPathByType((ItemSubTypes)num);
				foreach (string text in list3)
				{
					if (!list2.Contains(text))
					{
						list2.Add(text);
					}
				}
			}
			int? parentCategoryId = list[0].ParentCategoryId;
			LocalShopMenuClick.LastSelectedCategoryId = new int?((parentCategoryId == null) ? list[0].CategoryId : parentCategoryId.Value);
			if (!list2.Contains(LocalShopMenuClick.LastSelectedCategoryElementId))
			{
				list2.Add(LocalShopMenuClick.LastSelectedCategoryElementId);
			}
			LocalShopMenuClick.LastSelectedCategoryElementIdsPath = list2.ToArray();
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
			CacheLibrary.LocalCacheInstance.GetItemsFromCategory(LocalShopMenuClick.LastSelectedFullCategoryIdsList, StaticUserData.CurrentPond.PondId);
		}
		else if (this.ItemIds.Count > 0)
		{
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
			PhotonConnectionFactory.Instance.OnGettingItemsFailed += this.Instance_OnGettingItemsFailed;
			PhotonConnectionFactory.Instance.GetItemsByIds(this.ItemIds.ToArray(), 4, false);
		}
		else if (this.ProductTypes.Count > 0)
		{
			LocalShopMenuClick.LastSelectedCategoryId = new int?(this.ProductTypes[0]);
			IEnumerable<StoreProduct> enumerable2 = CacheLibrary.ProductCache.Products.Where((StoreProduct x) => this.ProductTypes.Any((int y) => y == x.TypeId));
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
			ShopMainPageHandler.Instance.ContentUpdater.InitProducts(enumerable2);
		}
	}

	private void Instance_OnGettingItemsFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGettingItemsFailed -= this.Instance_OnGettingItemsFailed;
		CursorManager.Instance.SetCursor(CursorType.Standart);
	}

	[SerializeField]
	private ShopMenuClick.ShopMenuTypes MenuType;

	public List<string> CategoryNames;

	public List<int> ItemIds;

	public List<int> CategoryIds;

	public List<int> ProductTypes;

	public int[] ChildrenCategoryIds;

	public static int? LastSelectedCategoryId;

	public static string LastSelectedCategoryElementId;

	public static string[] LastSelectedCategoryElementIdsPath;

	public static int[] LastSelectedFullCategoryIdsList;
}
