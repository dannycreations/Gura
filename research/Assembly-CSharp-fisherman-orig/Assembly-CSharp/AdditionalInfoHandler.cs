using System;
using System.Collections.Generic;
using System.Globalization;
using Assets.Scripts.Common.Managers;
using I2.Loc;
using InControl;
using ObjectModel;
using TMPro;
using UnityEngine;

public class AdditionalInfoHandler : MonoBehaviour
{
	public bool IsHintFishKeepnetActive { get; set; }

	private void Start()
	{
		this._userName.text = PhotonConnectionFactory.Instance.UserName;
		this._fishKeepnetTextIco.enabled = false;
		this._fishKeepnetText.text = string.Empty;
		this.weatherWidget = new WeatherWidget();
		this.weatherWidget.Init(this.WeatherControl, this.PressureControl, this.WindDirectionControl, this.WindPowerControl, this._windPowerSuffix, this.TemperatureControl, this.WindCompass, this._temperatureWaterControl, true);
		this.UpdateTimeAndKeepnet();
		this.weatherWidget.VisualizeWeather();
		this.SetHelpText();
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		ClientMissionsManager.Instance.TrackedMissionUpdated += this.TrackedMissionUpdated;
	}

	private void OnDestroy()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		ClientMissionsManager.Instance.TrackedMissionUpdated -= this.TrackedMissionUpdated;
	}

	private void Update()
	{
		this._timer += Time.deltaTime;
		if (this._timer >= this.timerMax)
		{
			this._timer = 0f;
			this.UpdateTimeAndKeepnet();
			this.weatherWidget.VisualizeWeather();
			this.CheckNightTime();
		}
		this.UpdateRentLabel();
		if (this.IsNotInTournament && this.IsNotInTutorial && !StaticUserData.DISABLE_MISSIONS)
		{
			bool flag = KeysHandlerAction.IsFishingInProgress();
			if (flag && this._compassManager.IsActive)
			{
				this._compassManager.SetActive(false);
			}
			else if (!flag && !this._compassManager.IsActive)
			{
				this._compassManager.SetActive(true);
			}
		}
		else if (this._compassManager.IsActive)
		{
			this._compassManager.SetActive(false);
		}
	}

	public bool IsMissionFinishing
	{
		get
		{
			return this.MissionInfo.IsFinishing;
		}
	}

	public void TournamentTimeEnded(TournamentIndividualResults result)
	{
		this.TournamentInfo.TournamentTimeEnded(result);
	}

	private void TrackedMissionUpdated(MissionOnClient m)
	{
		if (m != null && !this.MissionInfo.gameObject.activeSelf && this.IsNotInTutorial && this.IsNotInTournament)
		{
			this.MissionInfo.gameObject.SetActive(true);
			this.MissionInfo.Populate(m);
		}
	}

	private void UpdateTimeAndKeepnet()
	{
		if (TimeAndWeatherManager.CurrentTime == null)
		{
			return;
		}
		this.time = TimeAndWeatherManager.CurrentTime.Value;
		DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
		dateTime = dateTime.Add(this.time);
		this._timeControl.text = MeasuringSystemManager.TimeString(dateTime);
		int currentDay = PondHelper.CurrentDay;
		int allDays = PondHelper.AllDays;
		this._daysControl.text = string.Format("{2} {0}/{1}", currentDay.ToString(CultureInfo.InvariantCulture), allDays, ScriptLocalization.Get("DayCaption"));
		FishCageContents fishCage = PhotonConnectionFactory.Instance.Profile.FishCage;
		if (fishCage != null && fishCage.Cage != null && fishCage.Cage.Durability > 0)
		{
			this._fishKeepnetTextIco.enabled = true;
			TMP_Text fishKeepnetText = this._fishKeepnetText;
			string text = "{0} \n{1}/{2} {3}";
			object[] array = new object[4];
			array[0] = fishCage.Cage.Name;
			int num = 1;
			float? weight = fishCage.Weight;
			array[num] = MeasuringSystemManager.FishWeight((weight == null) ? 0f : weight.Value).ToString("n3");
			array[2] = this.WeightFormated(fishCage.Cage.TotalWeight);
			array[3] = MeasuringSystemManager.FishWeightSufix();
			fishKeepnetText.text = string.Format(text, array);
			this._fishKeepnetMaxFishWeightText.text = string.Format("{0}: {1} {2}", ScriptLocalization.Get("MaxSingleFishWeightShort"), this.WeightFormated(fishCage.Cage.MaxFishWeight), MeasuringSystemManager.FishWeightSufix());
			this._fishKeepnetMaxFishWeightTextIco.text = "\ue810";
			if (!this.IsHintFishKeepnetActive)
			{
				this._fishKeepnetText.color = ((fishCage.Weight == null || fishCage.Cage.TotalWeight > fishCage.Weight.Value) ? this._fishNetWeightColor : this._fishNetWeightMaxColor);
				this._fishKeepnetTextIco.color = ((fishCage.Weight == null || fishCage.Cage.TotalWeight > fishCage.Weight.Value) ? this._fishNetWeightColor : this._fishNetWeightMaxColor);
				this._fishKeepnetMaxFishWeightText.color = ((!CatchedFishInfoHandler.IsDisplayed() || CatchedFishInfoHandler.CaughtFish == null || CatchedFishInfoHandler.CaughtFish.Weight <= fishCage.Cage.MaxFishWeight) ? this._fishNetWeightColor : this._fishNetWeightMaxColor);
				this._fishKeepnetMaxFishWeightTextIco.color = this._fishKeepnetMaxFishWeightText.color;
			}
		}
		else
		{
			this._fishKeepnetTextIco.enabled = false;
			this._fishKeepnetText.text = string.Empty;
			this._fishKeepnetMaxFishWeightText.text = string.Empty;
			this._fishKeepnetMaxFishWeightTextIco.text = string.Empty;
		}
	}

	private string WeightFormated(float w)
	{
		float num = MeasuringSystemManager.FishWeight(w);
		if (Math.Abs(num - (float)((int)num)) < 0.01f)
		{
			return num.ToString("n0");
		}
		return num.ToString("n1");
	}

	private void CheckNightTime()
	{
		if (!this.lastTime.IsNightFrame() && this.time.IsNightFrame() && !KeysHandlerAction.IsFishingInProgress() && GameFactory.Player != null && !GameFactory.Player.IsTackleThrown && !GameFactory.Player.IsInteractionWithRodStand && !InfoMessageController.Instance.IsActive)
		{
			if (!GameFactory.Player.HasAdvancedLicense)
			{
				string text = ScriptLocalization.Get("NightFishingWarning");
				Action action = new Action(KeysHandlerAction.LicenseShopHandler);
				bool flag = !UIHelper.ShowYesNo(text, null, null, "BuyLicenseButton", null, "CloseButton", null, action, null);
			}
			this.lastTime = this.time;
		}
		if (!this.time.IsNightFrame())
		{
			this.lastTime = this.time;
		}
	}

	private void UpdateRentLabel()
	{
		if (PhotonConnectionFactory.Instance.Profile == null)
		{
			return;
		}
		List<BoatRent> activeBoatRents = PhotonConnectionFactory.Instance.Profile.ActiveBoatRents;
		int i = 0;
		if (activeBoatRents != null)
		{
			int pondDayStart = PhotonConnectionFactory.Instance.PondDayStart;
			int currentDay = PondHelper.CurrentDay;
			int num = ((this.time.Hours >= pondDayStart) ? (24 - this.time.Hours + pondDayStart) : (pondDayStart - this.time.Hours));
			int num2 = ((this.time.Minutes <= 0) ? 0 : (60 - this.time.Minutes));
			num -= ((num2 <= 0) ? 0 : 1);
			while (i < activeBoatRents.Count)
			{
				BoatRent boatRent = activeBoatRents[i];
				if (currentDay <= boatRent.EndDay)
				{
					string text = ScriptLocalization.Get((boatRent.BoatType != ItemSubTypes.Kayak) ? "MotorBoat" : "UGC_Kayak");
					int num3 = boatRent.EndDay - currentDay;
					TimeSpan timeSpan = new TimeSpan(num3, num, num2, 0);
					this._rentText[i].text = string.Format("{0} {1} ({2})", ScriptLocalization.Get("RentWillExpireIn"), timeSpan.GetFormated(true, false), text);
				}
				else
				{
					this._rentText[i].text = string.Empty;
				}
				i++;
			}
		}
		for (int j = i; j < this._rentText.Length; j++)
		{
			this._rentText[j].text = string.Empty;
		}
	}

	public void HudEnable()
	{
		this.TournamentInfo.HudEnable();
	}

	private void SetHelpText()
	{
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			this._helpText.text = string.Empty;
			return;
		}
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			this._helpText.text = string.Format(ScriptLocalization.Get("F1GamepadPCHelpText"), "<size=22>" + HotkeyIcons.KeyMappings[InputControlType.Back] + "</size>");
		}
		else
		{
			this._helpText.text = ScriptLocalization.Get("F1HelpText");
		}
	}

	private void OnEnable()
	{
		bool flag = PhotonConnectionFactory.Instance.Profile.Tournament != null && !PhotonConnectionFactory.Instance.Profile.Tournament.IsEnded;
		this.TournamentInfo.gameObject.SetActive(flag);
		this.TimeLockRewind.SetActive(flag);
		bool flag2 = ClientMissionsManager.Instance != null && ClientMissionsManager.Instance.CurrentTrackedMission != null;
		this.MissionInfo.gameObject.SetActive(!flag && this.IsNotInTutorial && flag2);
		this._daysControl.gameObject.SetActive(!flag);
		if (flag)
		{
			this.TournamentInfo.Init();
		}
	}

	private bool IsNotInTournament
	{
		get
		{
			return !TournamentHelper.IS_IN_TOURNAMENT;
		}
	}

	private bool IsNotInTutorial
	{
		get
		{
			return !StaticUserData.IS_IN_TUTORIAL;
		}
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		this.SetHelpText();
	}

	[SerializeField]
	private CompassManager _compassManager;

	[SerializeField]
	private TextMeshProUGUI _fishKeepnetText;

	[SerializeField]
	private TextMeshProUGUI _fishKeepnetMaxFishWeightText;

	[SerializeField]
	private TextMeshProUGUI _fishKeepnetMaxFishWeightTextIco;

	[SerializeField]
	private TextMeshProUGUI _fishKeepnetTextIco;

	[SerializeField]
	private TextMeshProUGUI _daysControl;

	[SerializeField]
	private TextMeshProUGUI _timeControl;

	[SerializeField]
	private TextMeshProUGUI _helpText;

	[SerializeField]
	private TextMeshProUGUI[] _rentText;

	[SerializeField]
	private TextMeshProUGUI _userName;

	[SerializeField]
	private GameObject _windPowerSuffix;

	[SerializeField]
	private GameObject _temperatureWaterControl;

	public GameObject TimeLockRewind;

	public GameObject WeatherControl;

	public GameObject PressureControl;

	public GameObject WindDirectionControl;

	public GameObject WindPowerControl;

	public GameObject WindCompass;

	public GameObject TemperatureControl;

	public HudTournamentHandler TournamentInfo;

	public HudMissionHandler MissionInfo;

	private WeatherWidget weatherWidget;

	private float _timer;

	private float timerMax = 1f;

	private TimeSpan lastTime = new TimeSpan(8, 0, 0);

	private TimeSpan time;

	private readonly Color _fishNetWeightMaxColor = new Color(1f, 0.7647059f, 0f, 1f);

	private readonly Color _fishNetWeightColor = new Color(0.96862745f, 0.96862745f, 0.96862745f, 1f);
}
