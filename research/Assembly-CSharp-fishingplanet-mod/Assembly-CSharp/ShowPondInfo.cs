using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.GlobalMap;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowPondInfo : MonoBehaviour
{
	public static ShowPondInfo Instance
	{
		get
		{
			return ShowPondInfo._instance;
		}
	}

	private SubMenuControllerNew SubMenuCtrl
	{
		get
		{
			if (this._subMenuCtrl == null)
			{
				this._subMenuCtrl = base.GetComponent<SubMenuControllerNew>();
			}
			return this._subMenuCtrl;
		}
	}

	internal void Awake()
	{
		ShowPondInfo._instance = this;
		CacheLibrary.MapCache.GetFishCategories();
		PhotonConnectionFactory.Instance.OnGotPondWeatherForecast += this.OnGotPondWeather;
		PhotonConnectionFactory.Instance.OnPondUnlocked += this.OnGotPondUnlocked;
		PhotonConnectionFactory.Instance.OnOperationFailed += this.OnOperationFailed;
		PhotonConnectionFactory.Instance.OnPreviewResidenceChange += this.OnPreviewResidenceChange;
		PhotonConnectionFactory.Instance.OnResidenceChanged += this.OnResidenceChanged;
		PhotonConnectionFactory.Instance.OnLicenseBought += this.OnLicenseUpdated;
		LicensesExpirationChecker instance = LicensesExpirationChecker.Instance;
		instance.OnLicenseLost = (Action<PlayerLicense>)Delegate.Combine(instance.OnLicenseLost, new Action<PlayerLicense>(this.OnLicenseUpdated));
		PhotonConnectionFactory.Instance.OnProductDelivered += this.Instance_OnProductDelivered;
		CacheLibrary.MapCache.OnGetPond += this.MapCache_OnGetPond;
		CacheLibrary.MapCache.OnRefreshedPondsInfo += this.MapCache_OnRefreshedPondsInfo;
		CacheLibrary.MapCache.OnGetBoatDescs += this.OnGetBoatDescs;
		this.WeatherForecastTitle.text = ScriptLocalization.Get("WeatherForecast").ToUpper(CultureInfo.InvariantCulture);
		this.FishSpeciesTitle.text = ScriptLocalization.Get("FishSpecies").ToUpper(CultureInfo.InvariantCulture);
		this.LicensesTitle.text = ScriptLocalization.Get("LicensesMenu").ToUpper(CultureInfo.InvariantCulture);
		this.TravelCostTitle.text = ScriptLocalization.Get("TravelCost").ToUpper(CultureInfo.InvariantCulture);
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.Instance_OnProductDelivered;
		PhotonConnectionFactory.Instance.OnGotPondWeatherForecast -= this.OnGotPondWeather;
		PhotonConnectionFactory.Instance.OnPondUnlocked -= this.OnGotPondUnlocked;
		PhotonConnectionFactory.Instance.OnOperationFailed -= this.OnOperationFailed;
		PhotonConnectionFactory.Instance.OnPreviewResidenceChange -= this.OnPreviewResidenceChange;
		PhotonConnectionFactory.Instance.OnResidenceChanged -= this.OnResidenceChanged;
		PhotonConnectionFactory.Instance.OnLicenseBought -= this.OnLicenseUpdated;
		LicensesExpirationChecker instance = LicensesExpirationChecker.Instance;
		instance.OnLicenseLost = (Action<PlayerLicense>)Delegate.Remove(instance.OnLicenseLost, new Action<PlayerLicense>(this.OnLicenseUpdated));
		CacheLibrary.MapCache.OnRefreshedPondsInfo -= this.MapCache_OnRefreshedPondsInfo;
		CacheLibrary.MapCache.OnGetPond -= this.MapCache_OnGetPond;
		CacheLibrary.MapCache.OnGetBoatDescs -= this.OnGetBoatDescs;
	}

	private void OnLicenseUpdated(PlayerLicense license)
	{
		if (CacheLibrary.MapCache.AllLicenses != null)
		{
			this.SetLicenses();
		}
	}

	private void MapCache_OnRefreshedPondsInfo(object sender, EventArgs e)
	{
		this.OnEnable();
	}

	private void OnGetBoatDescs(object sender, GlobalMapBoatDescCacheEventArgs e)
	{
		if (this.CurrentPond != null)
		{
			this.BoatPresent.SetActive(e.Items.Any((BoatDesc x) => x.Prices != null && x.Prices.Any((BoatPriceDesc p) => p.PondId == this.CurrentPond.PondId)));
		}
	}

	internal void OnEnable()
	{
		if (this.CurrentPond != null)
		{
			this.RequestPondInfo(this.CurrentPond.PondId);
		}
	}

	private void Update()
	{
		if (this.CurrentPond == null)
		{
			return;
		}
		this._timer += Time.deltaTime;
		if (this._timer >= this._maxTime && ShowPondInfo.Instance.CurrentPond != null)
		{
			this._timer = 0f;
			this.LockTransferButton(this.CurrentPond.PondLocked(), this.CurrentPond.MinLevel, this.CurrentPond.PondPaidLocked());
		}
	}

	public void RequestPondInfo(int pondId)
	{
		this._requestedPondId = pondId;
		if (this._licensesCount != PhotonConnectionFactory.Instance.Profile.Licenses.Count)
		{
			this._licensesCount = PhotonConnectionFactory.Instance.Profile.Licenses.Count;
			this._pondCache = new Dictionary<int, Pond>();
		}
		CacheLibrary.MapCache.GetPondInfo(pondId);
	}

	private void MapCache_OnGetPond(object sender, GlobalMapPondCacheEventArgs e)
	{
		this.FillPondInfo(e.Pond);
	}

	private void FillCurrentWeather(WeatherDesc[] weathers, bool day)
	{
		IEnumerable<WeatherDesc> enumerable = weathers.Where((WeatherDesc x) => x.TimeOfDay == TimeOfDay.Midday.ToString());
		IEnumerable<WeatherDesc> enumerable2 = weathers.Where((WeatherDesc x) => x.TimeOfDay == TimeOfDay.MidNight.ToString());
		this._currentWeather.Init(new WeatherPreviewPanel.WeatherData(enumerable.First<WeatherDesc>(), enumerable2.First<WeatherDesc>(), new WeatherPreviewPanel.MinMaxRange(weathers.GetDayMinTemperatures().First<int>(), weathers.GetDayMaxTemperatures().First<int>()), new WeatherPreviewPanel.MinMaxRange(weathers.GetNightMinTemperatures().First<int>(), weathers.GetNightMaxTemperatures().First<int>()), new WeatherPreviewPanel.MinMaxRange(weathers.GetDayMinTemperaturesWater().First<int>(), weathers.GetDayMaxTemperaturesWater().First<int>()), new WeatherPreviewPanel.MinMaxRange(weathers.GetNightMinTemperaturesWater().First<int>(), weathers.GetNightMaxTemperaturesWater().First<int>())), 0, true, day);
	}

	internal void FillPondInfo(Pond pond)
	{
		this.SubMenuCtrl.SetInteractableToggle<LicensesSubMenu>(!pond.PondPaidLocked());
		if (!this._pondCache.ContainsKey(pond.PondId))
		{
			this._pondCache.Add(pond.PondId, pond);
		}
		this.CurrentPond = pond;
		this._pondSubMenu.SetPond(this.CurrentPond);
		CacheLibrary.MapCache.GetBoatDescs();
		this.DescriptionField.text = pond.Desc;
		this.NameField.text = pond.Name;
		this.StateField.text = pond.State.Name.ToUpper();
		this.PondLevel.text = (ScriptLocalization.Get("LevelMenuCaption").ToLower() + " " + pond.OriginalMinLevel).ToUpper(CultureInfo.InvariantCulture);
		if (this.PondImage != null)
		{
			this.PondImageLoadable.Image = this.PondImage;
			this.PondImageLoadable.Load(string.Format("Textures/Inventory/{0}", pond.PhotoBID));
		}
		if (this.PondMap != null)
		{
			this.PondMapLoadable.Image = this.PondMap;
			this.PondMapLoadable.Load(string.Format("Textures/Inventory/{0}", pond.MapBID));
		}
		this.TravelInit.Init(this.CurrentPond, false);
		if (CacheLibrary.MapCache.AllLicenses != null)
		{
			this.SetLicenses();
		}
		PhotonConnectionFactory.Instance.GetPondWeather(this.CurrentPond.PondId, 7);
		this.LockTransferButton(pond.PondLocked(), pond.MinLevel, pond.PondPaidLocked());
	}

	private void SetLicenses()
	{
		if (this._licenseSubMenu != null)
		{
			IEnumerable<ShopLicense> enumerable = CacheLibrary.MapCache.AllLicenses.Where((ShopLicense x) => x.StateId == this.CurrentPond.State.StateId);
			this._licenseSubMenu.FillContent(enumerable.FirstOrDefault((ShopLicense l) => !l.IsAdvanced), enumerable.FirstOrDefault((ShopLicense l) => l.IsAdvanced), this.CurrentPond);
		}
	}

	private void OnGotPondUnlocked(int pondId, int accesibleLevel)
	{
		this.LockTransferButton(this.CurrentPond.PondLocked(), this.CurrentPond.MinLevel, this.CurrentPond.PondPaidLocked());
		this.FillPondInfo(this.CurrentPond);
	}

	private void LockTransferButton(bool locking, int level, bool isPaidLocked)
	{
		bool flag = this.CurrentPond != null && !GlobalMapHelper.IsActive(this.CurrentPond);
		if (locking)
		{
			this.TravelButtonText.text = ScriptLocalization.Get("Unlock").ToUpper();
		}
		else if (isPaidLocked)
		{
			this.TravelButtonText.text = ScriptLocalization.Get("Unlock").ToUpper();
		}
		else
		{
			this.TravelButtonText.text = ScriptLocalization.Get((!flag) ? "[TravelButton]" : "PondInDevelopingBtn").ToUpper();
		}
		string text = ((!flag) ? "\ue805" : InitPondPin.StateIcons[InitPondPin.PondStates.InDeveloping]);
		this.Lock.text = ((!locking && !isPaidLocked) ? string.Empty : "\ue630");
		this.Arrow.text = ((locking || isPaidLocked) ? string.Empty : text);
	}

	public void OnBuyLicensesClicked()
	{
		this.GotoLicenseShop();
	}

	public void GotoLicenseShop()
	{
		if (this.CurrentPond != null)
		{
			ShopMainPageHandler.GoToLicences(this.CurrentPond.State.StateId);
		}
		else
		{
			ShopMainPageHandler.GoToLicences();
		}
	}

	public void OpenCategory(string categoryName)
	{
		ShopMainPageHandler.OpenCategory(categoryName);
	}

	public void OpenInventory()
	{
		DashboardTabSetter.GetMenuTogglePairByForm(FormsEnum.Inventory).toggle.isOn = true;
	}

	private void OnGotPondWeather(WeatherDesc[] weather)
	{
		if (weather.Where((WeatherDesc x) => x.TimeOfDay == TimeOfDay.Midday.ToString()).ToList<WeatherDesc>().Count<WeatherDesc>() < 6)
		{
			return;
		}
		this.WeatherInfo.Refresh(weather.Where((WeatherDesc x) => x.TimeOfDay == TimeOfDay.Midday.ToString()).ToList<WeatherDesc>(), null);
		if (this._currentWeather != null)
		{
			this.FillCurrentWeather(weather, true);
		}
		if (this._weatherSubMenu != null)
		{
			this._weatherSubMenu.FillContent(weather.Where((WeatherDesc x) => x.TimeOfDay == TimeOfDay.Midday.ToString()).ToList<WeatherDesc>(), weather.Where((WeatherDesc x) => x.TimeOfDay == TimeOfDay.MidNight.ToString()), weather.GetDayMinTemperatures(), weather.GetDayMaxTemperatures(), weather.GetNightMinTemperatures(), weather.GetNightMaxTemperatures(), weather.GetDayMinTemperaturesWater(), weather.GetDayMaxTemperaturesWater(), weather.GetNightMinTemperaturesWater(), weather.GetNightMaxTemperaturesWater(), 1);
		}
		else
		{
			this.DetailWeatherInfo.Refresh(weather.Where((WeatherDesc x) => x.TimeOfDay == TimeOfDay.Midday.ToString()).ToList<WeatherDesc>(), weather.Where((WeatherDesc x) => x.TimeOfDay == TimeOfDay.MidNight.ToString()).ToList<WeatherDesc>());
		}
	}

	public void SetStateAsHome()
	{
		if (this.CurrentPond == null)
		{
			return;
		}
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		PhotonConnectionFactory.Instance.PreviewChangeResidenceState(this.CurrentPond.State.StateId);
	}

	private void OnPreviewResidenceChange(ChangeResidenceInfo change)
	{
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		if ((double)change.Amount <= PhotonConnectionFactory.Instance.Profile.SilverCoins)
		{
			this.messageBox = this.helpers.ShowMessageSelectable(gameObject, string.Empty, string.Format(ScriptLocalization.Get("ConfirmChangeState"), (change.Amount <= 0) ? 0 : change.Amount, MeasuringSystemManager.GetCurrencyIcon(change.Currency), "\n"), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, false);
			this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.ShowPondInfo_ConfirmActionCalled;
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CompleteMessage_ActionCalled;
		}
		else
		{
			this.messageBox = this.helpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("ConfirmChangeStateHaventMoney"), change.Amount, MeasuringSystemManager.GetCurrencyIcon(change.Currency)), false, false, false, null);
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
		}
	}

	private void BuyClick_ThirdButtonActionCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		ShopMainPageHandler.OpenPremiumShop();
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	private void ShowPondInfo_ConfirmActionCalled(object sender, EventArgs e)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		PhotonConnectionFactory.Instance.ChangeResidenceState(this.CurrentPond.State.StateId);
	}

	private void OnResidenceChanged(ChangeResidenceInfo change)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		this._pondCache.Clear();
		this.RequestPondInfo(this.CurrentPond.PondId);
	}

	private void OnOperationFailed(Failure failure)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		this.messageBox = this.helpers.ShowMessage(base.gameObject.transform.root.gameObject, ScriptLocalization.Get("MessageCaption"), failure.ErrorMessage, false, false, false, null);
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
	}

	private void Instance_OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		if (product.PaidPondsUnlocked != null && this.CurrentPond != null)
		{
			bool flag = this.CurrentPond.PondPaidLocked();
			this.SubMenuCtrl.SetInteractableToggle<LicensesSubMenu>(!flag);
			this.LockTransferButton(this.CurrentPond.PondLocked(), this.CurrentPond.MinLevel, flag);
		}
	}

	[SerializeField]
	private PondSubMenu _pondSubMenu;

	[SerializeField]
	private WeatherSubMenu _weatherSubMenu;

	[SerializeField]
	private WeatherPreviewPanel _currentWeather;

	[SerializeField]
	private LicensesSubMenu _licenseSubMenu;

	public Text DescriptionField;

	public TextMeshProUGUI NameField;

	public TextMeshProUGUI PondLevel;

	public TextMeshProUGUI StateField;

	public TextMeshProUGUI WeatherForecastTitle;

	public TextMeshProUGUI FishSpeciesTitle;

	public TextMeshProUGUI LicensesTitle;

	public TextMeshProUGUI TravelCostTitle;

	public WeatherInfoShort WeatherInfo;

	public WeatherInfoShort DetailWeatherInfo;

	[HideInInspector]
	public Pond CurrentPond;

	public Image PondImage;

	private ResourcesHelpers.AsyncLoadableImage PondImageLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public Image PondMap;

	private ResourcesHelpers.AsyncLoadableImage PondMapLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public TravelInit TravelInit;

	public GameObject LicensesContent;

	public GameObject LicensePrefab;

	public Color ActiveLicense;

	public Color InactiveLicense;

	public GameObject TravelButton;

	public TextMeshProUGUI TravelButtonText;

	public Text Arrow;

	public Text Lock;

	public TextMeshProUGUI LockArrowIcon;

	public GameObject BoatPresent;

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox messageBox;

	private int _requestedPondId = int.MinValue;

	private float _maxTime = 2f;

	private float _timer = 2f;

	private Dictionary<int, Pond> _pondCache = new Dictionary<int, Pond>();

	private static ShowPondInfo _instance;

	private const string LockIcon = "\ue630";

	private const string ArrowIcon = "\ue805";

	private SubMenuControllerNew _subMenuCtrl;

	private int _licensesCount;
}
