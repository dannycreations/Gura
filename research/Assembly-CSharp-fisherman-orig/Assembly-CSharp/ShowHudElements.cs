using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using DG.Tweening;
using I2.Loc;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;

public class ShowHudElements : MonoBehaviour
{
	public FishingHandler FishingHndl
	{
		get
		{
			return this._fishingHandler;
		}
	}

	public SailingHandler SailingHndl
	{
		get
		{
			return this._sailingHandler;
		}
	}

	public ShowingFishingHUD ShowingFishingHUD { get; private set; }

	public LureInfoHandler LureInfoHandler { get; private set; }

	public static ShowHudElements Instance { get; private set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnInteractiveObjectHidePanelHidden = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> ECloseCatchedItemWindow = delegate
	{
	};

	public bool IsCatchedFishWindowActive
	{
		get
		{
			return this._CatchedFishInfoHandler != null && this._CatchedFishInfoHandler.gameObject.activeSelf;
		}
	}

	public bool IsCatchedItemWindowActive
	{
		get
		{
			return this._ciw != null && this._ciw.gameObject.activeSelf;
		}
	}

	public bool IsCatchedWindowActive
	{
		get
		{
			return this.IsCatchedFishWindowActive || this.IsCatchedItemWindowActive;
		}
	}

	public ShowHudStates CurrentState
	{
		get
		{
			return this._currentState;
		}
	}

	public AdditionalInfoHandler AdditionalInfo
	{
		get
		{
			return this._addInfo;
		}
	}

	public RewindHoursHandler RewindHours
	{
		get
		{
			return this.rewindHoursScreen;
		}
	}

	private void Awake()
	{
		ShowHudElements.Instance = this;
		this._alphaFade = base.GetComponent<AlphaFade>();
		this._ciw = Object.Instantiate<CatchedItemWindow>(this._catchedItemWindow, Vector3.zero, Quaternion.identity, base.transform);
		this._CatchedFishInfoHandler = Object.Instantiate<CatchedFishInfoHandler>(this._catchedFishWindow, Vector3.zero, Quaternion.identity, base.transform);
		this._addInfo = Object.Instantiate<AdditionalInfoHandler>(this.AddInfoHandler, Vector3.zero, Quaternion.identity, base.transform);
		this._lurePanel = Object.Instantiate<EquipmentInGamePanel>(this.SetLurePanel, Vector3.zero, Quaternion.identity, base.transform);
		this._rodPanel = Object.Instantiate<EquipmentInGamePanel>(this.SetRodPanel, Vector3.zero, Quaternion.identity, base.transform);
		this._breakLine = Object.Instantiate<BreakLineHandler>(this.BreakLine, Vector3.zero, Quaternion.identity, base.transform);
		DebugDialog debugDialog = Object.Instantiate<DebugDialog>(this.DebugDialog, Vector3.zero, Quaternion.identity, base.transform);
		this._boatHintPanel = Object.Instantiate<BoatHintPanel>(this.BoatHintPanel, Vector3.zero, Quaternion.identity, base.transform);
		this._interactiveObjectPanel = Object.Instantiate<InteractiveObjectPanelHandler>(this.InteractiveObjectPanel, Vector3.zero, Quaternion.identity, base.transform);
		this._fishingHandler = Object.Instantiate<FishingHandler>(this.FishingHandler, Vector3.zero, Quaternion.identity, base.transform);
		this._sailingHandler = Object.Instantiate<SailingHandler>(this.SailingHandler, Vector3.zero, Quaternion.identity, base.transform);
		this.rewindHoursScreen = Object.Instantiate<RewindHoursHandler>(this.RewindHH, Vector3.zero, Quaternion.identity, base.transform);
		this._prolongOfStay = Object.Instantiate<ProlongationOfStay>(this.ProlongOfStay, Vector3.zero, Quaternion.identity, base.transform);
		HidesPanel hidesPanel = Object.Instantiate<HidesPanel>(this.Messages, Vector3.zero, Quaternion.identity, base.transform);
		this._crossHair = Object.Instantiate<CrossHair>(this.CrossHair, Vector3.zero, Quaternion.identity, base.transform);
		this._debugCrossHair = Object.Instantiate<TmpCrossHair>(this.DebugCrossHair, Vector3.zero, Quaternion.identity, base.transform);
		this._debugCrossHair.gameObject.SetActive(false);
		this._angleIndicator = Object.Instantiate<RodStandAngleIndicator>(this.AngleIndicator, base.transform);
		this._changeRoomPanel = Object.Instantiate<AlphaFade>(this.ChangeRoomPanel, base.transform, false);
		this._prolongOfStay.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		this.rewindHoursScreen.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		this._ciw.GetComponent<RectTransform>().anchoredPosition = new Vector2(15f, 0f);
		this._CatchedFishInfoHandler.GetComponent<RectTransform>().anchoredPosition = new Vector2(15f, 0f);
		this._addInfo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		this._lurePanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		this._rodPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		this._boatHintPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		this._interactiveObjectPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		this._fishingHandler.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		this._sailingHandler.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		hidesPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		(this._crossHair.transform as RectTransform).anchoredPosition = Vector2.zero;
		(this._debugCrossHair.transform as RectTransform).anchoredPosition = Vector2.zero;
		this._changeRoomPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		this._HidesPanels[0] = hidesPanel;
		this._HidesPanels[1] = this._fishingHandler.GetComponent<HidesPanel>();
		this._HidesPanels[2] = this._sailingHandler.GetComponent<HidesPanel>();
		this._HidesPanels[3] = this._addInfo.GetComponent<HidesPanel>();
		this.ShowingFishingHUD = this._fishingHandler.GetComponent<HidesPanel>().ActionObject.GetComponent<ShowingFishingHUD>();
		this.LureInfoHandler = this._fishingHandler.transform.GetComponentInChildren<LureInfoHandler>();
		this._equipmentChangeHandler = base.GetComponent<EquipmentChangeHandler>();
		this._CatchedFishInfoHandler.ActivatePhotoMode += this.OnActivatePhotoMode;
		this._ciw.ActivatePhotoMode += this.OnActivatePhotoMode;
		CatchedItemWindow ciw = this._ciw;
		ciw.EClose = (Action<bool>)Delegate.Combine(ciw.EClose, new Action<bool>(this.OnCloseCatchedItemWindow));
	}

	private void Start()
	{
		GameFactory.ChatInGameController = Object.Instantiate<NewChatInGameController>(this.Chat, Vector3.zero, Quaternion.identity, base.transform);
		this._chatCanvasGroup = GameFactory.ChatInGameController.GetComponent<CanvasGroup>();
		List<CanvasGroup> list = new List<CanvasGroup>
		{
			this._chatCanvasGroup,
			this._prolongOfStay.GetComponent<CanvasGroup>(),
			this._CatchedFishInfoHandler.GetComponent<CanvasGroup>(),
			this.rewindHoursScreen.GetComponent<CanvasGroup>(),
			this._breakLine.GetComponent<CanvasGroup>(),
			this._interactiveObjectPanel.GetComponent<CanvasGroup>(),
			this._ciw.GetComponent<CanvasGroup>()
		};
		this._ciw.transform.SetAsLastSibling();
		this._CatchedFishInfoHandler.transform.SetAsLastSibling();
		list.AddRange(this._HidesPanels.Select((HidesPanel t) => t.GetComponent<CanvasGroup>()));
		this._equipmentChangeHandler.Init(this._lurePanel, this._rodPanel, list.ToArray());
		GameFactory.InGameChatCreated();
	}

	public int GetCurrentRewindPanelHoursValue()
	{
		if (this.rewindHoursScreen == null)
		{
			return 0;
		}
		return this.rewindHoursScreen.HoursOffset;
	}

	private void OnEnable()
	{
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			this._alphaFade.FastHidePanel();
		}
		else
		{
			this._alphaFade.FastShowPanel();
		}
		if (this._addInfo != null)
		{
			this._addInfo.HudEnable();
		}
		Debug.Log("HUD::OnEnable()");
	}

