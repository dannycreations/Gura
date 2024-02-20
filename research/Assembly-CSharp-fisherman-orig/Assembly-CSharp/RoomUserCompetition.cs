using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using DG.Tweening;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomUserCompetition : ActivityStateControlled
{
	private bool HasTeam
	{
		get
		{
			return this.Competition != null && this.Competition.Format == UserCompetitionFormat.Team;
		}
	}

	private GameObject ItemPrefab
	{
		get
		{
			return (!this.HasTeam) ? this._itemPrefabSingle : this._itemsPrefabTeam;
		}
	}

	private void Awake()
	{
		this._defaultNameSharedMaterial = this._name.fontSharedMaterial;
		this._defaultNameColor = this._name.color;
		this._chatCaption.text = ScriptLocalization.Get("UGC_ChatRoomCaption").ToUpper();
		this._idTitle.text = ScriptLocalization.Get("UGC_IdNumber").ToUpper() + ":";
		this._pondTitle.text = ScriptLocalization.Get("TrophiesPondStatCaption").ToUpper();
		this._fishTitle.text = ScriptLocalization.Get("FishCaption").ToUpper() + ":";
		this._conditionsTitle.text = ScriptLocalization.Get("UGC_RulesetsTypeCaption").ToUpper() + ":";
		this._durationTitle.text = ScriptLocalization.Get("UGC_DurationCaption").ToUpper() + ":";
		this._startTimeTitle.text = ScriptLocalization.Get("StartsInStatus").ToUpper();
		this._prizeTitle.text = ScriptLocalization.Get("UGC_PrizeFundCaption").ToUpper();
		this._hostTitle.text = ScriptLocalization.Get("UGC_HostCaption").ToUpper();
		this._equipmentTitle.text = ScriptLocalization.Get("UGC_Equipment").ToUpper() + ":";
		this._ttTitle.text = ScriptLocalization.Get("UGC_Setups").ToUpper() + ":";
	}

	protected override void Start()
	{
		base.Start();
		this.ButtonsUpdate();
		this._sendChatMsgBtn.interactable = false;
		this._iF.onValueChanged.AddListener(delegate(string v)
		{
			this._sendChatMsgBtn.interactable = !string.IsNullOrEmpty(v);
		});
	}

	private void Update()
	{
		bool flag = base.ShouldUpdate();
		if (this.Competition != null)
		{
			if (this.Competition.SortType == UserCompetitionSortType.Automatic)
			{
				TimeSpan timeSpan = this.Competition.FixedStartDate.Value - TimeHelper.UtcTime();
				string formated = timeSpan.GetFormated(false, true);
				if (flag)
				{
					this._startTime.text = ((timeSpan.TotalSeconds > 10.0) ? formated : string.Format("<color=#FF0000FF>{0}</color>", formated));
				}
				if (timeSpan.TotalSeconds <= 60.0 && this.Competition.Format == UserCompetitionFormat.Team && UserCompetitionHelper.IsOwnerHost(this.Competition) && this._state == RoomUserCompetition.States.None && !this._isLockedAllPlayersInCompetitionTeam)
				{
					this._isLockedAllPlayersInCompetitionTeam = true;
					this.LockAllPlayersInCompetitionTeam();
				}
				if (timeSpan.TotalSeconds <= 10.0)
				{
					if (this._state != RoomUserCompetition.States.CountDown)
					{
						this.SetState(RoomUserCompetition.States.CountDown);
						this.PushWidgetTimer(this.Competition);
					}
					this.Coundown(timeSpan);
				}
			}
			else if (this._state == RoomUserCompetition.States.CountDown)
			{
				TimeSpan timeSpan2 = this.Competition.StartDate - TimeHelper.UtcTime();
				this.Coundown(timeSpan2);
				if (timeSpan2.TotalSeconds <= 0.0)
				{
					this.SetState(RoomUserCompetition.States.Start);
					bool flag2 = UserCompetitionHelper.IsOwnerHost(this.Competition);
					LogHelper.Log("___kocha StartCompetition TournamentId:{0} isHost:{1}", new object[]
					{
						this.Competition.TournamentId,
						flag2
					});
					if (flag2)
					{
						PhotonConnectionFactory.Instance.OnFailureStartCompetition += this.Instance_OnFailureStartCompetition;
						PhotonConnectionFactory.Instance.OnStartCompetition += this.Instance_OnStartCompetition;
						PhotonConnectionFactory.Instance.StartCompetition(this.Competition.TournamentId);
					}
				}
			}
		}
		if (flag && (Input.GetKeyDown(271) || Input.GetKeyDown(13)))
		{
			if (string.IsNullOrEmpty(this._iF.text))
			{
				this._iF.Select();
			}
			else
			{
				this.SendChatMsg();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.StopAllCoroutines();
	}

	protected override void SetHelp()
	{
		base.SetHelp();
		if (SettingsManager.InputType != InputModuleManager.InputType.Mouse)
		{
			UINavigation.SetSelectedGameObject(this._backBtn.gameObject);
		}
		PhotonConnectionFactory.Instance.OnUserCompetitionCancelled += this.Instance_OnUserCompetitionCancelled;
		PhotonConnectionFactory.Instance.OnUserCompetitionStarted += this.Instance_OnUserCompetitionStarted;
		if (this.Competition != null)
		{
			this.Init(this.Competition);
		}
	}

	protected override void HideHelp()
	{
		base.HideHelp();
		base.StopAllCoroutines();
		this._coroutine = null;
		this.Clear();
		this.UnsubscribeAutoArrangePlayersAndTeams();
		this.UnsubscribeOnUnregisterFromCompetition();
		this.UnsubscribeApproveParticipationInCompetition();
		this.UnsubscribeMovePlayerToCompetitionTeam();
		this.UnsubscribeLockPlayer();
		this.UnsubscribeGetUserCompetition();
		this.UnsubscribeRemovePlayer();
		this.UnsubscribeRequestStartCompetition();
		this.UnsubscribeRemoveCompetition();
		this.UnsubscribeStartCompetition();
		PhotonConnectionFactory.Instance.OnUserCompetitionCancelled -= this.Instance_OnUserCompetitionCancelled;
		PhotonConnectionFactory.Instance.OnUserCompetitionStarted -= this.Instance_OnUserCompetitionStarted;
	}

	protected UgcMenuStateManager MenuMgr
	{
		get
		{
			return MenuHelpers.Instance.MenuPrefabsList.UgcMenuManager;
		}
	}

	public int? TournamentId
	{
		get
		{
			return (this.Competition == null) ? null : new int?(this.Competition.TournamentId);
		}
	}

	public void ClearCurrent()
	{
		this.Competition = null;
		this.SetState(RoomUserCompetition.States.None);
		base.StopAllCoroutines();
		this._coroutine = null;
		this.UnsubscribeChat();
		this.UnsubscribeGetUserCompetition();
		this._timerValue = 0;
	}

	public void Init(UserCompetitionPublic competition)
	{
		UIHelper.Waiting(false, null);
		UIHelper.Waiting(true, null);
		if (UserCompetitionHelper.IsOwnerHost(competition))
		{
			base.StartCoroutine(this.HostPingServer(0.5f));
		}
		this.ClearChatMessages();
		this._deleteBtn.interactable = false;
		this.Clear();
		this._isLockedAllPlayersInCompetitionTeam = false;
		this._isKickedByHost = false;
		PhotonConnectionFactory.Instance.OnGetUserCompetition += this.Instance_OnGetUserCompetition;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetition += this.Instance_OnFailureGetUserCompetition;
		PhotonConnectionFactory.Instance.GetUserCompetition(competition.TournamentId);
	}

	public void OpenChat()
	{
		base.StartCoroutine(this.StartEdit());
	}

	public void AutoBalance()
	{
		if (this.Competition != null)
		{
			UIHelper.Waiting(true, null);
			PhotonConnectionFactory.Instance.OnAutoArrangePlayersAndTeams += this.Instance_OnAutoArrangePlayersAndTeams;
			PhotonConnectionFactory.Instance.OnFailureAutoArrangePlayersAndTeams += this.Instance_OnFailureAutoArrangePlayersAndTeams;
			PhotonConnectionFactory.Instance.AutoArrangePlayersAndTeams(this.Competition.TournamentId);
		}
	}

	public void Details()
	{
		if (this.Competition != null)
		{
			TournamentHelper.ShowingTournamentDetails(this.Competition, true, true, null, false);
		}
	}

	public void Leave()
	{
		if (this.Competition != null)
		{
			UIHelper.ShowYesNo(ScriptLocalization.Get("UGC_LeaveCompQuestionCaption"), delegate
			{
				UIHelper.Waiting(true, null);
				PhotonConnectionFactory.Instance.OnUnregisterFromCompetition += this.Instance_OnUnregisterFromCompetition;
				PhotonConnectionFactory.Instance.OnFailureUnregisterFromCompetition += this.Instance_OnFailureUnregisterFromCompetition;
				PhotonConnectionFactory.Instance.UnregisterFromCompetition(this.Competition.TournamentId);
			}, null, "YesCaption", null, "NoCaption", null, null, null);
		}
	}

	public void Back()
	{
		this.ClearCurrent();
		if (ScreenManager.Instance.GameScreen == GameScreenType.Tournaments)
		{
			this.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Room, false, false);
			this.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Sport, true, false);
		}
		else
		{
			this.MenuMgr.SetState(UgcMenuStateManager.UgcStates.Sport);
		}
	}

	public void Delete()
	{
		if (this.Competition != null)
		{
			UIHelper.ShowYesNo(ScriptLocalization.Get("UGC_DeleteCompQuestionCaption"), delegate
			{
				UIHelper.Waiting(true, null);
				PhotonConnectionFactory.Instance.OnRemoveCompetition += this.Instance_OnRemoveCompetition;
				PhotonConnectionFactory.Instance.OnFailureRemoveCompetition += this.Instance_OnFailureRemoveCompetition;
				PhotonConnectionFactory.Instance.RemoveCompetition(this.Competition.TournamentId, true);
			}, null, "YesCaption", null, "NoCaption", null, null, null);
		}
	}

	public void Ready(int flag)
	{
		if (this.Competition != null)
		{
			UIHelper.Waiting(true, null);
			PhotonConnectionFactory.Instance.OnFailureApproveParticipationInCompetition += this.Instance_OnFailureApproveParticipationInCompetition;
			PhotonConnectionFactory.Instance.OnApproveParticipationInCompetition += this.Instance_OnApproveParticipationInCompetition;
			PhotonConnectionFactory.Instance.ApproveParticipationInCompetition(this.Competition.TournamentId, flag == 1);
		}
	}

	public void StopCompetition()
	{
		if (this.Competition != null && this._state == RoomUserCompetition.States.CountDown)
		{
			this.SetState(RoomUserCompetition.States.RequestStartCancel);
			LogHelper.Log("___kocha start competition was canceled TournamentId:{0}", new object[] { this.Competition.TournamentId });
			PhotonConnectionFactory.Instance.OnFailureRequestStartCompetition += this.Instance_OnFailureRequestStartCompetition;
			PhotonConnectionFactory.Instance.OnRequestStartCompetition += this.Instance_OnRequestCancelCompetition;
			PhotonConnectionFactory.Instance.RequestStartCompetition(this.Competition.TournamentId, false);
		}
	}

	public void StartCompetition()
	{
		if (this.Competition != null)
		{
			if (StaticUserData.CurrentPond == null || StaticUserData.CurrentPond.PondId != this.Competition.PondId)
			{
				Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond p) => p.PondId == this.Competition.PondId);
				UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("UGC_CompetitionPlayerShouldBeOnPond"), UgcConsts.GetYellowTan(pond.Name)), TournamentCanceledInit.MessageTypes.Warning, null, false);
			}
			else
			{
				List<UserCompetitionPlayer> list = this.Competition.Players.FindAll((UserCompetitionPlayer p) => !p.IsApproved);
				if (list.Count == 0)
				{
					if (this.Competition.Players.Count < 2)
					{
						UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("UGC_CompetitionWrongTeamSingle"), TournamentCanceledInit.MessageTypes.Warning, null, false);
						return;
					}
					if (this.Competition.Format == UserCompetitionFormat.Team)
					{
						List<string> list2 = new List<string>();
						for (int i = 0; i < this.Competition.Players.Count; i++)
						{
							string text = this.Competition.Players[i].Team.ToUpper();
							if (!list2.Contains(text))
							{
								list2.Add(text);
							}
							if (list2.Count >= 2)
							{
								break;
							}
						}
						if (list2.Count < 2)
						{
							UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("UGC_CompetitionWrongTeam"), TournamentCanceledInit.MessageTypes.Warning, null, false);
							return;
						}
					}
					if (CompetitionMapController.Instance != null && !TournamentHelper.TOURNAMENT_ENDED_STILL_IN_ROOM && (TournamentHelper.IS_IN_TOURNAMENT || CompetitionMapController.Instance.IsJoinedToTournament))
					{
						ProfileTournament t = TournamentHelper.ProfileTournament;
						if (t != null)
						{
							string text2 = t.Name;
							if (t.IsUgc())
							{
								UserCompetitionPublic userCompetitionPublic = TournamentManager.Instance.UserGeneratedCompetitions.FirstOrDefault((UserCompetitionPublic p) => p.TournamentId == t.TournamentId);
								if (userCompetitionPublic != null)
								{
									text2 = UserCompetitionHelper.GetDefaultName(userCompetitionPublic);
								}
							}
							UIHelper.ShowYesNo(string.Format(ScriptLocalization.Get("UGC_AlreadyHaveActiveStartNewCaption"), UgcConsts.GetYellowTan(text2)), delegate
							{
								CompetitionMapController.Instance.FinishAccept();
								this.RequestStartCompetition();
							}, null, "YesCaption", null, "NoCaption", null, null, null);
							return;
						}
						LogHelper.Log("___kocha Ugc_Room: ResetJoined");
						CompetitionMapController.Instance.ResetJoined();
					}
					LogHelper.Log("___kocha Ugc_Room: RequestStartCompetition TournamentId:{0}", new object[] { this.Competition.TournamentId });
					this.RequestStartCompetition();
				}
				else
				{
					string text3 = string.Join(",", list.Select((UserCompetitionPlayer p) => p.Name).ToArray<string>());
					text3 = "<color=#FFEE44FF>" + text3 + "</color>";
					UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("UGC_PlayerUnapprovedCaption"), text3), TournamentCanceledInit.MessageTypes.Warning, null, false);
				}
			}
		}
	}

	public void SendChatMsg()
	{
		UserCompetitionPublic competition = this.Competition;
		string text = this._iF.text;
		if (competition != null && !string.IsNullOrEmpty(text))
		{
			this._iF.text = string.Empty;
			text = Regex.Replace(text, "<.*?>", string.Empty);
			PhotonConnectionFactory.Instance.SendChatMessageToChannel(competition.ChatChannelName(), text);
		}
	}

	private void Instance_OnIncomingChannelMessage(ChatMessage message)
	{
		if (this == null)
		{
			return;
		}
		if (this._isKickedByHost)
		{
			return;
		}
		UserCompetitionPublic competition = this.Competition;
		if (competition != null && message.Channel == competition.ChatChannelName() && !this._chatMessagesIds.Contains(message.Id))
		{
			string text = message.Message;
			UserCompetitionChatMessage userCompetitionChatMessage = UgcChatHelper.TryParseSpecialMsg(message);
			if (userCompetitionChatMessage != null)
			{
				if (userCompetitionChatMessage.UserCompetitionRuntime != null && userCompetitionChatMessage.UserCompetitionRuntime.TournamentId == competition.TournamentId)
				{
					competition.StartDate = userCompetitionChatMessage.UserCompetitionRuntime.StartDate;
					competition.EndDate = userCompetitionChatMessage.UserCompetitionRuntime.EndDate;
					competition.IsStartRequested = userCompetitionChatMessage.UserCompetitionRuntime.IsStartRequested;
					competition.IsStarted = userCompetitionChatMessage.UserCompetitionRuntime.IsStarted;
					competition.IsEnded = userCompetitionChatMessage.UserCompetitionRuntime.IsEnded;
					competition.IsCanceled = userCompetitionChatMessage.UserCompetitionRuntime.IsCanceled;
				}
				if (!UserCompetitionHelper.IsOwnerHost(competition))
				{
					UserCompetitionChatMessageType messageType = userCompetitionChatMessage.MessageType;
					if (messageType != UserCompetitionChatMessageType.UgcCompetitionStartRequested)
					{
						if (messageType != UserCompetitionChatMessageType.UgcCompetitionStartUnrequested)
						{
							if (messageType == UserCompetitionChatMessageType.UgcCompetitionRemoved)
							{
								LogHelper.Log("___kocha UgcCompetitionRemoved TournamentId:{0}", new object[] { competition.TournamentId });
								this.Back();
								return;
							}
						}
						else
						{
							this.SetState(RoomUserCompetition.States.None);
							this.ClearWidgetTimer();
						}
					}
					else
					{
						this.SetState(RoomUserCompetition.States.CountDown);
						this.PushWidgetTimer(competition);
					}
				}
				if (userCompetitionChatMessage.Players != null)
				{
					competition.Players = userCompetitionChatMessage.Players;
					competition.FillCompetitionPropertiesPlayers(PhotonConnectionFactory.Instance.Profile);
					this.UserCompetitionRefresh(competition);
					this.ButtonsUpdate();
				}
				if (!UgcChatHelper.HasLocalization(userCompetitionChatMessage.MessageType))
				{
					return;
				}
				text = UgcChatHelper.GetSpecialMsg(userCompetitionChatMessage);
			}
			GameObject gameObject = GUITools.AddChild(this._itemChatParent, this._itemChatPrefab);
			UgcChatItem component = gameObject.GetComponent<UgcChatItem>();
			component.Init(message, text);
			this._chatMessages.ForEach(delegate(UgcChatItem p)
			{
				p.SetAsReaded();
			});
			this._chatMessagesIds.Add(message.Id);
			this._chatMessages.Add(component);
			if (this._chatMessages.Count > 100)
			{
				this._chatMessages[0].Remove();
				this._chatMessages.RemoveAt(0);
				this._chatMessagesIds.RemoveAt(0);
			}
			if (this._coroutine != null)
			{
				base.StopCoroutine(this._coroutine);
			}
			this._coroutine = base.StartCoroutine(this.Scroll((this._chatScrollbar.direction != 2) ? 1f : 0f));
		}
	}

	private void ClearChatMessages()
	{
		this._chatMessages.ForEach(delegate(UgcChatItem p)
		{
			p.Remove();
		});
		this._chatMessages.Clear();
		this._chatMessagesIds.Clear();
	}

	private void UnsubscribeChat()
	{
		this.ClearChatMessages();
		UserCompetitionPublic competition = this.Competition;
		if (competition != null)
		{
			PhotonConnectionFactory.Instance.LeaveChatChannel(competition.ChatChannelName());
		}
		PhotonConnectionFactory.Instance.OnIncomingChannelMessage -= this.Instance_OnIncomingChannelMessage;
	}

	private IEnumerator Scroll(float v)
	{
		yield return new WaitForEndOfFrame();
		this._chatScrollRect.verticalNormalizedPosition = v;
		this._coroutine = null;
		UIAudioSourceListener.Instance.ChatMessage();
		yield break;
	}

	private void RequestStartCompetition()
	{
		this.SetState(RoomUserCompetition.States.RequestStart);
		PhotonConnectionFactory.Instance.OnFailureRequestStartCompetition += this.Instance_OnFailureRequestStartCompetition;
		PhotonConnectionFactory.Instance.OnRequestStartCompetition += this.Instance_OnRequestStartCompetition;
		PhotonConnectionFactory.Instance.RequestStartCompetition(this.Competition.TournamentId, true);
	}

	private void LockAllPlayersInCompetitionTeam()
	{
		if (PhotonConnectionFactory.Instance.Profile == null)
		{
			return;
		}
		LogHelper.Log("___kocha LockAllPlayersInCompetitionTeam");
		foreach (KeyValuePair<Guid, UgcRoomItemSingle> keyValuePair in this.Items)
		{
			if (keyValuePair.Key != PhotonConnectionFactory.Instance.Profile.UserId && !keyValuePair.Value.User.IsLocked)
			{
				PhotonConnectionFactory.Instance.LockPlayerInCompetitionTeam(this.Competition.TournamentId, keyValuePair.Key, true);
			}
		}
	}

	private void Instance_OnAutoArrangePlayersAndTeams(UserCompetitionPublic competition)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeAutoArrangePlayersAndTeams();
		this.UpdateCompetition(competition.Players);
	}

	private void Instance_OnUnregisterFromCompetition(UserCompetitionPublic competition)
	{
		if (TournamentManager.Instance != null)
		{
			TournamentManager.Instance.Refresh();
		}
		this.Clear();
		this.UnsubscribeOnUnregisterFromCompetition();
		UIHelper.Waiting(false, null);
		this.Back();
	}

	private IEnumerator HostPingServer(float time)
	{
		yield return new WaitForSeconds(time);
		if (this.Competition != null)
		{
			PhotonConnectionFactory.Instance.UpdateCompetitionLastActivityTime(this.Competition.TournamentId, true);
		}
		base.StartCoroutine(this.HostPingServer(20f));
		yield break;
	}

	private void Instance_OnRequestCancelCompetition(UserCompetition competition)
	{
		this.ClearWidgetTimer();
		this.SetState(RoomUserCompetition.States.None);
		this.UnsubscribeRequestStartCompetition();
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad && UserCompetitionHelper.IsOwnerHost(this.Competition))
		{
			UINavigation.SetSelectedGameObject(this._startBtn.gameObject);
		}
	}

	private void Instance_OnRequestStartCompetition(UserCompetition competition)
	{
		this.UnsubscribeRequestStartCompetition();
		if (competition.FixedStartDate != null)
		{
			this.Instance_OnStartCompetition(competition);
			return;
		}
		this.Competition = competition;
		this.InitCountDown();
	}

	private void InitCountDown()
	{
		this.SetState(RoomUserCompetition.States.CountDown);
		this.PushWidgetTimer(this.Competition);
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad && UserCompetitionHelper.IsOwnerHost(this.Competition))
		{
			UINavigation.SetSelectedGameObject(this._stopBtn.gameObject);
		}
	}

	private void PushWidgetTimer(UserCompetitionPublic competition)
	{
		HintSystem instance = HintSystem.Instance;
		MissionWidgetManager.TimerWidget timerWidget = new MissionWidgetManager.TimerWidget();
		timerWidget.Image = competition.ImageBID;
		timerWidget.Name = UserCompetitionHelper.GetDefaultName(competition);
		MissionWidgetManager.TimerWidget timerWidget2 = timerWidget;
		DateTime? fixedStartDate = competition.FixedStartDate;
		timerWidget2.Time = ((fixedStartDate == null) ? competition.StartDate : fixedStartDate.Value);
		instance.PushWidgetTimer(timerWidget);
	}

	private void ClearWidgetTimer()
	{
		this._timerValue = 0;
		HintSystem.Instance.ClearWidgetTimer();
	}

	private void Coundown(TimeSpan diff)
	{
		int num = (int)diff.TotalSeconds;
		if (this._timerValue != num)
		{
			this._timerValue = num;
			if (this._timerValue >= 0)
			{
				UIAudioSourceListener.Instance.Coundown();
			}
		}
	}

	private void Instance_OnStartCompetition(UserCompetition competition)
	{
		this.UnsubscribeStartCompetition();
		UIAudioSourceListener.Instance.SportHorn();
		this.Back();
		this._state = RoomUserCompetition.States.None;
		if (MenuHelpers.Instance.IsInMenu)
		{
			DashboardTabSetter.SwitchForms(FormsEnum.LocalMap, true);
		}
	}

	private void Instance_OnRemoveCompetition()
	{
		if (TournamentManager.Instance != null)
		{
			TournamentManager.Instance.Refresh();
		}
		this.Clear();
		this.UnsubscribeRemoveCompetition();
		UIHelper.Waiting(false, null);
		UIAudioSourceListener.Instance.Successfull();
		string text = ((!this.Competition.IsSponsored || string.IsNullOrEmpty(this.Competition.NameCustom)) ? UserCompetitionHelper.GetDefaultName(this.Competition) : this.Competition.NameCustom);
		UIHelper.ShowCanceledMsg(ScriptLocalization.Get("UGC_SuccessfulAction"), string.Format(ScriptLocalization.Get("UGC_DeleteCompSuccessfulCaption"), UgcConsts.GetYellowTan(text)), TournamentCanceledInit.MessageTypes.Ok, new Action(this.Back), false);
	}

	private void Instance_OnGetUserCompetition(UserCompetitionPublic competition)
	{
		UIHelper.Waiting(false, null);
		if (competition.IsStarted || competition.IsEnded || competition.IsCanceled)
		{
			this.Back();
			return;
		}
		PhotonConnectionFactory.Instance.JoinChatChannel(competition.ChatChannelName());
		PhotonConnectionFactory.Instance.OnIncomingChannelMessage += this.Instance_OnIncomingChannelMessage;
		this.UnsubscribeGetUserCompetition();
		this.UpdateCompetition(competition);
		if (competition.SortType == UserCompetitionSortType.Manual)
		{
			this._startTime.text = ScriptLocalization.Get("UGC_ManualStart");
		}
		bool flag = UserCompetitionHelper.IsOwnerHost(competition);
		if (flag)
		{
			if (!competition.Players.Any((UserCompetitionPlayer p) => PhotonConnectionFactory.Instance.Profile.UserId == p.UserId && p.IsApproved))
			{
				this.Ready(1);
			}
		}
		if (competition.IsStartRequested)
		{
			if (flag && competition.SortType != UserCompetitionSortType.Automatic)
			{
				this.InitCountDown();
			}
			return;
		}
		this.ButtonsUpdate();
	}

	private void UpdateCompetition(UserCompetitionPublic competition)
	{
		this.Competition = competition;
		this._id.text = this.Competition.TournamentId.ToString();
		Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond p) => p.PondId == this.Competition.PondId);
		this._pond.text = ((pond == null) ? string.Empty : pond.Name);
		if (pond != null && this._imagePond != null)
		{
			this.ImagePondLdbl.Load(new int?(pond.PhotoBID), this._imagePond, "Textures/Inventory/{0}");
		}
		this._entrySum.text = string.Format("{0}", UserCompetitionHelper.PrizePoolWithComission(this.Competition));
		this._entryCurrency.text = MeasuringSystemManager.GetCurrencyIcon(competition.Currency);
		this._tax.text = UserCompetitionHelper.FeeCommissionLoc;
		TMP_Text fish2 = this._fish;
		string text;
		if (competition.FishScore != null)
		{
			text = UserCompetitionHelper.JoinArrayAndLocalize<FishBrief>(UserCompetitionHelper.GetFishBriefs(competition.FishScore).ToArray<FishBrief>(), (FishBrief fish) => fish.Name, false);
		}
		else
		{
			text = null;
		}
		fish2.text = text;
		this._conditions.text = string.Format("{0}{1}", UgcConsts.GetScoreTypeLoc(competition.ScoringType).Name, UgcConsts.GetGrey(string.Format("({0})", UgcConsts.GetCompetitionScoringLoc(competition.FishSource).Name)));
		this._duration.text = UserCompetitionHelper.GetDurationText(this.Competition, true);
		if (this.Competition.ImageBID != null)
		{
			this.ImageLdbl.Load(this.Competition.ImageBID, this._image, "Textures/Inventory/{0}");
		}
		if (!this.Competition.IsSponsored)
		{
			this._name.fontSharedMaterial = this._defaultNameSharedMaterial;
			this._name.color = this._defaultNameColor;
		}
		UserCompetitionHelper.GetDefaultName(this._name, this.Competition);
		bool hasTeam = this.HasTeam;
		GameObject prefab = this.ItemPrefab;
		bool isOwnerHost = UserCompetitionHelper.IsOwnerHost(this.Competition);
		this.Competition.Players.ForEach(delegate(UserCompetitionPlayer p)
		{
			this.AddPlayer(p, prefab, hasTeam, isOwnerHost, UserCompetitionHelper.IsPlayerHost(this.Competition, p.UserId));
		});
		this.UpdatePlayersCount(this.Competition.Players, hasTeam);
		this._singleGo.SetActive(!hasTeam);
		this._teamGo.SetActive(hasTeam);
		this._host.text = this.Competition.HostName;
		this._equipment.text = UserCompetitionHelper.GetLoc<UserCompetitionEquipmentAllowed>(this.Competition.DollEquipment, (UserCompetitionEquipmentAllowed eq) => UgcConsts.GetEquipmentAllowedLoc(eq).Name);
		if (this.Competition.TournamentEquipment != null)
		{
			List<ItemSubTypes> list = UserCompetitionHelper.TournamentEquipment2List(this.Competition.TournamentEquipment);
			this._tackleTypes.text = UserCompetitionHelper.GetLoc<ItemSubTypes>(list.ToArray(), (ItemSubTypes v) => ItemSubTypesLocalization.Localize(v, false));
		}
		else
		{
			this._tackleTypes.text = "-";
		}
		this.ButtonsUpdate();
		this.UpdateHelpText(isOwnerHost, hasTeam);
	}

	private void UpdateHelpText(bool isOwnerHost, bool hasTeam)
	{
		object obj = ((!isOwnerHost || !hasTeam) ? null : string.Format(ScriptLocalization.Get("UGC_ChangePlayerTeamHint"), string.Format("<color=#FFDD77FF>{0}</color>", "[Left Mouse Button]")));
	}

	private void AddPlayer(UserCompetitionPlayer p, GameObject prefab, bool hasTeam, bool isHost, bool isPlayerHost)
	{
		if (this.Items.ContainsKey(p.UserId))
		{
			return;
		}
		GameObject gameObject = ((!hasTeam) ? this._itemsParentSingle : ((!(p.Team == "Red")) ? this._itemParentTeamBlue : this._itemParentTeamRed));
		GameObject gameObject2 = GUITools.AddChild(gameObject, prefab);
		UgcRoomItemSingle component = gameObject2.GetComponent<UgcRoomItemSingle>();
		component.Init(p, p.UserId == PhotonConnectionFactory.Instance.Profile.UserId, this._itemsToggleGroup, isHost, isPlayerHost);
		this.Items.Add(p.UserId, component);
		if (isHost)
		{
			component.OnDelete += delegate
			{
				UIHelper.Waiting(true, null);
				PhotonConnectionFactory.Instance.OnRemovePlayerFromCompetition += this.Instance_OnRemovePlayerFromCompetition;
				PhotonConnectionFactory.Instance.OnFailureRemovePlayerFromCompetition += this.Instance_OnFailureRemovePlayerFromCompetition;
				PhotonConnectionFactory.Instance.RemovePlayerFromCompetition(this.Competition.TournamentId, p.UserId);
			};
			if (hasTeam)
			{
				Action<List<UgcRoomItemTeam>, UgcRoomItemTeam> checkNavigation = delegate(List<UgcRoomItemTeam> list, UgcRoomItemTeam go)
				{
					bool flag = !list.Any((UgcRoomItemTeam pp) => pp.IsMenuActive);
					if (flag != this._nav.enabled)
					{
						this._nav.enabled = flag;
						if (this._nav.enabled && EventSystem.current.currentSelectedGameObject == null)
						{
							UINavigation.SetSelectedGameObject(go.gameObject);
						}
					}
				};
				UgcRoomItemTeam teamEl = (UgcRoomItemTeam)component;
				teamEl.OnLock += delegate(bool isLock)
				{
					UIHelper.Waiting(true, null);
					PhotonConnectionFactory.Instance.OnLockPlayerInCompetitionTeam += this.Instance_OnLockPlayerInCompetitionTeam;
					PhotonConnectionFactory.Instance.OnFailureLockPlayerInCompetitionTeam += this.Instance_OnFailureLockPlayerInCompetitionTeam;
					PhotonConnectionFactory.Instance.LockPlayerInCompetitionTeam(this.Competition.TournamentId, p.UserId, isLock);
				};
				teamEl.OnMenuOpen += delegate
				{
					foreach (KeyValuePair<Guid, UgcRoomItemSingle> keyValuePair in this.Items)
					{
						if (keyValuePair.Key != p.UserId)
						{
							((UgcRoomItemTeam)keyValuePair.Value).CloseMenu();
						}
					}
					checkNavigation(this.Items.Values.Select((UgcRoomItemSingle elem) => (UgcRoomItemTeam)elem).ToList<UgcRoomItemTeam>(), teamEl);
				};
				teamEl.OnMenuClose += delegate
				{
					checkNavigation(this.Items.Values.Select((UgcRoomItemSingle elem) => (UgcRoomItemTeam)elem).ToList<UgcRoomItemTeam>(), teamEl);
				};
			}
		}
		if (hasTeam)
		{
			((UgcRoomItemTeam)component).OnDblClick += delegate
			{
				this._lastPlayerActionUserId = p.UserId;
				if (isHost)
				{
					UIHelper.Waiting(true, null);
					PhotonConnectionFactory.Instance.OnFailureMovePlayerToCompetitionTeam += this.Instance_OnFailureMovePlayerToCompetitionTeam;
					PhotonConnectionFactory.Instance.OnMovePlayerToCompetitionTeam += this.Instance_OnMovePlayerToCompetitionTeam;
					PhotonConnectionFactory.Instance.MovePlayerToCompetitionTeam(this.Competition.TournamentId, p.UserId, (!(p.Team == "Red")) ? "Red" : "Blue");
				}
				else
				{
					UIHelper.Waiting(true, null);
					PhotonConnectionFactory.Instance.OnFailureMoveToCompetitionTeam += this.Instance_OnFailureMoveToCompetitionTeam;
					PhotonConnectionFactory.Instance.OnMoveToCompetitionTeam += this.Instance_OnMoveToCompetitionTeam;
					PhotonConnectionFactory.Instance.MoveToCompetitionTeam(this.Competition.TournamentId, (!(p.Team == "Red")) ? "Red" : "Blue");
				}
			};
		}
	}

	private void Instance_OnUserCompetitionCancelled(UserCompetitionCancellationMessage c)
	{
		this.LeaveRoom(c.TournamentId);
	}

	private void Instance_OnUserCompetitionStarted(UserCompetitionStartMessage cancellation)
	{
		UIAudioSourceListener.Instance.SportHorn();
		this.LeaveRoom(cancellation.TournamentId);
	}

	private void LeaveRoom(int tournamentId)
	{
		UserCompetitionPublic competition = this.Competition;
		LogHelper.Log("___kocha LeaveRoom tournamentId:{0} t.TournamentId:{1}", new object[]
		{
			tournamentId,
			(competition == null) ? (-1) : competition.TournamentId
		});
		if (competition != null && competition.TournamentId == tournamentId)
		{
			this.Back();
			if (MenuHelpers.Instance.IsInMenu)
			{
				DashboardTabSetter.SwitchForms(FormsEnum.LocalMap, false);
			}
		}
	}

	private void Instance_OnMoveToCompetitionTeam(UserCompetitionPublic competition)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeMoveToCompetitionTeam();
		this.Competition = competition;
		this.UpdateCompetition(competition.Players);
		this.SelectLastActionPlayer();
	}

	private void UpdateCompetition(List<UserCompetitionPlayer> players)
	{
		bool isOwnerHost = UserCompetitionHelper.IsOwnerHost(this.Competition);
		bool hasTeam = this.HasTeam;
		GameObject prefab = this.ItemPrefab;
		List<Guid> active = new List<Guid>();
		players.ForEach(delegate(UserCompetitionPlayer p)
		{
			active.Add(p.UserId);
			UgcRoomItemSingle ugcRoomItemSingle = ((!this.Items.ContainsKey(p.UserId)) ? null : this.Items[p.UserId]);
			if (ugcRoomItemSingle != null)
			{
				if (hasTeam && ugcRoomItemSingle.User.Team != p.Team)
				{
					this.Items[p.UserId].Remove();
					this.Items.Remove(p.UserId);
					this.AddPlayer(p, prefab, true, isOwnerHost, UserCompetitionHelper.IsPlayerHost(this.Competition, p.UserId));
				}
				else
				{
					this.Items[p.UserId].UpdateData(p);
				}
			}
			else
			{
				this.AddPlayer(p, prefab, hasTeam, isOwnerHost, UserCompetitionHelper.IsPlayerHost(this.Competition, p.UserId));
			}
		});
		List<Guid> list = this.Items.Keys.Where((Guid p) => !active.Contains(p)).ToList<Guid>();
		list.ForEach(delegate(Guid p)
		{
			this.Items[p].Remove();
			this.Items.Remove(p);
		});
		this.UpdatePlayersCount(players, hasTeam);
		this.UpdateHelpText(isOwnerHost, hasTeam);
		this._entrySum.text = string.Format("{0}", UserCompetitionHelper.PrizePoolWithComission(this.Competition));
	}

	private void UpdatePlayersCount(List<UserCompetitionPlayer> players, bool hasTeam)
	{
		if (hasTeam)
		{
			this.SetTeamPlayerCount(this._playersTeamRed, "Red", players);
			this.SetTeamPlayerCount(this._playersTeamBlue, "Blue", players);
		}
		this._playersSingle.text = string.Format("{0} <color=#E3E3E3FF>({1})</color>", ScriptLocalization.Get("UGC_PlayersCaption"), string.Format("{0}/{1}", players.Count, this.Competition.MaxParticipants));
	}

	private void SetTeamPlayerCount(TextMeshProUGUI value, string team, List<UserCompetitionPlayer> players)
	{
		value.text = string.Format("<color=#{0}>{1}</color> {2}", ColorUtility.ToHtmlStringRGBA(UgcConsts.GetTeamColor(team)), UgcConsts.GetTeamLoc(team), UgcConsts.GetGrey(players.Count((UserCompetitionPlayer p) => p.Team == team).ToString()));
	}

	private void Instance_OnMovePlayerToCompetitionTeam(UserCompetitionPublic competition)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeMovePlayerToCompetitionTeam();
		this.Competition = competition;
		this.UpdateCompetition(competition.Players);
		this.SelectLastActionPlayer();
	}

	private void SelectLastActionPlayer()
	{
		if (!this._lastPlayerActionUserId.Equals(Guid.Empty) && this.Items.ContainsKey(this._lastPlayerActionUserId))
		{
			UINavigation.SetSelectedGameObject(this.Items[this._lastPlayerActionUserId].gameObject);
			this._lastPlayerActionUserId = Guid.Empty;
		}
	}

	private void Instance_OnLockPlayerInCompetitionTeam(UserCompetitionPublic competition)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeLockPlayer();
		this.Competition = competition;
		this.UpdateCompetition(competition.Players);
		if (this._isLockedAllPlayersInCompetitionTeam)
		{
			this._isLockedAllPlayersInCompetitionTeam = false;
		}
	}

	private void Instance_OnRemovePlayerFromCompetition(UserCompetitionPublic competition)
	{
		this.UnsubscribeRemovePlayer();
		this.Competition = competition;
		this.UpdateCompetition(competition.Players);
		this.ButtonsUpdate();
		UIHelper.Waiting(false, null);
		List<UgcRoomItemSingle> list = this.Items.Values.ToList<UgcRoomItemSingle>();
		if (list.Count > 0)
		{
			UINavigation.SetSelectedGameObject(list[0].gameObject);
		}
	}

	private void Instance_OnApproveParticipationInCompetition(UserCompetitionPublic competition)
	{
		this.UnsubscribeApproveParticipationInCompetition();
		this.Competition = competition;
		this.UpdateCompetition(competition.Players);
		this.ButtonsUpdate();
		UIHelper.Waiting(false, null);
	}

	private void TurnOffButtons()
	{
		this._deleteBtn.gameObject.SetActive(false);
		this._startBtn.gameObject.SetActive(false);
		this._readyBtn.gameObject.SetActive(false);
		this._unreadyBtn.gameObject.SetActive(false);
		this._leaveBtn.gameObject.SetActive(false);
		this._autoBalanceBtn.gameObject.SetActive(false);
		this._stopBtn.gameObject.SetActive(false);
	}

	private void ButtonsUpdate()
	{
		if (this.Competition == null)
		{
			this.TurnOffButtons();
		}
		else
		{
			UserCompetitionPlayer userCompetitionPlayer = this.Competition.Players.FirstOrDefault((UserCompetitionPlayer p) => p.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
			if (userCompetitionPlayer == null)
			{
				this.TurnOffButtons();
				return;
			}
			bool flag = UserCompetitionHelper.IsOwnerHost(this.Competition);
			bool flag2 = this.Competition.Format == UserCompetitionFormat.Team;
			bool flag3 = this.Competition.SortType == UserCompetitionSortType.Automatic;
			this._deleteBtn.gameObject.SetActive(flag);
			this._deleteBtn.interactable = this._deleteBtn.gameObject.activeSelf && this._state == RoomUserCompetition.States.None;
			bool isApproved = userCompetitionPlayer.IsApproved;
			this._startBtn.gameObject.SetActive(flag && isApproved && !flag3);
			this._startBtn.interactable = this._startBtn.gameObject.activeSelf;
			this._autoBalanceBtn.gameObject.SetActive(flag && flag2);
			this._autoBalanceBtn.interactable = this._autoBalanceBtn.gameObject.activeSelf && this._state == RoomUserCompetition.States.None && this.Competition.Players.Count > 1;
			this._stopBtn.gameObject.SetActive(false);
			this._stopBtn.interactable = false;
			this._leaveBtn.gameObject.SetActive(!flag);
			this._readyBtn.gameObject.SetActive(!flag && !isApproved);
			this._unreadyBtn.gameObject.SetActive(!flag && isApproved);
			this._leaveBtn.interactable = this._leaveBtn.gameObject.activeSelf && this._state == RoomUserCompetition.States.None;
			this._readyBtn.interactable = this._readyBtn.gameObject.activeSelf && this._state == RoomUserCompetition.States.None;
			this._unreadyBtn.interactable = this._unreadyBtn.gameObject.activeSelf && this._state == RoomUserCompetition.States.None;
			this._backBtn.interactable = this._state == RoomUserCompetition.States.None;
			LogHelper.Log("___kocha ButtonsUpdate _state:{0}", new object[] { this._state });
			if (flag2)
			{
				for (int i = 0; i < this._disabledPlayersTeam.Length; i++)
				{
					this.SetVisibilityBgUnderPlayers(this._disabledPlayersTeam[i], this._state != RoomUserCompetition.States.None);
				}
			}
			else
			{
				this.SetVisibilityBgUnderPlayers(this._disabledPlayersSingle, this._state != RoomUserCompetition.States.None);
			}
			this.Items.Values.ToList<UgcRoomItemSingle>().ForEach(delegate(UgcRoomItemSingle p)
			{
				p.SetInteractable(this._state == RoomUserCompetition.States.None);
			});
			if (!flag3 && flag)
			{
				switch (this._state)
				{
				case RoomUserCompetition.States.RequestStart:
					this._startBtn.interactable = false;
					break;
				case RoomUserCompetition.States.RequestStartCancel:
				case RoomUserCompetition.States.Start:
					this._stopBtn.interactable = false;
					break;
				case RoomUserCompetition.States.CountDown:
					this._startBtn.gameObject.SetActive(false);
					this._stopBtn.gameObject.SetActive(true);
					this._stopBtn.interactable = true;
					break;
				}
			}
		}
		for (int j = 0; j < this._autoBalanceGo.Length; j++)
		{
			this._autoBalanceGo[j].SetActive(this._autoBalanceBtn.gameObject.activeSelf);
		}
	}

	private void SetState(RoomUserCompetition.States s)
	{
		this._state = s;
		this.ButtonsUpdate();
	}

	private void SetVisibilityBgUnderPlayers(Image im, bool isActive)
	{
		if (isActive)
		{
			ShortcutExtensions.DOFade(im, 0f, 0f);
			im.gameObject.SetActive(true);
			ShortcutExtensions.DOFade(im, 0.7f, 0.4f);
		}
		else
		{
			TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOFade(im, 0f, 0.4f), delegate
			{
				im.gameObject.SetActive(false);
			});
		}
	}

	private void Clear()
	{
		this.Items.Values.ToList<UgcRoomItemSingle>().ForEach(delegate(UgcRoomItemSingle p)
		{
			p.Remove();
		});
		this.Items.Clear();
	}

	private void UserCompetitionRefresh(UserCompetitionPublic competition)
	{
		if (this._isKickedByHost)
		{
			return;
		}
		this.Competition = competition;
		if (competition != null)
		{
			if (competition.Players.FirstOrDefault((UserCompetitionPlayer p) => p.UserId == PhotonConnectionFactory.Instance.Profile.UserId) == null)
			{
				this._isKickedByHost = true;
				LogHelper.Log("___kocha UGC_KickedByHost");
				this.UpdateCompetition(competition.Players);
				return;
			}
		}
		if (competition == null)
		{
			this.Clear();
			this.Back();
			return;
		}
		this.UpdateCompetition(competition.Players);
	}

	private void CheckCompetitionNotFound(UserCompetitionFailure failure, bool hideWaiting = false)
	{
		UserCompetitionFailureHandler.Fire(failure, delegate
		{
			if (failure.UserCompetitionErrorCode == 16)
			{
				this.Back();
			}
		}, hideWaiting);
	}

	private IEnumerator StartEdit()
	{
		yield return new WaitForEndOfFrame();
		this._iF.Select();
		this._iF.ActivateInputField();
		this._iF.MoveTextEnd(true);
		yield break;
	}

	private void TrySearch()
	{
		this.SendChatMsg();
	}

	private void Instance_OnFailureStartCompetition(UserCompetitionFailure failure)
	{
		this.SetState(RoomUserCompetition.States.None);
		this.UnsubscribeRequestStartCompetition();
		this.CheckCompetitionNotFound(failure, false);
	}

	private void Instance_OnFailureRequestStartCompetition(UserCompetitionFailure failure)
	{
		this.SetState(RoomUserCompetition.States.None);
		this.UnsubscribeRequestStartCompetition();
		this.CheckCompetitionNotFound(failure, false);
	}

	private void Instance_OnFailureGetUserCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeGetUserCompetition();
		UserCompetitionFailureHandler.Fire(failure, null, true);
	}

	private void Instance_OnFailureRemovePlayerFromCompetition(UserCompetitionFailure failure)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeRemovePlayer();
		this.CheckCompetitionNotFound(failure, false);
	}

	private void Instance_OnFailureLockPlayerInCompetitionTeam(UserCompetitionFailure failure)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeLockPlayer();
		this.CheckCompetitionNotFound(failure, false);
	}

	private void Instance_OnFailureMovePlayerToCompetitionTeam(UserCompetitionFailure failure)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeMovePlayerToCompetitionTeam();
		this.CheckCompetitionNotFound(failure, false);
	}

	private void Instance_OnFailureApproveParticipationInCompetition(UserCompetitionFailure failure)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeApproveParticipationInCompetition();
		this.CheckCompetitionNotFound(failure, false);
	}

	private void Instance_OnFailureMoveToCompetitionTeam(UserCompetitionFailure failure)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeMoveToCompetitionTeam();
		this.CheckCompetitionNotFound(failure, false);
	}

	private void Instance_OnFailureRemoveCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeRemoveCompetition();
		UserCompetitionFailureHandler.Fire(failure, null, true);
	}

	private void Instance_OnFailureUnregisterFromCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeOnUnregisterFromCompetition();
		UserCompetitionFailureHandler.Fire(failure, null, true);
	}

	private void Instance_OnFailureAutoArrangePlayersAndTeams(UserCompetitionFailure failure)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeAutoArrangePlayersAndTeams();
		this.CheckCompetitionNotFound(failure, false);
	}

	private void UnsubscribeAutoArrangePlayersAndTeams()
	{
		PhotonConnectionFactory.Instance.OnAutoArrangePlayersAndTeams -= this.Instance_OnAutoArrangePlayersAndTeams;
		PhotonConnectionFactory.Instance.OnFailureAutoArrangePlayersAndTeams -= this.Instance_OnFailureAutoArrangePlayersAndTeams;
	}

	private void UnsubscribeMovePlayerToCompetitionTeam()
	{
		PhotonConnectionFactory.Instance.OnFailureMovePlayerToCompetitionTeam -= this.Instance_OnFailureMovePlayerToCompetitionTeam;
		PhotonConnectionFactory.Instance.OnMovePlayerToCompetitionTeam -= this.Instance_OnMovePlayerToCompetitionTeam;
	}

	private void UnsubscribeLockPlayer()
	{
		PhotonConnectionFactory.Instance.OnLockPlayerInCompetitionTeam -= this.Instance_OnLockPlayerInCompetitionTeam;
		PhotonConnectionFactory.Instance.OnFailureLockPlayerInCompetitionTeam -= this.Instance_OnFailureLockPlayerInCompetitionTeam;
	}

	private void UnsubscribeRemovePlayer()
	{
		PhotonConnectionFactory.Instance.OnRemovePlayerFromCompetition -= this.Instance_OnRemovePlayerFromCompetition;
		PhotonConnectionFactory.Instance.OnFailureRemovePlayerFromCompetition -= this.Instance_OnFailureRemovePlayerFromCompetition;
	}

	private void UnsubscribeGetUserCompetition()
	{
		PhotonConnectionFactory.Instance.OnGetUserCompetition -= this.Instance_OnGetUserCompetition;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetition -= this.Instance_OnFailureGetUserCompetition;
	}

	private void UnsubscribeApproveParticipationInCompetition()
	{
		PhotonConnectionFactory.Instance.OnFailureApproveParticipationInCompetition -= this.Instance_OnFailureApproveParticipationInCompetition;
		PhotonConnectionFactory.Instance.OnApproveParticipationInCompetition -= this.Instance_OnApproveParticipationInCompetition;
	}

	private void UnsubscribeMoveToCompetitionTeam()
	{
		PhotonConnectionFactory.Instance.OnFailureMoveToCompetitionTeam -= this.Instance_OnFailureMoveToCompetitionTeam;
		PhotonConnectionFactory.Instance.OnMoveToCompetitionTeam -= this.Instance_OnMoveToCompetitionTeam;
	}

	private void UnsubscribeRequestStartCompetition()
	{
		PhotonConnectionFactory.Instance.OnRequestStartCompetition -= this.Instance_OnRequestCancelCompetition;
		PhotonConnectionFactory.Instance.OnFailureRequestStartCompetition -= this.Instance_OnFailureRequestStartCompetition;
		PhotonConnectionFactory.Instance.OnRequestStartCompetition -= this.Instance_OnRequestStartCompetition;
	}

	private void UnsubscribeStartCompetition()
	{
		PhotonConnectionFactory.Instance.OnFailureStartCompetition -= this.Instance_OnFailureStartCompetition;
		PhotonConnectionFactory.Instance.OnStartCompetition -= this.Instance_OnStartCompetition;
	}

	private void UnsubscribeRemoveCompetition()
	{
		PhotonConnectionFactory.Instance.OnRemoveCompetition -= this.Instance_OnRemoveCompetition;
		PhotonConnectionFactory.Instance.OnFailureRemoveCompetition -= this.Instance_OnFailureRemoveCompetition;
	}

	private void UnsubscribeOnUnregisterFromCompetition()
	{
		PhotonConnectionFactory.Instance.OnUnregisterFromCompetition -= this.Instance_OnUnregisterFromCompetition;
		PhotonConnectionFactory.Instance.OnFailureUnregisterFromCompetition -= this.Instance_OnFailureUnregisterFromCompetition;
	}

	[SerializeField]
	private Image _disabledPlayersSingle;

	[SerializeField]
	private Image[] _disabledPlayersTeam;

	[Space(8f)]
	[SerializeField]
	private GameObject _itemChatPrefab;

	[SerializeField]
	private GameObject _itemChatParent;

	[SerializeField]
	private BorderedButton _sendChatMsgBtn;

	[SerializeField]
	private InputField _iF;

	[SerializeField]
	private ScreenKeyboard _searchFieldScreenKeyboard;

	[SerializeField]
	private Scrollbar _chatScrollbar;

	[SerializeField]
	private ScrollRect _chatScrollRect;

	[Space(8f)]
	[SerializeField]
	private ToggleGroup _itemsToggleGroup;

	[SerializeField]
	private GameObject _itemPrefabSingle;

	[SerializeField]
	private GameObject _itemsParentSingle;

	[SerializeField]
	private GameObject _itemParentTeamRed;

	[SerializeField]
	private GameObject _itemParentTeamBlue;

	[SerializeField]
	private GameObject _itemsPrefabTeam;

	[SerializeField]
	private GameObject _singleGo;

	[SerializeField]
	private GameObject _teamGo;

	[SerializeField]
	private TextMeshProUGUI _playersSingle;

	[SerializeField]
	private TextMeshProUGUI _playersTeamRed;

	[SerializeField]
	private TextMeshProUGUI _playersTeamBlue;

	[SerializeField]
	private BorderedButton _backBtn;

	[SerializeField]
	private BorderedButton _leaveBtn;

	[SerializeField]
	private BorderedButton _deleteBtn;

	[SerializeField]
	private BorderedButton _startBtn;

	[SerializeField]
	private BorderedButton _stopBtn;

	[SerializeField]
	private BorderedButton _readyBtn;

	[SerializeField]
	private BorderedButton _unreadyBtn;

	[SerializeField]
	private BorderedButton _autoBalanceBtn;

	[SerializeField]
	private GameObject[] _autoBalanceGo;

	[SerializeField]
	private Image _image;

	[SerializeField]
	private TextMeshProUGUI _name;

	[SerializeField]
	private TextMeshProUGUI _pond;

	[SerializeField]
	private Image _imagePond;

	[SerializeField]
	private TextMeshProUGUI _id;

	[SerializeField]
	private TextMeshProUGUI _fish;

	[SerializeField]
	private TextMeshProUGUI _conditions;

	[SerializeField]
	private TextMeshProUGUI _duration;

	[SerializeField]
	private TextMeshProUGUI _startTime;

	[SerializeField]
	private TextMeshProUGUI _entrySum;

	[SerializeField]
	private TextMeshProUGUI _entryCurrency;

	[SerializeField]
	private TextMeshProUGUI _tax;

	[SerializeField]
	private TextMeshProUGUI _host;

	[SerializeField]
	private TextMeshProUGUI _equipment;

	[SerializeField]
	private TextMeshProUGUI _tackleTypes;

	[Space(5f)]
	[SerializeField]
	private UINavigation _nav;

	[SerializeField]
	private TextMeshProUGUI _chatCaption;

	[SerializeField]
	private TextMeshProUGUI _idTitle;

	[SerializeField]
	private TextMeshProUGUI _pondTitle;

	[SerializeField]
	private TextMeshProUGUI _fishTitle;

	[SerializeField]
	private TextMeshProUGUI _conditionsTitle;

	[SerializeField]
	private TextMeshProUGUI _durationTitle;

	[SerializeField]
	private TextMeshProUGUI _startTimeTitle;

	[SerializeField]
	private TextMeshProUGUI _prizeTitle;

	[SerializeField]
	private TextMeshProUGUI _hostTitle;

	[SerializeField]
	private TextMeshProUGUI _equipmentTitle;

	[SerializeField]
	private TextMeshProUGUI _ttTitle;

	private Material _defaultNameSharedMaterial;

	private Color _defaultNameColor;

	protected ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	protected ResourcesHelpers.AsyncLoadableImage ImagePondLdbl = new ResourcesHelpers.AsyncLoadableImage();

	protected UserCompetitionPublic Competition;

	protected Dictionary<Guid, UgcRoomItemSingle> Items = new Dictionary<Guid, UgcRoomItemSingle>();

	private const float LockAllPlayersInCompetitionTeamTime = 60f;

	private const float HostPingServerTime = 20f;

	private bool _isKickedByHost;

	private bool _isLockedAllPlayersInCompetitionTeam;

	private const int MaxChatMessagesCount = 100;

	private readonly List<UgcChatItem> _chatMessages = new List<UgcChatItem>();

	private readonly List<string> _chatMessagesIds = new List<string>();

	private Guid _lastPlayerActionUserId = Guid.Empty;

	private Coroutine _coroutine;

	private RoomUserCompetition.States _state;

	private int _timerValue;

	private enum States : byte
	{
		None,
		RequestStart,
		RequestStartCancel,
		CountDown,
		Start
	}
}
