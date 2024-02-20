using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using Photon.Interfaces;
using Photon.Interfaces.Tournaments;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserCompetitions : BaseCompetitions
{
	protected UserCompetitionPublic UserCompetition
	{
		get
		{
			UesrCompetitionItem curCompetition = base.CurCompetition;
			return (!(curCompetition != null)) ? null : curCompetition.Сompetition;
		}
	}

	public override void SetParentActive(bool flag)
	{
		if (flag)
		{
			this.SubscribeCompetitionOnReview();
		}
		else
		{
			this.UnSubscribeCompetitionOnReview();
		}
		if (flag && base.gameObject.activeSelf)
		{
			this.GetUserCompetitions();
		}
	}

	protected override void Awake()
	{
		UserCompetitionHelper.LoadSponsoredMaterials();
		base.Awake();
		this.SetupsError.SetActive(false);
		this.EquipmentError.SetActive(false);
		this._btnCreate.interactable = PhotonConnectionFactory.Instance.Profile.CanCreateUserCompetition() && UserCompetitionHelper.IsUgcEnabled;
		this.UpdateFiltersCounter();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.SubscribeCompetitionOnReview();
		PhotonConnectionFactory.Instance.OnInventoryMoved += this.InventoryUpdated;
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.InventoryUpdated;
		PhotonConnectionFactory.Instance.OnGetUserCompetitions += this.Instance_OnGetUserCompetitions;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetitions += new OnFailureUserCompetition(this.Instance_OnFailureGetUserCompetitions);
		this.Refresh();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.UnSubscribeCompetitionOnReview();
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.InventoryUpdated;
		PhotonConnectionFactory.Instance.OnInventoryMoved -= this.InventoryUpdated;
		this.UnsubscribeRegisterInCompetition();
		this.UnsubscribeGetUserCompetitions();
		this.UnsubscribeCanCreateCompetition();
		base.StopAllCoroutines();
	}

	public void Create()
	{
		UIHelper.Waiting(true, null);
		this.TryCreateCompetition(false);
	}

	public override void Join()
	{
		UserCompetitionPublic userCompetition = this.UserCompetition;
		if (userCompetition != null)
		{
			if (UserCompetitionHelper.IsOwnerHost(userCompetition) && userCompetition.IsSponsored && !userCompetition.IsRegistered)
			{
				base.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Sport, false, false);
				base.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Create, true, false);
				CreateTournamentPageHandler createTournamentCtrl = base.MenuMgr.CreateTournamentCtrl;
				if (createTournamentCtrl != null && createTournamentCtrl.CurrentCategory != CreationsCategories.Advanced)
				{
					createTournamentCtrl.Init();
				}
				if (createTournamentCtrl != null)
				{
					createTournamentCtrl.LoadCompetition(userCompetition);
				}
				return;
			}
			if (!userCompetition.IsRegistered || userCompetition.IsEnded || userCompetition.IsStarted || userCompetition.IsCanceled || !UserCompetitionHelper.IsUgcEnabled)
			{
				int tournamentId = userCompetition.TournamentId;
				string team = ((userCompetition.Format != UserCompetitionFormat.Team) ? null : "Red");
				bool isPrivate = userCompetition.IsPrivate;
				TournamentDetailsMessageNew component = TournamentHelper.ShowingTournamentDetails(userCompetition, false, true, null, false).GetComponent<TournamentDetailsMessageNew>();
				component.Ok += delegate
				{
					if (isPrivate)
					{
						GameObject gameObject = TournamentHelper.WindowTextField(string.Empty, "UGC_EnterPasswordCaption", null, false, null, null, false);
						gameObject.GetComponent<ChumRename>().OnRenamed += delegate(string pass)
						{
							this.RegisterInCompetition(tournamentId, team, pass, false);
						};
					}
					else
					{
						this.RegisterInCompetition(tournamentId, team, null, false);
					}
				};
			}
			else
			{
				this.Enter2Room(userCompetition);
			}
		}
	}

	public override void ResetAll()
	{
		base.ResetAll();
		base.StopAllCoroutines();
		this.FilterCompetitions = new FilterForUserCompetitions();
		this.IsInited = false;
		this.UpdateFiltersCounter();
		this.Refresh();
	}

	public override void Filter()
	{
		GameObject gameObject = TournamentHelper.WindowSearch(this.FilterCompetitions);
		WindowSearchOptions component = gameObject.GetComponent<WindowSearchOptions>();
		component.OnSelect += this.Refresh;
	}

	private void InitItems(List<UserCompetitionPublic> ts, Action onFinishActivation)
	{
		this.InitThreads<UserCompetitionPublic>(ts, this.ItemsParent.GetComponent<ToggleGroup>(), delegate
		{
			if (this.TournamentId != null && !this.Data.ContainsKey(this.TournamentId.Value))
			{
				this.TournamentId = null;
			}
			this.SelectNew();
			onFinishActivation();
		});
	}

	protected override void Clear()
	{
		this.PointerActiveTournamentId = null;
		foreach (KeyValuePair<int, UesrCompetitionItem> keyValuePair in this.Data)
		{
			keyValuePair.Value.Clear();
		}
		this.Data.Clear();
	}

	private void LazyClear(List<UserCompetitionPublic> o)
	{
		this.PointerActiveTournamentId = null;
		List<int> list = o.Select((UserCompetitionPublic p) => p.TournamentId).ToList<int>();
		List<int> list2 = new List<int>();
		foreach (KeyValuePair<int, UesrCompetitionItem> keyValuePair in this.Data)
		{
			if (!list.Contains(keyValuePair.Key))
			{
				keyValuePair.Value.Clear();
				list2.Add(keyValuePair.Key);
			}
		}
		list2.ForEach(delegate(int tournamentId)
		{
			this.Data.Remove(tournamentId);
		});
		for (int i = 0; i < list.Count; i++)
		{
			if (this.Data.ContainsKey(list[i]))
			{
				this.Data[list[i]].SetSiblingIndex(i);
			}
		}
	}

	protected void UpdateFiltersCounter()
	{
		int num = 0;
		this.UpFiltersCounter<int>(ref num, this.FilterCompetitions.PondIds);
		this.UpFiltersCounter<UserCompetitionFormat>(ref num, this.FilterCompetitions.Formats);
		this.UpFiltersCounter<TournamentScoreType>(ref num, this.FilterCompetitions.ScoreTypes);
		this.UpFiltersCounter<int>(ref num, this.FilterCompetitions.FishCategories);
		if (this.FilterCompetitions.ShowEnded)
		{
			num++;
		}
		this._counter.SetActive(num > 0);
		this._counterValue.text = num.ToString();
	}

	protected void UpFiltersCounter<T>(ref int count, T[] data)
	{
		if (data != null && data.Length > 0)
		{
			count++;
		}
	}

	protected override void Localize()
	{
		base.Localize();
		this.LevelCaption.text = ScriptLocalization.Get("LevelButtonPopup");
		this.TypeCaption.text = ScriptLocalization.Get("UGC_FormatTypes");
		this.EntryFeeCaption.text = ScriptLocalization.Get("UGC_EntryFeeTournamentCaption");
		this.JoinedCaption.text = ScriptLocalization.Get("UGC_AppliedPlayersCountCaption");
		this.StatusCaption.text = ScriptLocalization.Get("TournamentStatusCaption");
		this.StartTimeTitle.text = ScriptLocalization.Get("UGC_StartTimeCaption");
		this.DurationTitle.text = ScriptLocalization.Get("UGC_DurationCaption");
		this.PrizeFundTitle.text = ScriptLocalization.Get("UGC_PrizeFundCaption");
		this.HostTitle.text = ScriptLocalization.Get("UGC_HostCaption");
		this.Tax.text = UserCompetitionHelper.FeeCommissionLoc;
		this.Setups.text = ScriptLocalization.Get("UGC_CheckTackleRequirements");
		this.Equipment.text = ScriptLocalization.Get("UGC_RestrictedEquipment");
	}

	protected override int GetItemTournamentId<T>(T t)
	{
		UserCompetitionPublic userCompetitionPublic = t as UserCompetitionPublic;
		if (userCompetitionPublic != null)
		{
			return userCompetitionPublic.TournamentId;
		}
		return 0;
	}

	protected override void InitItem<T, TN>(T t, TN item)
	{
		UesrCompetitionItem uesrCompetitionItem = item;
		UserCompetitionPublic userCompetitionPublic = t as UserCompetitionPublic;
		if (userCompetitionPublic != null && uesrCompetitionItem != null)
		{
			uesrCompetitionItem.Init(userCompetitionPublic);
		}
	}

	protected override void UpdateCurCompetition()
	{
		UserCompetitionPublic userCompetition = this.UserCompetition;
		if (userCompetition != null)
		{
			if (userCompetition.ImageBID != null)
			{
				this.ImageLdbl.Load(userCompetition.ImageBID, this.Image, "Textures/Inventory/{0}");
			}
			DateTime dateTime = userCompetition.StartDate.ToLocalTime();
			if (!userCompetition.IsStarted)
			{
				if (userCompetition.SortType == UserCompetitionSortType.Automatic)
				{
					dateTime = userCompetition.FixedStartDate.Value.ToLocalTime();
				}
				else
				{
					this.StartTime.text = ScriptLocalization.Get("UGC_ManualStart");
				}
			}
			if (userCompetition.SortType == UserCompetitionSortType.Automatic || userCompetition.IsStarted)
			{
				if (DateTime.Now.Day < dateTime.Day || DateTime.Now.Month < dateTime.Month)
				{
					this.StartTime.text = string.Format("{0} {1}", MeasuringSystemManager.DateTimeShortWithoutYear(dateTime), MeasuringSystemManager.TimeString(dateTime));
				}
				else
				{
					this.StartTime.text = MeasuringSystemManager.TimeString(dateTime);
				}
			}
			this.Duration.text = UgcConsts.GetDurationLoc(userCompetition.Duration).Name;
			this.EntryCurrency.text = MeasuringSystemManager.GetCurrencyIcon(userCompetition.Currency);
			this.EntrySum.text = string.Format("{0}", UserCompetitionHelper.PrizePoolWithComission(userCompetition));
			this.Host.text = userCompetition.HostName;
			this.SetActiveRightMenu(true);
			this.InventoryUpdated();
		}
	}

	protected override void Refresh()
	{
		DateTime dateTime = TimeHelper.UtcTime();
		if (!this.IsInited)
		{
			this.IsInited = true;
			this.CurrentShowingDate = dateTime;
		}
		this.UpdateSelectedDt();
		FilterForUserCompetitions filterCompetitions = this.FilterCompetitions;
		DateTime dateTime2 = new DateTime(this.CurrentShowingDate.Year, this.CurrentShowingDate.Month, this.CurrentShowingDate.Day, 0, 0, 0);
		filterCompetitions.From = new DateTime?(dateTime2.AddTicks(-1L));
		FilterForUserCompetitions filterCompetitions2 = this.FilterCompetitions;
		DateTime dateTime3 = new DateTime(this.CurrentShowingDate.Year, this.CurrentShowingDate.Month, this.CurrentShowingDate.Day, 0, 0, 0);
		filterCompetitions2.To = new DateTime?(dateTime3.AddDays(1.0).AddTicks(-1L));
		this.FilterCompetitions.DontShowManualStart = this.DontShowManualStart(dateTime);
		this.GetUserCompetitions();
	}

	private void FailureCanCreateCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeCanCreateCompetition();
		UIHelper.Waiting(false, null);
		UserCompetitionErrorCode userCompetitionErrorCode = failure.UserCompetitionErrorCode;
		switch (userCompetitionErrorCode)
		{
		case 61:
		case 64:
			UserCompetitionFailureHandler.ShowMsgSportEventWait(failure);
			return;
		case 62:
			break;
		default:
			if (userCompetitionErrorCode != 1 && userCompetitionErrorCode != 42)
			{
				UserCompetitionFailureHandler.Fire(failure, null, true);
				return;
			}
			break;
		}
		if (failure.UserCompetition.IsStarted)
		{
			UserCompetitionFailureHandler.ShowMsgSportEventWait(failure);
		}
		else
		{
			UIHelper.ShowYesNo(string.Format(ScriptLocalization.Get("UGC_AlreadySignedToSportEventResign"), UserCompetitionFailureHandler.GetSportEventName(failure)), delegate
			{
				this.TryCreateCompetition(true);
			}, null, "YesCaption", null, "NoCaption", null, null, null);
		}
	}

	private void CanCreateCompetitionOk()
	{
		if (TournamentManager.Instance != null && StaticUserData.CurrentPond == null)
		{
			TournamentManager.Instance.Refresh();
		}
		this.UnsubscribeCanCreateCompetition();
		UIHelper.Waiting(false, null);
		this.OpenCreationForm();
	}

	private void TryCreateCompetition(bool removeOtherRegistrations)
	{
		PhotonConnectionFactory.Instance.OnTryCreateCompetition += this.CanCreateCompetitionOk;
		PhotonConnectionFactory.Instance.OnFailureTryCreateCompetition += this.FailureCanCreateCompetition;
		PhotonConnectionFactory.Instance.TryCreateCompetition(removeOtherRegistrations);
	}

	private IEnumerator CallRefresh()
	{
		if (this.IsSearching)
		{
			this.TrySearch();
		}
		yield return new WaitForSeconds(5f);
		this.GetUserCompetitions();
		yield break;
	}

	private void Refresh(FilterForUserCompetitions f)
	{
		base.StopAllCoroutines();
		this.FilterCompetitions = f;
		this.FilterCompetitions.DontShowManualStart = this.DontShowManualStart(this.CurrentShowingDate);
		this.UpdateFiltersCounter();
		this.GetUserCompetitions();
	}

	private bool DontShowManualStart(DateTime dt)
	{
		DateTime dateTime = this.FilterCompetitions.From.Value.AddTicks(1L);
		return dateTime.Year != dt.Year || dateTime.Month != dt.Month || dateTime.Day != dt.Day;
	}

	private void GetUserCompetitions()
	{
		if (base.ShouldUpdate())
		{
			this.FilterCompetitions.RequestId = this._requestIdGetUserCompetitions;
			PhotonConnectionFactory.Instance.GetUserCompetitions(this.FilterCompetitions);
		}
	}

	private void RegisterInCompetition(int tournamentId, string team, string password, bool removeOtherRegistrations = false)
	{
		UIHelper.Waiting(true, null);
		this._tournamentRegistrationData = new UserCompetitions.RegistrationData
		{
			Password = password,
			TournamentId = tournamentId,
			Team = team,
			RemoveOtherRegistrations = removeOtherRegistrations
		};
		LogHelper.Log("___kocha RegisterInCompetition tournamentId:{0}", new object[] { tournamentId });
		PhotonConnectionFactory.Instance.OnRegisterInCompetition += this.Instance_OnRegisterInCompetition;
		PhotonConnectionFactory.Instance.OnFailureRegisterInCompetition += this.Instance_OnFailureRegisterInCompetition;
		PhotonConnectionFactory.Instance.RegisterInCompetition(tournamentId, team, password, removeOtherRegistrations);
	}

	private void Instance_OnRegisterInCompetition(UserCompetitionPublic competition)
	{
		if (TournamentManager.Instance != null)
		{
			if (this._tournamentRegistrationData != null && this._tournamentRegistrationData.RemoveOtherRegistrations)
			{
				TournamentManager.Instance.FullRefresh();
			}
			else if (StaticUserData.CurrentPond == null)
			{
				TournamentManager.Instance.Refresh();
			}
		}
		this._tournamentRegistrationData = null;
		this.UnsubscribeRegisterInCompetition();
		UIHelper.Waiting(false, null);
		this.Enter2Room(competition);
	}

	private void Enter2Room(UserCompetitionPublic competition)
	{
		base.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Sport, false, false);
		base.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Room, true, false);
		base.MenuMgr.RoomUserCompetitionCtrl.Init(competition);
	}

	private List<UserCompetitionPublic> Sort(List<UserCompetitionPublic> list)
	{
		List<UserCompetitionPublic> list2 = new List<UserCompetitionPublic>();
		List<UserCompetitionPublic> list3 = new List<UserCompetitionPublic>();
		List<UserCompetitionPublic> list4 = new List<UserCompetitionPublic>();
		for (int i = 0; i < list.Count; i++)
		{
			UserCompetitionPublic userCompetitionPublic = list[i];
			if (userCompetitionPublic.IsEnded || userCompetitionPublic.IsCanceled || userCompetitionPublic.IsDone)
			{
				list2.Add(userCompetitionPublic);
			}
			else if (userCompetitionPublic.IsRegistered)
			{
				list3.Add(userCompetitionPublic);
			}
			else
			{
				list4.Add(userCompetitionPublic);
			}
		}
		list3 = list3.OrderBy((UserCompetitionPublic p) => p.StartDate).ToList<UserCompetitionPublic>();
		list3.AddRange(list4.OrderBy((UserCompetitionPublic p) => p.StartDate));
		list3.AddRange(list2.OrderByDescending((UserCompetitionPublic p) => p.StartDate));
		return list3;
	}

	private void Instance_OnGetUserCompetitions(Guid requestId, List<UserCompetitionPublic> list)
	{
		if (this._requestIdGetUserCompetitions != requestId)
		{
			return;
		}
		base.StopAllCoroutines();
		LogHelper.Log("___kocha GetUserCompetitions Count:{0}", new object[] { list.Count });
		if (list.Count == 0)
		{
			this.Clear();
			this.SetActiveRightMenu(false);
			base.StartCoroutine(this.CallRefresh());
		}
		else
		{
			list = this.Sort(list);
			this.LazyClear(list);
			this.InitItems(list, delegate
			{
				base.StartCoroutine(this.CallRefresh());
			});
		}
	}

	private void InventoryUpdated()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (this.UserCompetition != null && profile != null)
		{
			ErrorCode errorCode = profile.CheckTournamentStartPrerequisitesEquipment(this.UserCompetition);
			this.EquipmentError.SetActive(errorCode != 0);
			if (this.EquipmentError.activeSelf)
			{
				LogHelper.Log("___kocha EquipmentError:{0}", new object[] { errorCode });
			}
			ErrorCode errorCode2 = profile.CheckTournamentStartPrerequisitesRods(this.UserCompetition);
			this.SetupsError.SetActive(errorCode2 != 0);
			if (this.SetupsError.activeSelf)
			{
				LogHelper.Log("___kocha SetupsError:{0}", new object[] { this.SetupsError });
			}
		}
		else
		{
			this.SetupsError.SetActive(false);
			this.EquipmentError.SetActive(false);
		}
	}

	private void OpenCreationForm()
	{
		base.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Sport, false, false);
		base.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Create, true, false);
		CreateTournamentPageHandler createTournamentCtrl = base.MenuMgr.CreateTournamentCtrl;
		if (createTournamentCtrl != null && createTournamentCtrl.CurrentCategory != CreationsCategories.Advanced)
		{
			createTournamentCtrl.Init();
		}
	}

	private void Instance_OnUserCompetitionRemovedOnReview(UserCompetitionReviewMessage competition)
	{
		this.GetUserCompetitions();
	}

	private void Instance_OnUserCompetitionDeclinedOnReview(UserCompetitionReviewMessage competition)
	{
		this.GetUserCompetitions();
	}

	private void Instance_OnUserCompetitionApprovedOnReview(UserCompetitionReviewMessage competition)
	{
		this.GetUserCompetitions();
	}

	private void SubscribeCompetitionOnReview()
	{
		if (!this._isSubscribedCompetitionOnReview)
		{
			this._isSubscribedCompetitionOnReview = true;
			PhotonConnectionFactory.Instance.OnUserCompetitionApprovedOnReview += this.Instance_OnUserCompetitionApprovedOnReview;
			PhotonConnectionFactory.Instance.OnUserCompetitionDeclinedOnReview += this.Instance_OnUserCompetitionDeclinedOnReview;
			PhotonConnectionFactory.Instance.OnUserCompetitionRemovedOnReview += this.Instance_OnUserCompetitionRemovedOnReview;
		}
	}

	private void UnSubscribeCompetitionOnReview()
	{
		if (this._isSubscribedCompetitionOnReview)
		{
			this._isSubscribedCompetitionOnReview = false;
			PhotonConnectionFactory.Instance.OnUserCompetitionApprovedOnReview -= this.Instance_OnUserCompetitionApprovedOnReview;
			PhotonConnectionFactory.Instance.OnUserCompetitionDeclinedOnReview -= this.Instance_OnUserCompetitionDeclinedOnReview;
			PhotonConnectionFactory.Instance.OnUserCompetitionRemovedOnReview -= this.Instance_OnUserCompetitionRemovedOnReview;
		}
	}

	private void Instance_OnFailureGetUserCompetitions(Failure failure)
	{
		Debug.LogErrorFormat("FailureGetUserCompetitions FullErrorInfo:{0}", new object[] { failure.FullErrorInfo });
		base.StopAllCoroutines();
		base.StartCoroutine(this.CallRefresh());
	}

	private void Instance_OnFailureRegisterInCompetition(UserCompetitionFailure failure)
	{
		if (TournamentManager.Instance != null && StaticUserData.CurrentPond == null && this._tournamentRegistrationData.RemoveOtherRegistrations)
		{
			TournamentManager.Instance.Refresh();
		}
		this.UnsubscribeRegisterInCompetition();
		UIHelper.Waiting(false, null);
		UserCompetitionErrorCode userCompetitionErrorCode = failure.UserCompetitionErrorCode;
		switch (userCompetitionErrorCode)
		{
		case 61:
		case 64:
			UserCompetitionFailureHandler.ShowMsgSportEventWait(failure);
			return;
		case 62:
			break;
		default:
			if (userCompetitionErrorCode != 1 && userCompetitionErrorCode != 42)
			{
				UserCompetitionFailureHandler.Fire(failure, null, true);
				return;
			}
			break;
		}
		if (failure.UserCompetition.IsStarted)
		{
			UserCompetitionFailureHandler.ShowMsgSportEventWait(failure);
		}
		else
		{
			UIHelper.ShowYesNo(string.Format(ScriptLocalization.Get("UGC_AlreadySignedToSportEventJoinAnother"), UserCompetitionFailureHandler.GetSportEventName(failure)), delegate
			{
				this._tournamentRegistrationData.RemoveOtherRegistrations = true;
				this.RegisterInCompetition();
			}, null, "UGC_ApplyAnywayButton", null, "NoCaption", null, null, null);
		}
	}

	private void RegisterInCompetition()
	{
		UIHelper.Waiting(false, null);
		if (this._tournamentRegistrationData != null)
		{
			this.RegisterInCompetition(this._tournamentRegistrationData.TournamentId, this._tournamentRegistrationData.Team, this._tournamentRegistrationData.Password, this._tournamentRegistrationData.RemoveOtherRegistrations);
		}
	}

	private void UnsubscribeRegisterInCompetition()
	{
		PhotonConnectionFactory.Instance.OnRegisterInCompetition -= this.Instance_OnRegisterInCompetition;
		PhotonConnectionFactory.Instance.OnFailureRegisterInCompetition -= this.Instance_OnFailureRegisterInCompetition;
	}

	private void UnsubscribeGetUserCompetitions()
	{
		PhotonConnectionFactory.Instance.OnGetUserCompetitions -= this.Instance_OnGetUserCompetitions;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetitions -= new OnFailureUserCompetition(this.Instance_OnFailureGetUserCompetitions);
	}

	private void UnsubscribeCanCreateCompetition()
	{
		PhotonConnectionFactory.Instance.OnTryCreateCompetition -= this.CanCreateCompetitionOk;
		PhotonConnectionFactory.Instance.OnFailureTryCreateCompetition -= this.FailureCanCreateCompetition;
	}

	private void InitThreads<T>(List<T> ts, ToggleGroup tg, Action onFinishActivation)
	{
		int num = 10;
		float num2 = 0f;
		int i = 0;
		while (i < ts.Count)
		{
			base.StartCoroutine(this.AddRange<T>(i, i + num, num2, ts, tg, onFinishActivation));
			i += num;
			num2 += 2f;
		}
	}

	private IEnumerator AddRange<T>(int start, int end, float t, List<T> list, ToggleGroup tg, Action onFinishActivation)
	{
		yield return new WaitForSeconds(t);
		for (int i = start; i < end; i++)
		{
			this.Add<T>(list, i, tg);
			if (i >= list.Count - 1)
			{
				onFinishActivation();
				break;
			}
		}
		yield break;
	}

	private void Add<T>(List<T> ts, int i, ToggleGroup tg)
	{
		T t = ts[i];
		int itemTournamentId = this.GetItemTournamentId<T>(t);
		bool flag = this.Data.ContainsKey(itemTournamentId);
		UesrCompetitionItem uesrCompetitionItem = null;
		if (flag)
		{
			uesrCompetitionItem = this.Data[itemTournamentId];
			this.InitItem<T, UesrCompetitionItem>(t, uesrCompetitionItem);
			uesrCompetitionItem.SetActive(true);
			return;
		}
		bool flag2 = i < this.ItemsParent.transform.childCount;
		if (flag2)
		{
			uesrCompetitionItem = this.GetFreeItem;
		}
		if (uesrCompetitionItem == null)
		{
			uesrCompetitionItem = GUITools.AddChild(this.ItemsParent, this.ItemPrefab).GetComponent<UesrCompetitionItem>();
		}
		if (flag2)
		{
			uesrCompetitionItem.SetActive(true);
		}
		uesrCompetitionItem.ClearEvents();
		uesrCompetitionItem.OnPointerActive += delegate(bool v)
		{
			this.PointerActive<T>(v, t);
		};
		uesrCompetitionItem.OnTglValueChanged += delegate(bool b, int i1)
		{
			this.OnTglValueChanged(b, this.GetItemTournamentId<T>(t));
		};
		uesrCompetitionItem.Tgl.group = tg;
		this.InitItem<T, UesrCompetitionItem>(t, uesrCompetitionItem);
		this.Data[itemTournamentId] = uesrCompetitionItem;
	}

	private UesrCompetitionItem GetFreeItem
	{
		get
		{
			for (int i = 0; i < this.ItemsParent.transform.childCount; i++)
			{
				Transform child = this.ItemsParent.transform.GetChild(i);
				UesrCompetitionItem component = child.GetComponent<UesrCompetitionItem>();
				if (!this.Data.ContainsValue(component))
				{
					return component;
				}
			}
			return null;
		}
	}

	[SerializeField]
	private GameObject _counter;

	[SerializeField]
	private TextMeshProUGUI _counterValue;

	[SerializeField]
	private BorderedButton _btnCreate;

	[Space(5f)]
	[SerializeField]
	protected TextMeshProUGUI LevelCaption;

	[SerializeField]
	protected TextMeshProUGUI TypeCaption;

	[SerializeField]
	protected TextMeshProUGUI EntryFeeCaption;

	[SerializeField]
	protected TextMeshProUGUI JoinedCaption;

	[SerializeField]
	protected TextMeshProUGUI StatusCaption;

	[Space(5f)]
	[SerializeField]
	protected TextMeshProUGUI Duration;

	[SerializeField]
	protected TextMeshProUGUI StartTime;

	[SerializeField]
	protected TextMeshProUGUI Setups;

	[SerializeField]
	protected GameObject SetupsError;

	[SerializeField]
	protected GameObject EquipmentError;

	[SerializeField]
	protected TextMeshProUGUI Equipment;

	[SerializeField]
	protected TextMeshProUGUI Host;

	[SerializeField]
	protected TextMeshProUGUI Tax;

	[SerializeField]
	protected TextMeshProUGUI StartTimeTitle;

	[SerializeField]
	protected TextMeshProUGUI DurationTitle;

	[SerializeField]
	protected TextMeshProUGUI PrizeFundTitle;

	[SerializeField]
	protected TextMeshProUGUI HostTitle;

	protected FilterForUserCompetitions FilterCompetitions = new FilterForUserCompetitions();

	protected Pond CurrentPond;

	private const int ThreadsCount = 10;

	private const float RefreshTime = 5f;

	private bool _isSubscribedCompetitionOnReview;

	private readonly Guid _requestIdGetUserCompetitions = Guid.NewGuid();

	private UserCompetitions.RegistrationData _tournamentRegistrationData;

	private class RegistrationData
	{
		public int TournamentId { get; set; }

		public string Team { get; set; }

		public string Password { get; set; }

		public bool RemoveOtherRegistrations { get; set; }
	}
}
