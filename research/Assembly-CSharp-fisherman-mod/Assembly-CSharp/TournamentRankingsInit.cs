using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.PlayerProfile;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TournamentRankingsInit : MonoBehaviour
{
	private void Awake()
	{
		this._uNameTitle.text = PlayerProfileHelper.UsernameCaption;
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult -= this.OnGotTournamentIntermediateResult;
		PhotonConnectionFactory.Instance.OnGotTournamentFinalResult -= this.OnGotTournamentFinalResult;
	}

	public void Init(Tournament tournament)
	{
		this._currentTournament = tournament;
		this.ClearResultsList();
		this.StartHour.text = MeasuringSystemManager.TimeString(this._currentTournament.StartDate.ToLocalTime());
		this.EndHour.text = MeasuringSystemManager.TimeString(this._currentTournament.EndDate.ToLocalTime());
		if (this._currentTournament.IsActive)
		{
			this.Status.text = ScriptLocalization.Get("RunningStatusText");
			PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult += this.OnGotTournamentIntermediateResult;
			PhotonConnectionFactory.Instance.GetIntermediateTournamentResult(new int?(this._currentTournament.TournamentId));
			this.SetCompetitionHoursProgressBar();
		}
		else if (this._currentTournament.IsEnded)
		{
			this.Status.text = ScriptLocalization.Get("FinishedStatusText");
			PhotonConnectionFactory.Instance.OnGotTournamentFinalResult += this.OnGotTournamentFinalResult;
			PhotonConnectionFactory.Instance.GetFinalTournamentResult(this._currentTournament.TournamentId);
			this.HoursProgressBarRT.sizeDelta = new Vector2(this.HoursBackgroundRT.rect.width, this.HoursProgressBarRT.rect.height);
		}
		else if (this._currentTournament.IsCanceled)
		{
			this.Status.text = ScriptLocalization.Get("CanceledStatusText");
			this.HoursProgressBarRT.sizeDelta = new Vector2(0f, this.HoursProgressBarRT.rect.height);
			this.FillBlankRankingsList();
		}
		else if (this._currentTournament.IsStarted)
		{
			PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult += this.OnGotTournamentIntermediateResult;
			PhotonConnectionFactory.Instance.GetIntermediateTournamentResult(new int?(this._currentTournament.TournamentId));
		}
		else
		{
			this.Status.text = ScriptLocalization.Get("PlannedStatusText");
			this.HoursProgressBarRT.sizeDelta = new Vector2(0f, this.HoursProgressBarRT.rect.height);
			this.ScoreCriteria1.text = "0";
			this.ScoreCriteria2.text = "0";
			this.FillBlankRankingsList();
		}
		this.PlayersRegisteredCount.text = tournament.RegistrationsCount.ToString();
		this.PlayersInProgressCount.text = tournament.PlayingCount.ToString();
		this.PlayersFinishedCount.text = tournament.FinishedCount.ToString();
	}

	private void SetTournamentDetails()
	{
		if (this._currentTournament.IsActive)
		{
			this.Status.text = ScriptLocalization.Get("RunningStatusText");
			PhotonConnectionFactory.Instance.GetIntermediateTournamentResult(new int?(this._currentTournament.TournamentId));
			this.SetCompetitionHoursProgressBar();
		}
		else if (this._currentTournament.IsEnded)
		{
			this.Status.text = ScriptLocalization.Get("FinishedStatusText");
			PhotonConnectionFactory.Instance.GetFinalTournamentResult(this._currentTournament.TournamentId);
			this.HoursProgressBarRT.sizeDelta = new Vector2(this.HoursBackgroundRT.rect.width, this.HoursProgressBarRT.rect.height);
		}
		else if (this._currentTournament.IsCanceled)
		{
			this.Status.text = ScriptLocalization.Get("CanceledStatusText");
			this.HoursProgressBarRT.sizeDelta = new Vector2(0f, this.HoursProgressBarRT.rect.height);
		}
		else
		{
			this.Status.text = ScriptLocalization.Get("PlannedStatusText");
			this.HoursProgressBarRT.sizeDelta = new Vector2(0f, this.HoursProgressBarRT.rect.height);
		}
	}

	private void SetCompetitionHoursProgressBar()
	{
		float width = this.HoursBackgroundRT.rect.width;
		double totalHours = (this._currentTournament.EndDate - this._currentTournament.StartDate).TotalHours;
		int num = (int)((double)width / (totalHours * 60.0));
		int num2 = (int)(PhotonConnectionFactory.Instance.ServerUtcNow - this._currentTournament.StartDate).TotalMinutes;
		this.HoursProgressBarRT.sizeDelta = new Vector2((float)(num2 * num), this.HoursProgressBarRT.rect.height);
	}

	private void OnGotTournamentFinalResult(List<TournamentIndividualResults> results, int num)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentFinalResult -= this.OnGotTournamentFinalResult;
		this.ClearResultsList();
		this.ShowResults(results, num);
	}

	private void OnGotTournamentIntermediateResult(List<TournamentIndividualResults> results, int num)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult -= this.OnGotTournamentIntermediateResult;
		this.ClearResultsList();
		this.ShowResults(results, num);
	}

	private void ShowResults(List<TournamentIndividualResults> results, int num)
	{
		this._resultInited = true;
		List<TournamentIndividualResults> list = results.OrderBy((TournamentIndividualResults r) => r.Rank).ToList<TournamentIndividualResults>();
		TournamentIndividualResults tournamentIndividualResults = list.FirstOrDefault((TournamentIndividualResults r) => r.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
		int num2 = list.FindIndex((TournamentIndividualResults r) => r.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
		int num3 = ((this._currentTournament.WinnersCountAdmited2NextStage <= 0) ? 10 : this._currentTournament.WinnersCountAdmited2NextStage);
		if (tournamentIndividualResults != null)
		{
			this.ScoreCriteria1.text = MeasuringSystemManager.GetTournamentPrimaryScoreValueToString(this._currentTournament, tournamentIndividualResults);
			this.ScoreCriteria2.text = MeasuringSystemManager.GetTournamentSecondaryScoreValueToString(this._currentTournament, tournamentIndividualResults);
		}
		for (int i = 0; i < num3; i++)
		{
			GameObject gameObject = GUITools.AddChild(this.RankingsListContent, this.RankingsListItemPrefab);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			if (i < list.Count<TournamentIndividualResults>())
			{
				TournamentIndividualResults tournamentIndividualResults2 = list[i];
				gameObject.GetComponent<RankingsListItemInit>().Init(tournamentIndividualResults2, (tournamentIndividualResults2.Rank == null) ? "-" : tournamentIndividualResults2.Rank.ToString(), this._currentTournament);
			}
			else
			{
				gameObject.GetComponent<RankingsListItemInit>().Init(i + 1);
			}
			if (i % 2 != 0)
			{
				gameObject.GetComponent<Image>().enabled = false;
			}
		}
		if (tournamentIndividualResults != null && num2 > num3 && this.PlayerRanking != null)
		{
			this.RankingsListContent.transform.parent.GetComponent<RectTransform>().offsetMin += new Vector2(0f, this.PlayerRanking.GetComponent<LayoutElement>().preferredHeight);
			this.PlayerRanking.GetComponent<RankingsListItemInit>().Init(tournamentIndividualResults, (tournamentIndividualResults.Rank == null) ? "-" : tournamentIndividualResults.Rank.ToString(), this._currentTournament);
			this.PlayerRanking.transform.parent.gameObject.SetActive(true);
		}
		if (num2 < 0 && this.PlayerRanking != null)
		{
			this.RankingsListContent.transform.parent.GetComponent<RectTransform>().offsetMin += new Vector2(0f, this.PlayerRanking.GetComponent<LayoutElement>().preferredHeight);
			this.PlayerRanking.GetComponent<RankingsListItemInit>().Init();
			this.PlayerRanking.transform.parent.gameObject.SetActive(true);
		}
	}

	private void ClearResultsList()
	{
		for (int i = 0; i < this.RankingsListContent.transform.childCount; i++)
		{
			Object.Destroy(this.RankingsListContent.transform.GetChild(i).gameObject);
		}
	}

	private void FillBlankRankingsList()
	{
		int num = ((this._currentTournament.WinnersCountAdmited2NextStage <= 0) ? 10 : this._currentTournament.WinnersCountAdmited2NextStage);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = GUITools.AddChild(this.RankingsListContent, this.RankingsListItemPrefab);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			gameObject.GetComponent<RankingsListItemInit>().Init(i + 1);
			if (i % 2 != 0)
			{
				gameObject.GetComponent<Image>().enabled = false;
			}
		}
	}

	[SerializeField]
	private Text _uNameTitle;

	public Text Status;

	public Text ScoreCriteria1;

	public Text ScoreCriteria2;

	public Text PlayersRegisteredCount;

	public Text PlayersInProgressCount;

	public Text PlayersFinishedCount;

	public Text StartHour;

	public Text EndHour;

	public RectTransform HoursBackgroundRT;

	public RectTransform HoursProgressBarRT;

	public GameObject RankingsListItemPrefab;

	public GameObject RankingsListSeparatorPrefab;

	public GameObject PlayerRanking;

	public GameObject RankingsListContent;

	private Tournament _currentTournament;

	private bool _resultInited;
}
