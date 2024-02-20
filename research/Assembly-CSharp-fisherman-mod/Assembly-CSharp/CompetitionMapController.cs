using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Tournaments;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionMapController : ActivityStateControlled
{
	public bool IsJoinedToTournament
	{
		get
		{
			return this.isJoined;
		}
	}

	public bool IsCompetition
	{
		get
		{
			return this._currentCompetition.IsCompetition();
		}
	}

	public TournamentManager TournamentMgr
	{
		get
		{
			return this.TournamentManager;
		}
	}

	private void Awake()
	{
		this.TournamentManager = base.gameObject.AddComponent<TournamentManager>();
		TournamentManager tournamentManager = this.TournamentManager;
		tournamentManager.OnRefreshed = (Action)Delegate.Combine(tournamentManager.OnRefreshed, new Action(this.RefreshCurrentCompetition));
		this._pondCompetitionsMgr = new PondCompetitionsManager(this.TournamentManager, new Action(this.RefreshTournamentData));
		if (CompetitionMapController.Instance == null)
		{
			CompetitionMapController.Instance = this;
		}
		this.CompetitionPreviewPanel.SetActive(false);
		this.CompetitionPreviewPanel.OnShowDetails += delegate
		{
			this.ShowCompetitionInfo();
		};
		this.CompetitionPreviewPanel.OnJoinFinish += delegate
		{
			this.JoinFinishButtonPressed();
		};
	}

	protected override void Start()
	{
		base.Start();
		TournamentHelper.UpdateSeries();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.StopAllCoroutines();
		TournamentManager tournamentManager = this.TournamentManager;
		tournamentManager.OnRefreshed = (Action)Delegate.Remove(tournamentManager.OnRefreshed, new Action(this.RefreshCurrentCompetition));
		PhotonConnectionFactory.Instance.OnTournamentEnded -= this.OnTournamentEnded;
		this.UnsubscribeTournamentWeather();
		this.UnsubscribeTournamentStartPreview();
		this._pondCompetitionsMgr.Dispose();
	}

	private void Update()
	{
		if (StaticUserData.IS_IN_TUTORIAL || !base.ShouldUpdate())
		{
			return;
		}
		GameObject currentSelectedGo = UINavigation.CurrentSelectedGo;
		if (currentSelectedGo != null)
		{
			if (currentSelectedGo.transform.IsChildOf(this.EventToggle.transform) && this._mode != CompetitionMapController.CompetitionControllerMode.EventSelection)
			{
				this.SetMode(CompetitionMapController.CompetitionControllerMode.EventSelection);
			}
			else if (this.CompetitionPreviewPanel.IsJoinFinishBtnChildOf(currentSelectedGo.transform) && this._mode != CompetitionMapController.CompetitionControllerMode.ButtonSelection)
			{
				this.SetMode(CompetitionMapController.CompetitionControllerMode.ButtonSelection);
			}
			else if (!currentSelectedGo.transform.IsChildOf(this.EventToggle.transform) && !this.CompetitionPreviewPanel.IsJoinFinishBtnChildOf(currentSelectedGo.transform) && this._mode != CompetitionMapController.CompetitionControllerMode.PinSelection)
			{
				this.SetMode(CompetitionMapController.CompetitionControllerMode.PinSelection);
			}
		}
		if (this.locationInfo.CurrentLocation != null && TournamentHelper.SeriesInstances != null && this._currentCompetition != null)
		{
			this.UpdateStatus(TournamentHelper.GetCompetitionStatus(this._currentCompetition), false);
		}
	}

	private void UpdateStatus()
	{
		if (this.locationInfo.CurrentLocation != null && TournamentHelper.SeriesInstances != null && this._currentCompetition != null)
		{
			this._status = TournamentStatus.Unavailable;
			this.UpdateStatus(TournamentHelper.GetCompetitionStatus(this._currentCompetition), false);
		}
	}

	private void UpdateStatus(TournamentStatus status, bool forceUpdate = false)
	{
		if (this._status == status && !forceUpdate)
		{
			return;
		}
		this._status = status;
		this.CompetitionPreviewPanel.SetStatus(status, this.isJoined, this.isFinished);
		if (status == TournamentStatus.RegAndStarting)
		{
			this.highlightJoinArrow.SetActive(!this.isJoined);
			this.PlayerRoomsFoldoutToggle.interactable = !this.isJoined;
			if (this.PlayerRoomsColors != null)
			{
				if (this.PlayerRoomsFoldoutToggle.interactable)
				{
					this.PlayerRoomsColors.Enable();
				}
				else
				{
					this.PlayerRoomsColors.Disable();
				}
			}
			if (this.isJoined)
			{
				this.locationInfo.SetEventRoom();
			}
			this.EventToggle.interactable = false;
			this.SetMode(this._mode);
		}
		else if (status == TournamentStatus.Unavailable)
		{
			this.RefreshTournamentData();
			this.PlayerRoomsFoldoutToggle.interactable = !this.isJoined;
			if (this.PlayerRoomsColors != null && !this.isJoined)
			{
				this.PlayerRoomsColors.Enable();
			}
		}
		else
		{
			this.EventToggle.interactable = true;
			this.TurnOnTournament(false);
			this.PlayerRoomsFoldoutToggle.interactable = true;
			if (this.PlayerRoomsColors != null)
			{
				this.PlayerRoomsColors.Enable();
			}
		}
	}

	protected override void SetHelp()
	{
		if (this.PlayerRoomsFoldoutToggle != null)
		{
			this.PlayerRoomsColors = this.PlayerRoomsFoldoutToggle.GetComponent<ToggleColorTransitionChanges>();
		}
		HudTournamentHandler.IsInHUD = false;
		PhotonConnectionFactory.Instance.OnGotPlayerTournaments += this.Instance_OnGotPlayerTournaments;
		this._pondCompetitionsMgr.Start();
		this.TournamentManager.Refresh();
	}

	protected override void HideHelp()
	{
		base.StopAllCoroutines();
		this._pondCompetitionsMgr.Stop();
		PhotonConnectionFactory.Instance.OnGotPlayerTournaments -= this.Instance_OnGotPlayerTournaments;
	}

	private void Instance_OnGotPlayerTournaments(List<Tournament> tournaments)
	{
		if (this._currentCompetition != null && this.isJoined && !this._currentCompetition.IsUgc() && !tournaments.Exists((Tournament p) => p.TournamentId == this._currentCompetition.TournamentId) && TournamentHelper.ProfileTournament == null)
		{
			this._status = TournamentStatus.Unavailable;
			this.ResetJoined();
		}
		base.StopAllCoroutines();
		this._pondCompetitionsMgr.PushAllTournaments(tournaments);
		if (this._pondCompetitionsMgr.Competitions.Count > 0)
		{
			this.InitPreviewPanel(this._pondCompetitionsMgr.Competitions);
		}
		else
		{
			this.CompetitionPreviewPanel.SetActive(false);
			this.CompetitionPreviewPanel.Clear();
			this.TurnOnTournament(false);
		}
		base.StartCoroutine(this.RefreshTournamentDataByTime());
	}

	private void RequestTournamentWeather()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentWeather += this.Instance_OnGotTournamentWeather;
		PhotonConnectionFactory.Instance.OnGettingTournamentWeatherFailed += this.Instance_OnGettingTournamentWeatherFailed;
		PhotonConnectionFactory.Instance.GetTournamentWeather(this._currentCompetition.TournamentId);
	}

	private void Instance_OnGotTournamentWeather(WeatherDesc[] weathers)
	{
		this.UnsubscribeTournamentWeather();
		int num = 0;
		int? inGameStartHour = this._currentCompetition.InGameStartHour;
		TimeSpan timeSpan = new TimeSpan(num, (inGameStartHour == null) ? 0 : inGameStartHour.Value, this._currentCompetition.InGameStartMinute, 0);
		string timeOfDay = timeSpan.ToTimeOfDay().ToString();
		WeatherDesc weatherDesc = weathers.FirstOrDefault((WeatherDesc x) => x.TimeOfDay == timeOfDay);
		if (weatherDesc == null)
		{
			string text = "CompetitionMapController:OnGotTournamentWeather - unknown weather for TournamentId:{0} timeOfDay:{1} startTime:{2} TimeOfDays:{3}";
			object[] array = new object[4];
			array[0] = this._currentCompetition.TournamentId;
			array[1] = timeOfDay;
			array[2] = timeSpan;
			array[3] = string.Join(",", weathers.Select((WeatherDesc p) => p.TimeOfDay).ToArray<string>());
			LogHelper.Error(text, array);
			return;
		}
		this.locationInfo.OverrideWeather(timeSpan, weatherDesc, weathers);
	}

	public void JoinFinishButtonPressed()
	{
		if (!this.isJoined)
		{
			bool flag = CompetitionMapHelper.CanJoin(this._currentCompetition);
			LogHelper.Log("___kocha CoMapCtrl:JoinFinishButtonPressed canJoin:{0} TournamentId:{1}", new object[]
			{
				flag,
				this._currentCompetition.TournamentId
			});
			if (!flag)
			{
				return;
			}
			this.isJoined = true;
			this.RequestTournamentWeather();
			this.RefreshCurrentCompetition();
			this.UpdateStatus();
			UIAudioSourceListener.Instance.SportHorn();
			this.pinsNavigation.SetFirstActive();
		}
		else
		{
			UIHelper.ShowYesNo(ScriptLocalization.Get("FinishEventConfirm"), new Action(this.FinishAccept), null, "YesCaption", null, "NoCaption", null, null, null);
		}
	}

	public void FinishAccept()
	{
		this.isJoined = false;
		this.isFinished = true;
		this._status = TournamentStatus.Unavailable;
		this._finishedTournamentID = this._currentCompetition.TournamentId;
		if (TransferToLocation.Instance != null)
		{
			TransferToLocation.Instance.LeaveTournament(false);
		}
		PhotonConnectionFactory.Instance.OnTournamentEnded += this.OnTournamentEnded;
	}

	public void ResetJoined()
	{
		this.isJoined = false;
		this.locationInfo.SelectRandomRoom();
		this.RefreshCurrentCompetition();
		this.UpdateStatus(this._status, true);
	}

	private IEnumerator RefreshTournamentDataByTime()
	{
		yield return new WaitForSeconds(30f);
		this.RefreshTournamentData();
		yield break;
	}

	private void RefreshTournamentData()
	{
		LogHelper.Log("___kocha >>> CoMapCtrl: Refresh tournament data called!");
		if (!StaticUserData.IS_IN_TUTORIAL && StaticUserData.CurrentPond != null)
		{
			this._status = TournamentStatus.Unavailable;
			PhotonConnectionFactory.Instance.GetPlayerTournaments(new int?(StaticUserData.CurrentPond.PondId));
		}
	}

	private void TurnOnTournament(bool isOn)
	{
		if (!isOn)
		{
			this.isJoined = false;
		}
		LogHelper.Log("___kocha CoMapCtrl:TurnOnTournament isOn:{0}", new object[] { isOn });
		this.GoFishingButton.gameObject.SetActive(!isOn);
		this.TournamentButton.gameObject.SetActive(isOn);
	}

	private void GetPondTournaments(double daysAdvance)
	{
		DateTime dateTime = TimeHelper.UtcTime();
		PhotonConnectionFactory.Instance.GetTournaments(null, new int?(StaticUserData.CurrentPond.PondId), new DateTime?(dateTime), new DateTime?(dateTime.AddDays(daysAdvance)));
	}

	public void SetMode(CompetitionMapController.CompetitionControllerMode mode)
	{
		this._mode = mode;
		CompetitionMapController.CompetitionControllerMode mode2 = this._mode;
		if (mode2 != CompetitionMapController.CompetitionControllerMode.PinSelection)
		{
			if (mode2 != CompetitionMapController.CompetitionControllerMode.ButtonSelection)
			{
				if (mode2 == CompetitionMapController.CompetitionControllerMode.EventSelection)
				{
					this.TournamentButtonText.text = ScriptLocalization.Get("ViewInfoTitle").ToUpper(CultureInfo.InvariantCulture);
					this.TurnOnTournament(true);
				}
			}
			else
			{
				this.TournamentButtonText.text = this.CompetitionPreviewPanel.JoinFinishBtnText.ToUpper(CultureInfo.InvariantCulture);
				this.TurnOnTournament(true);
			}
		}
		else
		{
			this.TournamentButtonText.text = ScriptLocalization.Get(this.IsCompetition ? "EnterCompetitionCaption" : "EnterTournamentCaption").ToUpper(CultureInfo.InvariantCulture);
			this.TurnOnTournament(this.isJoined);
		}
	}

	public void TournamentButtonPressed()
	{
		CompetitionMapController.CompetitionControllerMode mode = this._mode;
		if (mode != CompetitionMapController.CompetitionControllerMode.PinSelection)
		{
			if (mode != CompetitionMapController.CompetitionControllerMode.ButtonSelection)
			{
				if (mode == CompetitionMapController.CompetitionControllerMode.EventSelection)
				{
					this.ShowCompetitionInfo();
				}
			}
			else
			{
				this.CompetitionPreviewPanel.JoinFinishBtnClick();
			}
		}
		else
		{
			this.EnterToTournament();
		}
	}

	public void EnterToTournament()
	{
		CompetitionMapHelper.EnterToTournament(this._currentCompetition, this.locationInfo, this.IsCompetition, new Action(this.Proceed));
	}

	private void Proceed()
	{
		CursorManager.HideCursor();
		PondHelpers pondHelpers = new PondHelpers();
		pondHelpers.PondPrefabsList.loadingFormAS.OnStart += this.TransferToLocation_OnStart;
		pondHelpers.ShowLoading();
	}

	private void TransferToLocation_OnStart()
	{
		new PondHelpers().PondPrefabsList.loadingFormAS.OnStart -= this.TransferToLocation_OnStart;
		StaticUserData.CurrentLocation = this.locationInfo.CurrentLocation;
		if (TournamentHelper.ProfileTournament == null || TournamentHelper.ProfileTournament.IsEnded)
		{
			PhotonConnectionFactory.Instance.OnTournamentStartPreview += this.PhotonServer_OnTournamentStartPreview;
			PhotonConnectionFactory.Instance.OnPreviewTournamentStartFailed += this.Instance_OnPreviewTournamentStartFailed;
			PhotonConnectionFactory.Instance.PreviewTournamentStart();
		}
		else
		{
			if (!PhotonConnectionFactory.Instance.IsInRoom)
			{
				TimeAndWeatherManager.ForceUpdateTime();
			}
			PhotonConnectionFactory.Instance.MoveToTournamentRoom(StaticUserData.CurrentPond.PondId, this._currentCompetition.TournamentId);
		}
	}

	private void PhotonServer_OnTournamentStartPreview()
	{
		Debug.Log("OnTournamentStartPreview() called");
		this.UnsubscribeTournamentStartPreview();
		TimeAndWeatherManager.ForceUpdateTime();
		PhotonConnectionFactory.Instance.MoveToTournamentRoom(StaticUserData.CurrentPond.PondId, this._currentCompetition.TournamentId);
	}

	private int GetMultyCount()
	{
		int num = this._pondCompetitionsMgr.Competitions.Count;
		if (this.isJoined)
		{
			num = 1;
		}
		else
		{
			this._currentCompetition = ((num <= 1) ? this._pondCompetitionsMgr.Competitions.FirstOrDefault<Tournament>() : null);
		}
		if (TournamentHelper.ProfileTournament != null)
		{
			num = 1;
			this._currentCompetition = this._pondCompetitionsMgr.Competitions.FirstOrDefault((Tournament p) => p.TournamentId == TournamentHelper.ProfileTournament.TournamentId);
		}
		return num;
	}

	public void ShowCompetitionInfo()
	{
		CompetitionMapHelper.ShowCompetitionInfo(this.GetMultyCount(), this._pondCompetitionsMgr.Competitions, this._currentCompetition, new Action(this.RefreshTournamentData), delegate(Tournament tournament)
		{
			this._currentCompetition = tournament;
			this.JoinFinishButtonPressed();
		});
	}

	private void EnsureFinishedStateCorrect()
	{
		if (this.isFinished && this._currentCompetition != null && this._currentCompetition.TournamentId != this._finishedTournamentID)
		{
			this.isFinished = false;
			this._finishedTournamentID = -1;
			this._status = TournamentStatus.Unavailable;
		}
	}

	private void InitPreviewPanel(List<Tournament> allCompetitions)
	{
		string text = "___kocha CoMapCtrl:InitPreviewPanel -> allCompetitions:{0}";
		object[] array = new object[1];
		array[0] = string.Join(",", allCompetitions.Select((Tournament p) => p.TournamentId.ToString()).ToArray<string>());
		LogHelper.Log(text, array);
		this.RefreshCurrentCompetition();
		if (allCompetitions.Count > 0)
		{
			this.EnsureFinishedStateCorrect();
			this.pinsNavigation.ForceUpdate();
		}
	}

	private void RefreshCurrentCompetition()
	{
		int multyCount = this.GetMultyCount();
		this.CompetitionPreviewPanel.SetActiveMulty(multyCount);
		this.CompetitionPreviewPanel.PushTournament(this._currentCompetition);
		this.CompetitionPreviewPanel.SetActive(this._currentCompetition != null || multyCount > 1);
		if (multyCount > 1)
		{
			TournamentStatus tournamentStatus = TournamentStatus.Unavailable;
			if (this._pondCompetitionsMgr.Competitions.Any((Tournament p) => TournamentHelper.GetCompetitionStatus(p) == TournamentStatus.RegAndStarting))
			{
				tournamentStatus = TournamentStatus.RegAndStarting;
			}
			this.CompetitionPreviewPanel.SetStatus(tournamentStatus, this.isJoined, this.isFinished);
		}
		else
		{
			this.CompetitionPreviewPanel.SetStatus(this._status, this.isJoined, this.isFinished);
		}
		WindowListCompetitions classManager = InfoMessageController.Instance.GetClassManager<WindowListCompetitions>();
		if (classManager != null)
		{
			if (multyCount > 1)
			{
				classManager.ReInit(CompetitionMapHelper.PrepareContainerCompetitions(CompetitionMapHelper.PrepareListCompetitions(this._pondCompetitionsMgr.Competitions), 0));
			}
			else
			{
				classManager.Close();
			}
		}
	}

	private void OnTournamentEnded()
	{
		PhotonConnectionFactory.Instance.OnTournamentEnded -= this.OnTournamentEnded;
		this.locationInfo.SelectRandomRoom();
		this.RefreshTournamentData();
	}

	private void Instance_OnGettingTournamentWeatherFailed(Failure failure)
	{
		this.UnsubscribeTournamentWeather();
		LogHelper.Log("CoMapCtrl:Instance_OnGettingTournamentWeatherFailed FullErrorInfo:{0}", new object[] { failure.FullErrorInfo });
	}

	private void Instance_OnPreviewTournamentStartFailed(Failure failure)
	{
		this.UnsubscribeTournamentStartPreview();
		LogHelper.Log("CoMapCtrl:Instance_OnPreviewTournamentStartFailed FullErrorInfo:{0}", new object[] { failure.FullErrorInfo });
	}

	private void UnsubscribeTournamentWeather()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentWeather -= this.Instance_OnGotTournamentWeather;
		PhotonConnectionFactory.Instance.OnGettingTournamentWeatherFailed -= this.Instance_OnGettingTournamentWeatherFailed;
	}

	private void UnsubscribeTournamentStartPreview()
	{
		PhotonConnectionFactory.Instance.OnTournamentStartPreview -= this.PhotonServer_OnTournamentStartPreview;
		PhotonConnectionFactory.Instance.OnPreviewTournamentStartFailed -= this.Instance_OnPreviewTournamentStartFailed;
	}

	[SerializeField]
	private UINavigation pinsNavigation;

	public ShowLocationInfo locationInfo;

	public Button TournamentButton;

	public Button GoFishingButton;

	public TextMeshProUGUI TournamentButtonText;

	public CompetitionsPreviewPanel CompetitionPreviewPanel;

	public Toggle EventToggle;

	public Toggle PlayerRoomsFoldoutToggle;

	public static CompetitionMapController Instance;

	[SerializeField]
	private GameObject highlightJoinArrow;

	private bool isJoined;

	private bool isFinished;

	private int _finishedTournamentID = -1;

	private Tournament _currentCompetition;

	private TournamentStatus _status = TournamentStatus.Unavailable;

	private PondCompetitionsManager _pondCompetitionsMgr;

	private ToggleColorTransitionChanges PlayerRoomsColors;

	private CompetitionMapController.CompetitionControllerMode _mode;

	protected TournamentManager TournamentManager;

	[Serializable]
	public enum CompetitionControllerMode
	{
		PinSelection,
		ButtonSelection,
		EventSelection
	}
}
