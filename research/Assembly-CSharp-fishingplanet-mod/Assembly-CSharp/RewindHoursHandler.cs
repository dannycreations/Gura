using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewindHoursHandler : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	public int HoursOffset
	{
		get
		{
			return this.currentTimeHourOffset.Hour;
		}
	}

	public bool IsActive
	{
		get
		{
			return this.RewindBackgroundPanel.activeSelf || this.WaitRewindBackgroundPanel.activeSelf || this.CooldownPanel.activeSelf;
		}
	}

	private void Awake()
	{
		this._retailRewindGo.SetActive(false);
		this._normalRewindGo.SetActive(true);
		string text = HotkeyIcons.KeyMappings[InputControlType.DPadLeft];
		Text helpTextRewind = this._helpTextRewind;
		string text2 = string.Format(ScriptLocalization.Get("Retail_FastForwardTimeCaption"), string.Format(" <size=26><color=#FFEE44FF>{0}</color></size> ", text), string.Format(" <size=26><color=#FFEE44FF>{0}</color></size> ", "\ue703"));
		this._helpTextRewindRetail.text = text2;
		helpTextRewind.text = text2;
		this.CooldownPanel.SetActive(false);
		this.RewindBackgroundPanel.SetActive(false);
		this.WaitRewindBackgroundPanel.SetActive(false);
		this._rewindBackgroundAlphaFade = this.RewindBackgroundPanel.GetComponent<AlphaFade>();
		this._waitRewindBackgroundAlphaFade = this.WaitRewindBackgroundPanel.GetComponent<AlphaFade>();
		AlphaFade component = this.CooldownPanel.GetComponent<AlphaFade>();
		this._waitRewindBackgroundAlphaFade.OnShowCalled.AddListener(new UnityAction(this.BlockInputShowCursor));
		this._waitRewindBackgroundAlphaFade.HideFinished += this.Unblock;
		component.OnShowCalled.AddListener(new UnityAction(this.BlockInputShowCursor));
		component.HideFinished += this.Unblock;
		this._rewindBackgroundAlphaFade.OnShowCalled.AddListener(new UnityAction(this.BlockInputShowCursor));
		this._rewindBackgroundAlphaFade.HideFinished += this.Unblock;
	}

	private void Start()
	{
		this._rewindBackgroundAlphaFade.FastHidePanel();
		this.RewindBackgroundPanel.SetActive(false);
		this._waitRewindBackgroundAlphaFade.FastHidePanel();
		this.WaitRewindBackgroundPanel.SetActive(false);
		this._rewindBackgroundAlphaFade.HideFinished += new EventHandler<EventArgsAlphaFade>(this.RewindHoursHandler_HideFinished);
		this._waitRewindBackgroundAlphaFade.HideFinished += new EventHandler<EventArgsAlphaFade>(this.WaitRewindBackgroundPanel_HideFinished);
		this._originalPremiumColor = this.PremiumTextElements[0].color;
		this.HoursValue = 1;
	}

	internal void OnDestroy()
	{
		AlphaFade component = this.CooldownPanel.GetComponent<AlphaFade>();
		component.OnShowCalled.RemoveListener(new UnityAction(this.BlockInputShowCursor));
		component.HideFinished -= this.Unblock;
		this._rewindBackgroundAlphaFade.OnShowCalled.RemoveListener(new UnityAction(this.BlockInputShowCursor));
		this._rewindBackgroundAlphaFade.HideFinished -= this.Unblock;
		this._rewindBackgroundAlphaFade.HideFinished -= new EventHandler<EventArgsAlphaFade>(this.RewindHoursHandler_HideFinished);
		this._waitRewindBackgroundAlphaFade.OnShowCalled.RemoveListener(new UnityAction(this.BlockInputShowCursor));
		this._waitRewindBackgroundAlphaFade.HideFinished -= this.Unblock;
	}

	internal void Update()
	{
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			return;
		}
		if (ControlsController.ControlsActions.NextHours.WasPressed && !ShowHudElements.Instance.IsEquipmentChangeBusy())
		{
			if (PhotonConnectionFactory.Instance.Profile.Tournament == null || PhotonConnectionFactory.Instance.Profile.Tournament.IsEnded)
			{
				if ((GameFactory.Player != null && GameFactory.Player.State != typeof(PlayerIdle) && GameFactory.Player.State != typeof(PlayerIdlePitch) && GameFactory.Player.State != typeof(PlayerDrawIn) && GameFactory.Player.State != typeof(PlayerEmpty) && GameFactory.Player.State != typeof(HandIdle) && (GameFactory.Player.State != typeof(PlayerOnBoat) || GameFactory.Player.IsBoatFishing)) || !RodPodHelper.IsFreeAllRodStands)
				{
					GameFactory.Message.ShowCantOpenMenuWhenCasted();
				}
				else if (!this._alreadyRequested)
				{
					this.RequestCooldownAndShowPanel();
				}
			}
			else
			{
				GameFactory.Message.ShowCantForwardTimeInTournament(TournamentHelper.IsCompetition);
			}
		}
	}

	private void RequestCooldownAndShowPanel()
	{
		this._alreadyRequested = true;
		PhotonConnectionFactory.Instance.OnGotTimeForwardCooldown += this.Instance_OnGotTimeForwardCooldown;
		PhotonConnectionFactory.Instance.OnGettingTimeForwardCooldownFailed += this.Instance_OnGettingTimeForwardCooldownFailed;
		PhotonConnectionFactory.Instance.GetTimeForwardCooldown();
	}

	private void BlockInputShowCursor()
	{
		CursorManager.ShowCursor();
		ControlsController.ControlsActions.BlockInput(null);
	}

	private void Unblock(object sender, EventArgsAlphaFade e)
	{
		ControlsController.ControlsActions.UnBlockInput();
		CursorManager.HideCursor();
	}

	private void Instance_OnGettingTimeForwardCooldownFailed(Failure failure)
	{
		LogHelper.Error(failure.ErrorMessage, new object[0]);
		this._alreadyRequested = false;
	}

	private void Instance_OnGotTimeForwardCooldown(DateTime? cooldown)
	{
		PhotonConnectionFactory.Instance.OnGotTimeForwardCooldown -= this.Instance_OnGotTimeForwardCooldown;
		PhotonConnectionFactory.Instance.OnGettingTimeForwardCooldownFailed -= this.Instance_OnGettingTimeForwardCooldownFailed;
		this._alreadyRequested = false;
		if (MenuHelpers.Instance.IsInMenu)
		{
			return;
		}
		UIStatsCollector.ChangeGameScreen(GameScreenType.Time, GameScreenTabType.Undefined, null, null, null, null, null);
		ShowHudElements.Instance.SetCrossHairState(CrossHair.CrossHairState.None);
		ShowHudElements.Instance.ActivateCrossHair(false);
		PhotonConnectionFactory.Instance.Game.Pause();
		GameFactory.ChatInGameController.SetActive(false);
		if (cooldown != null && cooldown.Value > TimeHelper.UtcTime())
		{
			this.ShowCooldownPanel(cooldown.Value);
		}
		else
		{
			this.ShowRewindPanel();
		}
	}

	private void RewindHoursHandler_HideFinished(object sender, EventArgs e)
	{
		this.RewindBackgroundPanel.SetActive(false);
	}

	private void ShowRewindPanel()
	{
		this.RewindBackgroundPanel.SetActive(true);
		this._rewindBackgroundAlphaFade.FastHidePanel();
		this._rewindBackgroundAlphaFade.ShowPanel();
		this._rewindBackgroundAlphaFade.CanvasGroup.interactable = true;
		this.UpdateData(1);
	}

	private void HideRewindPanel()
	{
		this._rewindBackgroundAlphaFade.CanvasGroup.interactable = false;
		this._rewindBackgroundAlphaFade.HidePanel();
	}

	private void StartChangeTime()
	{
		PhotonConnectionFactory.Instance.Game.Resume(false);
		this.WaitRewindBackgroundPanel.SetActive(true);
		this._waitRewindBackgroundAlphaFade.FastHidePanel();
		this._waitRewindBackgroundAlphaFade.ShowPanel();
		this._changesTime = true;
		base.Invoke("AskMoveTimeForward", 0.2f);
	}

	internal void AskMoveTimeForward()
	{
		PhotonConnectionFactory.Instance.OnMovedTimeForward += this.OnMovedTimeForward;
		PhotonConnectionFactory.Instance.OnMovingTimeForwardFailed += this.OnMovingTimeForwardFailed;
		if (this.HoursValue > 0)
		{
			PhotonConnectionFactory.Instance.MoveTimeForward(new int?(this.HoursValue));
		}
		else
		{
			PhotonConnectionFactory.Instance.MoveTimeForward(null);
		}
		UIStatsCollector.ChangeGameScreen(GameScreenType.Game, GameScreenTabType.Undefined, null, null, null, null, null);
		Debug.Log("Start change time: " + Time.time);
	}

	public void Increase()
	{
		if (this.HoursValue == 24)
		{
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			return;
		}
		this.UpdateData(++this.HoursValue);
	}

	public void Decrease()
	{
		if (this.HoursValue == 1)
		{
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			return;
		}
		this.UpdateData(--this.HoursValue);
	}

	public void UpdateData(int hours)
	{
		this.HoursValue = hours;
		if (this.HoursValue < 1)
		{
			this.HoursValue = 1;
		}
		if (this.HoursValue > 24)
		{
			this.HoursValue = 24;
		}
		TimeSpan value = TimeAndWeatherManager.CurrentTime.Value;
		TimeSpan timeSpan = new TimeSpan(this.HoursValue, -Convert.ToInt32(value.Minutes), 0);
		DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
		this.currentTimeHourOffset = dateTime.Add(value);
		this.currentTimeHourOffset = this.currentTimeHourOffset.Add(timeSpan);
		if (this.HoursValue == 24)
		{
			Text hoursRetail = this._hoursRetail;
			string text = ScriptLocalization.Get("1DayText");
			this.Hours.text = text;
			hoursRetail.text = text;
		}
		else
		{
			Text hoursRetail2 = this._hoursRetail;
			string text = MeasuringSystemManager.TimeString(this.currentTimeHourOffset);
			this.Hours.text = text;
			hoursRetail2.text = text;
		}
		this.ChangeTimeInfo();
	}

	private void ChangeTimeInfo()
	{
		DateTime? dateTime = PhotonConnectionFactory.Instance.PreviewMoveTimeForward(PhotonConnectionFactory.Instance.Profile.HasPremium, new int?(this.HoursValue));
		TimeSpan timeSpan = ((dateTime == null) ? TimeHelper.UtcTime() : dateTime.Value) - TimeHelper.UtcTime();
		this.InfoText.text = string.Format(ScriptLocalization.Get("CooldownText"), string.Concat(new string[]
		{
			timeSpan.Hours.ToString("d2"),
			":",
			timeSpan.Minutes.ToString("d2"),
			":",
			timeSpan.Seconds.ToString("d2")
		}));
	}

	public void OnScroll(PointerEventData eventData)
	{
		this.HoursValue += (int)eventData.scrollDelta.y;
		this.UpdateData(this.HoursValue);
	}

	public void ApplyRewind()
	{
		this.Rewind(false);
	}

	public void RewindToNextMorning()
	{
		this.Rewind(true);
	}

	public void CancelRewind()
	{
		GameFactory.ChatInGameController.SetActive(true);
		this.HideRewindPanel();
		this.ResumeGame(true);
	}

	private void Rewind(bool toNextMorning)
	{
		if ((double)PhotonConnectionFactory.Instance.RemoveTimeForwardCooldownCost > PhotonConnectionFactory.Instance.Profile.GoldCoins && this.CooldownPanel.activeInHierarchy)
		{
			string text = ScriptLocalization.Get("HaventMoney");
			MessageBox messageBox = this.helpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), text, false, false, false, null);
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			messageBox.GetComponent<EventAction>().ActionCalled += delegate(object e, EventArgs o)
			{
				if (messageBox != null)
				{
					messageBox.Close();
				}
			};
		}
		else
		{
			if (toNextMorning)
			{
				this.HoursValue = -1;
			}
			this.HideRewindPanel();
			this.StartChangeTime();
		}
	}

	private void OnMovedTimeForward(TimeSpan time)
	{
		this.MovingTime();
	}

	private void OnMovingTimeForwardFailed(Failure failure)
	{
		Debug.LogError(string.Format("Moving time forward failed: {0}", failure.FullErrorInfo));
		this.MovingTime();
	}

	private void MovingTime()
	{
		PhotonConnectionFactory.Instance.OnMovedTimeForward -= this.OnMovedTimeForward;
		PhotonConnectionFactory.Instance.OnMovingTimeForwardFailed -= this.OnMovingTimeForwardFailed;
		if (this._changesTime)
		{
			this._changesTime = false;
			GameFactory.SkyControllerInstance.ChangedSky += this.SkyControllerInstance_ChangedSky;
			GameFactory.SkyControllerInstance.UnnecessaryChangeSky += this.SkyControllerInstance_ChangedSky;
			TimeAndWeatherManager.ForceUpdateTime();
			GameFactory.Player.OnMovedTimeForward();
		}
	}

	private void SkyControllerInstance_ChangedSky(object sender, EventArgs e)
	{
		if (this._waitRewindBackgroundAlphaFade != null)
		{
			this._waitRewindBackgroundAlphaFade.HidePanel();
		}
		GameFactory.SkyControllerInstance.ChangedSky -= this.SkyControllerInstance_ChangedSky;
		GameFactory.SkyControllerInstance.UnnecessaryChangeSky -= this.SkyControllerInstance_ChangedSky;
		Debug.Log("End change time|: " + Time.time);
		this.ResumeGame(true);
	}

	private void WaitRewindBackgroundPanel_HideFinished(object sender, EventArgs e)
	{
		this.WaitRewindBackgroundPanel.SetActive(false);
	}

	private void ResumeGame(bool isUnBlockInput = true)
	{
		PhotonConnectionFactory.Instance.Game.Resume(true);
		if (!CursorManager.IsModalWindow() && isUnBlockInput)
		{
			GameFactory.ChatInGameController.SetActive(true);
		}
	}

	private void ShowCooldownPanel(DateTime cooldown)
	{
		this.CooldownPanel.SetActive(true);
		this.CooldownPanel.GetComponent<AlphaFade>().FastHidePanel();
		this.CooldownPanel.GetComponent<AlphaFade>().ShowPanel();
		this.CooldownPanel.GetComponent<AlphaFade>().CanvasGroup.interactable = true;
		this.SkipButtonText.text = string.Format(ScriptLocalization.Get("SkipCooldownButton"), "\ue62c " + PhotonConnectionFactory.Instance.RemoveTimeForwardCooldownCost).ToUpper();
		this.CooldownPanel.GetComponent<HotkeyPressRedirect>().ForceAppendHotkeyIcons();
		this._timeCoroutine = base.StartCoroutine(this.UpdateCooldownInfo(cooldown));
		bool hasPremium = PhotonConnectionFactory.Instance.Profile.HasPremium;
		this.YourPremium.enabled = hasPremium;
		this.YourNoPremium.enabled = !hasPremium;
		for (int i = 0; i < this.NonPremiumTextElements.Count; i++)
		{
			this.NonPremiumTextElements[i].color = ((!hasPremium) ? Color.white : Color.gray);
		}
		for (int j = 0; j < this.PremiumTextElements.Count; j++)
		{
			this.PremiumTextElements[j].color = ((!hasPremium) ? Color.gray : this._originalPremiumColor);
		}
		for (int k = 0; k < this.PremiumImageElements.Count; k++)
		{
			this.PremiumImageElements[k].color = ((!hasPremium) ? Color.gray : this._originalPremiumColor);
		}
		TimeSpan value = TimeAndWeatherManager.CurrentTime.Value;
		this.NightRewindButton.SetActive((value.Hours >= 21 && value.Hours <= 24) || (value.Hours >= 0 && value.Hours < 5));
	}

	public void SkipCooldown()
	{
		if (PhotonConnectionFactory.Instance.Profile.GoldCoins < (double)PhotonConnectionFactory.Instance.RemoveTimeForwardCooldownCost)
		{
			this._messageBox = this.helpers.ShowMessage(base.gameObject.transform.root.gameObject, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("HaventMoney"), false, false, false, null);
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			this._messageBox.GetComponent<EventAction>().ActionCalled += this.RewindHoursHandler_CancelActionCalled;
		}
		else
		{
			this._messageBox = this.helpers.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("ConfirmSkipText"), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, true);
			this._messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.RewindHoursHandler_ConfirmActionCalled;
			this._messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.RewindHoursHandler_CancelActionCalled;
		}
	}

	private void RewindHoursHandler_CancelActionCalled(object sender, EventArgs e)
	{
		this._messageBox.Close();
	}

	private void RewindHoursHandler_ConfirmActionCalled(object sender, EventArgs e)
	{
		this._messageBox.SetInteractable(false);
		PhotonConnectionFactory.Instance.OnRemovedTimeForwardCooldown += this.Instance_OnRemovedTimeForwardCooldown;
		PhotonConnectionFactory.Instance.RemoveTimeForwardCooldown();
		base.StopCoroutine(this._timeCoroutine);
	}

	private void Instance_OnRemovedTimeForwardCooldown()
	{
		PhotonConnectionFactory.Instance.OnRemovedTimeForwardCooldown -= this.Instance_OnRemovedTimeForwardCooldown;
		this._messageBox.Close();
		this.CooldownPanel.GetComponent<AlphaFade>().HidePanel();
		this.CooldownPanel.GetComponent<AlphaFade>().CanvasGroup.interactable = false;
		this.ShowRewindPanel();
	}

	public void NightRewindCooldown()
	{
		this.CancelCooldown();
		this.StartChangeTime();
		this.HoursValue = -1;
	}

	public void CancelCooldown()
	{
		this.HideCooldown();
		this.ResumeGame(true);
	}

	private void HideCooldown()
	{
		GameFactory.ChatInGameController.SetActive(true);
		this.CooldownPanel.GetComponent<AlphaFade>().HidePanel();
		this.CooldownPanel.GetComponent<AlphaFade>().CanvasGroup.interactable = false;
		if (this._timeCoroutine != null)
		{
			base.StopCoroutine(this._timeCoroutine);
		}
	}

	private IEnumerator UpdateCooldownInfo(DateTime cooldown)
	{
		bool yieldedOnce = false;
		for (;;)
		{
			DateTime serverUtcNow = TimeHelper.UtcTime();
			TimeSpan time = cooldown - serverUtcNow;
			string timeString = string.Format("{0}:{1}:{2}", time.Hours.ToString("d2"), time.Minutes.ToString("d2"), time.Seconds.ToString("d2"));
			if (PhotonConnectionFactory.Instance.Profile.HasPremium)
			{
				TimeSpan halfTime = new TimeSpan((cooldown.Ticks - serverUtcNow.Ticks) * 2L);
				string halfTimeString = string.Format("{0}:{1}:{2}", halfTime.Hours.ToString("d2"), halfTime.Minutes.ToString("d2"), halfTime.Seconds.ToString("d2"));
				this.PremiumTime.text = timeString;
				this.NonPremiumTime.text = halfTimeString;
				if (time.TotalSeconds <= 0.0)
				{
					this.ShowRewindPanel();
					if (!yieldedOnce)
					{
						yield return null;
					}
					this.HideCooldown();
				}
			}
			else
			{
				TimeSpan doubleTime = new TimeSpan(time.Ticks / 2L);
				string doubleTimeString = string.Format("{0}:{1}:{2}", doubleTime.Hours.ToString("d2"), doubleTime.Minutes.ToString("d2"), doubleTime.Seconds.ToString("d2"));
				this.NonPremiumTime.text = timeString;
				this.PremiumTime.text = ((doubleTime.TotalSeconds > 0.0) ? doubleTimeString : "00:00:00");
				if (time.TotalSeconds <= 0.0)
				{
					this.ShowRewindPanel();
					if (!yieldedOnce)
					{
						yield return null;
					}
					this.HideCooldown();
				}
			}
			yield return new WaitForSecondsRealtime(1f);
			yieldedOnce = true;
		}
		yield break;
	}

	[SerializeField]
	private Text _hoursRetail;

	[SerializeField]
	private Text _helpTextRewindRetail;

	[SerializeField]
	private GameObject _retailRewindGo;

	[SerializeField]
	private GameObject _normalRewindGo;

	[SerializeField]
	private Text _helpTextRewind;

	public Text Hours;

	public GameObject RewindBackgroundPanel;

	public GameObject WaitRewindBackgroundPanel;

	public Text InfoText;

	public GameObject CooldownPanel;

	public GameObject NightRewindButton;

	public Text SkipButtonText;

	public List<Text> NonPremiumTextElements;

	public List<Text> PremiumTextElements;

	public List<Image> PremiumImageElements;

	public Text PremiumTime;

	public Text NonPremiumTime;

	public Text YourPremium;

	public Text YourNoPremium;

	public Button SkipCooldownButton;

	[HideInInspector]
	public int HoursValue = 1;

	private bool _changesTime;

	private DateTime currentTimeHourOffset;

	private Color _originalPremiumColor;

	private AlphaFade _rewindBackgroundAlphaFade;

	private AlphaFade _waitRewindBackgroundAlphaFade;

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox _messageBox;

	private const string HelpRetailCtrl = " <size=26><color=#FFEE44FF>{0}</color></size> ";

	private bool _alreadyRequested;

	private Coroutine _timeCoroutine;
}
