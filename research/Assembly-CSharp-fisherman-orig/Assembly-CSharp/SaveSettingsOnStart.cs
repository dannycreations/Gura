using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.PlayerProfile;
using I2.Loc;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveSettingsOnStart : ActivityStateControlled
{
	private void Awake()
	{
		this._userNameCaption.text = PlayerProfileHelper.UsernameCaption;
		this._resetProfile.SetActive(true);
		this._controllerGameObject[0] = Object.Instantiate<GameObject>(this.XBoxController, this._controllerContent.transform);
		this._controllerGameObject[1] = Object.Instantiate<GameObject>(this.XBoxControllerLeftHanded, this._controllerContent.transform);
		for (int i = 0; i < this._controllerGameObject.Length; i++)
		{
			this._controllerGameObject[i].SetActive(false);
		}
		this._selectBinding = new HotkeyBinding
		{
			Hotkey = InputControlType.Action1,
			LocalizationKey = "Select"
		};
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		if (!DashboardTabSetter.IsTutorialSlidesEnabled)
		{
			this.ShowTutorialSlides.gameObject.SetActive(false);
			this.ShowTutorialSlides.transform.parent.gameObject.SetActive(false);
		}
	}

	protected override void SetHelp()
	{
		HelpLinePanel.SetActionHelp(this._selectBinding);
		for (int i = 0; i < this.apply.Length; i++)
		{
			this.ChangeInteractible(i, false);
		}
		this.StartForm(base.gameObject);
		this.changedLanguage = false;
		this._currentPanelType = SaveSettingsOnStart.PanelType.Gameplay;
		this.SetDefaultButton.SetActive(true);
		BindingInit.BindingChanged += this.UpdateBinding;
		this.keyBindingsChanged = false;
		PhotonConnectionFactory.Instance.OnDisconnect += this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnProfileUpdated += this.Instance_OnProfileUpdated;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed += this.OnProfileOperationFailed;
		PhotonConnectionFactory.Instance.OnCheckUsernameIsUnique += this.OnCheckUsernameIsUnique;
	}

	protected override void HideHelp()
	{
		HelpLinePanel.HideActionHelp(this._selectBinding);
		this._isInited = false;
		ChangeForm.OnChange = (Action<ActivityState, ChangeForm>)Delegate.Remove(ChangeForm.OnChange, new Action<ActivityState, ChangeForm>(this.OnChangeForm));
		BindingInit.BindingChanged -= this.UpdateBinding;
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnProfileUpdated -= this.Instance_OnProfileUpdated;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed -= this.OnProfileOperationFailed;
		PhotonConnectionFactory.Instance.OnCheckUsernameIsUnique -= this.OnCheckUsernameIsUnique;
	}

	private void OnChangeForm(ActivityState activityState, ChangeForm form)
	{
		if (this.messageBox != null || this.ParentAcrtivity == activityState)
		{
			return;
		}
		if (this.apply.Any((Button button) => button.interactable))
		{
			this._form = form;
			this._prevActivityState = activityState;
			this.messageBox = MenuHelpers.Instance.ShowYesNo(ScriptLocalization.Get("LeaveCaption"), ScriptLocalization.Get("YouHaveUnsavedChanges"), new Action(this.Leave), "YesCaption", new Action(this.Stay), "NoCaption", null, null, null);
		}
		else
		{
			ChangeForm.OnChange = (Action<ActivityState, ChangeForm>)Delegate.Remove(ChangeForm.OnChange, new Action<ActivityState, ChangeForm>(this.OnChangeForm));
			DashboardTabSetter.ForceSwitch(activityState);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ChangeForm.OnChange = (Action<ActivityState, ChangeForm>)Delegate.Remove(ChangeForm.OnChange, new Action<ActivityState, ChangeForm>(this.OnChangeForm));
		BindingInit.BindingChanged -= this.UpdateBinding;
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect;
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		this.UseController.isOn = type == InputModuleManager.InputType.GamePad;
	}

	private void UpdateBinding()
	{
		this.keyBindingsChanged = true;
	}

	protected virtual void Update()
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		if (!this._isInited)
		{
			this._isInited = true;
			this.GameSettingsToggle.group.SetAllTogglesOff();
			this.GameSettingsToggle.isOn = true;
			this.QualityPanel.Index = this._allowedRenderQualities.IndexOf(SettingsManager.RenderQuality);
			this.AntialiasingPanel.Index = this._antialiasingQualities.IndexOf(SettingsManager.Antialiasing);
			this.DynWaterPanel.Index = this._dynWaterQualities.IndexOf(SettingsManager.DynWater);
			this.MouseWheelPanel.isOn = SettingsManager.MouseWheel == MouseWheelValue.Reel;
			this.UseController.isOn = SettingsManager.SavedInputType == InputModuleManager.InputType.GamePad;
			this.SSAO.isOn = SettingsManager.SSAO;
			this.InvertMouse.isOn = SettingsManager.InvertMouse;
			this.InvertJoystick.isOn = SettingsManager.InvertController;
			this.JoystickSensitivitySlider.value = SettingsManager.ControllerSensitivity;
			this.VibrateJoystick.isOn = SettingsManager.Vibrate;
			this.VibrateJoystickIfLineIsNotTensioned.isOn = SettingsManager.VibrateIfNotTensioned;
			this.VSync.isOn = SettingsManager.VSync;
			this.FullScreen.isOn = SettingsManager.IsFullScreen;
			this.ShowTutorialTips.isOn = SettingsManager.ShowTips;
			this.ShowTutorialSlides.isOn = SettingsManager.ShowSlides;
			this.ShowCharacters.isOn = SettingsManager.ShowCharacters;
			this.ShowCharactersBubble.isOn = SettingsManager.ShowCharacterBubble;
			this.ReverseBoatBackwardsMoving.isOn = SettingsManager.ReverseBoatBackwardsMoving;
			this.SnagsWarnings.isOn = SettingsManager.SnagsWarnings;
			this.FishingIndicator.isOn = SettingsManager.FishingIndicator;
			this.BobberBiteSound.isOn = SettingsManager.BobberBiteSound;
			this.ShowCharacters.transform.parent.gameObject.SetActive(StaticUserData.CurrentPond == null);
			this.SoundVolumePanel.value = SettingsManager.SoundVolume;
			this.InterfaceVolumePanel.value = SettingsManager.InterfaceVolume;
			this.MusicVolumePanel.value = SettingsManager.MusicVolume;
			this.VoiceChatVolume.value = SettingsManager.VoiceChatVolume;
			this.EnvironmentVolumePanel.value = SettingsManager.EnvironmentVolume;
			this.MouseSensitivitySlider.value = SettingsManager.MouseSensitivity;
			this.RightHandLayout.isOn = SettingsManager.RightHandedLayout;
			if (this._allowedResolutions.Count > 0)
			{
				int num = this._allowedResolutions.IndexOf(SettingsManager.CurrentResolution);
				this.ResolutionPanel.Index = ((num <= 0) ? 0 : num);
			}
			this.KeyBindingsListConent.GetComponent<KeyBindingsList>().LoadList();
			this._initialMusicVolume = this.MusicVolumePanel.value;
			if (SettingsManager.FightIndicator == FightIndicator.ThreeBands)
			{
				this.DetailedFightIndicator.isOn = true;
			}
			else
			{
				this.DetailedFightIndicator.isOn = false;
			}
			this.BobberScalePanel.value = SettingsManager.BobberScale;
			ChangeForm.OnChange = (Action<ActivityState, ChangeForm>)Delegate.Combine(ChangeForm.OnChange, new Action<ActivityState, ChangeForm>(this.OnChangeForm));
		}
		this._controllerGameObject[0].SetActive(this.RightHandLayout.isOn);
		this._controllerGameObject[1].SetActive(!this.RightHandLayout.isOn);
		this.ChangeInteractible(0, this.DefaultGameplayChanged());
		this.ChangeInteractible(1, this.MouseSettingsChanged());
		this.ChangeInteractible(2, this.KeyBindingsChanged());
		this.ChangeInteractible(4, this.VideoSettingsChanged());
		this.ChangeInteractible(3, this.ControllerSettingsChanged());
		this.ChangeInteractible(5, this.AudioSettingsChanged());
		this.MetricSystemPanel.gameObject.SetActive(this.LanguagePanel.Index == 0);
		if (SettingsManager.MusicVolume != this.MusicVolumePanel.value)
		{
			SettingsManager.MusicVolume = this.MusicVolumePanel.value;
		}
	}

	private void ChangeInteractible(int index, bool interactible)
	{
		this.apply[index].interactable = interactible;
		this.cancel[index].interactable = interactible;
		this.apply[index].transform.Find("Label").GetComponent<Text>().color = ((!interactible) ? this.inactiveColor : this.activeColor);
		this.cancel[index].transform.Find("Label").GetComponent<Text>().color = ((!interactible) ? this.inactiveColor : this.activeColor);
	}

	internal void OnApplicationQuit()
	{
		this._isQuitting = true;
	}

	public void Save()
	{
		this.Save(base.gameObject);
	}

	public void StartForm(GameObject panel)
	{
		Language getCurrentLanguage = ChangeLanguage.GetCurrentLanguage;
		this.LanguagePanel.Items.Clear();
		foreach (KeyValuePair<CustomLanguages, Language> keyValuePair in ChangeLanguage.LanguagesList)
		{
			if (keyValuePair.Value.Id != ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric].Id)
			{
				this.LanguagePanel.Items.Add(keyValuePair.Value.InGameName);
			}
		}
		this.LanguagePanel.Index = getCurrentLanguage.ComboBoxId;
		this.MetricSystemPanel.Items = new List<string>
		{
			ScriptLocalization.Get("MetricSystemCaption"),
			ScriptLocalization.Get("Imperial")
		};
		this.MetricSystemPanel.Index = ((getCurrentLanguage.Id != 1) ? 0 : 1);
		this.MetricSystemElement.SetActive(this.LanguagePanel.Index == 0);
		this.InitResolution();
		this.InitViseoSettings();
		this.InitAudioSettings();
		this.InitAcountInfo();
	}

	public void UpdateName()
	{
		this.Name.text = this.Name.text.ReplaceNonLatin();
	}

	public void Close(GameObject change)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		change.GetComponent<ChangeForm>().Change(this.PreviouslyPanel, false);
		ControlsController.CancelChanges();
		this._isInited = false;
	}

	private void Confirm_ActionCalled(object sender, EventArgs e)
	{
		GracefulDisconnectHandler.OnGracefulDisconnectComplete += this.OnGracefulDisconnectComplete;
		GracefulDisconnectHandler.Disconnect();
	}

	private void OnGracefulDisconnectComplete()
	{
		GracefulDisconnectHandler.OnGracefulDisconnectComplete -= this.OnGracefulDisconnectComplete;
		DisconnectServerAction.Instance.OnDisconnectContinueClick(false);
	}

	private void Stay()
	{
		DashboardTabSetter.ForceSwitch(this.ParentAcrtivity);
	}

	private void Leave()
	{
		ChangeForm.OnChange = (Action<ActivityState, ChangeForm>)Delegate.Remove(ChangeForm.OnChange, new Action<ActivityState, ChangeForm>(this.OnChangeForm));
		DashboardTabSetter.ForceSwitch(this._prevActivityState);
	}

	private void OnDisconnect()
	{
		if (DisconnectServerAction.IsQuitDisconnect)
		{
			Process.GetCurrentProcess().Kill();
		}
	}

	public void SetCurrentPanelType(string panelType)
	{
		switch (panelType)
		{
		case "Video":
			this._currentPanelType = SaveSettingsOnStart.PanelType.Video;
			this.SetDefaultButton.SetActive(true);
			break;
		case "Audio":
			this._currentPanelType = SaveSettingsOnStart.PanelType.Audio;
			this.SetDefaultButton.SetActive(true);
			break;
		case "KeyBindings":
			this._currentPanelType = SaveSettingsOnStart.PanelType.KeyBindings;
			this.SetDefaultButton.SetActive(true);
			break;
		case "Mouse":
			this._currentPanelType = SaveSettingsOnStart.PanelType.Mouse;
			this.SetDefaultButton.SetActive(true);
			break;
		case "Controller":
			this._currentPanelType = SaveSettingsOnStart.PanelType.Controller;
			this.SetDefaultButton.SetActive(true);
			break;
		case "Gameplay":
			this._currentPanelType = SaveSettingsOnStart.PanelType.Gameplay;
			this.SetDefaultButton.SetActive(true);
			break;
		case "Account":
			this._currentPanelType = SaveSettingsOnStart.PanelType.Account;
			if (PhotonConnectionFactory.Instance.Profile != null)
			{
				this.Name.text = PhotonConnectionFactory.Instance.Profile.Name;
			}
			this.SetDefaultButton.SetActive(false);
			break;
		case "Quit":
			this._currentPanelType = SaveSettingsOnStart.PanelType.Quit;
			this.SetDefaultButton.SetActive(false);
			break;
		case "Credits":
			this._currentPanelType = SaveSettingsOnStart.PanelType.Credits;
			this.SetDefaultButton.SetActive(false);
			break;
		case "Support":
			this._currentPanelType = SaveSettingsOnStart.PanelType.Support;
			this.SetDefaultButton.SetActive(false);
			break;
		}
	}

	public void SetDefault()
	{
		this.SetDefault(base.gameObject);
	}

	public void Apply()
	{
		string text = string.Empty;
		Action action = null;
		Action action2 = null;
		switch (this._currentPanelType)
		{
		case SaveSettingsOnStart.PanelType.Video:
			action = new Action(this.SaveVideoSettings);
			action2 = new Action(this.RevertVideo);
			text = string.Format(ScriptLocalization.Get("SaveNewChanges"), ScriptLocalization.Get("VideoCaption"));
			break;
		case SaveSettingsOnStart.PanelType.Audio:
			action = new Action(this.SaveAudioSettings);
			action2 = new Action(this.RevertAudio);
			text = string.Format(ScriptLocalization.Get("SaveNewChanges"), ScriptLocalization.Get("AudioCaption"));
			break;
		case SaveSettingsOnStart.PanelType.Mouse:
			action = new Action(this.SaveMouseSettings);
			action2 = new Action(this.RevertMouse);
			text = string.Format(ScriptLocalization.Get("SaveNewChanges"), ScriptLocalization.Get("MouseCaption"));
			break;
		case SaveSettingsOnStart.PanelType.KeyBindings:
			action = new Action(this.SaveKeyBindings);
			action2 = new Action(this.RevertKeyBindings);
			text = string.Format(ScriptLocalization.Get("SaveNewChanges"), ScriptLocalization.Get("KeyBindingsCaption"));
			break;
		case SaveSettingsOnStart.PanelType.Gameplay:
			action = new Action(this.SaveDefaultGameplay);
			action2 = new Action(this.RevertGameplay);
			text = string.Format(ScriptLocalization.Get("SaveNewChanges"), ScriptLocalization.Get("GameplayCaption"));
			break;
		case SaveSettingsOnStart.PanelType.Controller:
			action = new Action(this.SaveControllerSettings);
			action2 = new Action(this.RevertController);
			text = string.Format(ScriptLocalization.Get("SaveNewChanges"), ScriptLocalization.Get("ControllerCaption").ToLower());
			break;
		}
		this.messageBox = MenuHelpers.Instance.ShowYesNo(text, string.Format(ScriptLocalization.Get("RevertInCaption"), 15), action, "ApplyButton", action2, "CancelButton", delegate
		{
			if (this.changedLanguage)
			{
				UIHelper.Waiting(true, null);
			}
		}, null, null);
		base.StartCoroutine(this.CountDown());
	}

	public void Revert()
	{
		switch (this._currentPanelType)
		{
		case SaveSettingsOnStart.PanelType.Video:
			this.RevertVideo();
			break;
		case SaveSettingsOnStart.PanelType.Audio:
			this.RevertAudio();
			break;
		case SaveSettingsOnStart.PanelType.Mouse:
			this.RevertMouse();
			break;
		case SaveSettingsOnStart.PanelType.KeyBindings:
			this.RevertKeyBindings();
			break;
		case SaveSettingsOnStart.PanelType.Gameplay:
		{
			Language getCurrentLanguage = ChangeLanguage.GetCurrentLanguage;
			this.LanguagePanel.Index = getCurrentLanguage.ComboBoxId;
			this.MetricSystemPanel.Index = ((getCurrentLanguage.Id != 1) ? 0 : 1);
			this.MetricSystemElement.SetActive(this.LanguagePanel.Index == 0);
			this.RevertGameplay();
			break;
		}
		case SaveSettingsOnStart.PanelType.Controller:
			this.RevertController();
			break;
		}
	}

	public void SetDefault(GameObject change)
	{
		string text = string.Empty;
		Action action = null;
		switch (this._currentPanelType)
		{
		case SaveSettingsOnStart.PanelType.Video:
			action = new Action(this.SetDefaultVideo);
			text = string.Format(ScriptLocalization.Get("ConfirmToResetText"), ScriptLocalization.Get("VideoCaption"));
			break;
		case SaveSettingsOnStart.PanelType.Audio:
			action = new Action(this.SetDefaultAudio);
			text = string.Format(ScriptLocalization.Get("ConfirmToResetText"), ScriptLocalization.Get("AudioCaption"));
			break;
		case SaveSettingsOnStart.PanelType.Mouse:
			action = new Action(this.SetDefaultMouse);
			text = string.Format(ScriptLocalization.Get("ConfirmToResetText"), ScriptLocalization.Get("MouseCaption"));
			break;
		case SaveSettingsOnStart.PanelType.KeyBindings:
			action = new Action(this.SetDefaultKeyBindings);
			text = string.Format(ScriptLocalization.Get("ConfirmToResetText"), ScriptLocalization.Get("KeyBindingsCaption"));
			break;
		case SaveSettingsOnStart.PanelType.Gameplay:
			action = new Action(this.SetDefaultGameplay);
			text = string.Format(ScriptLocalization.Get("ConfirmToResetText"), ScriptLocalization.Get("GameplayCaption"));
			break;
		case SaveSettingsOnStart.PanelType.Controller:
			action = new Action(this.SetDefaultController);
			text = string.Format(ScriptLocalization.Get("ConfirmToResetText"), ScriptLocalization.Get("ControllerCaption").ToLower());
			break;
		}
		this.messageBox = MenuHelpers.Instance.ShowYesNo(text, string.Format(ScriptLocalization.Get("RevertInCaption"), 15), action, "ApplyButton", null, "CancelButton", null, null, null);
		base.StartCoroutine(this.CountDown());
	}

	private IEnumerator CountDown()
	{
		this.seconds = 0;
		while (!(this.messageBox == null))
		{
			this.messageBox.GetComponent<MessageBox>().Message = string.Format(ScriptLocalization.Get("RevertInCaption"), 15 - this.seconds);
			yield return new WaitForSeconds(1f);
			if (++this.seconds == 15)
			{
				if (this.messageBox != null)
				{
					this.messageBox.GetComponent<EventConfirmAction>().CancelAction();
				}
				yield break;
			}
		}
		yield break;
		yield break;
	}

	private void InitResolution()
	{
		Resolution[] resolutions = Screen.resolutions;
		int num = 0;
		int num2 = 0;
		List<string> list = new List<string>();
		this._allowedResolutions.Clear();
		for (int i = 0; i < resolutions.Length; i++)
		{
			if (resolutions[i].width >= 1280 && resolutions[i].height >= 720)
			{
				string text = string.Format("{0}x{1} {2}Hz", resolutions[i].width, resolutions[i].height, resolutions[i].refreshRate);
				list.Add(text);
				this._allowedResolutions.Add(resolutions[i]);
				if (SettingsManager.CurrentResolution.width == resolutions[i].width && SettingsManager.CurrentResolution.height == resolutions[i].height && SettingsManager.CurrentResolution.refreshRate == resolutions[i].refreshRate)
				{
					num = num2;
				}
				num2++;
			}
		}
		if (list.Count > 0)
		{
			this.ResolutionPanel.Items = list;
			this.ResolutionPanel.Index = num;
		}
	}

	private void InitAcountInfo()
	{
		if (PhotonConnectionFactory.Instance.Profile != null)
		{
			this.Email.text = PhotonConnectionFactory.Instance.Profile.Email;
			this.Name.text = PhotonConnectionFactory.Instance.Profile.Name;
		}
	}

	public void SaveProfile()
	{
		if (PhotonConnectionFactory.Instance.Profile != null)
		{
			PhotonConnectionFactory.Instance.CheckUsernameIsUnique(this.Name.text);
			this._isSavingName = true;
		}
	}

	private void Instance_OnProfileUpdated()
	{
		if (this.Name.text == PhotonConnectionFactory.Instance.Profile.Name)
		{
			this.ChangeNameButton.GetComponent<BorderedButton>().interactable = false;
			this.CorrectImage.gameObject.SetActive(false);
		}
	}

	public void CheckRules()
	{
		this.InCorrectText.text = string.Empty;
		this.CorrectImage.gameObject.SetActive(false);
		if (this.Name.text == PhotonConnectionFactory.Instance.Profile.Name)
		{
			return;
		}
		if (AbusiveWords.HasAbusiveWords(this.Name.text))
		{
			this.InCorrectText.text = ScriptLocalization.Get("UserNameRequirements1");
			this.ChangeNameButton.GetComponent<BorderedButton>().interactable = false;
			return;
		}
		if (this.Name.text.Length < 3 || this.Name.text.Length > 30)
		{
			this.InCorrectText.text = ScriptLocalization.Get("UserNameRequirements2");
			this.ChangeNameButton.GetComponent<BorderedButton>().interactable = false;
			return;
		}
		if (!this.startWithRgx.IsMatch(this.Name.text))
		{
			this.InCorrectText.text = ScriptLocalization.Get("UserNameRequirements3");
			this.ChangeNameButton.GetComponent<BorderedButton>().interactable = false;
			return;
		}
		if (!this.symbolsRgx.IsMatch(this.Name.text))
		{
			this.InCorrectText.text = ScriptLocalization.Get("UserNameRequirements4");
			this.ChangeNameButton.GetComponent<BorderedButton>().interactable = false;
			return;
		}
		if (!InputCheckingName.rgx.IsMatch(this.Name.text))
		{
			this.InCorrectText.text = ScriptLocalization.Get("UserNameRequirements6");
			this.ChangeNameButton.GetComponent<BorderedButton>().interactable = false;
			return;
		}
		this.InCorrectText.text = string.Empty;
		this.CorrectImage.gameObject.SetActive(true);
		this.ChangeNameButton.GetComponent<BorderedButton>().interactable = true;
	}

	private void OnCheckUsernameIsUnique(bool unique)
	{
		if (this._isSavingName)
		{
			this._isSavingName = false;
			if (!unique)
			{
				this.ShowDuplicateNameMessage();
			}
			else if (PhotonConnectionFactory.Instance.ChangeNameCost != 0)
			{
				if (this.messageBox != null)
				{
					this.messageBox.Close();
				}
				this.messageBox = MenuHelpers.Instance.ShowYesNo(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("ChangeUsernameCost"), PhotonConnectionFactory.Instance.ChangeNameCost), new Action(this.ApplyChargeForNewName), "ApplyButton", delegate
				{
					this.Name.text = PhotonConnectionFactory.Instance.Profile.Name;
				}, "CancelButton", null, null, null);
			}
			else
			{
				PhotonConnectionFactory.Instance.Profile.Name = this.Name.text;
				PhotonConnectionFactory.Instance.UpdateProfile(PhotonConnectionFactory.Instance.Profile);
			}
		}
	}

	private void ApplyChargeForNewName()
	{
		if (this.messageBox != null)
		{
			this.messageBox.GetComponent<MessageBox>().Close();
		}
		if (PhotonConnectionFactory.Instance.Profile.GoldCoins < (double)PhotonConnectionFactory.Instance.ChangeNameCost)
		{
			MenuHelpers.Instance.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("HaventMoney"), false, true, true, delegate
			{
				this.Name.text = PhotonConnectionFactory.Instance.Profile.Name;
			});
		}
		else
		{
			PhotonConnectionFactory.Instance.Profile.Name = this.Name.text;
			PhotonConnectionFactory.Instance.UpdateProfile(PhotonConnectionFactory.Instance.Profile);
		}
	}

	private void ShowDuplicateNameMessage()
	{
		MenuHelpers.Instance.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("DuplicatedUserName"), false, true, false, delegate
		{
			this.Name.text = PhotonConnectionFactory.Instance.Profile.Name;
		});
	}

	private void OnProfileOperationFailed(ProfileFailure failure)
	{
		if (failure.SubOperation == 240)
		{
			this.ShowDuplicateNameMessage();
		}
	}

	public void QuitGame()
	{
		MessageBox _messageBox = this.helpers.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("QuiteInGameMessage"), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, false);
		_messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object e, EventArgs obj)
		{
			_messageBox.Close();
		};
		_messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object e, EventArgs obj)
		{
			DisconnectServerAction.IsQuitDisconnect = true;
			GracefulDisconnectHandler.Disconnect();
		};
	}

	private void InitAudioSettings()
	{
		this._speakerModes.Clear();
		List<string> list = new List<string>();
		list.Add(ScriptLocalization.Get("StereoCaption"));
		this._speakerModes.Add(2);
		list.Add(ScriptLocalization.Get("Surround5Caption"));
		this._speakerModes.Add(5);
		this.SpeakerMode.Items = list;
		this.SpeakerMode.Index = this._speakerModes.IndexOf(SettingsManager.SpeakerMode);
	}

	private void InitViseoSettings()
	{
		this._allowedRenderQualities.Clear();
		this.QualityPanel.Items.Clear();
		foreach (KeyValuePair<RenderQualities, string> keyValuePair in SaveSettingsOnStart.RenderQualitiesLocalizationIds)
		{
			this.QualityPanel.Items.Add(ScriptLocalization.Get(keyValuePair.Value));
			this._allowedRenderQualities.Add(keyValuePair.Key);
		}
		this.QualityPanel.Index = this._allowedRenderQualities.IndexOf(SettingsManager.RenderQuality);
		this._antialiasingQualities.Clear();
		List<string> list = new List<string>();
		list.Add(ScriptLocalization.Get("OffAntialiasingCaption"));
		this._antialiasingQualities.Add(AntialiasingValue.Off);
		list.Add(ScriptLocalization.Get("LowAntialiasingCaption"));
		this._antialiasingQualities.Add(AntialiasingValue.Low);
		list.Add(ScriptLocalization.Get("HighAntialiasingCaption"));
		this._antialiasingQualities.Add(AntialiasingValue.High);
		this.AntialiasingPanel.Items = list;
		this.AntialiasingPanel.Index = this._antialiasingQualities.IndexOf(SettingsManager.Antialiasing);
		this._dynWaterQualities.Clear();
		list = new List<string>();
		list.Add(ScriptLocalization.Get("OffDynWaterCaption"));
		this._dynWaterQualities.Add(DynWaterValue.Off);
		list.Add(ScriptLocalization.Get("LowDynWaterCaption"));
		this._dynWaterQualities.Add(DynWaterValue.Low);
		list.Add(ScriptLocalization.Get("HighDynWaterCaption"));
		this._dynWaterQualities.Add(DynWaterValue.High);
		this.DynWaterPanel.Items = list;
		this.DynWaterPanel.Index = this._dynWaterQualities.IndexOf(SettingsManager.DynWater);
	}

	public void Save(GameObject change)
	{
		if (this._isQuitting)
		{
			return;
		}
		if (EventSystem.current != null)
		{
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		}
	}

	public void ChangeXboxProfile()
	{
	}

	public void ResetProfile()
	{
		this.PrepareMsgBox(ScriptLocalization.Get("Retail_ResetProfileConfirmCaption"), delegate
		{
			PhotonConnectionFactory.Instance.OnResetProfileToDefault += this.Instance_OnResetProfileToDefault;
			PhotonConnectionFactory.Instance.OnResetProfileToDefaultFailed += this.Instance_OnResetProfileToDefaultFailed;
			PhotonConnectionFactory.Instance.ResetProfileToDefault();
		});
	}

	private void Instance_OnResetProfileToDefault()
	{
		PhotonConnectionFactory.Instance.OnResetProfileToDefault -= this.Instance_OnResetProfileToDefault;
		PhotonConnectionFactory.Instance.OnResetProfileToDefaultFailed -= this.Instance_OnResetProfileToDefaultFailed;
		try
		{
			CustomPlayerPrefs.DeleteKey("LastPondId");
			for (int i = 0; i < 300; i++)
			{
				ClientMissionsManager.UnreadQuest(i);
			}
			MissionWidgetManager.ClearPlayerPrefs();
		}
		catch
		{
		}
		this.RequestRestartGame();
	}

	private void Instance_OnResetProfileToDefaultFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnResetProfileToDefault -= this.Instance_OnResetProfileToDefault;
		PhotonConnectionFactory.Instance.OnResetProfileToDefaultFailed -= this.Instance_OnResetProfileToDefaultFailed;
	}

	private void PrepareMsgBox(string msg, Action confirmFunc)
	{
		if (this.messageBox != null)
		{
			this.messageBox.GetComponent<MessageBox>().Close();
		}
		this.messageBox = MenuHelpers.Instance.ShowYesNo(ScriptLocalization.Get("MessageCaption"), msg, confirmFunc, "ApplyButton", null, "CancelButton", null, null, null);
	}

	private bool VideoSettingsChanged()
	{
		return SettingsManager.IsFullScreen != this.FullScreen.isOn || SettingsManager.RenderQuality != this._allowedRenderQualities[this.QualityPanel.Index] || SettingsManager.Antialiasing != this._antialiasingQualities[this.AntialiasingPanel.Index] || SettingsManager.DynWater != this._dynWaterQualities[this.DynWaterPanel.Index] || SettingsManager.SSAO != this.SSAO.isOn || SettingsManager.VSync != this.VSync.isOn || (!Application.isEditor && this.IsResolutionChanged);
	}

	private bool IsResolutionChanged
	{
		get
		{
			if (this.ResolutionPanel.Index < this._allowedResolutions.Count)
			{
				Resolution currentResolution = SettingsManager.CurrentResolution;
				Resolution resolution = this._allowedResolutions[this.ResolutionPanel.Index];
				return currentResolution.width != resolution.width || currentResolution.height != resolution.height || currentResolution.refreshRate != resolution.refreshRate;
			}
			return false;
		}
	}

	private void SaveVideoSettings()
	{
		SettingsManager.IsFullScreen = this.FullScreen.isOn;
		SettingsManager.RenderQuality = this._allowedRenderQualities[this.QualityPanel.Index];
		SettingsManager.Antialiasing = this._antialiasingQualities[this.AntialiasingPanel.Index];
		SettingsManager.DynWater = this._dynWaterQualities[this.DynWaterPanel.Index];
		SettingsManager.SSAO = this.SSAO.isOn;
		SettingsManager.VSync = this.VSync.isOn;
		if (!Application.isEditor)
		{
			int index = this.ResolutionPanel.Index;
			if (index >= 0 && index < this._allowedResolutions.Count)
			{
				SettingsManager.CurrentResolution = this._allowedResolutions[index];
			}
			else
			{
				LogHelper.Error("SaveVideoSettings Resolution index out of range idx:{0} Count:{1}", new object[]
				{
					index,
					this._allowedResolutions.Count
				});
			}
		}
		this.ChangeInteractible(4, false);
	}

	private void SetDefaultVideo()
	{
		SettingsManager.SetDefaultVideo();
		this.RevertVideo();
	}

	private void RevertVideo()
	{
		this.QualityPanel.Index = this._allowedRenderQualities.IndexOf(SettingsManager.RenderQuality);
		this.AntialiasingPanel.Index = this._antialiasingQualities.IndexOf(SettingsManager.Antialiasing);
		this.DynWaterPanel.Index = this._dynWaterQualities.IndexOf(SettingsManager.DynWater);
		this.SSAO.isOn = SettingsManager.SSAO;
		this.VSync.isOn = SettingsManager.VSync;
		this.FullScreen.isOn = SettingsManager.IsFullScreen;
		if (this._allowedResolutions.Count > 0)
		{
			this.ResolutionPanel.Index = this._allowedResolutions.IndexOf(SettingsManager.CurrentResolution);
		}
		this.ChangeInteractible(4, false);
	}

	private bool AudioSettingsChanged()
	{
		return SettingsManager.SpeakerMode != this._speakerModes[this.SpeakerMode.Index] || this._initialMusicVolume != this.MusicVolumePanel.value || SettingsManager.SoundVolume != this.SoundVolumePanel.value || SettingsManager.VoiceChatVolume != this.VoiceChatVolume.value || SettingsManager.InterfaceVolume != this.InterfaceVolumePanel.value || SettingsManager.BobberBiteSound != this.BobberBiteSound.isOn || SettingsManager.EnvironmentVolume != this.EnvironmentVolumePanel.value;
	}

	private void SaveAudioSettings()
	{
		this._initialMusicVolume = this.MusicVolumePanel.value;
		SettingsManager.SpeakerMode = this._speakerModes[this.SpeakerMode.Index];
		SettingsManager.SoundVolume = this.SoundVolumePanel.value;
		SettingsManager.InterfaceVolume = this.InterfaceVolumePanel.value;
		SettingsManager.VoiceChatVolume = this.VoiceChatVolume.value;
		SettingsManager.BobberBiteSound = this.BobberBiteSound.isOn;
		SettingsManager.EnvironmentVolume = this.EnvironmentVolumePanel.value;
		this.ChangeInteractible(5, false);
	}

	private void SetDefaultAudio()
	{
		SettingsManager.SetDefaultAudio();
		this.RevertAudio();
	}

	private void RevertAudio()
	{
		this.SpeakerMode.Index = this._speakerModes.IndexOf(SettingsManager.SpeakerMode);
		this.VoiceChatVolume.value = SettingsManager.VoiceChatVolume;
		this.InterfaceVolumePanel.value = SettingsManager.InterfaceVolume;
		this.SoundVolumePanel.value = SettingsManager.SoundVolume;
		this.EnvironmentVolumePanel.value = SettingsManager.EnvironmentVolume;
		SliderValueChange musicVolumePanel = this.MusicVolumePanel;
		float initialMusicVolume = this._initialMusicVolume;
		SettingsManager.MusicVolume = initialMusicVolume;
		musicVolumePanel.value = initialMusicVolume;
		this.BobberBiteSound.isOn = SettingsManager.BobberBiteSound;
		this.ChangeInteractible(5, false);
	}

	private bool MouseSettingsChanged()
	{
		return (SettingsManager.MouseWheel == MouseWheelValue.Drag && this.MouseWheelPanel.isOn) || (SettingsManager.MouseWheel == MouseWheelValue.Reel && !this.MouseWheelPanel.isOn) || SettingsManager.InvertMouse != this.InvertMouse.isOn || SettingsManager.MouseSensitivity != this.MouseSensitivitySlider.value;
	}

	private void SaveMouseSettings()
	{
		SettingsManager.MouseWheel = ((!this.MouseWheelPanel.isOn) ? MouseWheelValue.Drag : MouseWheelValue.Reel);
		SettingsManager.InvertMouse = this.InvertMouse.isOn;
		SettingsManager.MouseSensitivity = this.MouseSensitivitySlider.value;
		this.ChangeInteractible(1, false);
	}

	private void SetDefaultMouse()
	{
		SettingsManager.SetDefaultMouse();
		this.RevertMouse();
	}

	private void RevertMouse()
	{
		this.InvertMouse.isOn = SettingsManager.InvertMouse;
		this.MouseSensitivitySlider.value = SettingsManager.MouseSensitivity;
		this.MouseWheelPanel.isOn = SettingsManager.MouseWheel == MouseWheelValue.Reel;
		this.ChangeInteractible(1, false);
	}

	private bool ControllerSettingsChanged()
	{
		return SettingsManager.InvertController != this.InvertJoystick.isOn || SettingsManager.ControllerSensitivity != this.JoystickSensitivitySlider.value || SettingsManager.Vibrate != this.VibrateJoystick.isOn || SettingsManager.VibrateIfNotTensioned != this.VibrateJoystickIfLineIsNotTensioned.isOn || SettingsManager.RightHandedLayout != this.RightHandLayout.isOn || (SettingsManager.SavedInputType == InputModuleManager.InputType.Mouse && this.UseController.isOn) || (SettingsManager.SavedInputType == InputModuleManager.InputType.GamePad && !this.UseController.isOn);
	}

	private void SaveControllerSettings()
	{
		SettingsManager.InvertController = this.InvertJoystick.isOn;
		SettingsManager.ControllerSensitivity = this.JoystickSensitivitySlider.value;
		SettingsManager.Vibrate = this.VibrateJoystick.isOn;
		SettingsManager.VibrateIfNotTensioned = this.VibrateJoystickIfLineIsNotTensioned.isOn;
		SettingsManager.RightHandedLayout = this.RightHandLayout.isOn;
		CursorManager.Instance.MouseCursor = !this.UseController.isOn;
		SettingsManager.InputType = ((!this.UseController.isOn) ? InputModuleManager.InputType.Mouse : InputModuleManager.InputType.GamePad);
		this.ChangeInteractible(3, false);
	}

	private void SetDefaultController()
	{
		SettingsManager.SetDefaultController();
		this.RevertController();
	}

	private void RevertController()
	{
		this.UseController.isOn = SettingsManager.SavedInputType == InputModuleManager.InputType.GamePad;
		this.InvertJoystick.isOn = SettingsManager.InvertController;
		this.JoystickSensitivitySlider.value = SettingsManager.ControllerSensitivity;
		this.RightHandLayout.isOn = SettingsManager.RightHandedLayout;
		this.VibrateJoystick.isOn = SettingsManager.Vibrate;
		this.VibrateJoystickIfLineIsNotTensioned.isOn = SettingsManager.VibrateIfNotTensioned;
		SettingsManager.InputType = ((!this.UseController.isOn) ? InputModuleManager.InputType.Mouse : InputModuleManager.InputType.GamePad);
		this.ChangeInteractible(3, false);
	}

	private bool KeyBindingsChanged()
	{
		return this.keyBindingsChanged;
	}

	private void SaveKeyBindings()
	{
		ControlsController.SaveBindings();
		this.ChangeInteractible(2, false);
		this.keyBindingsChanged = false;
	}

	private void SetDefaultKeyBindings()
	{
		ControlsController.ResetToDefault();
		this.RevertKeyBindings();
	}

	private void RevertKeyBindings()
	{
		ControlsController.CancelChanges();
		this.KeyBindingsListConent.GetComponent<KeyBindingsList>().ClearList();
		this.KeyBindingsListConent.GetComponent<KeyBindingsList>().LoadList();
		this.ChangeInteractible(2, false);
		this.keyBindingsChanged = false;
	}

	private bool DefaultGameplayChanged()
	{
		bool flag = this.MetricSystemPanel.Index == 0;
		return SettingsManager.ShowTips != this.ShowTutorialTips.isOn || SettingsManager.ShowCharacters != this.ShowCharacters.isOn || SettingsManager.ShowCharacterBubble != this.ShowCharactersBubble.isOn || SettingsManager.ReverseBoatBackwardsMoving != this.ReverseBoatBackwardsMoving.isOn || SettingsManager.SnagsWarnings != this.SnagsWarnings.isOn || SettingsManager.FishingIndicator != this.FishingIndicator.isOn || SettingsManager.ShowSlides != this.ShowTutorialSlides.isOn || (this.DetailedFightIndicator.isOn && SettingsManager.FightIndicator == FightIndicator.OneBand) || (!this.DetailedFightIndicator.isOn && SettingsManager.FightIndicator == FightIndicator.ThreeBands) || SettingsManager.BobberScale != this.BobberScalePanel.value || ChangeLanguage.GetCurrentLanguage != ChangeLanguage.GetLanguage(this.LanguagePanel.Index, flag);
	}

	private void SaveDefaultGameplay()
	{
		SettingsManager.ShowTips = this.ShowTutorialTips.isOn;
		SettingsManager.ShowSlides = this.ShowTutorialSlides.isOn;
		SettingsManager.ShowCharacters = this.ShowCharacters.isOn;
		SettingsManager.ShowCharacterBubble = this.ShowCharactersBubble.isOn;
		SettingsManager.ReverseBoatBackwardsMoving = this.ReverseBoatBackwardsMoving.isOn;
		SettingsManager.SnagsWarnings = this.SnagsWarnings.isOn;
		SettingsManager.FishingIndicator = this.FishingIndicator.isOn;
		if (this.DetailedFightIndicator.isOn)
		{
			SettingsManager.FightIndicator = FightIndicator.ThreeBands;
		}
		else
		{
			SettingsManager.FightIndicator = FightIndicator.OneBand;
		}
		SettingsManager.BobberScale = this.BobberScalePanel.value;
		bool flag = this.MetricSystemPanel.Index == 0;
		if (ChangeLanguage.GetCurrentLanguage != ChangeLanguage.GetLanguage(this.LanguagePanel.Index, flag))
		{
			this.changedLanguage = true;
		}
		if (PhotonConnectionFactory.Instance.Profile != null)
		{
			if (this.changedLanguage)
			{
				ChangeLanguage.LanguageChanged += this.ChangeLanguage_LanguageChanged;
				ChangeLanguage.ChangeLanguageAction(ChangeLanguage.GetLanguage(this.LanguagePanel.Index, flag));
			}
		}
		else
		{
			this._languageId = ChangeLanguage.ChangeLanguageActionWithoutProfile(ChangeLanguage.GetLanguage(this.LanguagePanel.Index, flag));
		}
		if (this._languageId == -1)
		{
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		}
		this._isInited = false;
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		this._isInited = false;
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		this.ChangeInteractible(0, false);
	}

	private void ChangeLanguage_LanguageChanged(object sender, EventArgs e)
	{
		this.changedLanguage = false;
		ScriptLocalization.OnLanguageChanged();
		ChangeLanguage.LanguageChanged -= this.ChangeLanguage_LanguageChanged;
		UIHelper.Waiting(false, null);
		this.RequestRestartGame();
	}

	private void RequestRestartGame()
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		EventAction messageBoxNew = this._menuHelpers.ShowMessage(this._menuHelpers.MenuPrefabsList.optionsForm, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("RestartGameCaption"), false, false, false, null).GetComponent<EventAction>();
		messageBoxNew.ActionCalled += delegate(object o, EventArgs args)
		{
			messageBoxNew.GetComponent<MessageBox>().CloseFast();
			MenuHelpers.Instance.HideMenu(false, false, true);
			this.Confirm_ActionCalled(o, args);
		};
	}

	private void SetDefaultGameplay()
	{
		SettingsManager.SetDefaultGameplay();
		this.RevertGameplay();
	}

	private void RevertGameplay()
	{
		if (SettingsManager.FightIndicator == FightIndicator.ThreeBands)
		{
			this.DetailedFightIndicator.isOn = true;
		}
		else
		{
			this.DetailedFightIndicator.isOn = false;
		}
		this.BobberScalePanel.value = SettingsManager.BobberScale;
		this.ShowTutorialSlides.isOn = SettingsManager.ShowSlides;
		this.ShowTutorialTips.isOn = SettingsManager.ShowTips;
		this.ShowCharacters.isOn = SettingsManager.ShowCharacters;
		this.ShowCharactersBubble.isOn = SettingsManager.ShowCharacterBubble;
		this.ReverseBoatBackwardsMoving.isOn = SettingsManager.ReverseBoatBackwardsMoving;
		this.SnagsWarnings.isOn = SettingsManager.SnagsWarnings;
		this.FishingIndicator.isOn = SettingsManager.FishingIndicator;
		this.MetricSystemPanel.Index = ((ChangeLanguage.GetCurrentLanguage.Id != 1) ? 0 : 1);
		this.ChangeInteractible(0, false);
	}

	[SerializeField]
	private GameObject _resetProfile;

	public VerticalPickList LanguagePanel;

	public VerticalPickList MetricSystemPanel;

	public GameObject MetricSystemElement;

	public TwoButtonToggle DetailedFightIndicator;

	public Slider BobberScalePanel;

	public SliderToggle ShowTutorialTips;

	public SliderToggle ShowTutorialSlides;

	public SliderToggle ShowCharacters;

	public SliderToggle ShowCharactersBubble;

	public SliderToggle ReverseBoatBackwardsMoving;

	public SliderToggle SnagsWarnings;

	public SliderToggle FishingIndicator;

	public SliderToggle UseOnlyPs4Helps;

	public SliderToggle UseOnlyXboxHelps;

	public SliderValueChange MouseSensitivitySlider;

	public SliderToggle InvertMouse;

	public TwoButtonToggle MouseWheelPanel;

	public SliderToggle UseController;

	public TwoButtonToggle RightHandLayout;

	public SliderToggle VibrateJoystick;

	public SliderToggle VibrateJoystickIfLineIsNotTensioned;

	public SliderToggle InvertJoystick;

	public SliderValueChange JoystickSensitivitySlider;

	public GameObject PSController;

	public GameObject XBoxController;

	public GameObject PSControllerLeftHanded;

	public GameObject XBoxControllerLeftHanded;

	private GameObject[] _controllerGameObject = new GameObject[2];

	[SerializeField]
	private GameObject _controllerContent;

	public VerticalPickList ResolutionPanel;

	public VerticalPickList QualityPanel;

	public VerticalPickList AntialiasingPanel;

	public VerticalPickList DynWaterPanel;

	public SliderToggle VSync;

	public SliderToggle SSAO;

	public SliderToggle FullScreen;

	public VerticalPickList SpeakerMode;

	public SliderValueChange VoiceChatVolume;

	public SliderValueChange SoundVolumePanel;

	public SliderValueChange InterfaceVolumePanel;

	public SliderValueChange MusicVolumePanel;

	public SliderValueChange EnvironmentVolumePanel;

	public SliderToggle BobberBiteSound;

	public SliderToggle VoiceChatMuted;

	[Space(8f)]
	[SerializeField]
	private Text _userNameCaption;

	public InputField Name;

	public LayoutElement NameLayout;

	public GameObject ChangeNameButton;

	public Text Email;

	public Image NameBorder;

	public Text InCorrectText;

	public Image CorrectImage;

	[Space(8f)]
	public GameObject QuitToggle;

	public ActivityState ParentAcrtivity;

	public Toggle GameSettingsToggle;

	public GameObject KeyBindingsListConent;

	public Toggle HideWhatsNew;

	public MessageBox messageBox;

	public GameObject SetDefaultButton;

	public static readonly Dictionary<RenderQualities, string> RenderQualitiesLocalizationIds = new Dictionary<RenderQualities, string>
	{
		{
			RenderQualities.Fastest,
			"FastestRenderCaption"
		},
		{
			RenderQualities.Fast,
			"FastRenderCaption"
		},
		{
			RenderQualities.Simple,
			"SimpleRenderCaption"
		},
		{
			RenderQualities.Good,
			"GoodRenderCaption"
		},
		{
			RenderQualities.Beautiful,
			"BeautifulRenderCaption"
		},
		{
			RenderQualities.Fantastic,
			"FantasticRenderCaption"
		},
		{
			RenderQualities.Ultra,
			"UltraRenderCaption"
		}
	};

	private SaveSettingsOnStart.PanelType _currentPanelType = SaveSettingsOnStart.PanelType.Gameplay;

	private List<Resolution> _allowedResolutions = new List<Resolution>();

	private List<RenderQualities> _allowedRenderQualities = new List<RenderQualities>();

	private List<AntialiasingValue> _antialiasingQualities = new List<AntialiasingValue>();

	private List<DynWaterValue> _dynWaterQualities = new List<DynWaterValue>();

	private List<AudioSpeakerMode> _speakerModes = new List<AudioSpeakerMode>();

	private MenuHelpers _menuHelpers = new MenuHelpers();

	private bool changedLanguage;

	private float _initialMusicVolume;

	private bool _isQuitting;

	private const int revertInSeconds = 15;

	private int seconds;

	protected bool _isInited;

	private MenuHelpers helpers = new MenuHelpers();

	[SerializeField]
	private Button[] apply;

	[SerializeField]
	private Button[] cancel;

	private bool keyBindingsChanged;

	private Color activeColor = new Color(0.96862745f, 0.96862745f, 0.96862745f, 1f);

	private Color inactiveColor = new Color(0.16470589f, 0.16470589f, 0.16470589f, 1f);

	[HideInInspector]
	public ActivityState PreviouslyPanel;

	private ActivityState _prevActivityState;

	private ChangeForm _form;

	private int _languageId = -1;

	private bool _isSavingName;

	private static string startWithPattern = "^[a-zA-Z0-9].*?[a-zA-Z0-9]$";

	private Regex startWithRgx = new Regex(SaveSettingsOnStart.startWithPattern, RegexOptions.IgnoreCase);

	private static string symbolsPattern = "^[a-zA-Z0-9._-]*$";

	private Regex symbolsRgx = new Regex(SaveSettingsOnStart.symbolsPattern, RegexOptions.IgnoreCase);

	private HotkeyBinding _selectBinding;

	public const int QuestsCount = 300;

	private enum PanelType
	{
		Video,
		Audio,
		Mouse,
		KeyBindings,
		Gameplay,
		Controller,
		Account,
		Quit,
		Credits,
		Support
	}
}
