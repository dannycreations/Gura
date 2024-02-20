using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using Assets.Scripts.UI._2D.Shop;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpdateContentItems : MonoBehaviour
{
	public event Action OnGetItems
	{
		add
		{
			this._onGetItems += value;
			this._onGetItemsListeners.Add(value);
		}
		remove
		{
			this._onGetItems -= value;
			this._onGetItemsListeners.Remove(value);
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private event Action _onGetItems = delegate
	{
	};

	public ushort Pages { get; set; }

	public ushort CurrentPage { get; set; }

	public List<InventoryItem> InventoryItems
	{
		get
		{
			return this._inventoryItems;
		}
	}

	public List<ShopLicenseContainer> LicensesItems
	{
		get
		{
			return this._licensesItems;
		}
	}

	public List<StoreProduct> ProductItems
	{
		get
		{
			return this._productItems;
		}
	}

	internal void Awake()
	{
		CacheLibrary.LocalCacheInstance.EnsureCacheCorrespondsPondId();
		CacheLibrary.LocalCacheInstance.OnUpdatedItems += this.LocalCacheInstanceOnUpdatedItems;
		CacheLibrary.MapCache.OnUpdateLicenses += this.MapCacheOnUpdateLicenses;
		CacheLibrary.GlobalShopCacheInstance.OnUpdatedItems += this.LocalCacheInstanceOnUpdatedItems;
		this._searchAction.OnFindItem += this._searchAction_OnFindItem;
	}

	internal void OnDestroy()
	{
		this._searchAction.OnFindItem -= this._searchAction_OnFindItem;
		CacheLibrary.LocalCacheInstance.OnUpdatedItems -= this.LocalCacheInstanceOnUpdatedItems;
		CacheLibrary.MapCache.OnUpdateLicenses -= this.MapCacheOnUpdateLicenses;
		CacheLibrary.GlobalShopCacheInstance.OnUpdatedItems -= this.LocalCacheInstanceOnUpdatedItems;
	}

	private void OnProductBought(ProfileProduct product, int count)
	{
		if (this._productItems != null)
		{
			this.SortCurrentContent(this.SortingInventory.SortType);
		}
	}

	private void LocalCacheInstanceOnUpdatedItems(object obj, GlobalCacheResponceEventArgs e)
	{
		this._inventoryItems = e.Items;
	}

	private void MapCacheOnUpdateLicenses(object obj, GlobalMapLicenseCacheEventArgs e)
	{
		this.FillLicenses(e.Items);
	}

	private void FillLicenses(IEnumerable<ShopLicense> licenses)
	{
		List<ShopLicense> list = licenses.ToList<ShopLicense>();
		this._licensesItems = new List<ShopLicenseContainer>();
		this.FullShopLicense = new List<ShopLicenseContainer>();
		for (int i = 0; i < list.Count; i++)
		{
			ShopLicenseContainer shopLicenseContainer = new ShopLicenseContainer(list[i]);
			this._licensesItems.Add(shopLicenseContainer);
			this.FullShopLicense.Add(shopLicenseContainer);
		}
	}

	internal void OnEnable()
	{
		CacheLibrary.GlobalShopCacheInstance.OnGetItems += this.GlobalShopCacheInstanceOnGetItems;
		CacheLibrary.LocalCacheInstance.OnGetItems += this.GlobalShopCacheInstanceOnGetItems;
		PhotonConnectionFactory.Instance.OnGotItems += this.OnGotItems;
		PhotonConnectionFactory.Instance.OnItemBought += this.OnItemBought;
		PhotonConnectionFactory.Instance.OnProductBought += this.OnProductBought;
	}

	internal void OnDisable()
	{
		CacheLibrary.GlobalShopCacheInstance.OnGetItems -= this.GlobalShopCacheInstanceOnGetItems;
		CacheLibrary.LocalCacheInstance.OnGetItems -= this.GlobalShopCacheInstanceOnGetItems;
		PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItems;
		PhotonConnectionFactory.Instance.OnItemBought -= this.OnItemBought;
		PhotonConnectionFactory.Instance.OnProductBought -= this.OnProductBought;
	}

	private void SaveItemsListAndRefresh(List<InventoryItem> items)
	{
		if (items == null)
		{
			items = new List<InventoryItem>();
		}
		DateTime now = PhotonConnectionFactory.Instance.ServerUtcNow;
		if (PondHelper.IsOnPond)
		{
			this.FullInventoryItems = items.Where((InventoryItem x) => x.IsAvailableLocally(now)).ToList<InventoryItem>();
		}
		else
		{
			this.FullInventoryItems = items.Where((InventoryItem x) => x.IsAvailableGlobally(now)).ToList<InventoryItem>();
		}
		this._inventoryItems = this.FullInventoryItems;
		this._licensesItems = null;
		this._productItems = null;
		this.SortCurrentContent(this.SortingInventory.SortType, this._inventoryItems);
	}

	private void GlobalShopCacheInstanceOnGetItems(object sender, GlobalCacheResponceEventArgs e)
	{
		this.SaveItemsListAndRefresh(e.Items);
		this._onGetItems();
		this._onGetItemsListeners.ForEach(delegate(Action p)
		{
			this._onGetItems -= p;
		});
		this._onGetItemsListeners.Clear();
	}

	private void OnGotItems(List<InventoryItem> items, int subscriberId)
	{
		if (subscriberId != 4)
		{
			return;
		}
		this.SaveItemsListAndRefresh(items);
	}

	public void SetLicenses(IEnumerable<ShopLicense> licenses)
	{
		this.OnGotLicenses(licenses);
	}

	private void OnGotLicenses(IEnumerable<ShopLicense> licenses)
	{
		this.FillLicenses(licenses);
		this._inventoryItems = null;
		this._productItems = null;
		this.SortLicenses(this.SortingInventory.SortType);
		this.UpdateShopLicense(this._licensesItems);
	}

	private void UpdateShopLicense(List<ShopLicenseContainer> licensesItems)
	{
		if (licensesItems == null || licensesItems.Count == 0)
		{
			this.SendLastLicenseItemTraverse(0);
			this.Pages = 0;
			this.CurrentPage = 0;
			this.UpdateContent(licensesItems, 1);
			return;
		}
		if (licensesItems.Count % 8 != 0)
		{
			this.Pages = (ushort)(licensesItems.Count / 8 + 1);
		}
		else
		{
			this.Pages = (ushort)(licensesItems.Count / 8);
		}
		this.CurrentPage = 1;
		this.UpdateContent(licensesItems, this.CurrentPage);
	}

	public void InitProducts(IEnumerable<StoreProduct> products)
	{
		this._productItems = products.ToList<StoreProduct>();
		this.FullProductItems = products.ToList<StoreProduct>();
		this._inventoryItems = null;
		this._licensesItems = null;
		this.SortCurrentContent(this.SortingInventory.SortType, this._productItems);
	}

	public void FilterCurrentContent(List<InventoryItem> items)
	{
		this._inventoryItems = items;
		this.SortCurrentContent(this.SortingInventory.SortType, this._inventoryItems);
	}

	public void FilterCurrentContent(List<StoreProduct> items)
	{
		this._productItems = items;
		this.SortCurrentContent(this.SortingInventory.SortType, this._productItems);
	}

	public void FilterCurrentContent(List<ShopLicenseContainer> items)
	{
		this._licensesItems = items;
		this.SortLicenses(this.SortingInventory.SortType);
		this.UpdateShopLicense(this._licensesItems);
	}

	public void SortCurrentContent(SortType sortType)
	{
		if (this._licensesItems != null)
		{
			this.SortLicenses(sortType);
			this.UpdateShopLicense(this._licensesItems);
		}
		else if (this._productItems != null)
		{
			this.SortCurrentContent(sortType, this._productItems);
		}
		else
		{
			this.SortCurrentContent(sortType, this._inventoryItems);
		}
	}

	private void SortLicenses(SortType sortType)
	{
		this._licensesItems = SortingItems.Sort(sortType, this._licensesItems);
	}

	public void SortCurrentContent(SortType sortType, List<InventoryItem> inentoryItems)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		if (inentoryItems == null || inentoryItems.Count == 0)
		{
			this.SendLastShopItemTraverse(0);
			this.UpdateContent(inentoryItems, 1);
			this.Pages = 0;
			this.CurrentPage = 0;
			return;
		}
		if (inentoryItems.Count % 10 != 0)
		{
			this.Pages = (ushort)(inentoryItems.Count / 10 + 1);
		}
		else
		{
			this.Pages = (ushort)(inentoryItems.Count / 10);
		}
		this.CurrentPage = 1;
		this._inventoryItems = SortingItems.Sort(sortType, this._inventoryItems);
		if (base.isActiveAndEnabled)
		{
			this.UpdateContent(this._inventoryItems, this.CurrentPage);
		}
	}

	public bool ContainsElement(int id)
	{
		return ShopMainPageHandler.Instance.MainContent.gameObject.activeInHierarchy && this._inventoryItems != null && this._inventoryItems.FirstOrDefault((InventoryItem x) => x.ItemId == id) != null;
	}

	public int FindPagingById(int id, UpdateContentItems.ItemsTypes iType)
	{
		if (iType == UpdateContentItems.ItemsTypes.InventoryItems)
		{
			return (this._inventoryItems == null) ? 0 : this.FindPaging(this._inventoryItems.FindIndex((InventoryItem x) => x.ItemId == id), 10);
		}
		if (iType == UpdateContentItems.ItemsTypes.Licensees)
		{
			return (this._licensesItems == null) ? 0 : this.FindPaging(this._licensesItems.FindIndex((ShopLicenseContainer x) => x.LicenseId == id), 8);
		}
		if (iType != UpdateContentItems.ItemsTypes.Products)
		{
			return 0;
		}
		return (this._productItems == null) ? 0 : this.FindPaging(this._productItems.FindIndex((StoreProduct x) => x.ProductId == id), 10);
	}

	public void SetPagingById(int id, UpdateContentItems.ItemsTypes iType)
	{
		if (iType != UpdateContentItems.ItemsTypes.InventoryItems)
		{
			if (iType != UpdateContentItems.ItemsTypes.Licensees)
			{
				if (iType == UpdateContentItems.ItemsTypes.Products)
				{
					if (this._productItems != null && this.SetPaging(this._productItems.FindIndex((StoreProduct x) => x.ProductId == id), 10))
					{
						this.UpdateContent(this._productItems, this.CurrentPage);
					}
				}
			}
			else if (this._licensesItems != null && this.SetPaging(this._licensesItems.FindIndex((ShopLicenseContainer x) => x.LicenseId == id), 8))
			{
				this.UpdateContent(this._licensesItems, this.CurrentPage);
			}
		}
		else if (this._inventoryItems != null && this.SetPaging(this._inventoryItems.FindIndex((InventoryItem x) => x.ItemId == id), 10))
		{
			this.UpdateContent(this._inventoryItems, this.CurrentPage);
		}
	}

	private int FindPaging(int index, int itemsOnPage)
	{
		if (index == -1)
		{
			return 0;
		}
		int num = (int)(this.CurrentPage - 1) * itemsOnPage;
		int num2 = num + itemsOnPage;
		if (index < num)
		{
			return -1;
		}
		if (index < num2)
		{
			return 0;
		}
		return 1;
	}

	private bool SetPaging(int index, int itemsOnPage)
	{
		if (index != -1)
		{
			ushort num = (ushort)Mathf.Ceil((float)(++index) / (float)itemsOnPage);
			if (num > this.CurrentPage)
			{
				this.CurrentPage = num;
				return true;
			}
		}
		return false;
	}

	public void SortCurrentContent(SortType sortType, List<StoreProduct> products)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		if (products == null || products.Count == 0)
		{
			this.SendLastProductItemTraverse(0);
			this.UpdateContent(products, 1);
			this.Pages = 0;
			this.CurrentPage = 0;
			return;
		}
		if (products.Count % 10 != 0)
		{
			this.Pages = (ushort)(products.Count / 10 + 1);
		}
		else
		{
			this.Pages = (ushort)(products.Count / 10);
		}
		this.CurrentPage = 1;
		if (products.Any((StoreProduct e) => this.ServicesTypes.Contains(e.TypeId)))
		{
			List<IGrouping<int, StoreProduct>> list = (from e in products
				group e by e.TypeId).ToList<IGrouping<int, StoreProduct>>();
			List<StoreProduct> list2 = new List<StoreProduct>();
			int i;
			for (i = 0; i < this.ServicesTypes.Count; i++)
			{
				IGrouping<int, StoreProduct> grouping = list.Find((IGrouping<int, StoreProduct> p) => p.Key == this.ServicesTypes[i]);
				if (grouping != null && grouping.Any<StoreProduct>())
				{
					list2.AddRange(grouping.OrderBy((StoreProduct p) => p.Price).ToList<StoreProduct>());
				}
			}
			this._productItems = list2;
			if (base.isActiveAndEnabled)
			{
				this.UpdateContent(list2, this.CurrentPage);
			}
			return;
		}
		this._productItems = SortingItems.Sort(sortType, this._productItems);
		if (base.isActiveAndEnabled)
		{
			this.UpdateContent(this._productItems, this.CurrentPage);
		}
	}

	public void ChangePage()
	{
		if (this._inventoryItems != null)
		{
			this.UpdateContent(this._inventoryItems, this.CurrentPage);
		}
		else if (this._productItems != null)
		{
			this.UpdateContent(this._productItems, this.CurrentPage);
		}
		else
		{
			this.UpdateContent(this._licensesItems, this.CurrentPage);
		}
	}

	public void SortInventoryItems(int itemId)
	{
		this.Pages = 0;
		this.UpdateContent(this._inventoryItems.FindAll((InventoryItem p) => p.ItemId == itemId), 1);
	}

	public Transform FindBuyBtn(int itemId)
	{
		return (!this._itemPanels.ContainsKey(itemId)) ? null : this._itemPanels[itemId].transform.Find("btnBuyItem");
	}

	public void UpdateContent(List<InventoryItem> inventoryItems, ushort currentPage)
	{
		this.DetailedPanel.Clear();
		this.FilterPanel.SetActive(true);
		this.ContentTransform.parent.parent.Find("Scrollbar").GetComponent<Scrollbar>().value = 0f;
		if (inventoryItems.Count > 0)
		{
			if (currentPage < this.Pages)
			{
				inventoryItems = inventoryItems.GetRange((int)((currentPage - 1) * 10), 10);
			}
			else
			{
				inventoryItems = inventoryItems.GetRange((int)((currentPage - 1) * 10), inventoryItems.Count - (int)((currentPage - 1) * 10));
			}
		}
		this.ContentTransform.gameObject.GetComponent<GridLayoutGroup>().cellSize = this.ButtonInventoryPrefab.GetComponent<RectTransform>().sizeDelta;
		for (int i = 0; i < this.ContentTransform.childCount; i++)
		{
			Object.Destroy(this.ContentTransform.GetChild(i).gameObject);
		}
		this._itemPanels.Clear();
		for (int j = 0; j < inventoryItems.Count; j++)
		{
			InventoryItem item = inventoryItems[j];
			GameObject gameObject = GUITools.AddChild(this.ContentTransform.gameObject, this.ButtonInventoryPrefab);
			gameObject.AddComponent<HintElementId>().SetElementId("Shop" + item.ItemId, (item.MinLevel > PhotonConnectionFactory.Instance.Profile.Level) ? null : new List<string>
			{
				item.ItemType.ToString(),
				item.ItemSubType.ToString()
			}, item);
			gameObject.name = "ShopItem" + j;
			gameObject.AddComponent<InventoryItemComponent>().InventoryItem = item;
			gameObject.transform.Find("Title").GetComponent<Text>().text = item.Name;
			gameObject.transform.Find("btnBuyItem").gameObject.AddComponent<HintElementId>().SetElementId("Shop" + item.ItemId + "Buy", null, null);
			Transform transform = gameObject.transform.Find("counts");
			this._itemPanels.Add(item.ItemId, gameObject);
			Inventory inventory = PhotonConnectionFactory.Instance.Profile.Inventory;
			List<InventoryItem> list = inventory.FindAll((InventoryItem inventoryItem) => inventoryItem.ItemId == item.ItemId);
			InventoryItem inventoryItem2 = null;
			if (list != null && list.Count > 0)
			{
				inventoryItem2 = list.FirstOrDefault((InventoryItem iitem) => iitem.Storage != StoragePlaces.Storage) ?? list.First<InventoryItem>();
			}
			if (inventoryItem2 != null)
			{
				this.AddIconToPanel(gameObject, inventoryItem2);
			}
			else
			{
				gameObject.transform.Find("New").gameObject.SetActive(item.IsNew);
				gameObject.transform.Find("Sale").gameObject.SetActive(item.DiscountPrice != null && item.DiscountPrice > 0.0);
				gameObject.transform.Find("Unlock").gameObject.SetActive(item.OriginalMinLevel == PhotonConnectionFactory.Instance.Profile.Level || (item.MinLevel <= PhotonConnectionFactory.Instance.Profile.Level && item.OriginalMinLevel > PhotonConnectionFactory.Instance.Profile.Level));
			}
			if (!item.IsUnstockable)
			{
				int num = InventoryHelper.ItemCount(item);
				transform.gameObject.SetActive(true);
				transform.Find("Text").GetComponent<Text>().text = string.Format("x{0}", num);
			}
			else
			{
				transform.gameObject.SetActive(false);
			}
			gameObject.transform.Find("Level").GetComponent<Text>().text = string.Format("\ue62d {1}", ScriptLocalization.Get("LevelButtonPopup"), item.OriginalMinLevel);
			Transform transform2 = gameObject.transform.Find("Price");
			if (item.PriceGold > 0.0)
			{
				transform2.Find("Value").GetComponent<Text>().text = item.PriceGold.ToString();
				transform2.Find("Currency").GetComponent<Text>().text = "\ue62c";
				transform2.Find("Currency").GetComponent<Text>().color = new Color(0.72156864f, 0.6117647f, 0f, 1f);
			}
			else
			{
				transform2.Find("Value").GetComponent<Text>().text = item.PriceSilver.ToString();
				transform2.Find("Currency").GetComponent<Text>().text = "\ue62b";
				transform2.Find("Currency").GetComponent<Text>().color = new Color(0.24313726f, 0.25490198f, 0.28627452f, 1f);
			}
			gameObject.transform.Find("btnBuyItem").GetComponent<Button>().enabled = item.MinLevel <= PhotonConnectionFactory.Instance.Profile.Level;
			Transform transform3 = gameObject.transform.Find("Border");
			if (this.imagesLdbl.Count <= j)
			{
				this.imagesLdbl.Add(new ResourcesHelpers.AsyncLoadableImage());
			}
			this.imagesLdbl[j].Image = gameObject.transform.Find("Thumbnail").GetComponent<Image>();
			this.imagesLdbl[j].Load((item.ThumbnailBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", item.ThumbnailBID));
			gameObject.GetComponent<EventAction>().ActionCalled += this.ShowDetailed_ActionCalled;
			Transform transform4 = gameObject.transform.Find("constraints");
			List<ListOfCompatibility.ConstraintType> list2 = null;
			if (ListOfCompatibility.ItemsConstraints.TryGetValue(item.ItemSubType, out list2))
			{
				transform4.gameObject.SetActive(true);
				for (int k = 0; k < transform4.childCount; k++)
				{
					if (list2.Contains((ListOfCompatibility.ConstraintType)k))
					{
						transform4.GetChild(k).GetComponent<Image>().color = this.greenColor;
					}
					else
					{
						transform4.GetChild(k).GetComponent<Image>().color = this.grayColor;
					}
				}
			}
			else
			{
				transform4.gameObject.SetActive(false);
			}
			if (j == 0)
			{
				PlayButtonEffect.Mute = true;
				if (BlockableRegion.Current != null)
				{
					BlockableRegion.Current.OverrideLastSelected(gameObject.gameObject);
				}
				else
				{
					gameObject.GetComponent<Button>().Select();
				}
				PlayButtonEffect.Mute = false;
			}
			gameObject.transform.Find("Id").gameObject.SetActive(false);
		}
	}

	private void OnItemBought(InventoryItem itemBought)
	{
		GameObject gameObject = null;
		bool flag = this._itemPanels.TryGetValue(itemBought.ItemId, out gameObject);
		if (flag)
		{
			this.AddIconToPanel(gameObject, itemBought);
		}
	}

	private void AddIconToPanel(GameObject panel, InventoryItem item)
	{
		Transform transform = panel.transform.Find("Storage");
		Transform transform2 = panel.transform.Find("Equipment");
		if (item.Storage == StoragePlaces.Storage)
		{
			transform.gameObject.SetActive(true);
		}
		else
		{
			transform2.gameObject.SetActive(true);
		}
		panel.transform.Find("New").gameObject.SetActive(false);
		panel.transform.Find("Unlock").gameObject.SetActive(false);
		panel.transform.Find("Sale").gameObject.SetActive(false);
	}

	private void UpdateContent(List<ShopLicenseContainer> licenses, ushort currentPage)
	{
		this.DetailedPanel.Clear();
		this.FilterPanel.SetActive(true);
		this.ContentTransform.parent.parent.Find("Scrollbar").GetComponent<Scrollbar>().value = 0f;
		if (licenses.Count > 0)
		{
			if (currentPage < this.Pages)
			{
				licenses = licenses.GetRange((int)((currentPage - 1) * 8), 8);
			}
			else
			{
				licenses = licenses.GetRange((int)((currentPage - 1) * 8), licenses.Count - (int)((currentPage - 1) * 8));
			}
		}
		this.ContentTransform.gameObject.GetComponent<GridLayoutGroup>().cellSize = this.ButtonLicensePrefab.GetComponent<RectTransform>().sizeDelta;
		for (int i = 0; i < this.ContentTransform.childCount; i++)
		{
			Object.Destroy(this.ContentTransform.GetChild(i).gameObject);
		}
		for (int j = 0; j < licenses.Count; j++)
		{
			ShopLicenseContainer shopLicenseContainer = licenses[j];
			GameObject gameObject = GUITools.AddChild(this.ContentTransform.gameObject, this.ButtonLicenseImagePrefab);
			gameObject.name = "ShopItem" + j;
			gameObject.GetComponent<LicenceItemShop>().Licence = shopLicenseContainer;
			gameObject.GetComponent<LicenceItemShop>().FillData(new EventHandler<EventArgs>(this.LicenseShowDetail_ActionCalled));
			gameObject.transform.Find("btnBuyItem").GetComponent<Button>().enabled = shopLicenseContainer.MinLevel <= PhotonConnectionFactory.Instance.Profile.Level;
			if (j == 0)
			{
				PlayButtonEffect.Mute = true;
				if (BlockableRegion.Current != null)
				{
					BlockableRegion.Current.OverrideLastSelected(gameObject.gameObject);
				}
				else
				{
					gameObject.GetComponent<Button>().Select();
				}
				gameObject.GetComponent<LicenceItemShop>().Change(gameObject.GetComponent<LicenceItemShop>().Term);
				PlayButtonEffect.Mute = false;
			}
		}
	}

	private void UpdateContent(List<StoreProduct> products, ushort currentPage)
	{
		this.DetailedPanel.Clear();
		this.FilterPanel.SetActive(true);
		this.ContentTransform.parent.parent.Find("Scrollbar").GetComponent<Scrollbar>().value = 0f;
		if (products.Count > 0)
		{
			if (currentPage < this.Pages)
			{
				products = products.GetRange((int)((currentPage - 1) * 10), 10);
			}
			else
			{
				products = products.GetRange((int)((currentPage - 1) * 10), products.Count - (int)((currentPage - 1) * 10));
			}
		}
		this.ContentTransform.gameObject.GetComponent<GridLayoutGroup>().cellSize = this.ButtonStorageBoxPrefab.GetComponent<RectTransform>().sizeDelta;
		for (int i = 0; i < this.ContentTransform.childCount; i++)
		{
			Object.Destroy(this.ContentTransform.GetChild(i).gameObject);
		}
		for (int j = 0; j < products.Count; j++)
		{
			StoreProduct storeProduct = products[j];
			GameObject gameObject = GUITools.AddChild(this.ContentTransform.gameObject, this.ButtonStorageBoxPrefab);
			gameObject.name = "ShopItem" + j;
			BuyClick component = gameObject.GetComponent<BuyClick>();
			if (component != null)
			{
				component.IsFastBuy = true;
			}
			StorageBoxItem component2 = gameObject.GetComponent<StorageBoxItem>();
			component2.Product = storeProduct;
			component2.FillData(new EventHandler<EventArgs>(this.StorageBoxShowDetail_ActionCalled), true);
			if (j == 0)
			{
				PlayButtonEffect.Mute = true;
				if (BlockableRegion.Current != null)
				{
					BlockableRegion.Current.OverrideLastSelected(gameObject.gameObject);
				}
				else
				{
					gameObject.GetComponent<Button>().Select();
				}
				PlayButtonEffect.Mute = false;
			}
		}
	}

	private void SendLastShopItemTraverse(int itemId = 0)
	{
		bool flag = StaticUserData.CurrentPond == null;
		UIStatsCollector.ChangeGameScreen((!flag) ? GameScreenType.LocalShop : GameScreenType.GlobalShop, GameScreenTabType.Undefined, ShopMenuClick.LastSelectedCategoryId, new int?(itemId), ShopMenuClick.LastSelectedFullCategoryIdsList, ShopMenuClick.LastSelectedCategoryElementId, ShopMenuClick.LastSelectedCategoryElementIdsPath);
	}

	private void SendLastLicenseItemTraverse(int itemId = 0)
	{
		UIStatsCollector.ChangeGameScreen(GameScreenType.LicensesShop, GameScreenTabType.Undefined, LicenseMenuClick.LastSelectedCategoryId, new int?(itemId), LicenseMenuClick.LastSelectedFullCategoryIdsList, LicenseMenuClick.LastSelectedCategoryElementId, LicenseMenuClick.LastSelectedCategoryElementIdsPath);
	}

	private void SendLastProductItemTraverse(int itemId = 0)
	{
		UIStatsCollector.ChangeGameScreen(GameScreenType.ServicesShop, GameScreenTabType.Undefined, ShopMenuClick.LastSelectedCategoryId, new int?(itemId), null, ShopMenuClick.LastSelectedCategoryElementId, null);
	}

	private void ShowDetailed_ActionCalled(object sender, EventArgs e)
	{
		InventoryItem inventoryItem = ((EventAction)sender).gameObject.GetComponent<InventoryItemComponent>().InventoryItem;
		if (inventoryItem != null)
		{
			this.DetailedPanel.Show(inventoryItem);
			this.SendLastShopItemTraverse(inventoryItem.ItemId);
		}
	}

	private void LicenseShowDetail_ActionCalled(object sender, EventArgs e)
	{
		LicenceItemShop component = ((MonoBehaviour)sender).gameObject.GetComponent<LicenceItemShop>();
		if (component != null)
		{
			this.SendLastLicenseItemTraverse(component.Licence.LicenseId);
			this.DetailedPanel.Show(component.Licence, component.Term);
		}
	}

	private void StorageBoxShowDetail_ActionCalled(object sender, EventArgs e)
	{
		StorageBoxItem component = ((MonoBehaviour)sender).gameObject.GetComponent<StorageBoxItem>();
		if (component != null)
		{
			this.SendLastProductItemTraverse(component.Product.ProductId);
			this.DetailedPanel.Show(component.Product);
		}
	}

	private void _searchAction_OnFindItem(string itemName)
	{
		if (this.isSearchingNow)
		{
			return;
		}
		this.isSearchingNow = true;
		if (string.IsNullOrEmpty(itemName))
		{
			this.ClearFiltersEndSearch();
			return;
		}
		if (this._licensesItems != null)
		{
			this._licensesItems = this._licensesItems.FindAll((ShopLicenseContainer p) => p.Name.ToUpper().Contains(itemName.ToUpper()));
			this.SortLicenses(this.SortingInventory.SortType);
			this.UpdateShopLicense(this._licensesItems);
		}
		else if (this._productItems != null)
		{
			this._productItems = this._productItems.FindAll((StoreProduct p) => p.Name.ToUpper().Contains(itemName.ToUpper()));
			this.SortCurrentContent(this.SortingInventory.SortType, this._productItems);
		}
		else
		{
			this._inventoryItems = this._inventoryItems.FindAll((InventoryItem p) => p.Name.ToUpper().Contains(itemName.ToUpper()));
			this.SortCurrentContent(this.SortingInventory.SortType, this._inventoryItems);
		}
		this.isSearchingNow = false;
	}

	public void ClearFiltersEndSearch()
	{
		this.ClearFilters();
		this.isSearchingNow = false;
	}

	public void ClearFilters()
	{
		this._searchAction.ClearFilters();
		if (this._licensesItems != null)
		{
			this._licensesItems = new List<ShopLicenseContainer>(this.FullShopLicense);
			this.SortLicenses(this.SortingInventory.SortType);
			this.UpdateShopLicense(this._licensesItems);
		}
		else if (this._productItems != null)
		{
			this._productItems = new List<StoreProduct>(this.FullProductItems);
			this.SortCurrentContent(this.SortingInventory.SortType, this._productItems);
		}
		else
		{
			this._inventoryItems = new List<InventoryItem>(this.FullInventoryItems);
			this.SortCurrentContent(this.SortingInventory.SortType, this._inventoryItems);
		}
	}

	[SerializeField]
	private SearchAction _searchAction;

	private List<Action> _onGetItemsListeners = new List<Action>();

	[SerializeField]
	private GameObject ButtonLicenseImagePrefab;

	public GameObject ButtonInventoryPrefab;

	public GameObject ButtonLicensePrefab;

	public GameObject ButtonStorageBoxPrefab;

	public ShopDetailedInfo DetailedPanel;

	public GameObject RootObject;

	public GameObject FilterPanel;

	public SortingInventory SortingInventory;

	public Transform ContentTransform;

	public float minHeigth = 100f;

	private List<InventoryItem> _inventoryItems;

	private List<ShopLicenseContainer> _licensesItems;

	private List<StoreProduct> _productItems;

	public List<InventoryItem> FullInventoryItems = new List<InventoryItem>();

	public List<StoreProduct> FullProductItems = new List<StoreProduct>();

	public List<ShopLicenseContainer> FullShopLicense = new List<ShopLicenseContainer>();

	private const ushort ItemsOnPage = 10;

	private const ushort LicensesOnPage = 8;

	private const ushort ProductsOnPage = 10;

	private Dictionary<int, GameObject> _itemPanels = new Dictionary<int, GameObject>();

	private Color grayColor = new Color(0.68235296f, 0.68235296f, 0.68235296f, 1f);

	private Color greenColor = new Color(0.41568628f, 0.6117647f, 0.42745098f, 1f);

	public readonly IList<int> ServicesTypes = new ReadOnlyCollection<int>(new List<int> { 7, 6, 5, 8 });

	private List<ResourcesHelpers.AsyncLoadableImage> imagesLdbl = new List<ResourcesHelpers.AsyncLoadableImage>();

	private bool isSearchingNow;

	public enum ItemsTypes : byte
	{
		Products,
		InventoryItems,
		Licensees
	}
}
