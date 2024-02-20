using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopMainPageHandler : ActivityStateControlled
{
	public static int MinTravelCost
	{
		get
		{
			return (StaticUserData.CurrentPond != null) ? ((int)StaticUserData.CurrentPond.StayFee.Value) : ShopMainPageHandler._minTravelCost;
		}
		private set
		{
			ShopMainPageHandler._minTravelCost = value;
		}
	}

	private AlphaFade MainPage
	{
		get
		{
			return (StaticUserData.CurrentPond != null) ? this._mainPageLocal : this._mainPage;
		}
	}

	private void Awake()
	{
		this._mainPage.gameObject.SetActive(StaticUserData.CurrentPond == null);
		this._mainPageLocal.gameObject.SetActive(!this._mainPage.gameObject.activeSelf);
		this._licensesContent.GetComponent<VisibleComponent>().SetEnable(false);
		this._selectBinding = new HotkeyBinding
		{
			Hotkey = InputControlType.Action1,
			LocalizationKey = "Select"
		};
		ShopMainPageHandler.Instance = this;
		this.ContentUpdater = base.GetComponent<UpdateContentItems>();
		this.PremiumPage.FastHidePanel();
		this.MainContent.FastHidePanel();
		this.MainPage.ShowPanel();
		this.HidePremium();
	}

	protected override void Start()
	{
		PhotonConnectionFactory.Instance.OnGotMinTravelCost += this.OnGotMinTravelCost;
		PhotonConnectionFactory.Instance.GetItemCategories(false);
		PhotonConnectionFactory.Instance.OnProductDelivered += this.Instance_OnProductDelivered;
		PhotonConnectionFactory.Instance.OnSubscriptionRemoved += this.Instance_OnSubscriptionRemoved;
		base.Start();
		this.InitLicense();
	}

	protected override void OnDestroy()
	{
		base.StopAllCoroutines();
		PhotonConnectionFactory.Instance.OnSubscriptionRemoved -= this.Instance_OnSubscriptionRemoved;
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.Instance_OnProductDelivered;
		PhotonConnectionFactory.Instance.OnGotMinTravelCost -= this.OnGotMinTravelCost;
		base.OnDestroy();
	}

	protected override void OnEnable()
	{
		PhotonConnectionFactory.Instance.GetMinTravelCost();
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		this.PremiumPage.FastHidePanel();
		this.MainContent.FastHidePanel();
		this.MainPage.FastShowPanel();
		base.OnDisable();
	}

	public void SetPremiumSalesAvailable()
	{
		if (this.PremiumShopButton == null)
		{
			this.PremiumShopButton = this._premium.GetComponent<PremiumShopHandler>();
		}
		this.PremiumShopButton.SetPremiumSalesAvailable();
	}

	protected override void SetHelp()
	{
		HelpLinePanel.SetActionHelp(this._selectBinding);
		UIStatsCollector.ChangeGameScreen((StaticUserData.CurrentPond != null) ? GameScreenType.LocalShop : GameScreenType.GlobalShop, GameScreenTabType.Undefined, null, null, null, null, null);
		this.CheckReInitLicense();
	}

	protected override void HideHelp()
	{
		HelpLinePanel.HideActionHelp(this._selectBinding);
	}

	public void SetFiltersNavigationEnabled(bool enabled)
	{
		this.FiltersNavigation.gameObject.SetActive(enabled);
	}

	public void MenuClick(bool isPremiumPage = false)
	{
		isPremiumPage = false;
		if (isPremiumPage)
		{
			UIStatsCollector.ChangeGameScreen(GameScreenType.PremiumShop, GameScreenTabType.Undefined, null, null, null, null, null);
			if (DashboardTabSetter.IsNewPremShopEnabled)
			{
				DashboardTabSetter.Instance.SetEnableCaptureActionInStats(false);
				DashboardTabSetter.SwitchForms(FormsEnum.PremiumShop, false);
			}
			else
			{
				this.MainContent.HidePanel();
				if (this._premium.activeSelf)
				{
					this.PremiumPage.ShowPanel();
				}
				this.MainPage.FastHidePanel();
			}
		}
		else
		{
			this.MainContent.ShowPanel();
			this.PremiumPage.HidePanel();
			this.MainPage.HidePanel();
		}
	}

	public void ShowMainPageImmediate()
	{
		this.MainContent.FastShowPanel();
		this.PremiumPage.FastHidePanel();
		this.MainPage.FastHidePanel();
	}

	public static void OpenPremiumShop()
	{
		if (ShopMainPageHandler.Instance != null)
		{
			ShopMainPageHandler.Instance.MenuClick(true);
		}
	}

	public static void OpenMainShopPage()
	{
		if (ShopMainPageHandler.Instance != null)
		{
			ShopMainPageHandler.Instance.MainPage.ShowPanel();
			ShopMainPageHandler.Instance.PremiumPage.HidePanel();
			ShopMainPageHandler.Instance.MainContent.HidePanel();
		}
	}

	public void SetActivePanels(bool flag)
	{
		this._searchAndSort.SetActive(flag);
		this.ContentUpdater.FilterPanel.SetActive(flag);
	}

	public Transform FindBuyBtn(int itemId)
	{
		return this.ContentUpdater.FindBuyBtn(itemId);
	}

	public void SetHeaderMenuPaused(bool flag)
	{
		this._selectedControl.GetComponent<UINavigation>().SetPaused(flag);
	}

	public void SetActiveButtons(bool flag)
	{
		for (int i = 0; i < this._selectedControl.transform.childCount; i++)
		{
			Transform child = this._selectedControl.transform.GetChild(i);
			Button component = child.GetComponent<Button>();
			if (component != null)
			{
				component.interactable = flag;
				EventTrigger component2 = child.GetComponent<EventTrigger>();
				if (component2 != null)
				{
					component2.enabled = flag;
				}
			}
		}
	}

	public bool ShopContentHasChild
	{
		get
		{
			return this.ContentUpdater.ContentTransform.childCount > 0;
		}
	}

	public void SetActiveToolsMenu(bool flag, List<Selectable> skipped)
	{
		if (flag)
		{
			foreach (Selectable selectable in Object.FindObjectsOfType<Selectable>())
			{
				if (!skipped.Contains(selectable))
				{
					if (selectable != this._toolBtn)
					{
						selectable.interactable = false;
						EventTrigger component = selectable.GetComponent<EventTrigger>();
						if (component != null)
						{
							component.enabled = false;
						}
					}
				}
			}
			UINavigation.SetSelectedGameObject(this._toolBtn.gameObject);
			this._searchAndSort.SetActive(false);
		}
		this._toolBtnHint.SetActive(flag);
		this._toolBtn.interactable = flag;
	}

	public void SortInventoryItems(int itemId)
	{
		this.ContentUpdater.SortInventoryItems(itemId);
	}

	private void OnGotMinTravelCost(int cost)
	{
		ShopMainPageHandler.MinTravelCost = cost;
	}

	public void InitLicenseFilters()
	{
		this.ResetLicenseFilters(new List<State>(CacheLibrary.MapCache.AllStates));
	}

	public void OpenLicences()
	{
		base.StartCoroutine(this.ClickOnLicences(null));
	}

	public void OpenLicences(int stateId)
	{
		base.StartCoroutine(this.ClickOnLicences(new int?(stateId)));
	}

	public static void GoToLicences()
	{
		DashboardTabSetter.GetMenuTogglePairByForm(FormsEnum.Shop).toggle.isOn = true;
		if (ShopMainPageHandler.Instance != null)
		{
			ShopMainPageHandler.Instance.ShowMainPageImmediate();
			ShopMainPageHandler.Instance.OpenLicences();
		}
	}

	public static void GoToLicences(int stateId)
	{
		DashboardTabSetter.GetMenuTogglePairByForm(FormsEnum.Shop).toggle.isOn = true;
		if (ShopMainPageHandler.Instance != null)
		{
			ShopMainPageHandler.Instance.ShowMainPageImmediate();
			ShopMainPageHandler.Instance.OpenLicences(stateId);
		}
	}

	public static void OpenCategory(string categoryName)
	{
		DashboardTabSetter.GetMenuTogglePairByForm(FormsEnum.Shop).toggle.isOn = true;
		if (ShopMainPageHandler.Instance != null)
		{
			ShopMainPageHandler.Instance.ShowMainPageImmediate();
		}
		GameObject gameObject = GameObject.Find("ShopCanvas/Image/HeaderMenu/" + categoryName);
		gameObject.GetComponent<Button>().OnSubmit(null);
	}

	private void ResetLicenseFilters(List<State> states)
	{
		this._licencesFilter.SetStates(states);
		this._licencesFilter.Init();
		this._filterHandler.SetupFilters(this._licencesFilter);
	}

	private IEnumerator ClickOnLicences(int? stateId)
	{
		yield return new WaitForEndOfFrame();
		if (stateId != null)
		{
			this.ResetLicenseFilters(null);
			ShopLicense shopLicense = CacheLibrary.MapCache.AllLicenses.FirstOrDefault((ShopLicense l) => l.StateId == stateId.GetValueOrDefault() && stateId != null);
			if (shopLicense != null)
			{
				this.ContentUpdater.SetPagingById(shopLicense.LicenseId, UpdateContentItems.ItemsTypes.Licensees);
				for (int i = 0; i < this._licensesContent.transform.childCount; i++)
				{
					Transform child = this._licensesContent.transform.GetChild(i);
					LicenseMenuClick component = child.GetComponent<LicenseMenuClick>();
					if (component && (component.StateId == stateId.GetValueOrDefault() && stateId != null))
					{
						component.OnClick();
						break;
					}
				}
			}
		}
		else
		{
			this.ResetLicenseFilters(new List<State>(CacheLibrary.MapCache.AllStates));
			LicenseMenuClick component2 = this._licencesFilter.GetComponent<LicenseMenuClick>();
			if (component2 != null)
			{
				component2.OnClick();
			}
		}
		yield break;
	}

	private void InitLicense()
	{
		if (StaticUserData.CurrentPond == null)
		{
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			if (profile != null && profile.PaidLockRemovals != null)
			{
				profile.PaidLockRemovals.ForEach(delegate(PaidLockRemoval p)
				{
					this._paidLockRemovals.AddRange(p.Ponds);
				});
			}
			this._licencesFilter.Init();
			RectTransform component = this._licensesContent.GetComponent<RectTransform>();
			component.sizeDelta = new Vector2(component.rect.width, 50f);
			List<Button> list = new List<Button>();
			IOrderedEnumerable<State> orderedEnumerable = CacheLibrary.MapCache.AllStates.OrderBy((State p) => p.Name.TrimStart(new char[0]).TrimEnd(new char[0]));
			List<IGrouping<int, ShopMainPageHandler.StateShop>> list2 = (from s in orderedEnumerable
				select new ShopMainPageHandler.StateShop
				{
					Ponds = CacheLibrary.MapCache.CachedPonds.Where((Pond p) => p.State.StateId == s.StateId).ToList<Pond>(),
					State = s
				} into p
				group p by p.RegionId).ToList<IGrouping<int, ShopMainPageHandler.StateShop>>();
			int num = 0;
			using (List<IGrouping<int, ShopMainPageHandler.StateShop>>.Enumerator enumerator = list2.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IGrouping<int, ShopMainPageHandler.StateShop> g = enumerator.Current;
					ShopMainPageHandler.StateShop stateShop = g.FirstOrDefault((ShopMainPageHandler.StateShop p) => p.RegionId == g.Key);
					if (stateShop.RegionId == -1)
					{
						LogHelper.Warning("ShopMainPageHandler:InitLicense - skipped Region with empty Ponds StateName:{0} PondsCount:{1}", new object[]
						{
							stateShop.State.Name,
							stateShop.Ponds.Count
						});
					}
					else
					{
						LicenseMenuClick licenseMenuClick = this.AddLicItem(stateShop.RegionName, null, component, ref num, list, (from p in g.ToList<ShopMainPageHandler.StateShop>()
							select p.State).ToList<State>(), false, true);
						licenseMenuClick.RegionId = new int?(g.Key);
						foreach (ShopMainPageHandler.StateShop stateShop2 in g)
						{
							if (stateShop2.Ponds.All(new Func<Pond, bool>(PondHelper.PondPaidLocked)))
							{
								LogHelper.Warning("ShopMainPageHandler:InitLicense - skipped State with all paid Ponds StateId:{0} Name:{1}", new object[]
								{
									stateShop2.State.StateId,
									stateShop2.State.Name
								});
							}
							else
							{
								licenseMenuClick = this.AddLicItem(stateShop2.State.Name, string.Format("Sh_Licenses_{0}", stateShop2.State.StateId), component, ref num, list, null, true, false);
								licenseMenuClick.StateId = stateShop2.State.StateId;
							}
						}
					}
				}
			}
			Button component2 = this._licencesFilter.GetComponent<Button>();
			for (int i = 0; i < list.Count; i++)
			{
				Selectable selectable = list[i];
				Navigation navigation = default(Navigation);
				navigation.mode = 4;
				navigation.selectOnUp = ((i != 0) ? list[i - 1] : component2);
				navigation.selectOnDown = ((i != list.Count - 1) ? list[i + 1] : component2);
				navigation.selectOnLeft = component2.navigation.selectOnLeft;
				navigation.selectOnRight = component2.navigation.selectOnRight;
				selectable.navigation = navigation;
			}
			List<Selectable> list3 = list.Cast<Selectable>().ToList<Selectable>();
			this._licencesFilter.GetComponent<SelectedControl>().AddRange(list3);
			this._selectedControl.AddRange(list3);
			this._licensesContent.GetComponent<VisibleComponent>().SetEnable(true);
		}
	}

	private LicenseMenuClick AddLicItem(string name, string elementId, RectTransform rt, ref int index, List<Button> btns, List<State> states, bool setLeftPadding, bool setTextBold)
	{
		GameObject gameObject = GUITools.AddChild(this._licensesContent, this._btnLicensePrefab);
		HintElementId hintElementId = gameObject.GetComponent<HintElementId>();
		if (hintElementId == null)
		{
			hintElementId = gameObject.AddComponent<HintElementId>();
		}
		hintElementId.SetElementId(elementId, null, null);
		LicenseMenuClick component = gameObject.GetComponent<LicenseMenuClick>();
		Text component2 = gameObject.transform.GetChild(0).GetComponent<Text>();
		component2.text = name.TrimStart(new char[0]).TrimEnd(new char[0]).ToUpper();
		if (setLeftPadding)
		{
			RectTransform component3 = component2.GetComponent<RectTransform>();
			component3.offsetMin = new Vector2(20f, component3.offsetMin.y);
		}
		if (setTextBold)
		{
			component2.fontStyle = 1;
		}
		Button component4 = gameObject.GetComponent<Button>();
		component4.onClick.AddListener(delegate
		{
			this._licencesFilter.SetStates(states);
			this._licencesFilter.Init();
			this._filterHandler.SetupFilters(this._licencesFilter);
		});
		if (index > 0)
		{
			rt.sizeDelta = new Vector2(rt.rect.width, rt.rect.height + 33f);
		}
		btns.Add(component4);
		index++;
		return component;
	}

	private void HidePremium()
	{
		this._premium.SetActive(false);
		Selectable services = this._services;
		Navigation navigation = default(Navigation);
		navigation.mode = 4;
		navigation.selectOnLeft = this._services.navigation.selectOnLeft;
		navigation.selectOnRight = this._converter;
		services.navigation = navigation;
		Selectable converter = this._converter;
		Navigation navigation2 = default(Navigation);
		navigation2.mode = 4;
		navigation2.selectOnLeft = this._services;
		navigation2.selectOnRight = this._converter.navigation.selectOnRight;
		converter.navigation = navigation2;
	}

	private void Instance_OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		if (product.PaidPondsUnlocked != null)
		{
			this.CheckReInitLicense();
		}
	}

	private void Instance_OnSubscriptionRemoved(Subscription[] subscriptions)
	{
		if (subscriptions.Any((Subscription s) => s.IsPaidUnlock))
		{
			this.CheckReInitLicense();
		}
	}

	private void CheckReInitLicense()
	{
		List<int> paidLockRemovals = new List<int>();
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile != null && profile.PaidLockRemovals != null)
		{
			profile.PaidLockRemovals.ForEach(delegate(PaidLockRemoval p)
			{
				paidLockRemovals.AddRange(p.Ponds);
			});
		}
		if (this._paidLockRemovals.Count > 0 && !paidLockRemovals.SequenceEqual(this._paidLockRemovals))
		{
			LogHelper.Warning("ShopMainPageHandler:OnEnable - Re-InitLicense", new object[0]);
			this._paidLockRemovals.Clear();
			for (int i = 0; i < this._licensesContent.transform.childCount; i++)
			{
				Object.Destroy(this._licensesContent.transform.GetChild(i).gameObject);
			}
			this.InitLicense();
		}
	}

	[SerializeField]
	private Selectable _services;

	[SerializeField]
	private Selectable _converter;

	[SerializeField]
	private GameObject _premium;

	[SerializeField]
	private SelectedControl _selectedControl;

	[SerializeField]
	private FilterHandler _filterHandler;

	[SerializeField]
	private LicencesFilterPonds _licencesFilter;

	[SerializeField]
	private AlphaFade _mainPage;

	[SerializeField]
	private AlphaFade _mainPageLocal;

	[SerializeField]
	private GameObject _btnLicensePrefab;

	[SerializeField]
	private GameObject _licensesContent;

	[SerializeField]
	private Button _toolBtn;

	[SerializeField]
	private GameObject _toolBtnHint;

	[SerializeField]
	private GameObject _searchAndSort;

	public AlphaFade PremiumPage;

	public AlphaFade MainContent;

	public static ShopMainPageHandler Instance;

	public PremiumShopHandler PremiumShopButton;

	public UpdateContentItems ContentUpdater;

	public UINavigation FiltersNavigation;

	private HotkeyBinding _selectBinding;

	private static int _minTravelCost;

	private const float LicensesContentHeight0 = 50f;

	private const float LicensesContentHeightStep = 33f;

	private readonly List<int> _paidLockRemovals = new List<int>();

	private class StateShop
	{
		public State State { get; set; }

		public List<Pond> Ponds { get; set; }

		public int RegionId
		{
			get
			{
				if (this.Ponds != null && this.Ponds.Count > 0)
				{
					return this.Ponds[0].Region.RegionId;
				}
				return -1;
			}
		}

		public string RegionName
		{
			get
			{
				if (this.Ponds != null && this.Ponds.Count > 0)
				{
					return this.Ponds[0].Region.Name;
				}
				return null;
			}
		}
	}
}
