using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopMenuClick : MonoBehaviour
{
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
		ShopMenuClick.LastSelectedCategoryElementId = this.CategoryElementId;
		if (this.CategoryNames.Count > 0)
		{
			List<string> catNamesUpper = this.CategoryNames.ConvertAll<string>((string y) => y.ToUpper());
			List<InventoryCategory> list = StaticUserData.AllCategories.Where((InventoryCategory x) => (x.ItemSubType != null && this.CheckItemType<ItemSubTypes>(catNamesUpper, x.ItemSubType.Value)) || (!this._childCategoryOnly && this.CheckItemType<ItemTypes>(catNamesUpper, x.ItemType))).ToList<InventoryCategory>();
			ShopMenuClick.LastSelectedFullCategoryIdsList = list.Select((InventoryCategory y) => y.CategoryId).ToArray<int>();
			List<string> list2 = new List<string>();
			for (int i = 0; i < ShopMenuClick.LastSelectedFullCategoryIdsList.Length; i++)
			{
				int num = ShopMenuClick.LastSelectedFullCategoryIdsList[i];
				List<string> list3 = HintSystem.GenerateShopPathByType((ItemSubTypes)num);
				for (int j = 0; j < list3.Count; j++)
				{
					string text = list3[j];
					if (!list2.Contains(text))
					{
						list2.Add(text);
					}
				}
			}
			if (list.Count == 0)
			{
				string text2 = string.Format("name:{0} AllCategoriesCount:{1} onGlobalMap:{2}", base.name, (StaticUserData.AllCategories == null) ? (-1) : StaticUserData.AllCategories.Count, StaticUserData.CurrentPond == null);
				string text3 = string.Empty;
				for (int k = 0; k < this.CategoryNames.Count; k++)
				{
					text3 = text3 + this.CategoryNames[k] + ";";
				}
				PhotonConnectionFactory.Instance.PinError(string.Format("ShopMenuClick:{0}", text3), text2);
				return;
			}
			int? parentCategoryId = list[0].ParentCategoryId;
			ShopMenuClick.LastSelectedCategoryId = new int?((parentCategoryId == null) ? list[0].CategoryId : parentCategoryId.Value);
			if (!list2.Contains(ShopMenuClick.LastSelectedCategoryElementId))
			{
				list2.Add(ShopMenuClick.LastSelectedCategoryElementId);
			}
			ShopMenuClick.LastSelectedCategoryElementIdsPath = list2.ToArray();
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
			if (StaticUserData.CurrentPond == null)
			{
				CacheLibrary.GlobalShopCacheInstance.GetItemsFromCategory(ShopMenuClick.LastSelectedFullCategoryIdsList);
			}
			else
			{
				CacheLibrary.LocalCacheInstance.GetItemsFromCategory(ShopMenuClick.LastSelectedFullCategoryIdsList, StaticUserData.CurrentPond.PondId);
			}
		}
		else if (this.ItemIds.Count > 0)
		{
			PhotonConnectionFactory.Instance.GetItemsByIds(this.ItemIds.ToArray(), 4, false);
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		}
		else if (this.ProductTypes.Count > 0)
		{
			ShopMenuClick.LastSelectedCategoryId = new int?(this.ProductTypes[0]);
			IEnumerable<StoreProduct> enumerable = CacheLibrary.ProductCache.Products.Where((StoreProduct x) => this.ProductTypes.Any((int y) => y == x.TypeId));
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
			ShopMainPageHandler.Instance.ContentUpdater.InitProducts(enumerable);
		}
	}

	public void OnClickByItemsId()
	{
		ShopMainPageHandler.Instance.MenuClick(false);
		PhotonConnectionFactory.Instance.GetItemsByIds(this.ItemIds.ToArray(), 4, false);
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
	}

	private bool CheckItemType<T>(List<string> catNamesUpper, T o)
	{
		string name = Enum.GetName(typeof(T), o);
		return !string.IsNullOrEmpty(name) && catNamesUpper.Contains(name.ToUpper());
	}

	[SerializeField]
	private ShopMenuClick.ShopMenuTypes MenuType;

	[Space(15f)]
	[SerializeField]
	private bool _childCategoryOnly;

	[Space(15f)]
	public const int ItemSubscriberId = 4;

	public List<string> CategoryNames;

	public List<int> ItemIds;

	public List<int> ProductTypes;

	public int[] ChildrenCategoryIds;

	public static int? LastSelectedCategoryId;

	public static string LastSelectedCategoryElementId;

	public static string[] LastSelectedCategoryElementIdsPath;

	public static int[] LastSelectedFullCategoryIdsList;

	public enum ShopMenuTypes : byte
	{
		None,
		Service
	}
}