	private void OnDisable()
	{
		this._alphaFade.FastHidePanel();
		Debug.Log("HUD::OnDisable()");
	}

	private void OnDestroy()
	{
		this.StopTimer();
		this._ttm.Dispose();
		PlayerPrefs.DeleteKey("SetupRodStandHelp");
		PlayerPrefs.DeleteKey("HintClipLine");
		PlayerPrefs.DeleteKey("RodStandReplaceWithKeysHelp");
		PlayerPrefs.DeleteKey("RodStandTakeWithKeysHelp");
		PlayerPrefs.DeleteKey("RodStandPutWithKeysHelp");
		this._interactiveObjectPanel.HideFinished -= this._interactiveObjectPanel_HideFinished;
	}

	private void Update()
	{
		this.UpdateTooltips();
		this.UpdateF1FirstTime();
		if (GameFactory.Player.State != typeof(PlayerPhotoMode) && this.IsShowHudWasPressed())
		{
			ShowHudStates currentState = this._currentState;
			if (currentState != ShowHudStates.ShowAll)
			{
				if (currentState != ShowHudStates.ShowWithoutChat)
				{
					if (currentState == ShowHudStates.HideAll)
					{
						for (int i = 0; i < this._HidesPanels.Length; i++)
						{
							this._HidesPanels[i].Show();
						}
						GameFactory.Player.SetLabelsVisibility(true);
						this._currentState = ShowHudStates.ShowAll;
						GameFactory.ChatInGameController.SetHudState(this._currentState);
					}
				}
				else
				{
					for (int j = 0; j < this._HidesPanels.Length; j++)
					{
						this._HidesPanels[j].Hide();
					}
					GameFactory.Player.SetLabelsVisibility(false);
					this._currentState = ShowHudStates.HideAll;
				}
			}
			else
			{
				this._currentState = ShowHudStates.ShowWithoutChat;
				GameFactory.ChatInGameController.SetHudState(this._currentState);
			}
		}
		if (GameFactory.Player.State != typeof(PlayerPhotoMode) && !this._mondatoryHide && !this._alphaFade.IsShow && !this._alphaFade.IsShowing && ShowHudElements._pondHelpers.PondControllerList.FirstPerson.activeSelf && !ShowHudElements._pondHelpers.PondControllerList.IsInMenu)
		{
			this._alphaFade.ShowPanel();
		}
	}

