using System;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RankingsListItemInit : UserShowProfile
{
	public TournamentIndividualResults Result { get; private set; }

	public void Init(TournamentIndividualResults result, string place, TournamentBase tournament)
	{
		this.Init(result, place, tournament.PrimaryScoring.ScoringType, result.Score, tournament.SecondaryScoring.ScoringType, result.SecondaryScore, false, tournament.PrimaryScoring.TotalScoreKind);
	}

	public void Init(TournamentIndividualResults result, string place, UserCompetitionPublic tournament)
	{
		this.Init(result, place, tournament.ScoringType, result.Score, tournament.SecondaryScoringType, result.SecondaryScore, tournament.Format == UserCompetitionFormat.Team, tournament.TotalScoreKind);
	}

	public void Init(int place)
	{
		this.PlaceText.text = place.ToString();
		this.NameText.gameObject.GetComponent<EventTrigger>().enabled = false;
	}

	public void Init()
	{
		this.NameText.color = this.CurrentPlayerColor;
		this.PlaceText.color = this.CurrentPlayerColor;
		this.Score1Text.color = this.CurrentPlayerColor;
		this.Score2Text.color = this.CurrentPlayerColor;
	}

	public override void ShowPlayerProfile()
	{
		if (this._clicked == null)
		{
			base.RequestById(this._userID);
		}
	}

	public void Init(TournamentIndividualResults result, TournamentBase tournament)
	{
		this.Init(result, (result == null || result.Rank == null) ? "-" : result.Rank.ToString(), tournament);
	}

	public void Init(TournamentIndividualResults result, UserCompetitionPublic tournament)
	{
		this.Init(result, (result == null || result.Rank == null) ? "-" : result.Rank.ToString(), tournament);
	}

	public void SetLast()
	{
		this.SetLast(true);
	}

	public void SetLastEmpty()
	{
		this._last.SetActive(false);
		this._notLast.SetActive(false);
	}

	public void SetNonLast()
	{
		this.SetLast(false);
	}

	public void UpdateInfo(string place, TournamentScoreType scoringType0, float? value0, TournamentScoreType scoringType1, float? value1, TournamentTotalScoreKind totalScoreKind)
	{
		this.PlaceText.text = place;
		this.Score1Text.text = MeasuringSystemManager.GetTournamentScoreValueToString(scoringType0, value0, totalScoreKind, "3");
		this.Score2Text.text = MeasuringSystemManager.GetTournamentScoreValueToString(scoringType1, value1, totalScoreKind, "3");
	}

	public void Remove()
	{
		Object.Destroy(base.gameObject);
	}

	public void SetSiblingIndex(int index)
	{
		base.transform.SetSiblingIndex(index);
	}

	private void SetLast(bool flag)
	{
		this._last.SetActive(flag);
		this._notLast.SetActive(!this._last.activeSelf);
	}

	public void UpdateResult(TournamentIndividualResults r)
	{
		this.Result = r;
	}

	private void Init(TournamentIndividualResults result, string place, TournamentScoreType scoringType0, float? value0, TournamentScoreType scoringType1, float? value1, bool isTeam, TournamentTotalScoreKind totalScoreKind)
	{
		this.Result = result;
		if (this._team != null)
		{
			this._team.color = ((!isTeam) ? Color.clear : UgcConsts.GetTeamColor(result.Team));
		}
		this._userID = result.UserId.ToString();
		this.NameText.text = result.Name;
		this.UpdateInfo(place, scoringType0, value0, scoringType1, value1, totalScoreKind);
		if (result.UserId == PhotonConnectionFactory.Instance.Profile.UserId)
		{
			this.NameText.color = this.CurrentPlayerColor;
			this.Score1Text.color = this.CurrentPlayerColor;
			this.Score2Text.color = this.CurrentPlayerColor;
			this.PlaceText.color = this.CurrentPlayerColor;
			Object.Destroy(this.NameText.gameObject.GetComponent<EventTrigger>());
		}
		else if (!isTeam)
		{
			base.gameObject.AddComponent<ChangeCursor>();
		}
	}

	[SerializeField]
	private GameObject _last;

	[SerializeField]
	private GameObject _notLast;

	[SerializeField]
	private Image _team;

	public Text NameText;

	public Text PlaceText;

	public Text Score1Text;

	public Text Score2Text;

	public Color CurrentPlayerColor;

	private string _userID;
}
