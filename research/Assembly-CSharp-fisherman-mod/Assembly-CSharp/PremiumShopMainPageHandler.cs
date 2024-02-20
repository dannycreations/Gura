using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using I2.Loc;
using ObjectModel;
using Photon.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class PremiumShopMainPageHandler : MainPageHandler<PremiumShopCategories>
{
	private List<OfferClient> Offers { get; set; }

	protected override void InitUi()
	{
		this.Category = PremiumShopCategories.PremiumAccs;
		for (int i = 0; i < this._categoriesContainers.Length; i++)
		{
			PremiumShopMainPageHandler.CategoriesContainer categoriesContainer = this._categoriesContainers[i];
			this.CategoriesGo[categoriesContainer.Category] = categoriesContainer.Go;
			this.CreateHeaderData(categoriesContainer.Header, categoriesContainer.Category);
			this.InitHeaderToggleClickLogic(categoriesContainer.Header, categoriesContainer.Category);
		}
	}

	public static bool IsIngameProduct(StoreProduct p)
	{
		return p != null && Enum.IsDefined(typeof(ProductTypes), p.TypeId) && PremiumShopMainPageHandler.IngameProducts.Contains(p.TypeId);
	}

	public static bool IsProductListAvailable
	{
		get
		{
			return true;
		}
	}

	private void Awake()
	{
		this.Category = PremiumShopCategories.None;
		PhotonConnectionFactory.Instance.OnGotOffers += this.Instance_OnGotOffers;
		this._conventor.OnActive += this._conventor_OnActive;
	}

	protected override void OnDestroy()
	{
		base.StopAllCoroutines();
		PhotonConnectionFactory.Instance.OnGotOffers -= this.Instance_OnGotOffers;
		this._conventor.OnActive -= this._conventor_OnActive;
		base.OnDestroy();
	}

	protected override void HideHelp()
	{
		base.HideHelp();
	}

	protected override void SetHelp()
	{
		this.SetHelpActivityState();
		if (!PremiumShopMainPageHandler.IsProductListAvailable)
		{
			for (int i = 0; i < this._categoriesContainers.Length; i++)
			{
				this._categoriesContainers[i].Header.gameObject.SetActive(false);
			}
			return;
		}
		if (this.CategoriesGo.Count == 0)
		{
			this.InitMainData();
			this.InitUi();
			for (int j = 0; j < this._categoriesContainers.Length; j++)
			{
				PremiumShopMainPageHandler.CategoriesContainer categoriesContainer = this._categoriesContainers[j];
				this.CategoriesGo[categoriesContainer.Category] = categoriesContainer.Go;
				this.CreateHeaderData(categoriesContainer.Header, categoriesContainer.Category);
				if (categoriesContainer.Category == PremiumShopCategories.PersonalOffers)
				{
					categoriesContainer.Header.gameObject.SetActive(false);
				}
				if (categoriesContainer.Category == PremiumShopCategories.Sales)
				{
					categoriesContainer.Header.gameObject.SetActive(this._productsByCategoriesDiscount.Count > 0);
				}
				else if (categoriesContainer.Category == PremiumShopCategories.MoneyPacks)
				{
					categoriesContainer.Header.gameObject.SetActive(this._productsByCategories.ContainsKey(2));
				}
				else if (categoriesContainer.Category == PremiumShopCategories.PremiumAccs)
				{
					categoriesContainer.Header.gameObject.SetActive(this._productsByCategories.ContainsKey(3));
				}
				else if (categoriesContainer.Category == PremiumShopCategories.FishingPacks)
				{
					categoriesContainer.Header.gameObject.SetActive(this._productsByCategories.Count > 0);
				}
				if (this.Category == PremiumShopCategories.None && categoriesContainer.Header.gameObject.activeSelf)
				{
					this.SetCategory(categoriesContainer.Category);
				}
			}
			this.CreateHeaderData(this._conventorToggle, PremiumShopCategories.None);
			this.UpdateCategory();
			this.ResetMainMenuNavigation();
		}
		PhotonConnectionFactory.Instance.GetOffers();
	}

	protected override void UpdateCategory()
	{
		base.UpdateCategory();
		PremiumShopMainPageHandler.IPremiumCategory component = this.CategoriesGo[this.Category].GetComponent<PremiumShopMainPageHandler.IPremiumCategory>();
		if (this.Category == PremiumShopCategories.MoneyPacks && !component.IsInited)
		{
			this.CategoryMgrInit<MoneyPackItem>(2, component, this.Category);
		}
		else if (this.Category == PremiumShopCategories.PremiumAccs && !component.IsInited)
		{
			this.CategoryMgrInit<PremiumItemBase>(3, component, this.Category);
		}
		else if (this.Category == PremiumShopCategories.PersonalOffers)
		{
			this.SetActivePersonalOffers(component);
		}
		else if (this.Category == PremiumShopCategories.Sales)
		{
			this.SetActiveFishingPacks(component, this._productsByCategoriesDiscount, PremiumShopCategories.Sales);
		}
		else if (this.Category == PremiumShopCategories.FishingPacks)
		{
			this.SetActiveFishingPacks(component, this._productsByCategories, PremiumShopCategories.FishingPacks);
		}
	}

	private void SetActiveFishingPacks(PremiumShopMainPageHandler.IPremiumCategory categoryMgr, Dictionary<int, List<StoreProduct>> productsByCategories, PremiumShopCategories cat)
	{
		List<PremiumShopMainPageHandler.ProductCategories> skip = new List<PremiumShopMainPageHandler.ProductCategories>
		{
			PremiumShopMainPageHandler.ProductCategories.MoneyPack,
			PremiumShopMainPageHandler.ProductCategories.PremiumAccount
		};
		bool flag = cat == PremiumShopCategories.Sales && productsByCategories.Count > 1;
		List<PremiumCategoryManager.PremiumItem> list = new List<PremiumCategoryManager.PremiumItem>();
		foreach (KeyValuePair<int, List<StoreProduct>> keyValuePair in productsByCategories)
		{
			if (!skip.Contains((PremiumShopMainPageHandler.ProductCategories)keyValuePair.Key))
			{
				List<StoreProduct> value = keyValuePair.Value;
				for (int i = 0; i < value.Count; i++)
				{
					PremiumCategoryManager.PremiumItem premiumItem = new PremiumCategoryManager.PremiumItem
					{
						Product = value[i],
						UiCategory = cat,
						Category = (PremiumShopMainPageHandler.ProductCategories)keyValuePair.Key
					};
					list.Add(premiumItem);
					if (flag)
					{
						premiumItem = premiumItem.Clone() as PremiumCategoryManager.PremiumItem;
						if (premiumItem != null)
						{
							premiumItem.Category = PremiumShopMainPageHandler.ProductCategories.All;
						}
						list.Add(premiumItem);
					}
				}
			}
		}
		bool hasWn = false;
		List<WhatsNewItem> list2;
		if (CacheLibrary.ProductCache.WhatsNewItems == null)
		{
			list2 = new List<WhatsNewItem>();
		}
		else
		{
			list2 = (from p in CacheLibrary.ProductCache.WhatsNewItems
				where p.Start != null && p.Start <= TimeHelper.UtcTime() && p.End != null && p.End >= TimeHelper.UtcTime()
				orderby p.OrderId
				select p).Take(2).ToList<WhatsNewItem>();
		}
		List<WhatsNewItem> list3 = list2;
		for (int j = 0; j < list3.Count; j++)
		{
			WhatsNewItem wn = list3[j];
			StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct el) => wn.ProductId != null && wn.ProductId == el.ProductId);
			if (storeProduct != null && PremiumItemBase.HasValue(storeProduct.Price))
			{
				if (cat != PremiumShopCategories.Sales || ((storeProduct.DiscountStart == null || storeProduct.DiscountEnd == null) ? PremiumItemBase.HasDiscount(storeProduct) : (PremiumItemBase.HasDiscount(storeProduct) && PremiumItemBase.HasActiveDiscount(storeProduct))))
				{
					LogHelper.Log("___kocha WhatsNew ItemId:{0} Name:{1} ProductId:{2}", new object[] { wn.ItemId, storeProduct.Name, storeProduct.ProductId });
					hasWn = true;
					list.Add(new PremiumCategoryManager.PremiumItem
					{
						Product = storeProduct,
						UiCategory = cat,
						WhatsNew = wn,
						Category = PremiumShopMainPageHandler.ProductCategories.Promotions
					});
				}
			}
		}
		List<int> categoriesIds = productsByCategories.Keys.Where((int p) => !skip.Contains((PremiumShopMainPageHandler.ProductCategories)p)).ToList<int>();
		List<ProductCategory> list4 = (from p in CacheLibrary.ProductCache.ProductCategories
			where categoriesIds.Contains(p.CategoryId) || (hasWn && p.CategoryId == 9)
			select p into el
			orderby el.OrderId
			select el).ToList<ProductCategory>();
		categoryMgr.Init<PremiumFishingPackInit>(list);
		if (flag)
		{
			list4.Insert(0, new ProductCategory
			{
				CategoryId = -1,
				Name = ScriptLocalization.Get("AllSalesCaption"),
				OrderId = 0
			});
		}
		categoryMgr.InitCategories(list4);
	}

	private void SetActivePersonalOffers(PremiumShopMainPageHandler.IPremiumCategory categoryMgr)
	{
		if (this.Offers != null)
		{
			List<OfferClient> list = this.Offers.OrderBy((OfferClient p) => p.OrderId).Take(3).ToList<OfferClient>();
			List<PremiumCategoryManager.PremiumItem> list2 = new List<PremiumCategoryManager.PremiumItem>();
			for (int i = 0; i < list.Count; i++)
			{
				OfferClient o = list[i];
				StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct el) => el.ProductId == o.ProductId);
				if (storeProduct != null && PremiumItemBase.HasValue(storeProduct.Price))
				{
					list2.Add(new PremiumCategoryManager.PremiumItem
					{
						Product = storeProduct,
						UiCategory = PremiumShopCategories.PersonalOffers,
						Offer = o
					});
				}
			}
			categoryMgr.Init<OfferPackInit>(list2);
		}
	}

	private void CategoryMgrInit<T>(int categoryId, PremiumShopMainPageHandler.IPremiumCategory categoryMgr, PremiumShopCategories cat) where T : PremiumItemBase
	{
		ProductCategory productCategory = CacheLibrary.ProductCache.ProductCategories.FirstOrDefault((ProductCategory p) => p.CategoryId == categoryId);
		if (productCategory != null)
		{
			List<int> productIds = productCategory.ProductIds.ToList<int>();
			categoryMgr.Init<T>((from p in CacheLibrary.ProductCache.Products
				where productIds.Contains(p.ProductId)
				orderby productIds.IndexOf(p.ProductId)
				select new PremiumCategoryManager.PremiumItem
				{
					Product = p,
					UiCategory = cat,
					Category = (PremiumShopMainPageHandler.ProductCategories)categoryId
				}).ToList<PremiumCategoryManager.PremiumItem>());
		}
	}

	protected override void SetActiveCategory(Dictionary<PremiumShopCategories, MainPageItem> categoriesGo)
	{
		foreach (KeyValuePair<PremiumShopCategories, MainPageItem> keyValuePair in categoriesGo)
		{
			if (this.Category == PremiumShopCategories.Sales || this.Category == PremiumShopCategories.FishingPacks)
			{
				keyValuePair.Value.SetActive(keyValuePair.Key == PremiumShopCategories.Sales || keyValuePair.Key == PremiumShopCategories.FishingPacks);
			}
			else
			{
				keyValuePair.Value.SetActive(keyValuePair.Key == this.Category);
			}
		}
	}

	private void _conventor_OnActive(bool isActive)
	{
		if (!isActive)
		{
			this._conventor.GetComponent<SelectedControl>().ResetSelected();
		}
		if (!PremiumShopMainPageHandler.IsProductListAvailable)
		{
			return;
		}
		if (isActive)
		{
			this.UpdateHeaders(PremiumShopCategories.None);
		}
		else
		{
			for (int i = 0; i < this._categoriesContainers.Length; i++)
			{
				if (this._categoriesContainers[i].Category == this.Category)
				{
					UINavigation.SetSelectedGameObject(this._categoriesContainers[i].Header.gameObject);
					break;
				}
			}
			this.UpdateCategory();
		}
	}

	private void Instance_OnGotOffers(List<OfferClient> offers)
	{
		LogHelper.Log("___kocha OnGotOffers offersCount:{0} _category:{1}", new object[]
		{
			(offers == null) ? (-1) : offers.Count,
			this.Category
		});
		bool flag = (this.Offers == null && offers != null && offers.Count > 0) || (offers == null && this.Offers != null && this.Offers.Count > 0);
		if (flag)
		{
			this.Offers = offers;
			bool flag2 = this.Offers != null && this.Offers.Count > 0;
			PremiumShopCategories premiumShopCategories = PremiumShopCategories.None;
			for (int i = 0; i < this._categoriesContainers.Length; i++)
			{
				PremiumShopMainPageHandler.CategoriesContainer categoriesContainer = this._categoriesContainers[i];
				if (categoriesContainer.Category == PremiumShopCategories.PersonalOffers)
				{
					categoriesContainer.Header.gameObject.SetActive(flag2);
				}
				if (premiumShopCategories == PremiumShopCategories.None && categoriesContainer.Header.gameObject.activeSelf)
				{
					this.SetCategory(categoriesContainer.Category);
				}
			}
			this.ResetMainMenuNavigation();
			if (this.Category == PremiumShopCategories.PersonalOffers)
			{
				if (!flag2)
				{
					this.SetCategory(premiumShopCategories);
				}
			}
			else
			{
				this.SetCategory(PremiumShopCategories.PersonalOffers);
			}
			base.StartCoroutine(this.UpdateCategoryWithDelay());
		}
	}

	private IEnumerator UpdateCategoryWithDelay()
	{
		yield return new WaitForEndOfFrame();
		this.UpdateCategory();
		yield break;
	}

	protected override List<PremiumShopCategories> CatsSkippedAnim()
	{
		return new List<PremiumShopCategories> { PremiumShopCategories.None };
	}

	protected override void InitMainData()
	{
		List<StoreProduct> products = CacheLibrary.ProductCache.Products;
		List<ProductCategory> list = CacheLibrary.ProductCache.ProductCategories.OrderBy((ProductCategory p) => p.OrderId).ToList<ProductCategory>();
		for (int i = 0; i < list.Count; i++)
		{
			ProductCategory productCategory = list[i];
			List<int> productsIds = productCategory.ProductIds.ToList<int>();
			List<StoreProduct> list2 = (from p in products
				where PremiumItemBase.HasValue(p.Price) && productsIds.Contains(p.ProductId)
				orderby productsIds.IndexOf(p.ProductId)
				select p).ToList<StoreProduct>();
			if (list2.Any<StoreProduct>())
			{
				this._productsByCategories[productCategory.CategoryId] = list2;
				List<StoreProduct> list3 = this._productsByCategories[productCategory.CategoryId].Where((StoreProduct p) => (p.DiscountStart == null || p.DiscountEnd == null) ? PremiumItemBase.HasDiscount(p) : (PremiumItemBase.HasDiscount(p) && PremiumItemBase.HasActiveDiscount(p))).ToList<StoreProduct>();
				if (list3.Count > 0)
				{
					this._productsByCategoriesDiscount[productCategory.CategoryId] = list3;
				}
				LogHelper.Log("___kocha Category:{0} Count:{1} Discount:{2}", new object[] { productCategory.Name, list2.Count, list3.Count });
			}
		}
	}

	[SerializeField]
	private ShowConverter _conventor;

	[SerializeField]
	private Toggle _conventorToggle;

	[SerializeField]
	private PremiumShopMainPageHandler.CategoriesContainer[] _categoriesContainers;

	public static readonly IList<ProductTypes> IngameProducts = new ReadOnlyCollection<ProductTypes>(new List<ProductTypes> { 1, 3, 4 });

	private readonly Dictionary<int, List<StoreProduct>> _productsByCategories = new Dictionary<int, List<StoreProduct>>();

	private readonly Dictionary<int, List<StoreProduct>> _productsByCategoriesDiscount = new Dictionary<int, List<StoreProduct>>();

	private const int PromotionsMax = 2;

	private const int OffersMax = 3;

	public interface IPremiumCategory
	{
		void Init<T>(List<PremiumCategoryManager.PremiumItem> products) where T : PremiumItemBase;

		void InitCategories(List<ProductCategory> list);

		bool IsInited { get; }
	}

	public enum ProductCategories
	{
		All = -1,
		Default = 1,
		MoneyPack,
		PremiumAccount,
		SportPacks,
		TournamentPacks,
		EventPacks,
		StarterPacks,
		BoatPacks,
		Promotions,
		Dlc
	}

	[Serializable]
	public class CategoriesContainer
	{
		public PremiumShopCategories Category;

		public MainPageItem Go;

		public Toggle Header;
	}
}
