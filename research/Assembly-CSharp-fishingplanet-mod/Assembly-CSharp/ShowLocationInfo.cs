using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Rooms;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowLocationInfo : ActivityStateControlled
{
	private Pond CurPond
	{
		get
		{
			return StaticUserData.CurrentPond;
		}
	}

	private void Awake()
	{
		ShowLocationInfo.Instance = this;
		this._roomsTitle.text = ScriptLocalization.Get("RoomsTitle");
		this._friendsTitle.text = ScriptLocalization.Get("ChatFriendsCaption").ToUpper();
		this._friendsRoomsTitle.text = string.Format("{0} ({1})", this._roomsTitle.text, this._friendsTitle.text);
		this.InitRoomsWindows();
		CacheLibrary.MapCache.GetFishCategories();
		PhotonConnectionFactory.Instance.OnMovedTimeForward += this.OnMovedTimeForward;
	}

	protected override void Start()
	{
		base.Start();
		PhotonConnectionFactory.Instance.OnGotPondWeatherForecast += this.OnGotPondWeatherForecast;
		PhotonConnectionFactory.Instance.OnGotPondWeather += this.OnGotPondWeather;
		PhotonConnectionFactory.Instance.OnGotRoomsPopulation += this.OnGotRoomsPopulation;
		PhotonConnectionFactory.Instance.OnGotLocationInfo += this.OnGotLocationInfo;
		PhotonConnectionFactory.Instance.OnLicenseBought += this.OnLicenseUpdated;
		LicensesExpirationChecker instance = LicensesExpirationChecker.Instance;
		instance.OnLicenseLost = (Action<PlayerLicense>)Delegate.Combine(instance.OnLicenseLost, new Action<PlayerLicense>(this.OnLicenseUpdated));
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		ScreenManager.Instance.OnScreenChanged += this.Instance_OnScreenChanged;
		CacheLibrary.MapCache.OnRefreshedPondsInfo += this.MapCache_OnRefreshedPondsInfo;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnGotPondWeatherForecast -= this.OnGotPondWeatherForecast;
		PhotonConnectionFactory.Instance.OnGotPondWeather -= this.OnGotPondWeather;
		PhotonConnectionFactory.Instance.OnGotRoomsPopulation -= this.OnGotRoomsPopulation;
		PhotonConnectionFactory.Instance.OnGotLocationInfo -= this.OnGotLocationInfo;
		PhotonConnectionFactory.Instance.OnLicenseBought -= this.OnLicenseUpdated;
		LicensesExpirationChecker instance = LicensesExpirationChecker.Instance;
		instance.OnLicenseLost = (Action<PlayerLicense>)Delegate.Remove(instance.OnLicenseLost, new Action<PlayerLicense>(this.OnLicenseUpdated));
		ScreenManager.Instance.OnScreenChanged -= this.Instance_OnScreenChanged;
		CacheLibrary.MapCache.OnRefreshedPondsInfo -= this.MapCache_OnRefreshedPondsInfo;
		PhotonConnectionFactory.Instance.OnMovedTimeForward -= this.OnMovedTimeForward;
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		base.StopAllCoroutines();
	}

	private void Update()
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		this._roomsTglsManager.Update();
		if (!this._weatherInited && TimeAndWeatherManager.CurrentTime != null && TimeAndWeatherManager.CurrentWeather != null && PhotonConnectionFactory.Instance.Profile != null)
		{
			this.FillWeatherForecast(TimeAndWeatherManager.CurrentTime.Value, TimeAndWeatherManager.CurrentWeather, this._weathers);
		}
		if (!this._initedLicenses && CacheLibrary.MapCache.AllLicenses != null && this.CurPond != null)
		{
			this.SetLicenses(this.CurPond);
			this._initedLicenses = true;
		}
		this.RoomsUpdate();
	}

	protected override void SetHelp()
	{
		if (this.CurPond != null)
		{
			if (this._weathers == null || !TournamentHelper.IS_IN_TOURNAMENT)
			{
				PhotonConnectionFactory.Instance.GetPondWeather(this.CurPond.PondId, 7);
				this._initedLicenses = false;
			}
			else if (TournamentHelper.IS_IN_TOURNAMENT)
			{
				PhotonConnectionFactory.Instance.GetPondWeather(this.CurPond.PondId, 1);
			}
			PhotonConnectionFactory.Instance.GetRoomsPopulation(this.CurPond.PondId);
		}
	}

	public string GetCurrentLocation()
	{
		return (this.CurrentLocation != null) ? this.CurrentLocation.Asset : string.Empty;
	}

	public void RestoreOnLocation()
	{
		this.SubMenuController.CloseButton();
	}

	public void GoFishingClick()
	{
		if (!ManagerScenes.InTransition)
		{
			this.RestoreOnLocation();
			this.TransferToLocation.GoFishingPressed();
		}
	}

	public void SetEventRoom()
	{
		this.RoomTypeData.text = ScriptLocalization.Get("EventRoom");
	}

	public void SelectRandomRoom()
	{
		this._roomsTglsManager.SelectRoomWindow("random");
	}

	public void SelectFriendRoom()
	{
	}

	public void SelectFriendToggle()
	{
	}

	public void CloseAdditionalWindow()
	{
		this.CloseWindow();
	}

	public void CloseWindow()
	{
		if (this._friendsTransform != null)
		{
			this._friendsTransform.gameObject.SetActive(false);
		}
		if (this._additionalRooms != null)
		{
			this._additionalRooms.gameObject.SetActive(false);
		}
	}

	public void GotoLicenseShop()
	{
		ShopMainPageHandler.GoToLicences();
	}

	public void OpenCategory(string categoryName)
	{
		ShopMainPageHandler.OpenCategory(categoryName);
	}

	public void OverrideWeather(TimeSpan time, WeatherDesc weather, WeatherDesc[] weathers)
	{
		this.FillCurrentWeatherInfo(time, weather, weathers);
	}

	public void OpenRoomsSubMenu()
	{
		this._curRoomsUpdateTime = 0f;
		if (this.CurPond != null)
		{
			PhotonConnectionFactory.Instance.GetRoomsPopulation(this.CurPond.PondId);
		}
		this._roomsTglsManager.Open();
	}

	private void OnGotLocationInfo(Location location)
	{
		this.CurrentLocation = location;
		this.NameLocationField.text = location.Name.ToUpper();
		if (this.LocationImage != null)
		{
			this.LocationImageLoadable.Load(new int?(location.PhotoBID), this.LocationImage, "Textures/Inventory/{0}");
		}
		this.LocationBrief.text = string.Format("<b>{0}</b>\n{1}\n{2}", location.Name, location.State.Name, location.Desc);
		if (!this._isPondInfoFilled)
		{
			this.SetPondInfo();
		}
	}

	private TransferToLocation TransferToLocation
	{
		get
		{
			if (this._transferToLocation == null)
			{
				this._transferToLocation = Object.FindObjectOfType<TransferToLocation>();
			}
			return this._transferToLocation;
		}
	}

	private SubMenuControllerNew SubMenuController
	{
		get
		{
			if (this._subMenuController == null)
			{
				this._subMenuController = base.GetComponentInChildren<SubMenuControllerNew>();
			}
			return this._subMenuController;
		}
	}

	private void OnMovedTimeForward(TimeSpan time)
	{
		PhotonConnectionFactory.Instance.GetPondWeather(this.CurPond.PondId, 7);
	}

	private void OnLicenseUpdated(PlayerLicense license)
	{
		if (this.CurPond != null)
		{
			this.SetLicenses(this.CurPond);
		}
	}

	private void MapCache_OnRefreshedPondsInfo(object sender, EventArgs e)
	{
		if (this.CurPond != null)
		{
			this.SetLicenses(this.CurPond);
		}
	}

	private void OnGotRoomsPopulation(IList<RoomPopulation> rooms)
	{
		if (!this.HaveMultiplayerPermission)
		{
			rooms.Clear();
		}
		this._roomsTglsManager.Refresh(rooms.ToList<RoomPopulation>());
	}

	private void SetPondInfo()
	{
		Pond curPond = this.CurPond;
		if (curPond != null)
		{
			this._isPondInfoFilled = true;
			this.NamePondField.text = curPond.Name;
			this.NameState.text = curPond.State.Name.ToUpper();
			this.PondLevel.text = ScriptLocalization.Get("LevelMenuCaption").ToLower() + " " + curPond.OriginalMinLevel;
		}
	}

	private IEnumerator CheckLicense()
	{
		if (PhotonConnectionFactory.Instance.Profile.ActiveLicenses != null)
		{
			List<PlayerLicense> activeLicenses = PhotonConnectionFactory.Instance.Profile.ActiveLicenses.Where((PlayerLicense x) => x.StateId == this.CurPond.State.StateId).ToList<PlayerLicense>();
			if (activeLicenses.Count > 0)
			{
				activeLicenses.Sort((PlayerLicense y, PlayerLicense z) => (y.End == null || z.End == null) ? 0 : ((int)(y.End.Value - z.End.Value).TotalSeconds));
				PlayerLicense activeLicense = activeLicenses[0];
				if (activeLicense != null && activeLicense.End != null)
				{
					double waitTime = (activeLicense.End.Value.ToLocalTime() - TimeHelper.UtcTime().ToLocalTime()).TotalSeconds;
					yield return new WaitForSeconds((float)waitTime);
					this.SetLicenses(this.CurPond);
				}
			}
		}
		yield break;
	}

	private void SetLicenses(Pond pond)
	{
		if (this._licenseSubMenu != null)
		{
			IEnumerable<ShopLicense> enumerable = CacheLibrary.MapCache.AllLicenses.Where((ShopLicense x) => x.StateId == pond.State.StateId);
			ShopLicense shopLicense = enumerable.FirstOrDefault((ShopLicense p) => !p.IsAdvanced);
			ShopLicense shopLicense2 = enumerable.FirstOrDefault((ShopLicense p) => p.IsAdvanced);
			this._licenseSubMenu.FillContent(shopLicense, shopLicense2 ?? shopLicense, pond);
			if (!this._licenseSubMenu.HasContent() && this._licenseSubMenu.Opened)
			{
				this.SubMenuController.CloseButton();
			}
		}
		if (this._checkLicenseCourutine != null)
		{
			base.StopCoroutine(this._checkLicenseCourutine);
		}
		this._checkLicenseCourutine = base.StartCoroutine(this.CheckLicense());
	}

	private void OnGotPondWeather(WeatherDesc[] weather)
	{
		if (TimeAndWeatherManager.CurrentTime != null)
		{
			this.OverrideWeather(TimeAndWeatherManager.CurrentTime.Value, TimeAndWeatherManager.CurrentWeather, TimeAndWeatherManager.CurrentWeathers);
		}
	}

	private void OnGotPondWeatherForecast(WeatherDesc[] weather)
	{
		string midday = TimeOfDay.Midday.ToString();
		if (weather.Count((WeatherDesc x) => x.TimeOfDay == midday) >= 6)
		{
			TimeSpan? curTime = TimeAndWeatherManager.CurrentTime;
			this.DetailWeatherInfo.Refresh(weather.Where((WeatherDesc x) => (curTime == null) ? (x.TimeOfDay == midday) : (x.TimeOfDay == curTime.Value.ToTimeOfDay().ToString())).ToList<WeatherDesc>(), weather.Where((WeatherDesc x) => TimeSpanExtension.IsNightFrame(x.TimeOfDay)).ToList<WeatherDesc>());
			this._weathers = weather;
			this._weatherInited = false;
		}
	}

	private void FillCurrentWeatherInfo(TimeSpan time, WeatherDesc weather, WeatherDesc[] weathers)
	{
		if (weathers == null)
		{
			return;
		}
		DateTime dateTime = this._timeFrom.Add(time);
		bool flag = this.IsDay(dateTime);
		string text = MeasuringSystemManager.TemperatureSufix();
		string text2 = string.Format("{0} {1}", WeatherHelper.GetTemperature(weather.WaterTemperature), text);
		string text3 = ScriptLocalization.Get(weather.WindDirection);
		string text4 = MeasuringSystemManager.WindSpeed(weather.WindSpeed).ToString("F1");
		string text5 = MeasuringSystemManager.WindSpeedSufix();
		this.CurrentAirTemp.text = string.Format("{0} {1}", WeatherHelper.GetTemperature(weather.AirTemperature), text);
		this.CurrentWaterTempAndWind.text = string.Format("{0}\n{1} {2} {3}", new object[] { text2, text3, text4, text5 });
		this.CurrentWeatherIcon.text = WeatherHelper.GetWeatherIcon(weather.Icon);
		this.CurrentPressureIcon.text = MeasuringSystemManager.GetPreassureIcon(weather);
		this.TimeField.text = MeasuringSystemManager.TimeString(dateTime);
		string text6 = PondHelper.CurrentDay.ToString(CultureInfo.InvariantCulture);
		if (TournamentHelper.IS_IN_TOURNAMENT || CompetitionMapController.Instance.IsJoinedToTournament)
		{
			this.DayField.text = string.Format("{0} {1}", ScriptLocalization.Get("DayCaption"), text6);
		}
		else
		{
			this.DayField.text = string.Format("{0} {1}/{2}", ScriptLocalization.Get("DayCaption"), text6, PondHelper.AllDays);
		}
		this.WeatherNightDiagram.SetActive(!flag);
		this.WeatherDayDiagram.SetActive(flag);
		if (flag)
		{
			float num = Mathf.Clamp(20f * ((float)(dateTime.Hour - 5) + (float)dateTime.Minute / 60f) + -160f, -160f, 160f);
			this.timeOfADay.anchoredPosition = new Vector2(num, this.timeOfADay.anchoredPosition.y);
		}
		else
		{
			int num2 = ((dateTime.Hour < 21) ? (3 + dateTime.Hour) : (dateTime.Hour - 21));
			float num3 = Mathf.Clamp(33.625f * ((float)num2 + (float)dateTime.Minute / 60f) + -139f, -139f, 130f);
			this.timeOfANight.anchoredPosition = new Vector2(num3, this.timeOfANight.anchoredPosition.y);
		}
		WeatherDesc weatherDesc;
		if (flag)
		{
			weatherDesc = weathers.FirstOrDefault((WeatherDesc x) => x.TimeOfDay == TimeOfDay.Midday.ToString());
		}
		else
		{
			weatherDesc = weathers.FirstOrDefault((WeatherDesc x) => x.TimeOfDay == TimeOfDay.MidNight.ToString());
		}
		WeatherDesc weatherDesc2 = weatherDesc;
		if (weatherDesc2 != null && weatherDesc2.FishingDiagramImageId != null)
		{
			ResourcesHelpers.AsyncLoadableImage asyncLoadableImage = ((!flag) ? this._nightImageLdbl : this._dayImageLdbl);
			asyncLoadableImage.Load(string.Format("Textures/Inventory/{0}", weatherDesc2.FishingDiagramImageId.Value.ToString(CultureInfo.InvariantCulture)));
		}
	}

	private bool IsDay(DateTime time)
	{
		return time.Hour >= 5 && time.Hour <= 21 && (time.Hour != 21 || (time.Minute <= 0 && time.Second <= 0));
	}

	private void FillWeatherForecast(TimeSpan time, WeatherDesc weather, WeatherDesc[] weathers)
	{
		IEnumerable<WeatherDesc> enumerable = weathers.Where((WeatherDesc x) => x.TimeOfDay == TimeOfDay.Midday.ToString());
		IEnumerable<WeatherDesc> enumerable2 = weathers.Where((WeatherDesc x) => x.TimeOfDay == TimeOfDay.MidNight.ToString());
		this._weatherSubMenu.FillContent(enumerable, enumerable2, weathers.GetDayMinTemperatures(), weathers.GetDayMaxTemperatures(), weathers.GetNightMinTemperatures(), weathers.GetNightMaxTemperatures(), weathers.GetDayMinTemperaturesWater(), weathers.GetDayMaxTemperaturesWater(), weathers.GetNightMinTemperaturesWater(), weathers.GetNightMaxTemperaturesWater(), time.Days - ((time.Hours >= 5) ? 0 : 1) + 1);
		this.FillCurrentWeatherInfo(time, weather, weathers);
		this._weatherInited = true;
	}

	private void RoomsUpdate()
	{
		if (ScreenManager.Instance != null && ScreenManager.Instance.GameScreen == GameScreenType.LocalMap)
		{
			this._curRoomsUpdateTime += Time.deltaTime;
			if (this._curRoomsUpdateTime > 10f)
			{
				this.OpenRoomsSubMenu();
			}
		}
	}

	private void Instance_OnScreenChanged(GameScreenType currentScreen)
	{
		if (currentScreen == GameScreenType.LocalMap && ScreenManager.Game3DScreens.Contains(ScreenManager.Instance.GameScreenPrev))
		{
			this.OpenRoomsSubMenu();
		}
	}

	private void InitRoomsWindows()
	{
		this._roomsTglsManager = new RoomPondMapTglsManager(this._joinTogglesTransform.GetComponent<ToggleGroup>(), new Dictionary<RoomPondMapTglsManager.WindowTypes, RoomPondMapTglsManager.WindowRoomData>
		{
			{
				RoomPondMapTglsManager.WindowTypes.Rooms,
				new RoomPondMapTglsManager.WindowRoomData
				{
					MainGo = this._joinTogglesTransform.gameObject,
					RootContent = this._joinTogglesTransform.gameObject
				}
			},
			{
				RoomPondMapTglsManager.WindowTypes.FriendsRooms,
				new RoomPondMapTglsManager.WindowRoomData
				{
					MainGo = this._additionalRooms.gameObject,
					RootContent = this._friendsRoomsRoot.gameObject
				}
			},
			{
				RoomPondMapTglsManager.WindowTypes.Friends,
				new RoomPondMapTglsManager.WindowRoomData
				{
					MainGo = this._friendsTransform.gameObject,
					RootContent = this._friendsRoot.gameObject
				}
			}
		}, this._roomElemPrefab, new Dictionary<string, bool>
		{
			{ "top", false },
			{
				"random",
				!this.BlockedRandomRoom
			},
			{ "friends", this.HaveMultiplayerPermission },
			{ "private", true }
		}, delegate(string roomId, string roomLoc)
		{
			LogHelper.Log("___kocha >>> SetRoomID:{0}", new object[] { roomId });
			this.TransferToLocation.SetRoomID(roomId);
			this.RoomTypeData.text = roomLoc;
		}, (!this.BlockedRandomRoom) ? "random" : "private", this._navigation);
	}

	private void OnMoved(TravelDestination destination)
	{
		if (destination == TravelDestination.Room && PhotonConnectionFactory.Instance.Room != null)
		{
			this._roomsTglsManager.OnMovedToRoom(PhotonConnectionFactory.Instance.Room.RoomId);
		}
	}

	private bool BlockedRandomRoom
	{
		get
		{
			return false;
		}
	}

	private bool HaveMultiplayerPermission
	{
		get
		{
			return true;
		}
	}

	public Text NameLocationField;

	public Text NamePondField;

	public Text NameState;

	public Text DayField;

	public Text TimeField;

	public Text PondLevel;

	public Image LocationImage;

	private ResourcesHelpers.AsyncLoadableImage LocationImageLoadable = new ResourcesHelpers.AsyncLoadableImage();

	[HideInInspector]
	public Location CurrentLocation;

	[SerializeField]
	private WeatherSubMenu _weatherSubMenu;

	[SerializeField]
	private LicensesSubMenu _licenseSubMenu;

	[SerializeField]
	private TextMeshProUGUI CurrentAirTemp;

	[SerializeField]
	private TextMeshProUGUI CurrentWaterTempAndWind;

	[SerializeField]
	private TextMeshProUGUI CurrentWeatherIcon;

	[SerializeField]
	private TextMeshProUGUI CurrentPressureIcon;

	[SerializeField]
	private GameObject WeatherDayDiagram;

	[SerializeField]
	private GameObject WeatherNightDiagram;

	[SerializeField]
	private ResourcesHelpers.AsyncLoadableImage _dayImageLdbl;

	[SerializeField]
	private ResourcesHelpers.AsyncLoadableImage _nightImageLdbl;

	[SerializeField]
	private Transform _joinTogglesTransform;

	[SerializeField]
	private Transform _additionalRooms;

	[SerializeField]
	private Transform _friendsTransform;

	private TransferToLocation _transferToLocation;

	[SerializeField]
	private RectTransform timeOfADay;

	[SerializeField]
	private RectTransform timeOfANight;

	public Text LocationBrief;

	public WeatherInfoShort DetailWeatherInfo;

	public Text RoomTypeData;

	[Space(5f)]
	[SerializeField]
	private TextMeshProUGUI _roomsTitle;

	[SerializeField]
	private TextMeshProUGUI _friendsRoomsTitle;

	[SerializeField]
	private TextMeshProUGUI _friendsTitle;

	[SerializeField]
	private GameObject _roomElemPrefab;

	[SerializeField]
	private Transform _friendsRoomsRoot;

	[SerializeField]
	private Transform _friendsRoot;

	[SerializeField]
	private UINavigation _navigation;

	public static ShowLocationInfo Instance;

	private const float rightmostDayTimePosition = 160f;

	private const float leftmostDayTimePosition = -160f;

	private const float rightmostNightTimePosition = 130f;

	private const float leftmostNightTimePosition = -139f;

	private const float RoomsUpdateDelayTime = 10f;

	private bool _initedLicenses;

	private bool _isPondInfoFilled;

	private bool _weatherInited = true;

	private WeatherDesc[] _weathers;

	private float _curRoomsUpdateTime;

	private RoomPondMapTglsManager _roomsTglsManager;

	private DateTime _timeFrom = new DateTime(2000, 1, 1, 0, 0, 0);

	private Coroutine _checkLicenseCourutine;

	private SubMenuControllerNew _subMenuController;
}