	private void UpdateF1FirstTime()
	{
		if (UIHelper.IsShowHelpFirstTime && (InfoMessageController.Instance == null || !InfoMessageController.Instance.IsActive) && MessageFactory.InfoMessagesQueue.Count == 0 && !StaticUserData.IS_IN_TUTORIAL)
		{
			UIHelper.ShowHelpFirstTime();
		}
	}

	public void OnInputDeviceActivated(InputModuleManager.InputType type)
	{
		if (GameFactory.ChatInGameController != null)
		{
			GameFactory.ChatInGameController.OnInputDeviceActivated(type);
		}
	}

	public void ActivateCrossHair(bool flag)
	{
		flag = flag && !this._equipmentChangeHandler.IsBusy;
		flag = flag && !Cursor.visible;
		this._crossHair.Activate(flag);
	}

	public bool IsEquipmentChangeBusy()
	{
		return this._equipmentChangeHandler != null && this._equipmentChangeHandler.IsBusy;
	}

	public void ActivateDebugCrossHair(bool flag)
	{
		this._debugCrossHair.gameObject.SetActive(flag);
	}

	public bool ActivateAngleMeter(bool flag)
	{
		flag = flag && !this._equipmentChangeHandler.IsBusy;
		flag = flag && !Cursor.visible;
		this._angleIndicator.SetActive(flag);
		return flag;
	}

