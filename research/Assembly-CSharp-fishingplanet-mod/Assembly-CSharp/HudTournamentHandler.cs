using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HudTournamentHandler : MonoBehaviour
{
	public static bool IsInHUD { get; set; }

	private bool IsTeam
	{
		get
		{
			return PhotonConnectionFactory.Instance.Profile.Tournament != null && UserCompetitionLogic.IsTeamCompetition(PhotonConnectionFactory.Instance.Profile.Tournament.TournamentType);
		}
	}

	private void Update()
	{
		this._timer += Time.deltaTime;
		if (this._timer >= 15f)
		{
			this._timer = 0f;
			if (PhotonConnectionFactory.Instance.Profile.Tournament != null)
			{
				if (this.IsTeam)
				{
					PhotonConnectionFactory.Instance.OnGotIntermediateTournamentTeamResult += this.GotTeamResult;
					PhotonConnectionFactory.Instance.GetIntermediateTournamentTeamResult(new int?(PhotonConnectionFactory.Instance.Profile.Tournament.TournamentId));
				}
				else
				{
					PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult += this.InstanceOnGotTournamentIntermediateResult;
					PhotonConnectionFactory.Instance.GetIntermediateTournamentResult(new int?(PhotonConnectionFactory.Instance.Profile.Tournament.TournamentId));
				}
			}
		}
		this._timeTimer += Time.deltaTime;
		if (this._timeTimer >= 0.1f)
		{
			this._timeTimer = 0f;
			TimeSpan timeSpan;
			if (PhotonConnectionFactory.Instance.TournamentRemainingTime != null && PhotonConnectionFactory.Instance.TournamentRemainingTimeReceived != null)
			{
				timeSpan = PhotonConnectionFactory.Instance.TournamentRemainingTime.Value.Subtract(DateTime.UtcNow - PhotonConnectionFactory.Instance.TournamentRemainingTimeReceived.Value);
			}
			else
			{
				timeSpan = PhotonConnectionFactory.Instance.Profile.Tournament.EndDate - PhotonConnectionFactory.Instance.ServerUtcNow;
			}
			if (PhotonConnectionFactory.Instance.Profile.Tournament != null && timeSpan.TotalSeconds > 0.0)
			{
				string text = string.Format("{0}:{1}:{2}", timeSpan.Hours.ToString("D2"), timeSpan.Minutes.ToString("D2"), timeSpan.Seconds.ToString("D2"));
				if (this.TimeText.text != text)
				{
					this.TimeText.text = text;
					float num = (float)timeSpan.TotalSeconds / (((float)PhotonConnectionFactory.Instance.Profile.Tournament.InGameDuration * 60f + (float)PhotonConnectionFactory.Instance.Profile.Tournament.InGameDurationMinute) * 15f);
					this.TimeImage.fillAmount = num;
				}
				if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds <= 10)
				{
					this.EnableEndWarning(this.TimeText.gameObject, true);
				}
			}
			else
			{
				this.TimeText.text = "00:00:00";
				this.TimeImage.fillAmount = 0f;
				this.EnableEndWarning(this.TimeText.gameObject, false);
				this.TimeText.color = Color.red;
			}
		}
		if (!this._eventSigned && GameFactory.FishSpawner != null)
		{
			GameFactory.FishSpawner.FishCaught += this.FishSpawnerFishCaught;
			this._eventSigned = true;
		}
	}

	private void Awake()
	{
		this.PrepareTournamentMaterial();
		PhotonConnectionFactory.Instance.OnTournamentUpdate += this.Instance_OnTournamentUpdate;
		GameFactory.OnChatInGameCreated += this.GameFactory_OnChatInGameCreated;
	}

	private void OnEnable()
	{
		HudTournamentHandler.IsInHUD = true;
		this.TimeText.color = Color.white;
		this._timer = 0f;
		ProfileTournament tournament = PhotonConnectionFactory.Instance.Profile.Tournament;
		if (tournament != null)
		{
			TimeSpan timeSpan2;
			TimeSpan timeSpan = (timeSpan2 = tournament.EndDate - TimeHelper.UtcTime());
			if (timeSpan2.TotalSeconds > 0.0 && timeSpan.Hours <= 0 && timeSpan.Minutes <= 0 && timeSpan.Seconds <= 10)
			{
				goto IL_99;
			}
		}
		this.EnableEndWarning(this.TimeText.gameObject, false);
		IL_99:
		if (this.IsTeam)
		{
			PhotonConnectionFactory.Instance.OnGotIntermediateTournamentTeamResult += this.GotTeamResult;
			PhotonConnectionFactory.Instance.GetIntermediateTournamentTeamResult(new int?(tournament.TournamentId));
		}
		else
		{
			PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult += this.InstanceOnGotTournamentIntermediateResult;
			if (tournament != null)
			{
				PhotonConnectionFactory.Instance.GetIntermediateTournamentResult(new int?(tournament.TournamentId));
			}
		}
		PhotonConnectionFactory.Instance.GetTime();
	}

	private void OnDestroy()
	{
		GameFactory.OnChatInGameCreated -= this.GameFactory_OnChatInGameCreated;
		PhotonConnectionFactory.Instance.OnTournamentUpdate -= this.Instance_OnTournamentUpdate;
		this.UnsubscribeGetUserCompetition();
	}

	public static bool IsWarningOfEnd
	{
		get
		{
			ProfileTournament tournament = PhotonConnectionFactory.Instance.Profile.Tournament;
			return tournament != null && (tournament.EndDate - TimeHelper.UtcTime()).TotalSeconds <= 10.0;
		}
	}

	private void FishSpawnerFishCaught(object sender, FishCaughtEventArgs e)
	{
	}

	private void InstanceOnGotTournamentIntermediateResult(List<TournamentIndividualResults> results, int totalParticipants)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult -= this.InstanceOnGotTournamentIntermediateResult;
		ProfileTournament tournament = PhotonConnectionFactory.Instance.Profile.Tournament;
		TournamentIndividualResults tournamentIndividualResults = results.FirstOrDefault((TournamentIndividualResults x) => x.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
		if (tournamentIndividualResults == null || tournament == null)
		{
			return;
		}
		this._result.Init(tournament);
		this._result.UpdateData(results, 5);
		this.YourResult.Init(this._result.YourResult, tournament);
	}

	private void EnableEndWarning(GameObject enableWarnOn, bool enable)
	{
		TextColorPulsation textColorPulsation = enableWarnOn.GetComponent<TextColorPulsation>();
		if (textColorPulsation == null)
		{
			textColorPulsation = enableWarnOn.AddComponent<TextColorPulsation>();
			textColorPulsation.enabled = false;
		}
		if (enable && textColorPulsation.enabled)
		{
			return;
		}
		textColorPulsation.enabled = enable;
		textColorPulsation.StartColor = Color.red;
		textColorPulsation.MinAlpha = 0f;
		textColorPulsation.PulseTime = 1f;
		if (enable)
		{
			this.previousTimeText = this.TimeText.text;
			base.StartCoroutine(this.CountdownTick());
		}
	}

	private IEnumerator CountdownTick()
	{
		while (this.previousTimeText != "00:00:00")
		{
			if (this.previousTimeText != this.TimeText.text)
			{
				UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.CoundownClip, SettingsManager.InterfaceVolume);
				this.previousTimeText = this.TimeText.text;
			}
			yield return new WaitForSeconds(0.1f);
		}
		yield break;
		yield break;
	}

	private void Instance_OnTournamentUpdate(TournamentUpdateInfo info)
	{
		this._timer = 0f;
		if (this.IsTeam)
		{
			PhotonConnectionFactory.Instance.OnGotIntermediateTournamentTeamResult += this.GotTeamResult;
			PhotonConnectionFactory.Instance.GetIntermediateTournamentTeamResult(new int?(PhotonConnectionFactory.Instance.Profile.Tournament.TournamentId));
		}
		else
		{
			PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult += this.InstanceOnGotTournamentIntermediateResult;
			PhotonConnectionFactory.Instance.GetIntermediateTournamentResult(new int?(info.TournamentId));
		}
	}

	private void Instance_OnTournamentTimeEnded(TournamentIndividualResults result)
	{
		this._timer = 0f;
		if (this.IsTeam)
		{
			PhotonConnectionFactory.Instance.OnGotIntermediateTournamentTeamResult += this.GotTeamResult;
			PhotonConnectionFactory.Instance.GetIntermediateTournamentTeamResult(new int?(PhotonConnectionFactory.Instance.Profile.Tournament.TournamentId));
		}
		else
		{
			PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult += this.InstanceOnGotTournamentIntermediateResult;
			PhotonConnectionFactory.Instance.GetIntermediateTournamentResult(new int?(result.TournamentId));
		}
	}

	public void TournamentTimeEnded(TournamentIndividualResults result)
	{
		this.Instance_OnTournamentTimeEnded(result);
	}

	public void Init()
	{
		ProfileTournament tournament = PhotonConnectionFactory.Instance.Profile.Tournament;
		if (tournament.IsUgc())
		{
			PhotonConnectionFactory.Instance.OnGetUserCompetition += this.Instance_OnGetUserCompetition;
			PhotonConnectionFactory.Instance.OnFailureGetUserCompetition += this.Instance_OnFailureGetUserCompetition;
			PhotonConnectionFactory.Instance.GetUserCompetition(tournament.TournamentId);
		}
		else
		{
			this.TournamentName.text = tournament.Name;
		}
		if (tournament.ImageBID != null)
		{
			this.TournamentLogoLdbl.Load(tournament.ImageBID, this.TournamentLogo, "Textures/Inventory/{0}");
		}
		if (this.HasRankingShow)
		{
			this.HideRankingsPanel(this.RankingShow == 0);
		}
		else
		{
			this.ExpandRankingsPanel();
		}
	}

	public void HudEnable()
	{
		if (this._ugc != null && PhotonConnectionFactory.Instance.Profile.Tournament != null && PhotonConnectionFactory.Instance.Profile.Tournament.TournamentId == this._ugc.TournamentId && !PhotonConnectionFactory.Instance.Profile.Tournament.IsEnded)
		{
			this.JoinChatChannel(UserCompetitionHelper.GetDefaultName(this._ugc));
		}
	}

	public void ExpandRankingsPanel()
	{
		int num = 1 - this.RankingShow;
		this.HideRankingsPanel(num == 0);
		this.RankingShow = num;
	}

	public bool HasRankingShow
	{
		get
		{
			return PlayerPrefs.HasKey("ExpandRankingsPanelInTournament");
		}
	}

	public int RankingShow
	{
		get
		{
			return PlayerPrefs.GetInt("ExpandRankingsPanelInTournament", 0);
		}
		set
		{
			PlayerPrefs.SetInt("ExpandRankingsPanelInTournament", value);
		}
	}

	public void ShowTournamentInfo()
	{
		if (CatchedFishInfoHandler.IsDisplayed())
		{
			return;
		}
		if (PhotonConnectionFactory.Instance.Profile.Tournament == null)
		{
			return;
		}
		if (PhotonConnectionFactory.Instance.Profile.Tournament.IsUgc())
		{
			TournamentHelper.ShowingInGameTournamentDetails(this._ugc, false, true);
			return;
		}
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		PhotonConnectionFactory.Instance.OnGotTournament += this.PhotonServerOnGotTournament;
		PhotonConnectionFactory.Instance.OnGettingTournamentFailed += this.InstanceOnGettingTournamentFailed;
		PhotonConnectionFactory.Instance.GetTournament(PhotonConnectionFactory.Instance.Profile.Tournament.TournamentId);
	}

	private void InstanceOnGettingTournamentFailed(Failure failure)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		PhotonConnectionFactory.Instance.OnGotTournament -= this.PhotonServerOnGotTournament;
		PhotonConnectionFactory.Instance.OnGettingTournamentFailed -= this.InstanceOnGettingTournamentFailed;
		Debug.LogError(failure.FullErrorInfo);
	}

	private void PhotonServerOnGotTournament(Tournament tournament)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		PhotonConnectionFactory.Instance.OnGotTournament -= this.PhotonServerOnGotTournament;
		PhotonConnectionFactory.Instance.OnGettingTournamentFailed -= this.InstanceOnGettingTournamentFailed;
		TournamentHelper.ShowingTournamentDetails(tournament, false);
	}

	private void HideRankingsPanel(bool flag)
	{
		this.YourResult.gameObject.SetActive(flag);
		if (flag)
		{
			this.RankingHidesPanel.Hide();
			this.RankingIcon.text = "\ue651";
			this.RankingIcon.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
		}
		else
		{
			this.RankingHidesPanel.Show();
			this.RankingIcon.text = "\ue650";
			this.RankingIcon.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
		}
	}

	private void Instance_OnGetUserCompetition(UserCompetitionPublic competition)
	{
		if (competition == null)
		{
			LogHelper.Error("HudTournamentHandler - Ugc can't found.", new object[0]);
			return;
		}
		LogHelper.Log("___kocha HUD:GetUserCompetition TournamentId:{0} ToggleChatController is null:{1}", new object[]
		{
			competition.TournamentId,
			ToggleChatController.Instance == null
		});
		if (this._ugc == null || this._ugc.TournamentId != competition.TournamentId)
		{
			this.JoinChatChannel(UserCompetitionHelper.GetDefaultName(competition));
		}
		this._ugc = competition;
		this.UnsubscribeGetUserCompetition();
		if (!competition.IsSponsored)
		{
			this.PrepareTournamentMaterial();
			this.TournamentName.fontSharedMaterial = this._defaultNameSharedMaterial;
			this.TournamentName.color = this._defaultNameColor;
		}
		UserCompetitionHelper.GetDefaultName(this.TournamentName, competition);
		bool flag = this._ugc.Format == UserCompetitionFormat.Team;
		this._resultTeamsGo.SetActive(flag);
		this._resultGo.SetActive(!flag);
		if (flag)
		{
			this._resultTeams.Init(competition);
			this._timer = 15f;
		}
	}

	private void GotTeamResult(List<TournamentTeamResult> results, int totalParticipants)
	{
		PhotonConnectionFactory.Instance.OnGotIntermediateTournamentTeamResult -= this.GotTeamResult;
		this._resultTeams.UpdateData(results, totalParticipants);
	}

	private void Instance_OnFailureGetUserCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeGetUserCompetition();
	}

	private void UnsubscribeGetUserCompetition()
	{
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetition -= this.Instance_OnFailureGetUserCompetition;
		PhotonConnectionFactory.Instance.OnGetUserCompetition -= this.Instance_OnGetUserCompetition;
	}

	private void PrepareTournamentMaterial()
	{
		if (this._defaultNameSharedMaterial == null)
		{
			this._defaultNameSharedMaterial = this.TournamentName.fontSharedMaterial;
			this._defaultNameColor = this.TournamentName.color;
		}
	}

	private void JoinChatChannel(string tournamentName)
	{
		if (ToggleChatController.Instance != null && !StaticUserData.ChatController.IsChatChannel(tournamentName))
		{
			ToggleChatController.Instance.AddTournamentChat(tournamentName);
			StaticUserData.ChatController.JoinChatChannel();
		}
	}

	private void GameFactory_OnChatInGameCreated()
	{
		LogHelper.Log("___kocha GameFactory_OnChatInGameCreated()");
		this.HudEnable();
	}

	[SerializeField]
	private GameObject _resultGo;

	[SerializeField]
	private GameObject _resultTeamsGo;

	[SerializeField]
	private HudTournamentIndividualManager _result;

	[SerializeField]
	private HudTournamentTeamsManager _resultTeams;

	public Text TimeText;

	public Image TimeImage;

	public TextMeshProUGUI TournamentName;

	public Image TournamentLogo;

	private ResourcesHelpers.AsyncLoadableImage TournamentLogoLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public HidesPanel RankingHidesPanel;

	public Text RankingIcon;

	private float _timer;

	private const float TimerMax = 15f;

	private float _timeTimer;

	private bool _eventSigned;

	private const int _secondsBeforeWarn = 10;

	public RankingsListItemInit YourResult;

	private const string RankingShowKey = "ExpandRankingsPanelInTournament";

	private Material _defaultNameSharedMaterial;

	private Color _defaultNameColor;

	private UserCompetitionPublic _ugc;

	private const float _timeTimerMax = 0.1f;

	private string previousTimeText;
}
