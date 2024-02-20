using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.PlayerProfile;
using CodeStage.AntiCheat.ObscuredTypes;
using I2.Loc;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewChatInGameController : MonoBehaviour
{
	public bool IsEditingMode
	{
		get
		{
			return this._chatIsActive;
		}
	}

	public bool IsMenuState { get; private set; }

	private void Awake()
	{
		this.MessagesTextPanel.text = string.Empty;
		this.ShortMessagesTextPanel.text = string.Empty;
		this.FullChat.OnShow.AddListener(new UnityAction(this.SetInteractableFullChat));
		this.FullChat.HideFinished += this.SetNotInteractableFullChat;
		this.interactableCallbacksSubscribed = true;
	}

	private void SetNotInteractableFullChat(object sender, EventArgsAlphaFade e)
	{
		this.FullChat.CanvasGroup.interactable = false;
	}

	private void SetInteractableFullChat()
	{
		this.FullChat.CanvasGroup.interactable = true;
	}

	private void Start()
	{
		this.CorrectPosition();
		this._currentToggle = this.AllToggle;
		this.FullChat.SetInteractable = true;
		foreach (UINavigation uinavigation in this.navigations)
		{
			uinavigation.OverrideCanvasGroup(this.FullChat.CanvasGroup);
		}
		this.FullChat.FastHidePanel();
		this.ShortChat.FastHidePanel();
		this._menus[0] = this.SettingsMenu;
		this._menus[1] = this.PlayersPanel;
		this._menus[2] = this.Rooms;
		for (int j = 0; j < this._menus.Length; j++)
		{
			this._menus[j].OnActive += this.PopUp_OnActive;
			this._menus[j].FastHidePanel();
		}
		this.SettingsMenu.OnFonstSizeActive += this.SettingsMenu_OnFonstSizeActive;
		this.Rooms.OnRoomsPopulation += this.Rooms_OnRoomsPopulation;
		this.Rooms_OnRoomsPopulation(this.Rooms.RoomsCount);
		this.PlayersToggle.onValueChanged.AddListener(delegate(bool on)
		{
			if (on && this.SettingsMenu.IsActive)
			{
				this.SettingsMenu.Activate(false);
			}
			this.PlayersPanel.Activate(on);
		});
		this.RoomsToggle.onValueChanged.AddListener(delegate(bool on)
		{
			if (on && this.SettingsMenu.IsActive)
			{
				this.SettingsMenu.Activate(false);
			}
			this.Rooms.Activate(on);
		});
		this.IncreaseFontButton.onClick.AddListener(delegate
		{
			if (this.MessagesTextPanel.fontSize >= 16 && this.MessagesTextPanel.fontSize < 25)
			{
				this.MessagesTextPanel.fontSize++;
				this.ShortMessagesTextPanel.fontSize++;
				PlayerPrefs.SetInt("ChatFontSize", this.MessagesTextPanel.fontSize);
			}
		});
		this.DecreaseFontButton.onClick.AddListener(delegate
		{
			if (this.MessagesTextPanel.fontSize > 16 && this.MessagesTextPanel.fontSize <= 25)
			{
				this.MessagesTextPanel.fontSize--;
				this.ShortMessagesTextPanel.fontSize--;
				PlayerPrefs.SetInt("ChatFontSize", this.MessagesTextPanel.fontSize);
			}
		});
		this.ShowTimestamp.onClick.AddListener(delegate
		{
			this._showTimestamp = !this._showTimestamp;
			this.UpdateTimestampText();
			ObscuredPrefs.SetBool("ChatShowTimestamp", this._showTimestamp);
		});
		this.UpdateTimestampText();
		this.MessagesScrollBar.GetComponent<ScrollEvents>().OnDragEvent += this.NewChatInGameController_OnDragEvent;
		this.MinimizeButton.onClick.AddListener(delegate
		{
			this.ShowShortChat();
		});
		this.MaximizeButton.onClick.AddListener(delegate
		{
			this.ShowFullChat();
		});
		this.MessagesScrollBar.value = 0f;
		this.HidesChatPanel = base.GetComponent<HidesPanel>();
		this.HidesChatPanel.IsShowing = true;
		if (PlayerPrefs.HasKey("ChatFontSize"))
		{
			this.MessagesTextPanel.fontSize = PlayerPrefs.GetInt("ChatFontSize");
			this.ShortMessagesTextPanel.fontSize = PlayerPrefs.GetInt("ChatFontSize");
		}
		if (ObscuredPrefs.HasKey("ChatShowTimestamp"))
		{
			this._showTimestamp = ObscuredPrefs.GetBool("ChatShowTimestamp");
		}
		if (ObscuredPrefs.HasKey("ShortChatShow"))
		{
			this._showShortChat = ObscuredPrefs.GetBool("ShortChatShow");
		}
		else
		{
			this._showShortChat = true;
		}
		this.ChatOpenHelp.text = string.Format(ScriptLocalization.Get("OpenChatCaption"), HotkeyIcons.KeyMappings[InputControlType.TouchPadButton]);
		ToggleChatController.Instance.OnActivate += this.Toggle_OnActivate;
		this.OnInputTypeChanged(SettingsManager.InputType);
		if (ObscuredPrefs.HasKey("ShowUserCompetitionPromotion"))
		{
			this._showUserCompetitionPromotion = ObscuredPrefs.GetBool("ShowUserCompetitionPromotion");
		}
		if (ObscuredPrefs.HasKey("ShowJoinEvents"))
		{
			this._showJoinEvents = ObscuredPrefs.GetBool("ShowJoinEvents");
		}
		if (ObscuredPrefs.HasKey("ShowLevelEvents"))
		{
			this._showLevelEvents = ObscuredPrefs.GetBool("ShowLevelEvents");
		}
		if (ObscuredPrefs.HasKey("ShowAchivementEvents"))
		{
			this._showAchivementEvents = ObscuredPrefs.GetBool("ShowAchivementEvents");
		}
		if (ObscuredPrefs.HasKey("ShowCaughtEvents"))
		{
			this._showCaughtEvents = ObscuredPrefs.GetBool("ShowCaughtEvents");
		}
		if (ObscuredPrefs.HasKey("ShowUnderWaterItemEvents"))
		{
			this._showUnderwaterItemEvents = ObscuredPrefs.GetBool("ShowUnderWaterItemEvents");
		}
		if (ObscuredPrefs.HasKey("ShowBrokenEvents"))
		{
			this._showBrokenEvents = ObscuredPrefs.GetBool("ShowBrokenEvents");
		}
		this.UserCompetitionPromotionToggle.isOn = this._showUserCompetitionPromotion;
		this.JoinToggle.isOn = this._showJoinEvents;
		this.LevelToggle.isOn = this._showLevelEvents;
		this.AchivementToggle.isOn = this._showAchivementEvents;
		this.CaughtToggle.isOn = this._showCaughtEvents;
		this.UnderWaterItemToggle.isOn = this._showUnderwaterItemEvents;
		this.BrokenToggle.isOn = this._showBrokenEvents;
		this.UserCompetitionPromotionToggle.onValueChanged.AddListener(delegate(bool on)
		{
			this._showUserCompetitionPromotion = on;
			ObscuredPrefs.SetBool("ShowUserCompetitionPromotion", this._showUserCompetitionPromotion);
		});
		this.JoinToggle.onValueChanged.AddListener(delegate(bool on)
		{
			this._showJoinEvents = on;
			ObscuredPrefs.SetBool("ShowJoinEvents", this._showJoinEvents);
		});
		this.LevelToggle.onValueChanged.AddListener(delegate(bool on)
		{
			this._showLevelEvents = on;
			ObscuredPrefs.SetBool("ShowLevelEvents", this._showLevelEvents);
		});
		this.AchivementToggle.onValueChanged.AddListener(delegate(bool on)
		{
			this._showAchivementEvents = on;
			ObscuredPrefs.SetBool("ShowAchivementEvents", this._showAchivementEvents);
		});
		this.CaughtToggle.onValueChanged.AddListener(delegate(bool on)
		{
			this._showCaughtEvents = on;
			ObscuredPrefs.SetBool("ShowCaughtEvents", this._showCaughtEvents);
		});
		this.UnderWaterItemToggle.onValueChanged.AddListener(delegate(bool on)
		{
			this._showUnderwaterItemEvents = on;
			ObscuredPrefs.SetBool("ShowUnderWaterItemEvents", this._showUnderwaterItemEvents);
		});
		this.BrokenToggle.onValueChanged.AddListener(delegate(bool on)
		{
			this._showBrokenEvents = on;
			ObscuredPrefs.SetBool("ShowBrokenEvents", this._showBrokenEvents);
		});
		PhotonConnectionFactory.Instance.OnGotChatPlayersCount += this.OnGotChatPlayersCount;
		PhotonConnectionFactory.Instance.OnGotServerTime += this.PhotonServerOnGotServerTime;
		this._inEndText = true;
	}

	public void UpdateVoice(string uName, bool isMuted, bool isTalking)
	{
		StaticUserData.ChatController.UpdateVoice(uName, isMuted, isTalking);
		this.PlayersPanel.UpdateVoice(uName, isMuted, isTalking);
	}

	public void ShowCatchedFish(bool isNewFish)
	{
		this._isCatchedFish = isNewFish;
	}

	private void Toggle_OnActivate(bool flag)
	{
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			this.isToggleMode = flag;
			if (flag)
			{
				this.PopUp_OnActive(flag);
			}
			else
			{
				this.PopUp_OnActive(this._menus.FirstOrDefault((MenuBase p) => p.IsActive));
			}
		}
	}

	public void OnInputDeviceActivated(InputModuleManager.InputType type)
	{
		CursorManager.Instance.MouseCursor = type == InputModuleManager.InputType.Mouse;
		SettingsManager.InputType = type;
		if (type == InputModuleManager.InputType.Mouse && !this._chatIsActive && !this._isCatchedFish)
		{
			ControlsController.ControlsActions.UnBlockInput();
		}
	}

	private void SettingsMenu_OnFonstSizeActive(bool flag)
	{
		if (this._menus.Any((MenuBase p) => p.IsActive))
		{
			if (flag)
			{
				this.hints[4].text = HotkeyIcons.KeyMappings[InputControlType.DPadLeft] + " " + ScriptLocalization.Get("DpadPlusMinus");
			}
			else
			{
				this.PopUp_OnActive(this.IsMenuState);
			}
		}
	}

	private void PopUp_OnActive(bool flag)
	{
		if (this._showShortChat)
		{
			return;
		}
		MenuBase menuBase = this._menus.FirstOrDefault((MenuBase p) => p.IsActive);
		bool flag2 = menuBase != null;
		if (flag2)
		{
			this.isToggleMode = false;
		}
		this.hints[2].gameObject.SetActive(flag2);
		this.hints[4].gameObject.SetActive(flag2);
		this.hints[5].gameObject.SetActive(flag2 || this.isToggleMode);
		bool flag3 = ToggleChatController.Instance.IsTournamentToggle(this._currentToggle);
		if (flag3)
		{
			this.hints[5].gameObject.SetActive(false);
		}
		this.hints[4].text = HotkeyIcons.KeyMappings[InputControlType.Action1] + " " + ScriptLocalization.Get((!(menuBase is RoomsChatInit)) ? "Action1Chat" : "Action1RoomChat");
		if (flag && this._chatIsActive && SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			this._chatIsActive = false;
			this.InputPanel.transform.parent.GetComponent<Image>().color = this.InActiveColor;
			this.InputPanel.text = string.Empty;
		}
		if (!flag)
		{
			flag = flag2;
		}
		this.IsMenuState = flag;
	}

	private void Rooms_OnRoomsPopulation(int count)
	{
		LogHelper.Log("___kocha Chat:Rooms_OnRoomsPopulation Rooms:{0} IS_IN_TOURNAMENT:{1}", new object[]
		{
			count,
			TournamentHelper.IS_IN_TOURNAMENT
		});
		this.RoomsToggle.gameObject.SetActive(count > 0 && !TournamentHelper.IS_IN_TOURNAMENT);
	}

	private void NewChatInGameController_OnDragEvent(object sender, EventArgs e)
	{
		Debug.Log("DragOn");
		this._inEndText = Math.Abs(this.MessagesScrollBar.value) < 0.001f;
	}

	private void OnEnable()
	{
		LogHelper.Log("___kocha Chat:OnEnable");
		this.Rooms.UpdateRoomsPopulation();
		PhotonConnectionFactory.Instance.GetChatPlayersCount();
		base.StartCoroutine(this.CallChatUpdate());
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	private void OnDisable()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		base.StopAllCoroutines();
	}

	private void OnDestroy()
	{
		if (this.interactableCallbacksSubscribed)
		{
			this.FullChat.OnShow.RemoveListener(new UnityAction(this.SetInteractableFullChat));
			this.FullChat.HideFinished -= this.SetNotInteractableFullChat;
		}
		this.Rooms.OnRoomsPopulation -= this.Rooms_OnRoomsPopulation;
		this.SettingsMenu.OnFonstSizeActive -= this.SettingsMenu_OnFonstSizeActive;
		for (int i = 0; i < this._menus.Length; i++)
		{
			if (this._menus[i] != null)
			{
				this._menus[i].OnActive -= this.PopUp_OnActive;
			}
		}
		ToggleChatController.Instance.OnActivate -= this.Toggle_OnActivate;
		PhotonConnectionFactory.Instance.OnGotChatPlayersCount -= this.OnGotChatPlayersCount;
		PhotonConnectionFactory.Instance.OnGotServerTime -= this.PhotonServerOnGotServerTime;
	}

	private void Update()
	{
		bool flag = CursorManager.IsModalWindow() || CursorManager.IsRewindHoursActive;
		this.hints[0].gameObject.SetActive(!this._showShortChat && this.MessagesScrollBar != null && this.MessagesScrollBar.gameObject.activeSelf);
		if (this._showShortChat && SettingsManager.InputType == InputModuleManager.InputType.GamePad && ((this.ShortInputPanel.isFocused && this.ChatOpenHelp.gameObject.activeSelf) || (!this.ShortInputPanel.isFocused && !this.ChatOpenHelp.gameObject.activeSelf)))
		{
			this.ChatOpenHelp.gameObject.SetActive(!this.ShortInputPanel.isFocused);
		}
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			this.CheckOnAction2WasPressed();
		}
		if (!this._isEnabled && this.ChatOpenHelp.gameObject.activeSelf)
		{
			this.ChatOpenHelp.gameObject.SetActive(false);
		}
		if (PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.PondId != null && PhotonConnectionFactory.Instance.Profile.PondId != 2)
		{
			if (ToggleChatController.Instance != null)
			{
				this._currentToggle = ToggleChatController.Instance.ActiveToggle;
			}
			if (!this.LockChat && !flag && ControlsController.ControlsActions.ChatGamePad.WasPressedMandatory && !ShowHudElements.Instance.IsEquipmentChangeBusy())
			{
				if (this._showShortChat)
				{
					this.ShowFullChat();
					if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
					{
						this.ActivateFullChat();
					}
				}
				else
				{
					this.HideChat();
				}
			}
			if (!this.LockChat && ControlsController.ControlsActions.IsBlockedKeyboardInput && ControlsController.ControlsActions.UISubmit.WasPressedMandatory)
			{
				this.SubmitMessage();
			}
			if (!ControlsController.ControlsActions.ChatGamePad.WasPressedMandatory)
			{
				this.UpdateChatInput();
			}
		}
	}

	private void HideChat()
	{
		this.ShowShortChat();
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			ControlsController.ControlsActions.UnBlockInput();
		}
	}

	private IEnumerator CallChatUpdate()
	{
		for (;;)
		{
			this._chatMessagesUpdated = false;
			yield return new WaitForSeconds(0.5f);
			this.UpdateChat();
		}
		yield break;
	}

	private void CheckOnAction2WasPressed()
	{
		if (InputManager.ActiveDevice.GetControl(InputControlType.Action2).WasClicked)
		{
			MenuBase menuBase = this._menus.FirstOrDefault((MenuBase p) => p.IsActive);
			if (menuBase != null)
			{
				this.IsMenuState = false;
				menuBase.Activate(false);
				if (!this._showShortChat)
				{
					this.ActivateFullChat();
				}
			}
			else if (this.isToggleMode)
			{
				ToggleChatController.Instance.RemovePrivateChat();
				this.IsMenuState = false;
				this.isToggleMode = false;
				this.hints[5].gameObject.SetActive(this.IsMenuState);
				if (!this._showShortChat)
				{
					this.ActivateFullChat();
				}
			}
			else if (!this._showShortChat && !this._chatIsActive)
			{
				this.ActivateFullChat();
			}
			else if (!this._showShortChat && !this.LockChat && !InfoMessageController.Instance.IsActive && !ShowHudElements.Instance.IsEquipmentChangeBusy())
			{
				this.HideChat();
			}
		}
	}

	private void UpdateChat()
	{
		if (this._chatMessagesUpdated)
		{
			return;
		}
		this.TabAllCaption.text = string.Format("{0} ({1})", ScriptLocalization.Get("ChatAllCaption"), StaticUserData.ChatController.PlayerOnLocaionCount);
		this._chatMessagesUpdated = true;
		if (!this._showShortChat)
		{
			this.FullChatUpdateHandler();
		}
		else
		{
			this.ShortChatUpdateHandler();
		}
	}

	private void UpdateChatInput()
	{
		if (!this._showShortChat)
		{
			this.FullChatUpdateUserInput();
		}
		else
		{
			this.ShortChatUpdateUserInput();
		}
	}

	private void FullChatUpdateHandler()
	{
		string text;
		if (this.GetMessagesType() != MessageChatType.Private)
		{
			text = ChatHelper.GetText(this.GetActualMessages(StaticUserData.ChatController.GetMessages(this.GetMessagesType())), this._showTimestamp);
		}
		else
		{
			Player player = this._currentToggle.GetComponent<TogleChatHandler>().Player;
			text = ChatHelper.GetText(this.GetActualMessages(StaticUserData.ChatController.GetPrivateMessages(player)), this._showTimestamp);
			StaticUserData.ChatController.UnreadChatTabs.Remove(StaticUserData.ChatController.UnreadChatTabs.FirstOrDefault((Player x) => x.UserId == player.UserId));
		}
		if (this.MessagesTextPanel.text.GetHashCode() != text.GetHashCode())
		{
			this.MessagesTextPanel.text = text;
		}
	}

	private bool IsCatchedFish
	{
		get
		{
			return GameFactory.Player != null && GameFactory.Player.IsCatchedSomething;
		}
	}

	private void FullChatUpdateUserInput()
	{
		if (this._inEndText && Math.Abs(this.MessagesScrollBar.value) > 0.01f)
		{
			this.MessagesScrollBar.value = 0f;
		}
		bool flag = CursorManager.IsModalWindow() || CursorManager.IsRewindHoursActive;
		if (!this.LockChat && !flag && ControlsController.ControlsActions.Chat.WasPressedMandatory && this.HidesChatPanel.IsShowing && !this.IsCatchedFish && !ShowHudElements.Instance.IsEquipmentChangeBusy())
		{
			if (ControlsController.ControlsActions.IsBlockedKeyboardInput)
			{
				this.ExitFromChatMode();
				if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
				{
					this.OnInputTypeChanged(SettingsManager.InputType);
				}
				else if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
				{
					ControlsController.ControlsActions.UnBlockInput();
				}
			}
			else
			{
				this.ActivateFullChat();
			}
		}
		if (this._chatIsActive && !flag && ControlsController.ControlsActions.UICancel.WasReleasedMandatory && ControlsController.ControlsActions.IsBlockedKeyboardInput)
		{
			this.ExitFromChatMode();
			ControlsController.ControlsActions.UnBlockInput();
			if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
			{
				this.OnInputTypeChanged(SettingsManager.InputType);
			}
		}
		if (this._chatIsActive)
		{
			if (!ControlsController.ControlsActions.IsBlockedAxis)
			{
				ControlsController.ControlsActions.BlockInput(null);
			}
			if (!object.ReferenceEquals(this.InputPanel.gameObject, EventSystem.current.currentSelectedGameObject) && !object.ReferenceEquals(this.MinimizeButton.gameObject, EventSystem.current.currentSelectedGameObject) && this.FullChat.CanvasGroup.interactable && !this.SettingsMenu.IsActive && !this._showShortChat)
			{
				this.ExitFromChatMode();
				UINavigation.SetSelectedGameObject(this.InputPanel.gameObject);
			}
		}
		if (!this.LockChat && ControlsController.ControlsActions.ChatScrollDown.WasPressedMandatory && this.MessagesScrollBar != null && this.MessagesScrollBar.gameObject.activeSelf)
		{
			this.MessagesScrollBar.value -= this.MessagesScrollBar.size;
			this._inEndText = Math.Abs(this.MessagesScrollBar.value) < 0.001f;
		}
		if (!this.LockChat && ControlsController.ControlsActions.ChatScrollUp.WasPressedMandatory && this.MessagesScrollBar != null && this.MessagesScrollBar.gameObject.activeSelf)
		{
			this.MessagesScrollBar.value += this.MessagesScrollBar.size;
			this._inEndText = Math.Abs(this.MessagesScrollBar.value) < 0.001f;
		}
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse && !ControlsController.ControlsActions.ShowCursor.IsPressedMandatory)
		{
			for (int i = 0; i < this._menus.Length; i++)
			{
				if (this._menus[i].IsActive)
				{
					this._menus[i].Activate(false);
				}
			}
		}
	}

	public void BeginEnteringmessage()
	{
		if (this._chatIsActive)
		{
			return;
		}
		this._chatIsActive = true;
		this.InputPanel.GetComponent<ScreenKeyboard>().OnInputFieldSelect(string.Empty, true, ScreenKeyboard.VirtualKeyboardScope.Default);
		this.InputPanel.ActivateInputField();
		if (EventSystem.current.currentSelectedGameObject != this.InputPanel.gameObject && InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse)
		{
			UINavigation.SetSelectedGameObject(this.InputPanel.gameObject);
		}
		this.InputPanel.transform.parent.GetComponent<Image>().color = this.ActiveColor;
	}

	public void ActivateFullChat()
	{
		MonoBehaviour.print("ActivateFullChat");
		if (!ControlsController.ControlsActions.IsBlockedKeyboardInput)
		{
			ControlsController.ControlsActions.BlockInput(null);
		}
		this.BeginEnteringmessage();
	}

	private void ShortChatUpdateHandler()
	{
		string text = string.Empty;
		if (this.GetMessagesType() != MessageChatType.Private)
		{
			text = ChatHelper.GetShortText(this.GetActualMessages(StaticUserData.ChatController.GetMessages(this.GetMessagesType())), this._showTimestamp);
		}
		else
		{
			text = ChatHelper.GetShortText(this.GetActualMessages(StaticUserData.ChatController.GetPrivateMessages(this._currentToggle.GetComponent<TogleChatHandler>().Player)), this._showTimestamp);
		}
		if (this.ShortMessagesTextPanel.text.GetHashCode() != text.GetHashCode())
		{
			this.ShortMessagesTextPanel.text = text;
		}
	}

	private void ShortChatUpdateUserInput()
	{
		bool flag = CursorManager.IsModalWindow() || CursorManager.IsRewindHoursActive;
		if (!this.LockChat && !flag && ControlsController.ControlsActions.Chat.WasPressedMandatory && this.HidesChatPanel.IsShowing && !this.IsCatchedFish && !ShowHudElements.Instance.IsEquipmentChangeBusy())
		{
			if (ControlsController.ControlsActions.IsBlockedKeyboardInput)
			{
				this.ExitFromChatMode();
				if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
				{
					ControlsController.ControlsActions.UnBlockInput();
				}
			}
			else
			{
				this.ActivateShortChat();
			}
		}
		if (this._chatIsActive && EventSystem.current.currentSelectedGameObject != this.ShortInputPanel.gameObject)
		{
			UINavigation.SetSelectedGameObject(this.ShortInputPanel.gameObject);
		}
		if (this._chatIsActive && !flag && ControlsController.ControlsActions.UICancel.WasReleasedMandatory && ControlsController.ControlsActions.IsBlockedKeyboardInput)
		{
			this.ExitFromChatMode();
			ControlsController.ControlsActions.UnBlockInput();
		}
		if (this._chatIsActive && !ControlsController.ControlsActions.IsBlockedAxis)
		{
			ControlsController.ControlsActions.BlockInput(null);
		}
	}

	public void ActivateShortChat()
	{
		this._chatIsActive = true;
		this.ShortInputPanel.GetComponent<ScreenKeyboard>().OnInputFieldSelect(string.Empty, false, ScreenKeyboard.VirtualKeyboardScope.Default);
		if (!ControlsController.ControlsActions.IsBlockedKeyboardInput)
		{
			ControlsController.ControlsActions.BlockInput(null);
		}
		this.ShortInputPanel.ActivateInputField();
		if (EventSystem.current.currentSelectedGameObject != this.ShortInputPanel.gameObject)
		{
			UINavigation.SetSelectedGameObject(this.ShortInputPanel.gameObject);
		}
	}

	public void SettingsButtonClick()
	{
		this.UpdateTimestampText();
		this.SettingsMenu.Activate(!this.SettingsMenu.IsActive);
		if (this.SettingsMenu.IsActive)
		{
			this.PlayersPanel.Activate(false);
			this.Rooms.Activate(false);
		}
	}

	public void ExitFromChatMode()
	{
		MonoBehaviour.print("ExitFromChatMode");
		this._chatIsActive = false;
		UINavigation.SetSelectedGameObject(base.gameObject);
		this.InputPanel.transform.parent.GetComponent<Image>().color = this.InActiveColor;
		if (this.SettingsMenu.IsActive)
		{
			this.SettingsMenu.Activate(false);
		}
		this.ShortInputPanel.text = string.Empty;
		this.InputPanel.text = string.Empty;
	}

	public void SubmitMessage()
	{
		if (PhotonConnectionFactory.Instance.Profile.ChatBanEndDate != null && PhotonConnectionFactory.Instance.Profile.ChatBanEndDate.Value > TimeHelper.UtcTime())
		{
			this._selfMessagesQueue.Enqueue(string.Format(ScriptLocalization.Get("BannedInChat"), PhotonConnectionFactory.Instance.Profile.ChatBanEndDate.Value.ToLocalTime().ToShortDateString(), PhotonConnectionFactory.Instance.Profile.ChatBanEndDate.Value.ToLocalTime().ToShortTimeString()));
			PhotonConnectionFactory.Instance.GetServerTime(0);
		}
		else
		{
			string text = ((!this._showShortChat) ? this.InputPanel.text : this.ShortInputPanel.text);
			if (string.IsNullOrEmpty(text))
			{
				if (this._showShortChat)
				{
					this.ShortInputPanel.ActivateInputField();
				}
				else
				{
					this.InputPanel.ActivateInputField();
				}
				if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
				{
					this.OnInputTypeChanged(SettingsManager.InputType);
				}
				return;
			}
			if (text.StartsWith("/"))
			{
				this.HandleSpecialCommand(text);
			}
			else
			{
				this._selfMessagesQueue.Enqueue(text);
				PhotonConnectionFactory.Instance.GetServerTime(0);
			}
		}
		if (this.MessagesScrollBar != null && this._inEndText)
		{
			this.MessagesScrollBar.value = 0f;
		}
		InputField inputPanel = this.InputPanel;
		string empty = string.Empty;
		this.ShortInputPanel.text = empty;
		inputPanel.text = empty;
		if (this._showShortChat)
		{
			this.ShortInputPanel.ActivateInputField();
		}
		else
		{
			this.InputPanel.ActivateInputField();
		}
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			this.OnInputTypeChanged(SettingsManager.InputType);
		}
	}

	public void SubmitMessageOnConsole()
	{
		string text = ((!this._showShortChat) ? this.InputPanel.text : this.ShortInputPanel.text);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (text.StartsWith("/"))
		{
			this.HandleSpecialCommand(text);
		}
		else
		{
			this._selfMessagesQueue.Enqueue(text);
			PhotonConnectionFactory.Instance.GetServerTime(0);
		}
		if (this.MessagesScrollBar != null && this._inEndText)
		{
			this.MessagesScrollBar.value = 0f;
		}
	}

	private void HandleSpecialCommand(string text)
	{
		string text2 = text;
		string[] array = text.Split(new char[] { ' ' });
		string text3 = array[0].Substring(1);
		string text4 = null;
		if (array.Length >= 2)
		{
			text4 = string.Join(" ", array.SubArray(1, array.Length - 1));
		}
		string text5 = text3.ToLower();
		if (text5 != null)
		{
			if (text5 == "ignore")
			{
				if (!string.IsNullOrEmpty(text4))
				{
					PhotonConnectionFactory.Instance.SendChatCommand(2, text4);
				}
				else
				{
					text2 = ScriptLocalization.Get("ChatCommandIncorrect");
				}
				goto IL_159;
			}
			if (text5 == "unignore")
			{
				if (!string.IsNullOrEmpty(text4))
				{
					PhotonConnectionFactory.Instance.SendChatCommand(3, text4);
				}
				else
				{
					text2 = ScriptLocalization.Get("ChatCommandIncorrect");
				}
				goto IL_159;
			}
			if (text5 == "report")
			{
				if (!string.IsNullOrEmpty(text4))
				{
					PhotonConnectionFactory.Instance.SendChatCommand(1, text4);
				}
				else
				{
					text2 = ScriptLocalization.Get("ChatCommandIncorrect");
				}
				goto IL_159;
			}
			if (text5 == "incognito")
			{
				PhotonConnectionFactory.Instance.SendChatCommand(4, null);
				goto IL_159;
			}
			if (text5 == "help")
			{
				text2 = "/ignore Username \n /unignore Username \n /report Username \n /incognito ";
				goto IL_159;
			}
		}
		text2 = ScriptLocalization.Get("ChatCommandIncorrect");
		IL_159:
		StaticUserData.ChatController.AddMessage(string.Format("<color=#00ffff##>{0}</color>", text2), MessageChatType.Global, true);
		this.ClearAndActivateInputField();
		if (!this._showShortChat)
		{
			UINavigation.SetSelectedGameObject(this.InputPanel.gameObject);
		}
		else
		{
			UINavigation.SetSelectedGameObject(this.ShortInputPanel.gameObject);
		}
	}

	private void ClearAndActivateInputField()
	{
		if (!this._showShortChat)
		{
			this.InputPanel.text = string.Empty;
			this.InputPanel.ActivateInputField();
		}
		else
		{
			this.ShortInputPanel.text = string.Empty;
			this.ShortInputPanel.ActivateInputField();
		}
	}

	public void OpenChat()
	{
		this.ShowFullChat();
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			this.ActivateFullChat();
		}
	}

	private void PhotonServerOnGotServerTime(int callerId, DateTime time)
	{
		if (callerId == 0 && this._selfMessagesQueue.Count > 0)
		{
			this.HandleNormalMessage(this._selfMessagesQueue.Dequeue(), time);
		}
	}

	private void HandleNormalMessage(string text, DateTime serverTimestamp)
	{
		ChatMessage chatMessage = new ChatMessage
		{
			Sender = PlayerProfileHelper.ProfileToPlayer(PhotonConnectionFactory.Instance.Profile),
			Message = text
		};
		MessageChatType messageChatType = this.GetMessagesType();
		chatMessage.Channel = "Local";
		if (messageChatType != MessageChatType.Private)
		{
			if (messageChatType == MessageChatType.All)
			{
				messageChatType = MessageChatType.Location;
				chatMessage.Channel = "Local";
			}
		}
		else
		{
			messageChatType = MessageChatType.Private;
			chatMessage.Recepient = this._currentToggle.GetComponent<TogleChatHandler>().Player;
			chatMessage.Channel = "Private";
		}
		chatMessage.Timestamp = new DateTime?(serverTimestamp);
		StaticUserData.ChatController.SendMessage(chatMessage, messageChatType);
		this.UpdateChat();
	}

	public void SetHudState(ShowHudStates state)
	{
		this._currentState = state;
		if (this._currentState == ShowHudStates.ShowWithoutChat)
		{
			this.SetActive(false);
		}
		else if (this._currentState == ShowHudStates.ShowAll)
		{
			this.SetActive(true);
		}
	}

	public void SetActive(bool flag)
	{
		if (this.HidesChatPanel == null || !this._isEnabled || this.LockChat == !flag || (flag && this._currentState != ShowHudStates.ShowAll))
		{
			return;
		}
		this.LockChat = !flag;
		if (flag)
		{
			this.HidesChatPanel.Show();
			if (SettingsManager.InputType == InputModuleManager.InputType.GamePad && !this._showShortChat && !ControlsController.ControlsActions.IsBlockedAxis)
			{
				ControlsController.ControlsActions.BlockInput(null);
			}
		}
		else
		{
			this.HidesChatPanel.Hide();
		}
	}

	public void SetEnableFullChat(bool flag)
	{
		this.FullChat.gameObject.SetActive(flag);
	}

	public void TurnOff()
	{
		this._isEnabled = false;
		this.ShortChat.gameObject.SetActive(this._isEnabled);
		this.ShortInputPanel.interactable = false;
		this.SetEnableFullChat(this._isEnabled);
		this.SetActive(this._isEnabled);
		this.LockChat = !this._isEnabled;
	}

	private void ShowShortChat()
	{
		this._showShortChat = true;
		this.FullChat.HidePanel();
		foreach (UINavigation uinavigation in this.navigations)
		{
			uinavigation.SetPaused(true);
		}
		this.ShortChat.gameObject.SetActive(true);
		this.ShortChat.ShowPanel();
		ObscuredPrefs.SetBool("ShortChatShow", this._showShortChat);
		if (this._chatIsActive)
		{
			this.ExitFromChatMode();
			ControlsController.ControlsActions.UnBlockInput();
			if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
			{
				this.OnInputTypeChanged(SettingsManager.InputType);
			}
		}
		else
		{
			this.ExitFromChatMode();
		}
	}

	private void ActivateFullChatInputFieldOnShow()
	{
		this.FullChat.OnShow.RemoveListener(new UnityAction(this.ActivateFullChatInputFieldOnShow));
		base.StartCoroutine(this.WaitAndActivate());
	}

	private IEnumerator WaitAndActivate()
	{
		yield return new WaitForEndOfFrame();
		this.ActivateFullChat();
		yield break;
	}

	private void ShowFullChat()
	{
		if (!this._isEnabled)
		{
			return;
		}
		foreach (UINavigation uinavigation in this.navigations)
		{
			uinavigation.SetPaused(false);
		}
		this._showShortChat = false;
		this.ShortChat.HidePanel();
		ObscuredPrefs.SetBool("ShortChatShow", this._showShortChat);
		if (this._chatIsActive)
		{
			this.FullChat.OnShow.AddListener(new UnityAction(this.ActivateFullChatInputFieldOnShow));
		}
		this.FullChat.gameObject.SetActive(true);
		this.FullChat.ShowPanel();
		ToggleChatController.Instance.SetOnActiveToggle(true);
		for (int j = 0; j < this.hints.Length; j++)
		{
			this.hints[j].gameObject.SetActive(false);
		}
		this.hints[0].gameObject.SetActive(true);
		this.hints[1].gameObject.SetActive(true);
		this.hints[6].gameObject.SetActive(true);
		this.hints[6].text = HotkeyIcons.KeyMappings[InputControlType.TouchPadButton] + " " + ScriptLocalization.Get("CloseChatCaption");
		this.hints[0].text = HotkeyIcons.KeyMappings[InputControlType.LeftStickLeft] + " " + ScriptLocalization.Get("LeftStickChat");
		this.hints[1].text = HotkeyIcons.KeyMappings[InputControlType.DPadUp] + " " + ScriptLocalization.Get("DpadUpDownChat");
		this.hints[2].text = HotkeyIcons.KeyMappings[InputControlType.RightStickRight] + " " + ScriptLocalization.Get("RightStickChat");
		this.hints[4].text = HotkeyIcons.KeyMappings[InputControlType.Action1] + " " + ScriptLocalization.Get("Action1Chat");
		this.hints[5].text = HotkeyIcons.KeyMappings[InputControlType.Action2] + " " + ScriptLocalization.Get("Action2Chat");
	}

	private IEnumerable<ChatMessage> GetActualMessages(IEnumerable<ChatMessage> source)
	{
		IEnumerable<ChatMessage> enumerable;
		try
		{
			IList<ChatMessage> list = (source as IList<ChatMessage>) ?? source.ToList<ChatMessage>();
			List<ChatMessage> list2 = list.Where((ChatMessage x) => x.MessageType != LocalEventType.Join && x.MessageType != LocalEventType.Leave && x.MessageType != LocalEventType.Level && x.MessageType != LocalEventType.Achivement && x.MessageType != LocalEventType.FishCaught && x.MessageType != LocalEventType.ItemCaught && x.MessageType != LocalEventType.TackleBroken).ToList<ChatMessage>();
			if (this._showJoinEvents)
			{
				list2.AddRange(list.Where((ChatMessage x) => x.MessageType == LocalEventType.Join || x.MessageType == LocalEventType.Leave));
			}
			if (this._showLevelEvents)
			{
				list2.AddRange(list.Where((ChatMessage x) => x.MessageType == LocalEventType.Level));
			}
			if (this._showAchivementEvents)
			{
				list2.AddRange(list.Where((ChatMessage x) => x.MessageType == LocalEventType.Achivement));
			}
			if (this._showCaughtEvents)
			{
				list2.AddRange(list.Where((ChatMessage x) => x.MessageType == LocalEventType.FishCaught));
			}
			if (this._showUnderwaterItemEvents)
			{
				list2.AddRange(list.Where((ChatMessage x) => x.MessageType == LocalEventType.ItemCaught));
			}
			if (this._showBrokenEvents)
			{
				list2.AddRange(list.Where((ChatMessage x) => x.MessageType == LocalEventType.TackleBroken));
			}
			enumerable = list2;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			enumerable = new List<ChatMessage>();
		}
		return enumerable;
	}

	private MessageChatType GetMessagesType()
	{
		if (ToggleChatController.Instance == null || this._currentToggle == ToggleChatController.Instance.GeneralToggle)
		{
			return MessageChatType.All;
		}
		if (ToggleChatController.Instance.IsTournamentToggle(this._currentToggle))
		{
			return MessageChatType.Tournament;
		}
		return MessageChatType.Private;
	}

	private void OnGotChatPlayersCount(int peerCount)
	{
		StaticUserData.ChatController.PlayerOnLocaionCount = peerCount;
	}

	public void OnScrolling()
	{
		this._inEndText = Math.Abs(this.MessagesScrollBar.value) < 0.001f;
	}

	public void ScrollDown()
	{
		if (this.MessagesScrollBar.value != 0f)
		{
			this.Scroll(-1);
		}
	}

	public void ScrollUp()
	{
		if (this.MessagesScrollBar.value != 1f)
		{
			this.Scroll(1);
		}
	}

	private void Scroll(sbyte direction)
	{
		float height = this.MessagesTextPanel.GetComponent<RectTransform>().rect.height;
		float height2 = this.MessagesTextPanel.transform.parent.GetComponent<RectTransform>().rect.height;
		float num = height2 / (3f * (height - height2));
		this.MessagesScrollBar.value = Mathf.Clamp(this.MessagesScrollBar.value + num * (float)direction, 0f, 1f);
		this._inEndText = (int)direction != 1 && this.MessagesScrollBar.value == 0f;
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		base.StartCoroutine(this.UpdateInputTypeChanged(type));
	}

	private IEnumerator UpdateInputTypeChanged(InputModuleManager.InputType type)
	{
		yield return null;
		bool isGamePad = type == InputModuleManager.InputType.GamePad;
		if (isGamePad)
		{
			this._showShortChat = true;
		}
		if (this._showShortChat)
		{
			this.ShowShortChat();
		}
		else
		{
			this.ShowFullChat();
			if (isGamePad)
			{
				this.ActivateFullChat();
			}
		}
		yield break;
	}

	private void UpdateTimestampText()
	{
		this.ShowTimestampText.text = ((!this._showTimestamp) ? ScriptLocalization.Get("ShowTimestampCaption") : ScriptLocalization.Get("HideTimestampCaption"));
	}

	private void CorrectPosition()
	{
		RectTransform component = base.GetComponent<RectTransform>();
		component.anchoredPosition = new Vector2(64f, 36f);
	}

	private bool _inEndText = true;

	private bool _chatIsActive;

	private bool _showTimestamp;

	private bool _showShortChat;

	private bool _showJoinEvents = true;

	private bool _showLevelEvents = true;

	private bool _showAchivementEvents = true;

	private bool _showCaughtEvents = true;

	private bool _showUnderwaterItemEvents = true;

	private bool _showBrokenEvents = true;

	private bool _showUserCompetitionPromotion = true;

	private bool _chatMessagesUpdated;

	private Toggle _currentToggle;

	private HidesPanel HidesChatPanel;

	private Queue<string> _selfMessagesQueue = new Queue<string>();

	private const float _chatUpdateInterval = 0.5f;

	public AlphaFade FullChat;

	public AlphaFade ShortChat;

	[Space(10f)]
	[SerializeField]
	private SettingsMenu SettingsMenu;

	[SerializeField]
	private UsersChatInit PlayersPanel;

	[SerializeField]
	private RoomsChatInit Rooms;

	private MenuBase[] _menus = new MenuBase[3];

	[Space(10f)]
	public Toggle PlayersToggle;

	public Toggle RoomsToggle;

	[Space(10f)]
	public Button IncreaseFontButton;

	public Button DecreaseFontButton;

	public Button ShowTimestamp;

	public Text ShowTimestampText;

	public Button MinimizeButton;

	public Button MaximizeButton;

	[Space(20f)]
	public Text MessagesTextPanel;

	public Scrollbar MessagesScrollBar;

	public InputField InputPanel;

	[Space(10f)]
	public Text ShortMessagesTextPanel;

	public InputField ShortInputPanel;

	[Space(10f)]
	public Toggle JoinToggle;

	public Toggle LevelToggle;

	public Toggle AchivementToggle;

	public Toggle CaughtToggle;

	public Toggle BrokenToggle;

	public Toggle UnderWaterItemToggle;

	public Toggle UserCompetitionPromotionToggle;

	[HideInInspector]
	public bool LockChat;

	public Text TabAllCaption;

	public Text ChatOpenHelp;

	public Color ActiveColor;

	public Color InActiveColor;

	public Text[] hints;

	public UINavigation[] navigations;

	public HotkeyPressRedirect[] presses;

	public Toggle AllToggle;

	private bool _isEnabled = true;

	private bool isToggleMode;

	private bool _isCatchedFish;

	private ShowHudStates _currentState;

	private bool interactableCallbacksSubscribed;
}