	public bool IsAngleMeterActive()
	{
		return this._angleIndicator.IsActive();
	}

	public void SetNormalizedAngle(float angle)
	{
		this._angleIndicator.SetNormalizedAngle(angle, false);
	}

	public void SetCrossHairState(CrossHair.CrossHairState state)
	{
		try
		{
			this._crossHair.SetState(state, false);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void SetTutorial(bool flag)
	{
		GameFactory.Message.IgnoreMessages = true;
		GameFactory.ChatInGameController.TurnOff();
	}

	public void ActivateGameMap(bool flag)
	{
		if (flag)
		{
			this.HideHud();
		}
		else
		{
			this.ShowHud();
		}
		base.GetComponent<CanvasGroup>().blocksRaycasts = !flag;
	}

	public bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	public void SetCanvasAlpha(float v)
	{
		base.GetComponent<CanvasGroup>().alpha = v;
		if (v >= 1f && GameFactory.Player != null && GameFactory.Player.InFishingZone)
		{
			this.OnEnterFishingZone();
		}
	}

	public void SetCatchedFishActiveReleaseButton(bool flag)
	{
		this._CatchedFishInfoHandler.SetActiveReleaseButton(flag);
	}

	public void ShowCatchedFishWindow(bool isNewFish)
	{
		GameFactory.ChatInGameController.ShowCatchedFish(isNewFish);
		this._CatchedFishInfoHandler.Activate(isNewFish);
	}

	public void SetCatchedFishWindowActivity(bool flag)
	{
		this._CatchedFishInfoHandler.gameObject.SetActive(flag);
	}

	public void TournamentTimeEnded(TournamentIndividualResults result)
	{
		this._CatchedFishInfoHandler.TournamentTimeEnded(result);
		this.AddInfoHandler.TournamentTimeEnded(result);
	}

	public void ShowCatchedItemWindow(string itemName, string itemType, GameFactory.RodSlot rodSlot)
	{
		ControlsController.ControlsActions.BlockAxis();
		CursorManager.ShowCursor();
		this._ciw.Activate(itemName, itemType, rodSlot);
	}

	private void OnCloseCatchedItemWindow(bool wasTaken)
	{
		ControlsController.ControlsActions.UnBlockAxis();
		CursorManager.HideCursor();
		this.ECloseCatchedItemWindow(wasTaken);
	}

	public void OnEnterFishingZone()
	{
		bool flag = RodPodHelper.FindPodOnDoll() != null;
		if (flag && GameFactory.Player != null && !GameFactory.Player.IsSailing)
		{
			string text;
			string text2;
			if (SettingsManager.InputType != InputModuleManager.InputType.GamePad)
			{
				text = "SetupRodStandHelp";
				text2 = "<size=18><b><color=#FFEE44FF>9</color></b></size>";
			}
			else
			{
				bool flag2;
				string icoByActionName = HotkeyIcons.GetIcoByActionName("RodPanel", out flag2);
				text2 = "<size=24><color=#FFEE44FF>" + icoByActionName + "</color></size>";
				text = "SetupRodStandHelpGamepad";
			}
			this.ShowHintCount("SetupRodStandHelp", string.Format(ScriptLocalization.Get(text), text2), 1);
		}
	}

	public void ShowHintCount(string locId, CustomPlayerAction action)
	{
		if (this._boatHintPanel == null)
		{
			return;
		}
		string text = string.Format("{0}", locId);
		int @int = PlayerPrefs.GetInt(text, 0);
		if (@int <= 0 && !this._boatHintPanel.IsActive)
		{
			PlayerPrefs.SetInt(text, @int + 1);
			this._boatHintPanel.ShowPanel(locId, action, 5f);
		}
	}

	public void ShowHintCount(string locId, string text, int maxCount = 1)
	{
		if (this._boatHintPanel == null)
		{
			return;
		}
		string text2 = string.Format("{0}", locId);
		int @int = PlayerPrefs.GetInt(text2, 0);
		if (@int <= Mathf.Max(0, maxCount - 1) && !this._boatHintPanel.IsActive)
		{
			PlayerPrefs.SetInt(text2, @int + 1);
			this._boatHintPanel.ShowPanel(text, 5f);
		}
	}

	public void ShowBoatRent()
	{
		this._boatHintPanel.ShowPanel("RentPrompt", ControlsController.ControlsActions.InteractObject, 0.1f);
	}

	public void ShowBoatBoarding()
	{
		this._boatHintPanel.ShowPanel("BoatBoarding", ControlsController.ControlsActions.InteractObject, 0.1f);
	}

	public void ShowBoatUnBoarding()
	{
		this._boatHintPanel.ShowPanel("BoatUnBoarding", ControlsController.ControlsActions.InteractObject, 0.1f);
	}

	public void ShowTakeOarPrompt()
	{
		this._boatHintPanel.ShowPanel("PutRodPrompt", ControlsController.ControlsActions.StartFishing, 0.1f);
	}

	public void ShowTakeAnchorPrompt()
	{
		string text = "WeightAnchorPrompt";
		this._boatHintPanel.ShowPanel(text, ControlsController.ControlsActions.UseAnchor, 0.1f);
	}

	public void ShowBoatExitZone()
	{
		this._boatHintPanel.ShowPanel("BoatExitZone", null, 0.1f);
	}

	public void ShowZodiacEnterIgnitionState()
	{
		this._boatHintPanel.ShowPanel("EngineIgnition", ControlsController.ControlsActions.StartStopBoatEngine, 3f);
	}

	public void ShowZodiacStopEngine()
	{
		this._boatHintPanel.ShowPanel("StopEngine", ControlsController.ControlsActions.StartStopBoatEngine, 2f);
	}

	public void ShowTrollingSpeedTooHigh()
	{
		this._boatHintPanel.ShowPanel(ScriptLocalization.Get("TrollingSpeedTooHigh"), 3f);
	}

	public void ShowCantCastAnchorWhenEngine()
	{
		this._boatHintPanel.ShowPanel("CantCastAnchorWhenEngine", null, 0.1f);
	}

	public void ShowCantStartEngineWhenAnchor()
	{
		this._boatHintPanel.ShowPanel("CantStartEngineWhenAnchor", null, 0.1f);
	}

	public void ShowZodiacIgnitionIdleState()
	{
		bool flag;
		string icoByActionName = HotkeyIcons.GetIcoByActionName("IgnitionForward", out flag);
		string text = this.AddHintIco(icoByActionName);
		this._boatHintPanel.ShowPanel(string.Format(ScriptLocalization.Get("BoatIgnitionIdleState"), text, text), 4f);
	}

	public void GoFishingModeHint()
	{
		this.ShowHintText("StartFishing", "SwitchToFishingMode", 0.5f);
	}

	public void GoNavigationModeHint()
	{
		this.ShowHintText("StartFishing", "SwitchToNavigationMode", 0.5f);
	}

	public void PlaceTackleOnPodHint()
	{
		this.ShowHintText("RodStandSubmit", "PlaceTackleOnRodHolder", 0.5f);
	}

	public void TakeTackleFromPodHint()
	{
		this.ShowHintText("RodStandSubmit", "TakeTackleFromRodHolder", 0.5f);
	}

	private void ShowHintText(string actionName, string locKey, float time = 4f)
	{
		bool flag;
		string icoByActionName = HotkeyIcons.GetIcoByActionName(actionName, out flag);
		string text = this.AddHintIco(icoByActionName);
		this._boatHintPanel.ShowPanel(string.Format(ScriptLocalization.Get(locKey), text), time);
	}

	private string AddHintIco(string ico)
	{
		return "<size=24><color=#FFEE44FF>" + ico + "</color></size>";
	}

	public void ShowZodiacConfirmIgnition()
	{
		this._boatHintPanel.ShowPanel("ConfirmEngineIgnitionPrompt", null, 1f);
	}

	public void ShowZodiacIgnitionSuccess()
	{
		this._boatHintPanel.ShowPanel("EngineIgnitionSuccess", null, 1f);
	}

	public void ShowZodiacIgnitionFailed()
	{
		this._boatHintPanel.ShowPanel("EngineIgnitionFailed", null, 1f);
	}

	public void ShowBoatIdlePrompt(bool isAnchored)
	{
		string text = ((!isAnchored) ? "CastAnchorPrompt" : "WeightAnchorPrompt");
		this._boatHintPanel.ShowPanel(text, "TakeRodPrompt", ControlsController.ControlsActions.UseAnchor, ControlsController.ControlsActions.StartFishing, 0.1f);
	}

	public void ShowPhotomodeHUD(bool cantSitdown)
	{
		this._currentHUD = GUITools.AddChild(null, this.Photomode.gameObject);
		this._currentHUD.transform.SetAsLastSibling();
		this._currentHUD.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this._currentHUD.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this._currentHUD.GetComponent<PhotomodeHUD>().Init(cantSitdown);
	}

	public void HidePhotomodeHUD()
	{
		if (this._currentHUD != null)
		{
			Object.Destroy(this._currentHUD);
		}
	}

	public void InteractiveObjectChange(string text)
	{
		this._interactiveObjectPanel.Change(text);
	}

	public void InteractiveObjectHidePanel(bool isFast = false)
	{
		this._interactiveObjectPanel.HidePanel(isFast);
		base.StopAllCoroutines();
	}

	public void InteractiveObjectShowPanel(string text)
	{
		this._interactiveObjectPanel.gameObject.SetActive(true);
		this._interactiveObjectPanel.ShowPanel(text);
		base.StopAllCoroutines();
	}

	public void InteractiveObjectShowPanelForTime(string text, float time = 0.5f)
	{
		this._interactiveObjectPanel.ShowPanel(text);
		base.StartCoroutine(this.ShowPanelForTime(time));
	}

	private IEnumerator ShowPanelForTime(float time)
	{
		yield return new WaitForSeconds(time);
		this._interactiveObjectPanel.HideFinished += this._interactiveObjectPanel_HideFinished;
		this.InteractiveObjectHidePanel(false);
		yield break;
	}

	private void _interactiveObjectPanel_HideFinished()
	{
		this._interactiveObjectPanel.HideFinished -= this._interactiveObjectPanel_HideFinished;
		this.OnInteractiveObjectHidePanelHidden();
	}

	public void HideHud()
	{
		this._mondatoryHide = true;
		this._alphaFade.HidePanel();
	}

	public void ShowHud()
	{
		this._mondatoryHide = false;
	}

	public void StartTimer(float t, Action endTimerFunc)
	{
		this._timerCoroutine = base.StartCoroutine(this.Timer(t, endTimerFunc));
	}

	public void StopTimer()
	{
		if (this._timerCoroutine != null)
		{
			base.StopCoroutine(this._timerCoroutine);
			this._timerCoroutine = null;
		}
	}

	private IEnumerator Timer(float t, Action endTimerFunc)
	{
		yield return new WaitForSeconds(t);
		this._timerCoroutine = null;
		endTimerFunc();
		yield break;
	}

	private void OnActivatePhotoMode(bool flag, CatchedInfoTypes type)
	{
		GameFactory.Player.SetLookWithFishMode(flag);
		if (flag)
		{
			this.HideHud();
			ControlsController.ControlsActions.UnBlockAxis();
			CursorManager.HideCursor();
		}
		else
		{
			this.ShowHud();
			ControlsController.ControlsActions.BlockAxis();
			CursorManager.ShowCursor();
		}
	}

	public void ShowChangeRoomPanel()
	{
		this._changeRoomPanelShown = true;
		this._changeRoomPanel.ShowPanel();
		this._chatCanvasGroup.alpha = 0f;
		if (GameFactory.ChatInGameController != null)
		{
			GameFactory.ChatInGameController.SetActive(false);
		}
	}

	public void HideChangeRoomPanel()
	{
		if (this._changeRoomPanel != null && this._changeRoomPanelShown && GameFactory.ChatInGameController != null)
		{
			this._changeRoomPanel.OnHide.AddListener(new UnityAction(this.ShowChatPanelAfterChangingRoom));
		}
		if (this._changeRoomPanel != null)
		{
			this._changeRoomPanel.HidePanel();
		}
		this._changeRoomPanelShown = false;
	}

	private void ShowChatPanelAfterChangingRoom()
	{
		this._changeRoomPanel.OnHide.RemoveListener(new UnityAction(this.ShowChatPanelAfterChangingRoom));
		ShortcutExtensions.DOFade(this._chatCanvasGroup, 1f, 0.1f);
		GameFactory.ChatInGameController.SetActive(true);
	}

	private bool IsShowHudWasPressed()
	{
		bool flag = ControlsController.ControlsActions.ShowHud.WasPressed;
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			flag = this.IsShowHudWasPressedController(flag);
		}
		return flag;
	}

	private bool IsShowHudWasPressedController(bool pressed)
	{
		if (pressed)
		{
			pressed = InputManager.ActiveDevice.GetControl(InputControlType.RightStickButton).IsPressed;
		}
		else if (InputManager.ActiveDevice.GetControl(InputControlType.RightStickButton).WasPressed)
		{
			pressed = ControlsController.ControlsActions.ShowHud.IsPressed;
		}
		return pressed;
	}

	private void UpdateTooltips()
	{
		if (PhotonConnectionFactory.Instance.Profile == null || PhotonConnectionFactory.Instance.Profile.Level >= 10)
		{
			return;
		}
		string text = ((this._fishingHandler.CurFightIndicator != FightIndicator.ThreeBands) ? "HudOneBandFightTooltip" : "HudThreeBandTooltip");
		bool flag = ((this._fishingHandler.CurFightIndicator != FightIndicator.ThreeBands) ? this.IsRodReelLineOverloadedOneBand() : this.IsRodReelLineOverloadedThreeBand());
		if (flag)
		{
			this.CheckTooltipActivation(true, text);
		}
		else if (this._ttm.IsActive(text))
		{
			if (this._curOverloadedTooltipTime < 5f)
			{
				this._curOverloadedTooltipTime += Time.deltaTime;
			}
			else
			{
				this._curOverloadedTooltipTime = 0f;
				this.CheckTooltipActivation(false, text);
			}
		}
		if (!this._ttm.IsActive(text))
		{
			FishCageContents fishCage = PhotonConnectionFactory.Instance.Profile.FishCage;
			if (fishCage != null && fishCage.Cage != null && fishCage.Cage.Durability > 0)
			{
				this.CheckTooltipActivation(fishCage.Weight != null && fishCage.Cage.TotalWeight <= fishCage.Weight.Value, "HUDFishKeepnetPanelTooltip");
			}
		}
	}

	private void CheckTooltipActivation(bool flag, string elementId)
	{
		if ((flag && !this._ttm.IsActive(elementId)) || (!flag && this._ttm.IsActive(elementId)))
		{
			if (flag && !this._ttm.IsAdded(elementId))
			{
				HudTooltip hudTooltip = HudTooltips.Tooltips.First((HudTooltip p) => p.ElementId == elementId);
				this._ttm.Add(hudTooltip.ElementId, hudTooltip.Scale, ScriptLocalization.Get(hudTooltip.Text), hudTooltip.Side);
			}
			this._ttm.SetActive(elementId, flag);
		}
	}

	private bool IsRodReelLineOverloadedThreeBand()
	{
		return GameFactory.Player.Rod != null && GameFactory.Player.Rod.AssembledRod != null && GameFactory.Player.Reel.IsIndicatorOn && (this.CheckOverloaded(GameFactory.Player.Rod.MaxLoad, GameFactory.Player.Rod.AppliedForce) || this.CheckOverloaded(GameFactory.Player.Reel.MaxLoad, GameFactory.Player.Reel.AppliedForce) || this.CheckOverloaded(GameFactory.Player.Line.MaxLoad, GameFactory.Player.Line.AppliedForce));
	}

	private bool IsRodReelLineOverloadedOneBand()
	{
		return GameFactory.Player.Rod != null && GameFactory.Player.Rod.AssembledRod != null && GameFactory.Player.Reel.IndicatorForce * 100f >= 80f;
	}

	private bool CheckOverloaded(float maxLoad, float appliedForce)
	{
		return appliedForce * 100f / maxLoad >= 80f;
	}

	[SerializeField]
	private NewChatInGameController Chat;

	[SerializeField]
	private ProlongationOfStay ProlongOfStay;

	[SerializeField]
	private RewindHoursHandler RewindHH;

	[SerializeField]
	private CatchedItemWindow _catchedItemWindow;

	[SerializeField]
	private CatchedFishInfoHandler _catchedFishWindow;

	[SerializeField]
	private AdditionalInfoHandler AddInfoHandler;

	[SerializeField]
	private EquipmentInGamePanel SetLurePanel;

	[SerializeField]
	private EquipmentInGamePanel SetRodPanel;

	[SerializeField]
	private BreakLineHandler BreakLine;

	[SerializeField]
	private DebugDialog DebugDialog;

	[SerializeField]
	private BoatHintPanel BoatHintPanel;

	[SerializeField]
	private InteractiveObjectPanelHandler InteractiveObjectPanel;

	[SerializeField]
	private FishingHandler FishingHandler;

	[SerializeField]
	private SailingHandler SailingHandler;

	[SerializeField]
	private HidesPanel Messages;

	[SerializeField]
	private HidesPanel ToolIndicator;

	[SerializeField]
	private AlphaFade ChangeRoomPanel;

	[SerializeField]
	private CrossHair CrossHair;

	[SerializeField]
	private RodStandAngleIndicator AngleIndicator;

	[SerializeField]
	private TmpCrossHair DebugCrossHair;

	private InteractiveObjectPanelHandler _interactiveObjectPanel;

	private FishingHandler _fishingHandler;

	private SailingHandler _sailingHandler;

	private BoatHintPanel _boatHintPanel;

	private CatchedFishInfoHandler _CatchedFishInfoHandler;

	private CatchedItemWindow _ciw;

	private AdditionalInfoHandler _addInfo;

	private EquipmentChangeHandler _equipmentChangeHandler;

	public PhotomodeHUD Photomode;

	private static PondHelpers _pondHelpers = new PondHelpers();

	private ShowHudStates _currentState;

	private AlphaFade _alphaFade;

	private GameObject _currentHUD;

	private bool _mondatoryHide;

	private HidesPanel[] _HidesPanels = new HidesPanel[4];

	private RewindHoursHandler rewindHoursScreen;

	private ProlongationOfStay _prolongOfStay;

	private BreakLineHandler _breakLine;

	private AlphaFade _changeRoomPanel;

	private CrossHair _crossHair;

	private RodStandAngleIndicator _angleIndicator;

	private TmpCrossHair _debugCrossHair;

	private EquipmentInGamePanel _lurePanel;

	private EquipmentInGamePanel _rodPanel;

	private float _curOverloadedTooltipTime;

	private const float OverloadedTooltipTime = 5f;

	private const float OverloadedTooltipPrc = 80f;

	private const int TooltipMaxLevel = 10;

	private TooltipManager _ttm = new TooltipManager(1);

	private Coroutine _timerCoroutine;

	private CanvasGroup _chatCanvasGroup;

	private bool _changeRoomPanelShown;
}
