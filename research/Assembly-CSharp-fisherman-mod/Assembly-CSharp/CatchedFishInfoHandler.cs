using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.PlayerProfile;
using DG.Tweening;
using I2.Loc;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CatchedFishInfoHandler : CatchedInfoBase
{
	public static Fish CaughtFish
	{
		get
		{
			return CatchedFishInfoHandler._caughtFish;
		}
	}

	public static bool IsDisplayed()
	{
		return CatchedFishInfoHandler.Instance != null && CatchedFishInfoHandler.Instance.gameObject.activeInHierarchy;
	}

	public static bool IsPersonalRecordDisplayed()
	{
		return CatchedFishInfoHandler.Instance != null && CatchedFishInfoHandler.Instance.gameObject.activeInHierarchy && CatchedFishInfoHandler.Instance.PersonalRecord != null && (CatchedFishInfoHandler.Instance.PersonalRecord.IsShow || CatchedFishInfoHandler.Instance.PersonalRecord.IsShowing);
	}

	private void Start()
	{
		CatchedFishInfoHandler.Instance = this;
		Text text = this._helpPanel.GetComponentsInChildren<Text>().FirstOrDefault((Text x) => x.font != this._consoleHotKey.font);
		if (text != null)
		{
			this._consoleHotKey.font = text.font;
			this._consoleHotKey.fontSize = text.fontSize;
			this._consoleHotKey.resizeTextMaxSize = text.fontSize;
		}
		this.PenaltyPanelInfo.FastHidePanel();
		this.InfoPanel.GetComponent<AlphaFade>().FastHidePanel();
		this.FishCagePanel.GetComponent<RectTransform>().anchoredPosition = this._originalPosition;
		this.FishCagePanel.FastHidePanel();
		this.PersonalRecord.FastHidePanel();
		this._takeButton = this.TakeButton.GetComponent<Button>();
		base.gameObject.SetActive(false);
		this.MustTakeImageRoot.SetActive(false);
		this.MustReleaseImageRoot.SetActive(false);
		GameFactory.FishSpawner.FishCaught += this.FishSpawnerFishCaught;
		this.FishCagePanel.HideFinished += this.FishCagePanel_HideFinished;
		this.FishCagePanel.OnShowCalled.AddListener(new UnityAction(this.RefreshTextHinParent));
	}

	private void OnDestroy()
	{
		this.FishCagePanel.HideFinished -= this.FishCagePanel_HideFinished;
		this.FishCagePanel.OnShowCalled.RemoveListener(new UnityAction(this.RefreshTextHinParent));
	}

	private void FishCagePanel_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this._photoModeLabels.SetActive(true);
		this.RefreshTextHinParent();
	}

	private void RefreshTextHinParent()
	{
		if (HintSystem.Instance != null && HintSystem.Instance.TextHintParent != null)
		{
			HintSystem.Instance.TextHintParent.Refresh();
		}
	}

	public void SetActiveReleaseButton(bool flag)
	{
		this._releaseButton.interactable = flag;
	}

	public void Activate(bool isNewFish)
	{
		if (CatchedFishInfoHandler._caughtFish == null)
		{
			this._waitingFish = true;
			UIHelper.Waiting(true, null);
			return;
		}
		this.Subscribe();
		base.gameObject.SetActive(true);
		this.OpenMessage(isNewFish);
		this._isInPhotoMode = false;
		if (PhotonConnectionFactory.Instance.Profile.Tournament != null)
		{
			LogoUpload.SendStatic();
		}
		if (TournamentHelper.TOURNAMENT_ENDED_STILL_IN_ROOM)
		{
			this.DisableTakeButton();
			if (this.IsKeepnet)
			{
				this.FishCagePanel.FastHidePanel();
			}
		}
		this._helpPanel.SetActive(PhotonConnectionFactory.Instance.Profile.IsTutorialFinished);
	}

	protected override void Update()
	{
		base.Update();
		if (ControlsController.ControlsActions.ShowKeepnetIn3D.WasPressed && !this._isInPhotoMode && !TournamentHelper.TOURNAMENT_ENDED_STILL_IN_ROOM)
		{
			this.KeepnetOnClick();
		}
		if (this._isOpened && !this._isInPhotoMode && !ControlsController.ControlsActions.IsBlockedAxis)
		{
			ControlsController.ControlsActions.BlockAxis();
		}
	}

	private void FishSpawnerFishCaught(object sender, FishCaughtEventArgs e)
	{
		CatchedFishInfoHandler._caughtFish = e.CaughtFish;
		this._isIllegal = e.IsIllegal;
		this._canTake = e.CanTake;
		this._canRelease = e.CanRelease;
		this._penalty = e.Penalty;
		InGameMap.Lure = ((StaticUserData.RodInHand.Bait == null) ? StaticUserData.RodInHand.Hook : StaticUserData.RodInHand.Bait);
		InGameMap.FishInfo = new CaughtFish
		{
			Fish = e.CaughtFish,
			LineId = StaticUserData.RodInHand.Line.ItemId,
			LineMaxLoad = StaticUserData.RodInHand.Line.MaxLoad,
			Time = TimeAndWeatherManager.CurrentInGameTime().TimeOfDay.ToTimeOfDay(),
			RodTemplate = StaticUserData.RodInHand.RodTemplate,
			WeatherIcon = TimeAndWeatherManager.CurrentWeather.Icon
		};
		if (InGameMap.FishInfo.RodTemplate.IsLureBait())
		{
			List<int> list = new List<int>();
			if (StaticUserData.RodInHand.Hook != null)
			{
				list.Add(StaticUserData.RodInHand.Hook.ItemId);
			}
			if (StaticUserData.RodInHand.Bait != null)
			{
				list.Add(StaticUserData.RodInHand.Bait.ItemId);
			}
			InGameMap.FishInfo.AllBaitIds = list.ToArray();
		}
		else
		{
			InGameMap.FishInfo.AllBaitIds = new int[] { (InGameMap.Lure != null) ? InGameMap.Lure.ItemId : 0 };
		}
		if (this._waitingFish)
		{
			UIHelper.Waiting(false, null);
			this.Activate(GameFactory.Player.IsNewFish);
		}
	}

	public void KeepnetOnClick()
	{
		if (this.FishCagePanel.IsShow)
		{
			this.FishCagePanel.HidePanel();
		}
		else
		{
			this._photoModeLabels.SetActive(false);
			this.FishCagePanel.ShowPanel();
			this.TournamentFishCage.Init();
		}
	}

	public void TournamentTimeEnded(TournamentIndividualResults result)
	{
		this.Instance_OnTournamentTimeEnded();
	}

	internal void Subscribe()
	{
		if (this._subscried)
		{
			return;
		}
		this._subscried = true;
		this.TournamentFishCage.OnFishReleased += this.TakeButtonRefresh;
	}

	private void Instance_OnTournamentTimeEnded()
	{
		if (this.IsInTournament)
		{
			this.DisableTakeButton();
			if (this.IsKeepnet)
			{
				this.FishCagePanel.HidePanel();
			}
		}
	}

	internal void Unsubscribe()
	{
		if (!this._subscried)
		{
			return;
		}
		this._subscried = false;
		this.TournamentFishCage.OnFishReleased -= this.TakeButtonRefresh;
	}

	internal void TakeButtonRefresh()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (TournamentHelper.TOURNAMENT_ENDED_STILL_IN_ROOM || profile.FishCage == null || profile.FishCage.Cage == null || (profile.Tournament != null && profile.Tournament.MaxFishInCage <= profile.FishCage.Fish.Count) || !profile.FishCage.CanAdd(CatchedFishInfoHandler._caughtFish))
		{
			this.DisableTakeButton();
		}
		else
		{
			this.EnableTakeButton();
		}
	}

	public bool IsInTournament
	{
		get
		{
			return PhotonConnectionFactory.Instance.CurrentTournamentId != null;
		}
	}

	public bool IsKeepnet
	{
		get
		{
			return this.FishCagePanel.IsShow || this.FishCagePanel.IsShowing;
		}
	}

	private void UpdateButtonTexts(Text[] buttonTexts, bool flag)
	{
		Color color = (flag ? Color.white : this.CanActionColor);
		foreach (Text text in buttonTexts)
		{
			text.color = color;
		}
	}

	private string CalcExperience(float coef, float maxExp, string[] bonusTexts)
	{
		if (coef > 0f && coef <= maxExp * 0.33f)
		{
			return bonusTexts[0];
		}
		if (coef > maxExp * 0.33f && coef <= maxExp * 0.66f)
		{
			return bonusTexts[1];
		}
		if (coef > maxExp * 0.66f && coef <= maxExp + 1f)
		{
			return bonusTexts[2];
		}
		return string.Empty;
	}

	private void OpenMessage(bool isNewFish)
	{
		this._waitingFish = false;
		this._isOpened = true;
		this.Money.text = "\ue62b 0";
		CursorManager.ShowCursor();
		ControlsController.ControlsActions.BlockAxis();
		this.TournamentIcon.SetActive(CatchedFishInfoHandler._caughtFish.TournamentScore != null);
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		this.FishCagePanel.GetComponent<RectTransform>().anchoredPosition = ((this._canTake && this._canRelease) ? this._originalPosition : this._penaltyPosition);
		this.UpdateButtonTexts(this.TakeButtonTexts, this._canTake);
		this.UpdateButtonTexts(this.ReleaseButtonTexts, this._canRelease);
		if (this._isIllegal)
		{
			this.PenaltyInfoText.text = string.Format(ScriptLocalization.Get((!GameFactory.Player.IsBoatFishing) ? "IllegalFishInfo" : "BoatFishingFine"), "\n", "\ue62b", this._penalty);
		}
		else
		{
			this.MustTakeImageRoot.SetActive(!this._canRelease);
			this.MustReleaseImageRoot.SetActive(!this._canTake);
			if (!this._canRelease)
			{
				this.PenaltyInfoText.text = string.Format(ScriptLocalization.Get("MustTakeInfo"), "\ue62b", this._penalty, "\n");
			}
			if (!this._canTake)
			{
				this.PenaltyInfoText.text = string.Format(ScriptLocalization.Get("MustReleaseInfo"), "\ue62b", this._penalty, "\n");
			}
		}
		if (this._isIllegal || !this._canRelease || !this._canTake)
		{
			this.PenaltyPanelInfo.ShowPanel();
		}
		if (isNewFish && StaticUserData.CurrentPond.PondId != 2 && CatchedFishInfoHandler._caughtFish.IsPersonalRecord != null && CatchedFishInfoHandler._caughtFish.IsPersonalRecord.Value)
		{
			this.PersonalRecordFish.text = CacheLibrary.MapCache.GetFishCategory(CatchedFishInfoHandler._caughtFish.CategoryId).Name;
			this.PersonalRecord.ShowPanel();
			base.Invoke("ClosePersonalRecord", 7f);
		}
		this.InfoPanel.GetComponent<AlphaFade>().ShowPanel();
		this.Name.text = CatchedFishInfoHandler._caughtFish.Name;
		this.Weight.text = string.Format("{0} {1}", MeasuringSystemManager.FishWeight(CatchedFishInfoHandler._caughtFish.Weight).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
		this.Length.text = string.Format("{0} {1}", MeasuringSystemManager.FishLength(CatchedFishInfoHandler._caughtFish.Length).ToString("N3"), MeasuringSystemManager.FishLengthSufix());
		if (CatchedFishInfoHandler._caughtFish.Experience != null && CatchedFishInfoHandler._caughtFish.BaseExperience != null)
		{
			this.BonusSliderText.text = string.Empty;
			float num = CatchedFishInfoHandler._caughtFish.Experience.Value - CatchedFishInfoHandler._caughtFish.BaseExperience.Value;
			if (num > 0f)
			{
				this.BonusSliderText.color = Color.green;
				this.BonusSliderText.text = this.CalcExperience(num / CatchedFishInfoHandler._caughtFish.BaseExperience.Value, PhotonConnectionFactory.Instance.MaxBonusExp, new string[] { "\ue731", "\ue731 \ue731", "\ue731 \ue731 \ue731" });
			}
			else if (num < 0f)
			{
				this.BonusSliderText.color = Color.red;
				this.BonusSliderText.text = this.CalcExperience(Mathf.Abs(num / CatchedFishInfoHandler._caughtFish.BaseExperience.Value), PhotonConnectionFactory.Instance.MaxPenaltyExp, new string[] { "\ue730", "\ue730 \ue730", "\ue730 \ue730 \ue730" });
			}
			if (isNewFish)
			{
				this.SliderText.text = string.Format(ScriptLocalization.Get("XPGained"), Math.Round((double)CatchedFishInfoHandler._caughtFish.Experience.Value, 0));
				ShortcutExtensions.DOShakeScale(this.SliderText.transform, 1f, 0.7f, 9, 60f, true);
			}
			if (CatchedFishInfoHandler._caughtFish.GoldCost != null && CatchedFishInfoHandler._caughtFish.GoldCost > 0f)
			{
				this.Money.text = string.Format("{0} {1}", "\ue62c ", (int)CatchedFishInfoHandler._caughtFish.GoldCost.Value);
			}
			else if (CatchedFishInfoHandler._caughtFish.SilverCost != null && CatchedFishInfoHandler._caughtFish.SilverCost > 0f)
			{
				this.Money.text = string.Format("{0} {1}", "\ue62b ", (int)CatchedFishInfoHandler._caughtFish.SilverCost.Value);
			}
			else
			{
				this.Money.text = string.Format("{0} {1}", "\ue62b ", 0);
			}
		}
		if (isNewFish)
		{
			GameFactory.ChatListener.OnLocalEvent(new LocalEvent
			{
				EventType = LocalEventType.FishCaught,
				Player = PlayerProfileHelper.ProfileToPlayer(profile),
				Fish = CatchedFishInfoHandler._caughtFish
			});
		}
		float progress = PlayerStatisticsInit.GetProgress(profile);
		float num2 = ((CatchedFishInfoHandler._caughtFish.Experience == null) ? progress : this.PrevProgress(profile, CatchedFishInfoHandler._caughtFish.Experience.Value));
		if (isNewFish)
		{
			this.LevelProgress.InitProgressDisplay(num2, progress);
		}
		FishCageContents fishCage = profile.FishCage;
		if (fishCage == null || !fishCage.CanAdd(CatchedFishInfoHandler._caughtFish))
		{
			if (fishCage == null || fishCage.Cage == null)
			{
				GameFactory.Message.ShowHaventFishkeeper();
			}
			else if (CatchedFishInfoHandler._caughtFish.Weight > fishCage.Cage.MaxFishWeight)
			{
				GameFactory.Message.ShowCantTakeFishTooBig();
			}
			else
			{
				GameFactory.Message.ShowCantTakeFish();
			}
			this.DisableTakeButton();
		}
		else
		{
			this.EnableTakeButton();
		}
		if (this.FishCagePanel.IsShow)
		{
			this.TournamentFishCage.Init();
		}
		if (fishCage != null && fishCage.Fish != null && profile.Tournament != null && profile.Tournament.MaxFishInCage != null && profile.Tournament.MaxFishInCage <= fishCage.Fish.Count)
		{
			this.ShowTournamentFishCage(profile.Tournament.MaxFishInCage.Value);
		}
		if (TournamentHelper.TOURNAMENT_ENDED_STILL_IN_ROOM)
		{
			this.DisableTakeButton();
		}
	}

	private float PrevProgress(Profile profile, float expGained)
	{
		if (profile == null)
		{
			return 0f;
		}
		bool flag = profile.Level != PhotonConnectionFactory.Instance.LevelCap;
		long num = ((!flag) ? profile.RankExperience : profile.Experience);
		long num2 = ((!flag) ? profile.ExpToNextRank : profile.ExpToNextLevel);
		long num3 = ((!flag) ? profile.ExpToThisRank : profile.ExpToThisLevel);
		float num4 = (float)num - expGained;
		if (num2 - num3 == 0L)
		{
			return 1f;
		}
		if (num4 < (float)num3)
		{
			return 0f;
		}
		if (num4 > (float)num2)
		{
			return 1f;
		}
		return (num4 - (float)num3) / (float)(num2 - num3);
	}

	private void ShowTournamentFishCage(int allowedFish)
	{
		this.DisableTakeButton();
		this._photoModeLabels.SetActive(false);
		this.FishCagePanel.ShowPanel();
		this.TournamentFishCage.Init(allowedFish);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.Unsubscribe();
	}

	public void ClosePersonalRecord()
	{
		MonoBehaviour.print("Personal Record Hide Panel performs");
		this.PersonalRecord.HidePanel();
	}

	private void EnableTakeButton()
	{
		if (this.TakeButton != null)
		{
			this._takeButton.interactable = true;
		}
	}

	private void DisableTakeButton()
	{
		if (this.TakeButton != null && this._takeButton.interactable)
		{
			this._takeButton.interactable = false;
		}
	}

	public void ReleaseClick()
	{
		this.PenaltyPanelInfo.FastHidePanel();
		if (this._isInPhotoMode)
		{
			return;
		}
		if (GameFactory.Player.State == typeof(PlayerShowFishLineIdle) || GameFactory.Player.State == typeof(PlayerShowFishIdle))
		{
			PhotonConnectionFactory.Instance.SaveTelemetryInfo(1, string.Format("ReleaseClick: {0}; {1}", Input.mousePosition.x.ToString("##.000"), Input.mousePosition.y.ToString("##.000")));
			GameActionAdapter.Instance.ReleaseFish();
			AnalyticsFacade.CatchFish(CatchedFishInfoHandler._caughtFish, StaticUserData.CurrentPond.PondId, TimeAndWeatherManager.CurrentWeather.Name, TimeAndWeatherManager.CurrentInGameTime(), true);
			this.CloseMessage();
			if (!this._canRelease)
			{
				FishPenaltyHelper fishPenaltyHelper = MessageBoxList.Instance.gameObject.AddComponent<FishPenaltyHelper>();
				fishPenaltyHelper.CheckPenalty(false);
			}
		}
	}

	public void PhotoModeClick()
	{
		GameFactory.Player.IsPhotoModeRequested = true;
	}

	public void TakeClick()
	{
		this.PenaltyPanelInfo.FastHidePanel();
		if (this._isInPhotoMode)
		{
			return;
		}
		if (GameFactory.Player.State == typeof(PlayerShowFishLineIdle) || GameFactory.Player.State == typeof(PlayerShowFishIdle))
		{
			PhotonConnectionFactory.Instance.SaveTelemetryInfo(1, string.Format("TakeClick: {0}; {1}", Input.mousePosition.x.ToString("##.000"), Input.mousePosition.y.ToString("##.000")));
			GameActionAdapter.Instance.TakeFish();
			AnalyticsFacade.CatchFish(CatchedFishInfoHandler._caughtFish, StaticUserData.CurrentPond.PondId, TimeAndWeatherManager.CurrentWeather.Name, TimeAndWeatherManager.CurrentInGameTime(), false);
			this.CloseMessage();
			if (!this._canTake)
			{
				FishPenaltyHelper fishPenaltyHelper = MessageBoxList.Instance.gameObject.AddComponent<FishPenaltyHelper>();
				fishPenaltyHelper.CheckPenalty(false);
			}
		}
		if (PhotonConnectionFactory.Instance.Profile.FishCage.Cage.Durability <= 2)
		{
			GameFactory.Message.ShowCageSoonBrake();
		}
	}

	private void CloseMessage()
	{
		this._isOpened = false;
		GameFactory.Player.Tackle.IsShowing = false;
		TutorialSlidesController.FishCounter++;
		CatchedFishInfoHandler._caughtFish = null;
		ControlsController.ControlsActions.UnBlockAxis();
		CursorManager.HideCursor();
		base.gameObject.SetActive(false);
	}

	protected override void UpdateHelp()
	{
		this.HelpInfo[1].gameObject.SetActive(false);
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this.HelpInfo[0].text = string.Format(ScriptLocalization.Get("ShowHideKeepnetHotkey"), this.GetKeyMapping(InputControlType.Action4));
			this.HelpInfo[2].text = string.Format(ScriptLocalization.Get("ExamineFishHotkey"), this.GetKeyMapping(InputControlType.RightBumper) + " + " + this.GetKeyMapping(InputControlType.RightStickRight));
			this.HelpInfo[1].text = string.Format(ScriptLocalization.Get("Fish3DHotkey"), this.GetKeyMapping(InputControlType.LeftBumper));
		}
		else
		{
			this.HelpInfo[0].text = string.Format(ScriptLocalization.Get("ShowHideKeepnetHotkey"), this.GetColored("Y"));
			this.HelpInfo[2].text = string.Format(ScriptLocalization.Get("ExamineFishHotkey"), this.GetColored("RMB"));
		}
		this._consoleHotKey.gameObject.SetActive(InputModuleManager.IsConsoleMode);
		this._pcHotKey.gameObject.SetActive(!InputModuleManager.IsConsoleMode);
	}

	[SerializeField]
	private GameObject _photoModeLabels;

	[SerializeField]
	private Button _releaseButton;

	[SerializeField]
	private Text _pcHotKey;

	[SerializeField]
	private Text _consoleHotKey;

	[SerializeField]
	private RectTransform _PhotoModePanel;

	[SerializeField]
	private GameObject _helpPanel;

	public GameObject InfoPanel;

	public Text Name;

	public Text Weight;

	public Text Length;

	public Text Money;

	public Text Expiriences;

	public GameObject TakeButton;

	public Text[] TakeButtonTexts;

	public Text[] ReleaseButtonTexts;

	public AlphaFade PenaltyPanelInfo;

	public ProgressDisplayer LevelProgress;

	public Text SliderText;

	public Text BonusSliderText;

	public Text PenaltyInfoText;

	public GameObject TournamentIcon;

	public GameObject MustTakeImageRoot;

	public GameObject MustReleaseImageRoot;

	public FishcagePanelInit TournamentFishCage;

	public AlphaFade FishCagePanel;

	public Text[] HelpInfo;

	public AlphaFade PersonalRecord;

	public Text PersonalRecordFish;

	private static Fish _caughtFish;

	private bool _isIllegal;

	private bool _canTake;

	private bool _canRelease;

	private bool _isOpened;

	private Button _takeButton;

	private Vector3 _originalPosition = new Vector3(0f, 85f, 0f);

	private Vector3 _penaltyPosition = new Vector3(0f, 177f, 0f);

	private int _penalty;

	private readonly Color CanActionColor = new Color(0.7607843f, 0.38039216f, 0.38431373f);

	private bool _waitingFish;

	private bool _subscried;

	public static CatchedFishInfoHandler Instance;

	private MessageBox _checkPenaltyPanel;
}
