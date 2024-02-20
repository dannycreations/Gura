using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Shop.PremiumShop;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using DG.Tweening;
using I2.Loc;
using InControl;
using ObjectModel;
using Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PremiumShopMainHandler : ActivityStateControlled
{
	public bool EnabledAddProductWithoutDiscountPrice
	{
		get
		{
			return false;
		}
	}

	private bool HideBought
	{
		get
		{
			return false;
		}
	}

	private Camera Cam
	{
		get
		{
			return MenuHelpers.Instance.GUICamera;
		}
	}

	private bool IsHome
	{
		get
		{
			return this._category == PremiumShopMainHandler.ProductCategories.Home;
		}
	}

	private bool IsPondPasses
	{
		get
		{
			return this._category == PremiumShopMainHandler.ProductCategories.PondPasses;
		}
	}

	private int VisibleCount
	{
		get
		{
			return this._rtItems.Count((RectTransform p) => p.gameObject.activeSelf);
		}
	}

	private bool IsMouse
	{
		get
		{
			return InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse;
		}
	}

	private void Awake()
	{
		this._titleConverter.text = ScriptLocalization.Get("PremShop_Converter");
		this._rmbHelp = string.Format("{0} {1}", UgcConsts.GetYellowTan(HotkeyIcons.MouseIcoMappings[Mouse.RightButton]), ScriptLocalization.Get("Details").ToUpper());
		this._menuBgX0 = this._menuBg.anchoredPosition.x;
		this._menuContentX0 = this._menuContent.anchoredPosition.x;
		this._titleLeftRtX0 = this._titleLeftRt.anchoredPosition.x;
		this._titleRightRtX0 = this._titleRightRt.anchoredPosition.x;
		this.InitConverter();
		this._btnBack.onClick.AddListener(delegate
		{
			int num = this._menuItems.FindIndex((PremiumShopMenuItemHandler menu) => menu.IsActive);
			this.FullReset((num == -1) ? 0 : num);
		});
		this._btnBack.PointerEnter += delegate(bool b)
		{
			this._btnBackArrow.text = ((!b) ? "\ue625" : "\ue626");
			this._btnBackHelpTitle.gameObject.SetActive(b);
		};
		this._btnBackHelpTitle.text = ScriptLocalization.Get("PremShop_Menu");
		this.ResetBackBtn();
		this._titleLeft.gameObject.SetActive(false);
		this._titleRight.gameObject.SetActive(false);
		this._titlePondPasses.gameObject.SetActive(false);
		for (int i = 0; i < 2; i++)
		{
			PremShopItemHandler component = GUITools.AddChild(this._prefabItemContent, this._prefabItemBig).GetComponent<PremShopItemHandler>();
			component.SetActive(false);
			this._items.Add(component);
			this._rtItems.Add(component.GetComponent<RectTransform>());
		}
		for (int j = 0; j < 24; j++)
		{
			GameObject gameObject = GUITools.AddChild(this._prefabItemContent, this._prefabItem);
			for (int k = 0; k < 2; k++)
			{
				PremShopItemHandler component2 = GUITools.AddChild(gameObject, this._prefabItemSmall).GetComponent<PremShopItemHandler>();
				component2.SetParent(gameObject);
				component2.SetActive(false);
				this._items.Add(component2);
			}
			PremShopItemHandler premShopItemHandler = this._items[this._items.Count - 2];
			PremShopItemHandler premShopItemHandler2 = this._items[this._items.Count - 1];
			premShopItemHandler.SetLinked(premShopItemHandler2);
			premShopItemHandler2.SetLinked(premShopItemHandler);
			gameObject.SetActive(false);
			this._rtItems.Add(gameObject.GetComponent<RectTransform>());
		}
		PhotonConnectionFactory.Instance.OnGotOffers += this.Instance_OnGotOffers;
		PhotonConnectionFactory.Instance.OnProductDelivered += this.OnProductDelivered;
		this._mouseOverMovement = new PremiumShopMouseOverMovement(this._mouseOverAndMovement, this._rtItems, this.Cam, new Func<bool>(this.get_IsMouse), () => this.IsHome || this.IsPondPasses, new Func<bool>(this.get_IsLastInVisibleRect), -1472.4f, 233.55f, 544f, this._mouseOverAndMovementLeft);
		this._mouseOverMovement.OnStart += delegate
		{
			this.SetActiveMenu(true, false, 0.25f);
		};
		this._mouseOverMovement.OnMoveAllItems += delegate(int index, float diff)
		{
			this._index = index;
			this.MoveAllItems(diff, 0f);
			if (this._index == 0)
			{
				this.SetActiveMenu(false, true, 0.25f);
			}
		};
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnGotOffers -= this.Instance_OnGotOffers;
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.OnProductDelivered;
	}

	private void Update()
	{
		if (this._mouseOverMovement != null)
		{
			this._mouseOverMovement.Update(this.MouseOverAndMovementTimeForStartAnim, this.MouseOverAndMovementAnimSpeed);
			if (this.IsMouse)
			{
				this.UpdateScrollBtns();
			}
		}
	}

	protected override void SetHelp()
	{
		HelpLinePanel.SetVisibleBg(false);
		base.SetHelp();
		this.CheckActiveMouseHelp(false);
		if (!this._isInited)
		{
			this._isInited = true;
			this.InitMainData();
		}
		else
		{
			this.FullReset(0);
		}
		PhotonConnectionFactory.Instance.GetOffers();
		if (ScreenManager.Instance != null && ScreenManager.Instance.GameScreen != GameScreenType.PremiumShop)
		{
			UIStatsCollector.ChangeGameScreen(GameScreenType.PremiumShop, GameScreenTabType.Undefined, null, null, null, null, null);
		}
	}

	protected override void HideHelp()
	{
		HelpLinePanel.SetVisibleBg(true);
		HelpLinePanel.SetMouseHelp(string.Empty);
		base.HideHelp();
		this.CheckActiveMouseHelp(true);
	}

	protected override void OnInputTypeChangedAs(InputModuleManager.InputType inputType)
	{
		this.FullReset(0);
	}

	public static bool IsFastBuy
	{
		get
		{
			return CacheLibrary.MapCache.IsAbTestActive(Constants.AB_TESTS.PREM_SHOP_BUY_OPTIONS);
		}
	}

	public void BtnsLeftMouseOver()
	{
		if (this._mouseOverMovement != null)
		{
			this._mouseOverMovement.SetPause(true, true);
		}
	}

	public void BtnsLeftMouseOut()
	{
		if (this._mouseOverMovement != null)
		{
			this._mouseOverMovement.SetPause(false, true);
		}
	}

	public void BtnsRightMouseOver()
	{
		if (this._mouseOverMovement != null)
		{
			this._mouseOverMovement.SetPause(true, false);
		}
	}

	public void BtnsRightMouseOut()
	{
		if (this._mouseOverMovement != null)
		{
			this._mouseOverMovement.SetPause(false, false);
		}
	}

	public void Scroll(int value)
	{
		if (this._mouseOverMovement != null)
		{
			this._mouseOverMovement.Stop();
		}
		int visibleCount = this.VisibleCount;
		int num = this._index + value;
		LogHelper.Log("___kocha PSH Scroll _index:{0} value:{1} i:{2} _noAnimMovement:{3} _selectedItemIndex:{4}", new object[] { this._index, value, num, this._noAnimMovement, this._selectedItemIndex });
		if (!this.IsMouse)
		{
			bool flag = this.DiffNoSrcoll.ContainsKey(visibleCount);
			bool flag2 = (this._index == 0 && num == 1) || (this._index == 1 && num == 0);
			if (this.IsHome || flag || (this.IsPondPasses && visibleCount <= 6))
			{
				if (flag2)
				{
					bool flag3 = num > 0;
					this.SetActiveMenu(flag3, true, 0.25f);
					float num2 = ((!this.IsHome && (!this.IsPondPasses || visibleCount > 6)) ? this.DiffNoSrcoll[visibleCount] : 141.5f);
					if (flag3)
					{
						num2 *= -1f;
					}
					this.MoveAllItems(num2, 0.25f);
					if (this.IsHome)
					{
						ShortcutExtensions.DOKill(this._titleRightRt, true);
						TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._titleRightRt, new Vector2(this._titleRightRt.anchoredPosition.x + num2, this._titleRightRt.anchoredPosition.y), 0.25f, false), 6);
						ShortcutExtensions.DOKill(this._titleLeftRt, true);
						TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._titleLeftRt, new Vector2(this._titleLeftRt.anchoredPosition.x + num2, this._titleLeftRt.anchoredPosition.y), 0.25f, false), 6);
					}
					else if (this.IsPondPasses)
					{
						ShortcutExtensions.DOKill(this._titlePondPassesRt, true);
						TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._titlePondPassesRt, new Vector2(this._titlePondPassesRt.anchoredPosition.x + num2, this._titlePondPassesRt.anchoredPosition.y), 0.25f, false), 6);
					}
				}
				this._index = num;
			}
			else
			{
				if (num > this._index && this.IsLastInVisibleRect)
				{
					this._noAnimMovement++;
					return;
				}
				if (num < this._index && this._noAnimMovement > 0)
				{
					this._noAnimMovement--;
					return;
				}
				this.MovePosItems(num);
			}
		}
		else if (num <= visibleCount - 3 && num >= 0)
		{
			this.MovePosItems(num);
		}
		else
		{
			LogHelper.Log("___kocha PSH Scroll Can't move");
		}
	}

	private void InitMainData()
	{
		List<ProductCategory> list = CacheLibrary.ProductCache.ProductCategories.OrderBy((ProductCategory p) => p.OrderId).ToList<ProductCategory>();
		List<int> productsIdsInCategories = new List<int>();
		list.ForEach(delegate(ProductCategory c)
		{
			productsIdsInCategories.AddRange(c.ProductIds);
		});
		List<StoreProduct> products = new List<StoreProduct>();
		CacheLibrary.ProductCache.Products.ForEach(delegate(StoreProduct p)
		{
			if (this.HideBought && p.TypeId == 2 && string.IsNullOrEmpty(p.PriceString))
			{
				LogHelper.Warning("___kocha PSH [GD] PREM_SHOP:InitMainData - Skipped StarterKit(was buy on PS) ProductId:{0}", new object[] { p.ProductId });
			}
			else if (!productsIdsInCategories.Contains(p.ProductId))
			{
				LogHelper.Warning("___kocha PSH [GD] PREM_SHOP:InitMainData - Unknown product category ProductId:{0} TypeId:{1}", new object[] { p.ProductId, p.TypeId });
			}
			else if (!PremiumItemBase.HasValue(p.Price))
			{
				LogHelper.Error("___kocha PSH [GD] PREM_SHOP:InitMainData - PRICE ERROR ProductId:{0} Price:{1} PriceString:{2}", new object[] { p.ProductId, p.Price, p.PriceString });
			}
			else
			{
				products.Add(p);
			}
		});
		if (this.HideBought)
		{
			List<int> bundlesForHide = new List<int>();
			int i;
			PSBundlesMapping.AllBundles.ForEach(delegate(int bundleId)
			{
				List<int> bundleProducts = PSBundlesMapping.GetBundleProducts(bundleId);
				bool flag = PSBundlesMapping.BundleRemoveIfAllItemsBought.Contains(bundleId);
				if (flag)
				{
					bool flag2 = true;
					int i2;
					for (i2 = 0; i2 < bundleProducts.Count; i2++)
					{
						if (products.Any((StoreProduct p) => p.ProductId == bundleProducts[i2]))
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						bundlesForHide.Add(bundleId);
					}
				}
				else
				{
					int i;
					for (i = 0; i < bundleProducts.Count; i++)
					{
						if (products.All((StoreProduct p) => p.ProductId != bundleProducts[i]))
						{
							bundlesForHide.Add(bundleId);
							break;
						}
					}
				}
			});
			products.RemoveAll((StoreProduct p) => bundlesForHide.Contains(p.ProductId));
		}
		List<ProductCategory> catsMoneyPacksAndPremiumAccounts = list.Where((ProductCategory p) => this.MoneyPacksAndPremiumAccounts.Contains(p.CategoryId)).ToList<ProductCategory>();
		this._titleLeft.text = catsMoneyPacksAndPremiumAccounts[0].Name;
		this._titleRight.text = catsMoneyPacksAndPremiumAccounts[1].Name;
		ProductCategory productCategory = list.FirstOrDefault((ProductCategory p) => p.CategoryId == 13);
		this._titlePondPasses.text = ((productCategory == null) ? string.Empty : productCategory.Name);
		List<int> list2 = new List<int>();
		List<StoreProduct> list3 = new List<StoreProduct>();
		for (int i = 0; i < products.Count; i++)
		{
			StoreProduct storeProduct = products[i];
			if (storeProduct.HasActiveDiscount(TimeHelper.UtcTime()))
			{
				if (storeProduct.DiscountPrice != null || (this.EnabledAddProductWithoutDiscountPrice && storeProduct.DiscountImageBID != null))
				{
					list3.Add(storeProduct);
				}
				else
				{
					LogHelper.Error("___kocha PSH [GD] PREM_SHOP:InitMainData - HasActiveDiscount, but DiscountPrice=null or DiscountImageBID=null ProductId:{0} DiscountEnd:{1} DiscountPrice:{2} DiscountImageBID:{3}", new object[]
					{
						storeProduct.ProductId,
						storeProduct.DiscountEnd.ToString(),
						storeProduct.DiscountPrice,
						storeProduct.DiscountImageBID
					});
				}
			}
		}
		list3.Sort(new Comparison<StoreProduct>(PremiumShopMainHandler.ComparisonTwoProductForPrice));
		LogHelper.Log("___kocha PSH Discounts [ADD] Count:{0}", new object[] { list3.Count });
		for (int j = 0; j < list3.Count; j++)
		{
			list2.Add(list3[j].ProductId);
		}
		List<int> list4;
		if (CacheLibrary.ProductCache.WhatsNewItems == null)
		{
			list4 = new List<int>();
		}
		else
		{
			list4 = (from p in CacheLibrary.ProductCache.WhatsNewItems
				where p.ProductId != null && p.Start != null && p.Start <= TimeHelper.UtcTime() && p.End != null && p.End >= TimeHelper.UtcTime()
				orderby p.OrderId
				select p.ProductId.Value).ToList<int>();
		}
		List<int> list5 = list4;
		LogHelper.Log("___kocha PSH Whats New [ADD] Count:{0}", new object[] { list5.Count });
		for (int k = 0; k < list5.Count; k++)
		{
			list2.Add(list5[k]);
		}
		list.First((ProductCategory c) => c.CategoryId == 4).ProductIds = list2.ToArray();
		ProductCategory productCategory2 = list.FirstOrDefault((ProductCategory c) => c.CategoryId == 6);
		if (productCategory2 != null)
		{
			productCategory2.ProductIds = new int[0];
		}
		for (int l = 0; l < list.Count; l++)
		{
			ProductCategory productCategory3 = list[l];
			if (!this.MoneyPacksAndPremiumAccounts.Contains(productCategory3.CategoryId))
			{
				List<int> productsIds = productCategory3.ProductIds.ToList<int>();
				if (productCategory3.CategoryId == 1)
				{
					for (int m = 0; m < catsMoneyPacksAndPremiumAccounts.Count; m++)
					{
						productsIds.AddRange(catsMoneyPacksAndPremiumAccounts[m].ProductIds);
					}
				}
				List<StoreProduct> list6 = (from p in products
					where productsIds.Contains(p.ProductId)
					orderby productsIds.IndexOf(p.ProductId)
					select p).ToList<StoreProduct>();
				if (list6.Count == 0)
				{
					LogHelper.Warning("___kocha PSH [GD] PREM_SHOP:InitMainData - Skipped empty category CategoryId:{0}", new object[] { productCategory3.CategoryId });
				}
				else
				{
					this.CreateCategory(productCategory3, list6, (int productId) => catsMoneyPacksAndPremiumAccounts[0].ProductIds.Contains(productId));
					if (this._menuItems.Count == 4)
					{
						GUITools.AddChild(this._menuPrefabContent, this._menuPrefabItemLine);
					}
				}
			}
		}
		this._menuItems[this._menuItems.Count - 1].SetLast();
		this.OnInputTypeChangedAs(InputModuleManager.GameInputType);
	}

	private void CreateCategory(ProductCategory cat, List<StoreProduct> catProducts, Func<int, bool> isHomeLeftFn)
	{
		PremiumShopMainHandler.<CreateCategory>c__AnonStorey6 <CreateCategory>c__AnonStorey = new PremiumShopMainHandler.<CreateCategory>c__AnonStorey6();
		<CreateCategory>c__AnonStorey.cat = cat;
		<CreateCategory>c__AnonStorey.isHomeLeftFn = isHomeLeftFn;
		<CreateCategory>c__AnonStorey.$this = this;
		<CreateCategory>c__AnonStorey.isHome = <CreateCategory>c__AnonStorey.cat.CategoryId == 1;
		<CreateCategory>c__AnonStorey.isPondPasses = <CreateCategory>c__AnonStorey.cat.CategoryId == 13;
		<CreateCategory>c__AnonStorey.menuItem = GUITools.AddChild(this._menuPrefabContent, this._menuPrefabItem).GetComponent<PremiumShopMenuItemHandler>();
		<CreateCategory>c__AnonStorey.menuItem.Init(<CreateCategory>c__AnonStorey.cat, catProducts, this._menuContentToggleGroup);
		<CreateCategory>c__AnonStorey.menuItem.OnActive += delegate(bool b)
		{
			<CreateCategory>c__AnonStorey.$this._converter.gameObject.SetActive(<CreateCategory>c__AnonStorey.isHome);
			if (<CreateCategory>c__AnonStorey.menuItem.IsLast)
			{
				<CreateCategory>c__AnonStorey.menuItem.SetLast();
			}
			<CreateCategory>c__AnonStorey.$this._nav.SetUpdateRegion(UINavigation.Bindings.Down, !<CreateCategory>c__AnonStorey.menuItem.IsLast);
			<CreateCategory>c__AnonStorey.$this._nav.SetUpdateRegion(UINavigation.Bindings.Left, true);
			<CreateCategory>c__AnonStorey.$this._selectedParent = null;
			if (b)
			{
				<CreateCategory>c__AnonStorey.$this._noAnimMovement = 0;
				<CreateCategory>c__AnonStorey.$this._selectedItemIndex = -1;
				<CreateCategory>c__AnonStorey.$this._productId = 0;
				<CreateCategory>c__AnonStorey.$this._category = (PremiumShopMainHandler.ProductCategories)<CreateCategory>c__AnonStorey.cat.CategoryId;
				LogHelper.Log("___kocha PSH ............sel_cat _category:{0}", new object[] { <CreateCategory>c__AnonStorey.$this._category });
				<CreateCategory>c__AnonStorey.$this.CheckActiveMouseHelp(false);
				int num = 0;
				for (int i = 0; i < <CreateCategory>c__AnonStorey.$this._items.Count; i++)
				{
					PremShopItemHandler item = <CreateCategory>c__AnonStorey.$this._items[i];
					if ((<CreateCategory>c__AnonStorey.isHome || <CreateCategory>c__AnonStorey.isPondPasses) && i < 2)
					{
						item.SetActive(false);
					}
					else
					{
						if (i >= 2)
						{
							item.SetParentActive(false);
						}
						if (num < <CreateCategory>c__AnonStorey.menuItem.Products.Count)
						{
							StoreProduct p = <CreateCategory>c__AnonStorey.menuItem.Products[num];
							DateTime? dateTime = ((<CreateCategory>c__AnonStorey.menuItem.LifetimeRemain == null || !<CreateCategory>c__AnonStorey.menuItem.LifetimeRemain.ContainsKey(p.ProductId)) ? null : <CreateCategory>c__AnonStorey.menuItem.LifetimeRemain[p.ProductId]);
							item.Init(p, <CreateCategory>c__AnonStorey.$this._menuContentToggleGroup, dateTime, (PremiumShopMainHandler.ProductCategories)<CreateCategory>c__AnonStorey.cat.CategoryId, PremiumShopMainHandler.IsFastBuy, false);
							item.OnClick += delegate(StoreProduct product, PremiumShopMainHandler.ProductCategories category)
							{
								<CreateCategory>c__AnonStorey.ClickBuy(product, category);
							};
							item.OnClickDetails += delegate(StoreProduct product, PremiumShopMainHandler.ProductCategories category)
							{
								<CreateCategory>c__AnonStorey.OpenDetails(product, category);
							};
							item.OnBack += delegate
							{
								int num2 = <CreateCategory>c__AnonStorey._menuItems.FindIndex((PremiumShopMenuItemHandler menu) => menu == <CreateCategory>c__AnonStorey.menuItem);
								<CreateCategory>c__AnonStorey.FullReset((num2 == -1) ? 0 : num2);
							};
							item.OnActive += delegate(bool b1)
							{
								if (b1)
								{
									if (<CreateCategory>c__AnonStorey._productId == p.ProductId && !<CreateCategory>c__AnonStorey.IsMouse)
									{
										return;
									}
									<CreateCategory>c__AnonStorey._productId = p.ProductId;
									<CreateCategory>c__AnonStorey._nav.SetUpdateRegion(UINavigation.Bindings.Left, true);
									<CreateCategory>c__AnonStorey._nav.SetUpdateRegion(UINavigation.Bindings.Down, true);
									if (<CreateCategory>c__AnonStorey.isHome)
									{
										TMP_Text titleRight = <CreateCategory>c__AnonStorey._titleRight;
										float num3 = 0.5f;
										<CreateCategory>c__AnonStorey._titleLeft.alpha = num3;
										titleRight.alpha = num3;
										if (b1)
										{
											if (<CreateCategory>c__AnonStorey.isHomeLeftFn(p.ProductId))
											{
												<CreateCategory>c__AnonStorey._titleLeft.alpha = 1f;
											}
											else
											{
												<CreateCategory>c__AnonStorey._titleRight.alpha = 1f;
											}
										}
									}
									if (!<CreateCategory>c__AnonStorey.IsMouse)
									{
										<CreateCategory>c__AnonStorey.menuItem.StopShowNew(<CreateCategory>c__AnonStorey._productId);
										int num4 = <CreateCategory>c__AnonStorey._rtItems.FindIndex((RectTransform rt) => item.transform.IsChildOf(rt));
										if (<CreateCategory>c__AnonStorey.IsHome || <CreateCategory>c__AnonStorey.isPondPasses)
										{
											num4 -= 2;
											if (num4 != 0)
											{
												num4 = <CreateCategory>c__AnonStorey._rtItems.FindIndex((RectTransform rt) => item.LinkedItem.transform.IsChildOf(rt)) - 2;
											}
										}
										if (num4 == 0)
										{
											<CreateCategory>c__AnonStorey._nav.SetUpdateRegion(UINavigation.Bindings.Left, false);
											item.SetNavigation(<CreateCategory>c__AnonStorey.menuItem.Sel);
											if (<CreateCategory>c__AnonStorey.IsHome || <CreateCategory>c__AnonStorey.isPondPasses)
											{
												item.LinkedItem.SetNavigation(<CreateCategory>c__AnonStorey.menuItem.Sel);
											}
										}
										<CreateCategory>c__AnonStorey._menuBg.gameObject.SetActive(num4 == 0);
										<CreateCategory>c__AnonStorey._menuContent.gameObject.SetActive(num4 == 0);
										bool flag = <CreateCategory>c__AnonStorey._selectedParent != null && <CreateCategory>c__AnonStorey._selectedParent == item.Parent;
										LogHelper.Log("___kocha PSH ............sel_prod ProductId:{0} index:{1} _index:{2} sameParent:{3} _selectedItemIndex:{4}", new object[] { p.ProductId, num4, <CreateCategory>c__AnonStorey._index, flag, <CreateCategory>c__AnonStorey._selectedItemIndex });
										if (flag)
										{
											return;
										}
										<CreateCategory>c__AnonStorey._selectedParent = item.Parent;
										if ((!<CreateCategory>c__AnonStorey.isHome && !<CreateCategory>c__AnonStorey.isPondPasses) || <CreateCategory>c__AnonStorey._selectedItemIndex <= 0 || <CreateCategory>c__AnonStorey._index != 1)
										{
											if (num4 > <CreateCategory>c__AnonStorey._selectedItemIndex)
											{
												<CreateCategory>c__AnonStorey.Scroll(1);
											}
											else if (num4 < <CreateCategory>c__AnonStorey._selectedItemIndex)
											{
												<CreateCategory>c__AnonStorey.Scroll(-1);
											}
										}
										<CreateCategory>c__AnonStorey._selectedItemIndex = num4;
									}
									else if (PremiumShopMainHandler.IsFastBuy || p.TypeId == 1)
									{
										<CreateCategory>c__AnonStorey.ClickBuy(p, (PremiumShopMainHandler.ProductCategories)<CreateCategory>c__AnonStorey.cat.CategoryId);
									}
									else
									{
										<CreateCategory>c__AnonStorey.OpenDetails(p, (PremiumShopMainHandler.ProductCategories)<CreateCategory>c__AnonStorey.cat.CategoryId);
									}
								}
							};
							item.SetActive(true);
						}
						else
						{
							item.SetActive(false);
						}
						num++;
					}
				}
				for (int j = 2; j < <CreateCategory>c__AnonStorey.$this._items.Count; j++)
				{
					PremShopItemHandler premShopItemHandler = <CreateCategory>c__AnonStorey.$this._items[j];
					if (premShopItemHandler.IsActive)
					{
						premShopItemHandler.SetParentActive(true);
					}
				}
				if (<CreateCategory>c__AnonStorey.$this._index != 0)
				{
					<CreateCategory>c__AnonStorey.$this.Scroll(-<CreateCategory>c__AnonStorey.$this._index);
				}
				else
				{
					<CreateCategory>c__AnonStorey.$this.UpdatePositions();
					<CreateCategory>c__AnonStorey.$this.UpdateScrollBtns();
				}
			}
			<CreateCategory>c__AnonStorey.$this._nav.ForceUpdate();
		};
		this._menuItems.Add(<CreateCategory>c__AnonStorey.menuItem);
	}

	private void UpdatePositions()
	{
		this._mouseOverMovement.ForceStop();
		this._titleLeft.gameObject.SetActive(this.IsHome);
		this._titleRight.gameObject.SetActive(this.IsHome);
		this._titlePondPasses.gameObject.SetActive(this.IsPondPasses);
		if (this.IsPondPasses)
		{
			this.ResetPositionTitlePondPasses();
		}
		int num = 0;
		if (this.IsHome || this.IsPondPasses)
		{
			float num2 = ((!this.IsPondPasses || this.VisibleCount > 3) ? (-1622.4f) : (-1251.899f));
			for (int i = 2; i < this._rtItems.Count; i++)
			{
				float num3 = ((num < 3 || this.IsPondPasses) ? 0f : 16f);
				this._rtItems[i].anchoredPosition = new Vector2(num2 + (this._rtItems[i].rect.width + 24f) * (float)num + num3, 0f);
				num++;
			}
		}
		else
		{
			for (int j = 0; j < this._rtItems.Count; j++)
			{
				if (j < 2)
				{
					this._rtItems[j].anchoredPosition = new Vector2(-1472.4f + (this._rtItems[j].rect.width + 24f) * (float)j, 0f);
				}
				else
				{
					RectTransform rectTransform = this._rtItems[1];
					float num4 = rectTransform.anchoredPosition.x + rectTransform.rect.width / 2f + this._rtItems[j].rect.width / 2f + 24f;
					this._rtItems[j].anchoredPosition = new Vector2(num4 + (this._rtItems[j].rect.width + 24f) * (float)num, 0f);
					num++;
				}
			}
		}
	}

	private void MovePosItems(int newIndex)
	{
		if (newIndex == this._index)
		{
			return;
		}
		bool flag = newIndex == 1 && this._index == 0;
		bool flag2 = newIndex == 0 && this._index == 1;
		float num = ((!flag && !flag2 && !this.IsPondPasses) ? 544f : 233.55f);
		if (this.IsMouse)
		{
			if (flag)
			{
				newIndex = 2;
				num += 544f;
			}
			if (newIndex == 0 && this._index > 1)
			{
				flag2 = true;
				num = Math.Abs(Math.Abs(-1472.4f) - Math.Abs(this._rtItems[0].anchoredPosition.x));
			}
			if (newIndex == 1 && this._index == 2)
			{
				this._index = 1;
				newIndex = 0;
				flag2 = true;
				num = Math.Abs(Math.Abs(-1472.4f) - Math.Abs(this._rtItems[0].anchoredPosition.x));
			}
		}
		if (newIndex > this._index)
		{
			num *= -1f;
		}
		this.SetActiveMenu(flag, flag2, 0.25f);
		LogHelper.Log("___kocha PSH MovePosItems _index:{0} newIndex:{1} diff:{2}", new object[] { this._index, newIndex, num });
		this.MoveAllItems(num, 0.25f);
		if (this.IsPondPasses)
		{
			float num2 = ((this.VisibleCount > 3) ? this._titleLeftRtX0 : (-1007.9f));
			float num3 = ((!flag2) ? (-1145.8f) : num2);
			this._titlePondPasses.alignment = ((!flag2) ? 514 : 513);
			ShortcutExtensions.DOKill(this._titlePondPassesRt, true);
			TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._titlePondPassesRt, new Vector2(num3, this._titlePondPassesRt.anchoredPosition.y), 0.25f, false), 6);
		}
		this._index = newIndex;
	}

	private void MoveAllItems(float diff, float animTime = 0.25f)
	{
		int indexComplete = this._rtItems.Count - 1;
		for (int i = 0; i < this._rtItems.Count; i++)
		{
			RectTransform rectTransform = this._rtItems[i];
			ShortcutExtensions.DOKill(rectTransform, true);
			if (animTime > 0f)
			{
				TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(rectTransform, new Vector2(rectTransform.anchoredPosition.x + diff, rectTransform.anchoredPosition.y), animTime, false), 6), delegate
				{
					indexComplete--;
					if (indexComplete == 0)
					{
						this.UpdateScrollBtns();
					}
				});
			}
			else
			{
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + diff, rectTransform.anchoredPosition.y);
				indexComplete--;
				if (indexComplete == 0)
				{
					this.UpdateScrollBtns();
				}
			}
		}
	}

	private void UpdateScrollBtns()
	{
		bool flag2;
		bool flag = (flag2 = this.IsMouse && this.VisibleCount > 3 && !this.IsHome);
		flag2 = flag2 && !this._mouseOverMovement.IsMenuVisible;
		flag = flag && !this.IsLastInVisibleRect;
		if (this.IsMouse && !flag)
		{
			PremiumShopMenuItemHandler premiumShopMenuItemHandler = this._menuItems.FirstOrDefault((PremiumShopMenuItemHandler m) => m.Category.CategoryId == (int)this._category);
			if (premiumShopMenuItemHandler != null && premiumShopMenuItemHandler.Products != null)
			{
				if (premiumShopMenuItemHandler.Products.Any((StoreProduct p) => p.IsNew))
				{
					premiumShopMenuItemHandler.StopShowNew();
				}
			}
		}
		if (!flag2)
		{
			this._mouseOverMovement.SetPause(false, true);
		}
		if (!flag)
		{
			this._mouseOverMovement.SetPause(false, false);
		}
		this._btnLeft.gameObject.SetActive(flag2);
		this._btnRight.gameObject.SetActive(flag);
	}

	private bool IsLastInVisibleRect
	{
		get
		{
			RectTransform rectTransform = this._rtItems.LastOrDefault((RectTransform p) => p.gameObject.activeSelf);
			return rectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(this._itemsContent, this.Cam.WorldToScreenPoint(new Vector3(rectTransform.position.x + rectTransform.rect.width / 2f, rectTransform.position.y)), this.Cam);
		}
	}

	private void HideMenu(RectTransform rt, float animTime, float offset = 0f)
	{
		ShortcutExtensions.DOKill(rt, true);
		TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(rt, new Vector2(rt.anchoredPosition.x - rt.rect.width - offset, rt.anchoredPosition.y), animTime, false), 6), delegate
		{
			if (this.IsMouse)
			{
				this._btnBack.gameObject.SetActive(true);
			}
		});
	}

	private void ShowMenu(RectTransform rt, float animTime, float offset = 0f)
	{
		if (this.IsMouse)
		{
			this.ResetBackBtn();
		}
		ShortcutExtensions.DOKill(rt, true);
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(rt, new Vector2(rt.anchoredPosition.x + rt.rect.width + offset, rt.anchoredPosition.y), animTime, false), 6);
	}

	private void SetActiveMenu(bool isHideMenu, bool isShowMenu, float animTime = 0.25f)
	{
		if (isHideMenu)
		{
			UIAudioSourceListener.Instance.Slide();
			this.HideMenu(this._menuBg, animTime, 0f);
			this.HideMenu(this._menuContent, animTime, 64f);
			this._mouseOverMovement.SetMenuVisible(false);
		}
		else if (isShowMenu)
		{
			UIAudioSourceListener.Instance.Slide();
			this.ShowMenu(this._menuBg, animTime, 0f);
			this.ShowMenu(this._menuContent, animTime, 64f);
			this._mouseOverMovement.SetMenuVisible(true);
		}
	}

	private void InitConverter()
	{
		this._btnConverter.onClick.AddListener(delegate
		{
			if (!this.IsMouse)
			{
				this.OpenConverter();
			}
		});
		this._hkprConverter.SetPausedFromScript(true);
		this._hkprConverter.StopListenForHotKeys();
		this._converter.group = this._menuContentToggleGroup;
		this._converter.onValueChanged.AddListener(delegate(bool b)
		{
			this._hkprConverter.SetPausedFromScript(!b);
			if (b)
			{
				this._hkprConverter.StartListenForHotkeys();
			}
			else
			{
				this._hkprConverter.StopListenForHotKeys();
			}
			if (this.IsMouse)
			{
				if (b)
				{
					this.OpenConverter();
				}
				return;
			}
		});
	}

	private void ResetBackBtn()
	{
		this._btnBackHelpTitle.gameObject.SetActive(false);
		this._btnBackArrow.text = "\ue625";
		this._btnBack.gameObject.SetActive(false);
	}

	private void OpenConverter()
	{
		this._productId = 0;
		GameObject msgBox = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.converterMoneyPrefab);
		msgBox.GetComponent<AlphaFade>().HideFinished += delegate(object sender, EventArgsAlphaFade fade)
		{
			Object.Destroy(msgBox);
			if (InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse)
			{
				UINavigation.SetSelectedGameObject(null);
			}
		};
		RectTransform component = msgBox.GetComponent<RectTransform>();
		component.anchoredPosition = Vector3.zero;
		component.sizeDelta = Vector2.zero;
		msgBox.GetComponent<MoneyConverter>().Init();
		msgBox.GetComponent<AlphaFade>().ShowPanel();
	}

	private void ClickBuy(StoreProduct p, PremiumShopMainHandler.ProductCategories cat)
	{
		LogHelper.Log("___kocha PSH ^^^^^^^^ ClickBuy ProductId:{0} cat:{1} IsFastBuy:{2}", new object[]
		{
			p.ProductId,
			cat,
			PremiumShopMainHandler.IsFastBuy
		});
		if (PremiumShopMainHandler.IsFastBuy || p.TypeId == 1)
		{
			this.Buy(p, cat);
		}
		else
		{
			this.OpenDetails(p, cat);
		}
	}

	private void OpenDetails(StoreProduct p, PremiumShopMainHandler.ProductCategories cat)
	{
		LogHelper.Log("___kocha PSH &&&&&&&& OpenDetails ProductId:{0} cat:{1}", new object[] { p.ProductId, cat });
		if (p.TypeId != 1)
		{
			bool flag = p.TypeId == 4;
			PhotonConnectionFactory.Instance.CaptureActionInStats("PrShopClickAction", p.ProductId.ToString(), cat.ToString(), null);
			GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.PremShopItemInfoWindowPrefab);
			PremShopItemInfo component = gameObject.GetComponent<PremShopItemInfo>();
			string priceStr = this._items.FirstOrDefault((PremShopItemHandler el) => el.Product != null && el.Product.ProductId == p.ProductId).PriceStr;
			component.Init(p, cat, PremiumShopMainHandler.IsFastBuy && !flag, priceStr, (!flag) ? null : this._menuItems.FirstOrDefault((PremiumShopMenuItemHandler m) => m.Category.CategoryId == (int)cat).GetProductsPondPasses(p));
			component.OnBuy += this.Buy;
			InfoMessage component2 = component.GetComponent<InfoMessage>();
			component2.MessageType = InfoMessageTypes.PremShopItemInfo;
			MessageFactory.InfoMessagesQueue.Enqueue(component2);
		}
	}

	private void Buy(StoreProduct p, PremiumShopMainHandler.ProductCategories c)
	{
		PhotonConnectionFactory.Instance.CaptureActionInStats("PrShopBuyAction", p.ProductId.ToString(), c.ToString(), null);
		BuyProductManager.InitiateProductPurchase(p);
	}

	private void Instance_OnGotOffers(List<OfferClient> offers)
	{
		LogHelper.Log("___kocha PSH Offers [ADD] Count:{0}", new object[] { offers.Count });
		if (offers.Count > 0)
		{
			offers = offers.OrderBy((OfferClient p) => p.OrderId).ToList<OfferClient>();
			Dictionary<int, DateTime?> dictionary = new Dictionary<int, DateTime?>();
			List<StoreProduct> list = new List<StoreProduct>();
			for (int i = 0; i < offers.Count; i++)
			{
				OfferClient o = offers[i];
				StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct el) => el.ProductId == o.ProductId && PremiumItemBase.HasValue(el.Price));
				if (storeProduct != null)
				{
					if (o.LifetimeRemain != null && o.LifetimeRemain.Value <= TimeHelper.UtcTime())
					{
						LogHelper.Log("___kocha PSH ---- skipped product ProductId:{0} LifetimeRemain > server_Time", new object[] { storeProduct.ProductId });
					}
					else
					{
						list.Add(storeProduct);
						dictionary[o.ProductId] = o.LifetimeRemain;
					}
				}
			}
			PremiumShopMenuItemHandler premiumShopMenuItemHandler = this._menuItems.FirstOrDefault((PremiumShopMenuItemHandler m) => m.Category.CategoryId == 6);
			if (premiumShopMenuItemHandler != null)
			{
				premiumShopMenuItemHandler.SetProducts(list);
				premiumShopMenuItemHandler.SetLifetimeRemain(dictionary);
			}
		}
	}

	private static int ComparisonTwoProductForPrice(StoreProduct a, StoreProduct b)
	{
		double? discountPrice = a.DiscountPrice;
		double num = ((discountPrice == null) ? 0.0 : discountPrice.Value) / a.Price;
		double? discountPrice2 = b.DiscountPrice;
		double num2 = ((discountPrice2 == null) ? 0.0 : discountPrice2.Value) / b.Price;
		if (num == num2)
		{
			return b.Price.CompareTo(a.Price);
		}
		return num2.CompareTo(num);
	}

	private void CheckActiveMouseHelp(bool forceHide = false)
	{
		PremiumShopMenuItemHandler premiumShopMenuItemHandler = this._menuItems.FirstOrDefault((PremiumShopMenuItemHandler m) => m.Category.CategoryId == (int)this._category);
		HelpLinePanel.SetMouseHelp((forceHide || !PremiumShopMainHandler.IsFastBuy || !this.IsMouse || !(premiumShopMenuItemHandler != null) || premiumShopMenuItemHandler.Products == null || premiumShopMenuItemHandler.Products.Count <= 0) ? string.Empty : this._rmbHelp);
	}

	private void FullReset(int idxMenuForSelect = 0)
	{
		this._rtItems.ForEach(delegate(RectTransform p)
		{
			ShortcutExtensions.DOKill(p, false);
		});
		ShortcutExtensions.DOKill(this._menuBg, false);
		ShortcutExtensions.DOKill(this._menuContent, false);
		this._menuBg.anchoredPosition = new Vector2(this._menuBgX0, this._menuBg.anchoredPosition.y);
		this._menuContent.anchoredPosition = new Vector2(this._menuContentX0, this._menuContent.anchoredPosition.y);
		this._menuBg.gameObject.SetActive(true);
		this._menuContent.gameObject.SetActive(true);
		this.ResetBackBtn();
		this._index = 0;
		this._selectedItemIndex = -1;
		this._noAnimMovement = 0;
		ShortcutExtensions.DOKill(this._titleRightRt, false);
		this._titleRightRt.anchoredPosition = new Vector2(this._titleRightRtX0, this._titleRightRt.anchoredPosition.y);
		ShortcutExtensions.DOKill(this._titleLeftRt, false);
		this._titleLeftRt.anchoredPosition = new Vector2(this._titleLeftRtX0, this._titleLeftRt.anchoredPosition.y);
		this.ResetPositionTitlePondPasses();
		this.UpdatePositions();
		this.UpdateScrollBtns();
		if (this._menuItems.Count > 0)
		{
			this._menuItems[idxMenuForSelect].Select();
		}
		this.CheckActiveMouseHelp(false);
	}

	private void ResetPositionTitlePondPasses()
	{
		float num = ((this.VisibleCount > 3) ? this._titleLeftRtX0 : (-1007.9f));
		ShortcutExtensions.DOKill(this._titlePondPassesRt, false);
		this._titlePondPassesRt.anchoredPosition = new Vector2(num, this._titlePondPassesRt.anchoredPosition.y);
		this._titlePondPasses.alignment = 513;
	}

	private void OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		if (this.HideBought && product.TypeId == 2)
		{
			int bundleId = PSBundlesMapping.GetBundleId(product.ProductId);
			bool isBundleRemoveIfAllItemsBought = PSBundlesMapping.BundleRemoveIfAllItemsBought.Contains(bundleId);
			int removed = 0;
			this._menuItems.ForEach(delegate(PremiumShopMenuItemHandler p)
			{
				removed += p.HideProduct(product.ProductId);
				if (!isBundleRemoveIfAllItemsBought && bundleId > 0)
				{
					removed += p.HideProduct(bundleId);
				}
			});
			ClientDebugHelper.Log(ProfileFlag.ProductDelivery, string.Format("PSH::OnProductDelivered removed - ProductId:{0} bundleId:{1} removed:{2} isBundleRemoveIfAllItemsBought:{3}", new object[] { product.ProductId, bundleId, removed, isBundleRemoveIfAllItemsBought }));
			CacheLibrary.ProductCache.Products.ForEach(delegate(StoreProduct p)
			{
				if (p.ProductId == product.ProductId || (!isBundleRemoveIfAllItemsBought && p.ProductId == bundleId))
				{
					p.PriceString = null;
				}
			});
			if (isBundleRemoveIfAllItemsBought && bundleId > 0)
			{
				List<int> bundleProducts = PSBundlesMapping.GetBundleProducts(bundleId);
				bool flag = CacheLibrary.ProductCache.Products.Where((StoreProduct p) => bundleProducts.Contains(p.ProductId)).All((StoreProduct p) => string.IsNullOrEmpty(p.PriceString));
				if (flag)
				{
					this._menuItems.ForEach(delegate(PremiumShopMenuItemHandler p)
					{
						removed += p.HideProduct(bundleId);
					});
					CacheLibrary.ProductCache.Products.ForEach(delegate(StoreProduct p)
					{
						if (p.ProductId == bundleId)
						{
							p.PriceString = null;
						}
					});
				}
			}
			if (removed > 0)
			{
				int idx = this._menuItems.FindIndex((PremiumShopMenuItemHandler menu) => (menu.Category != null && menu.Category.CategoryId == (int)this._category) || menu.IsActive);
				idx = ((idx == -1) ? 0 : idx);
				this._menuItems.ForEach(delegate(PremiumShopMenuItemHandler p)
				{
					if (p.Products.Count == 0)
					{
						idx = 0;
						p.SetActive(false);
					}
				});
				this.FullReset(idx);
				if (this._menuItems.Count > 0)
				{
					this._menuItems[idx].DeSelect();
					this._menuItems[idx].Select();
				}
			}
		}
	}

	[SerializeField]
	private float MouseOverAndMovementTimeForStartAnim = 1f;

	[SerializeField]
	private float MouseOverAndMovementAnimSpeed = 1.5f;

	[Space(6f)]
	[SerializeField]
	private Graphic _arrowLeft;

	[SerializeField]
	private Graphic _arrowRight;

	[SerializeField]
	private Button _btnLeft;

	[SerializeField]
	private Button _btnRight;

	[SerializeField]
	private BorderedButton _btnBack;

	[SerializeField]
	private TextMeshProUGUI _btnBackArrow;

	[SerializeField]
	private TextMeshProUGUI _btnBackHelpTitle;

	[SerializeField]
	private GameObject _menuPrefabItem;

	[SerializeField]
	private GameObject _menuPrefabItemLine;

	[SerializeField]
	private GameObject _menuPrefabContent;

	[SerializeField]
	private GameObject _prefabItemBig;

	[SerializeField]
	private GameObject _prefabItemSmall;

	[SerializeField]
	private GameObject _prefabItem;

	[SerializeField]
	private GameObject _prefabItemContent;

	[SerializeField]
	private ToggleGroup _menuContentToggleGroup;

	[SerializeField]
	private TextMeshProUGUI _titleLeft;

	[SerializeField]
	private TextMeshProUGUI _titleRight;

	[SerializeField]
	private RectTransform _titleLeftRt;

	[SerializeField]
	private RectTransform _titleRightRt;

	[SerializeField]
	private TextMeshProUGUI _titlePondPasses;

	[SerializeField]
	private RectTransform _titlePondPassesRt;

	[SerializeField]
	private RectTransform _menuBg;

	[SerializeField]
	private RectTransform _menuContent;

	[SerializeField]
	private RectTransform _itemsContent;

	[SerializeField]
	private UINavigation _nav;

	[SerializeField]
	private Toggle _converter;

	[SerializeField]
	private BorderedButton _btnConverter;

	[SerializeField]
	private HotkeyPressRedirect _hkprConverter;

	[SerializeField]
	private TextMeshProUGUI _titleConverter;

	[SerializeField]
	private RectTransform _mouseOverAndMovement;

	[SerializeField]
	private RectTransform _mouseOverAndMovementLeft;

	private readonly List<PremiumShopMenuItemHandler> _menuItems = new List<PremiumShopMenuItemHandler>();

	private readonly List<PremShopItemHandler> _items = new List<PremShopItemHandler>();

	private readonly List<RectTransform> _rtItems = new List<RectTransform>();

	private bool _isInited;

	private int _index;

	private int _noAnimMovement;

	private int _selectedItemIndex = -1;

	private readonly List<int> MoneyPacksAndPremiumAccounts = new List<int> { 3, 2 };

	private const float ScreenOffset = 64f;

	private const int MaxItemsBig = 2;

	private const int MaxItemsSmall = 24;

	private const float XBig0 = -1472.4f;

	private const float Xsmall0 = -1622.4f;

	private const float Xsmall0PondPasses = -1251.899f;

	private const float Padding = 24f;

	private const float PaddingPremiumAccountAndMoneyPack = 16f;

	private const int MaxItemsWithoutScroll = 3;

	private const float AnimTime = 0.25f;

	private const float Diff0 = 233.55f;

	private const float Diff = 544f;

	private const float DiffHome = 141.5f;

	private readonly Dictionary<int, float> DiffNoSrcoll = new Dictionary<int, float>
	{
		{ 1, -326.5f },
		{ 2, -54.5f },
		{ 3, 67.5f },
		{ 4, 189.5f }
	};

	private GameObject _selectedParent;

	private int _productId;

	private PremiumShopMainHandler.ProductCategories _category = PremiumShopMainHandler.ProductCategories.All;

	private float _menuBgX0;

	private float _menuContentX0;

	private float _titleLeftRtX0;

	private float _titleRightRtX0;

	private const float TitlePondPassesRtX0 = -1007.9f;

	private const float TitlePondPassesRtXCenter = -1145.8f;

	private string _rmbHelp;

	private PremiumShopMouseOverMovement _mouseOverMovement;

	public enum ProductCategories
	{
		All = -1,
		Home = 1,
		MoneyPack,
		PremiumAccount,
		Deals,
		Popular,
		PersonalOffers,
		BoatPacks,
		EventPacks,
		SportPacks,
		TacklePacks,
		TournametPacks,
		FishingPack,
		PondPasses
	}
}
