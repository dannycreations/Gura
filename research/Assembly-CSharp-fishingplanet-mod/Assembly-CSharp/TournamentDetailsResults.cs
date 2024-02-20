using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;
using ObjectModel.Tournaments;
using UnityEngine;

[Obsolete("The Old class for TournamentDetails.")]
public class TournamentDetailsResults : MonoBehaviour
{
	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentSecondaryResult -= this.PhotonServerGetSecondaryGotTournamentResult;
		PhotonConnectionFactory.Instance.OnGotTournamentSecondaryResult -= this.GetSecondaryGotTournamentResultTeam;
		this._data.ForEach(delegate(TournamentDetailsResult p)
		{
			Object.Destroy(p.gameObject);
		});
		this._data.Clear();
	}

	public void Init(UserCompetitionPublic ugc, List<TournamentIndividualResults> results, int num)
	{
		this._ugc = ugc;
		this._results = results;
		PhotonConnectionFactory.Instance.OnGotTournamentSecondaryResult += this.PhotonServerGetSecondaryGotTournamentResult;
		PhotonConnectionFactory.Instance.GetSecondaryTournamentResult(ugc.TournamentId);
	}

	public void Init(Tournament t, List<TournamentIndividualResults> results, int num)
	{
		this._tournament = t;
		this._results = results;
		PhotonConnectionFactory.Instance.OnGotTournamentSecondaryResult += this.PhotonServerGetSecondaryGotTournamentResult;
		PhotonConnectionFactory.Instance.GetSecondaryTournamentResult(t.TournamentId);
	}

	public void Init(UserCompetitionPublic ugc, List<TournamentTeamResult> results, int totalParticipants)
	{
		this._ugc = ugc;
		this._teamTesults = results;
		List<TournamentIndividualResults> iResult = new List<TournamentIndividualResults>();
		results.ForEach(delegate(TournamentTeamResult p)
		{
			iResult.AddRange(p.IndividualResults);
		});
		this._results = iResult;
		PhotonConnectionFactory.Instance.OnGotTournamentSecondaryResult += this.GetSecondaryGotTournamentResultTeam;
		PhotonConnectionFactory.Instance.GetSecondaryTournamentResult(ugc.TournamentId);
	}

	public void Init(Tournament tournament, List<TournamentTeamResult> results, int totalParticipants)
	{
	}

	private void GetSecondaryGotTournamentResultTeam(List<TournamentSecondaryResult> results)
	{
		if (this._results == null || this._teamTesults == null)
		{
			return;
		}
		PhotonConnectionFactory.Instance.OnGotTournamentSecondaryResult -= this.GetSecondaryGotTournamentResultTeam;
		bool isDraw = this._teamTesults.All((TournamentTeamResult p) => p.Place == null);
		IEnumerable<TournamentTeamResult> enumerable = this._teamTesults.Where((TournamentTeamResult p) => p.Place == null);
		List<TournamentTeamResult> list = (from p in this._teamTesults
			where p.Place != null
			orderby p.Place
			select p).ToList<TournamentTeamResult>();
		list.AddRange(enumerable);
		list.ForEach(delegate(TournamentTeamResult p)
		{
			string scoreString = UserCompetitionHelper.GetScoreString(this._ugc.ScoringType, p.Score, this._ugc.TotalScoreKind);
			string scoreString2 = UserCompetitionHelper.GetScoreString(this._ugc.SecondaryScoringType, p.SecondaryScore, this._ugc.TotalScoreKind);
			TournamentDetailsResultTeam component = GUITools.AddChild(this._itemsRoot, this._itemTeamPrefab).GetComponent<TournamentDetailsResultTeam>();
			component.Init(p.Team, p.Place, this._ugc, scoreString, scoreString2, isDraw);
			TournamentIndividualResults tournamentIndividualResults = (from r in p.IndividualResults.ToList<TournamentIndividualResults>()
				where r.UserId == PhotonConnectionFactory.Instance.Profile.UserId
				select r).FirstOrDefault<TournamentIndividualResults>();
			if (tournamentIndividualResults != null)
			{
				this.AddResult(tournamentIndividualResults, this._ugc.ScoringType, p.Place);
			}
		});
		this.SetActive();
	}

	private void PhotonServerGetSecondaryGotTournamentResult(List<TournamentSecondaryResult> results)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentSecondaryResult -= this.PhotonServerGetSecondaryGotTournamentResult;
		if ((this._tournament == null && this._ugc == null) || this._results == null)
		{
			return;
		}
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		List<TournamentIndividualResults> list = (from r in this._results
			where r.Place != null
			where r.Place <= 3
			select r into p
			orderby p.Place
			select p).Take(3).ToList<TournamentIndividualResults>();
		bool flag = false;
		TournamentScoreType tournamentScoreType = ((this._tournament == null) ? this._ugc.ScoringType : this._tournament.PrimaryScoring.ScoringType);
		LogHelper.Log("___kocha UGC Result +++++++++++++++++++++++++++++++ count:{0}", new object[] { this._results.Count });
		for (int i = 0; i < list.Count; i++)
		{
			LogHelper.Log("___kocha UGC i:{0} Name:{1} Place:{2} Score:{3} SecondaryScore:{4}", new object[]
			{
				i,
				list[i].Name,
				list[i].Place,
				list[i].Score,
				list[i].SecondaryScore
			});
		}
		for (int j = 0; j < list.Count; j++)
		{
			TournamentIndividualResults tournamentIndividualResults = list[j];
			this.AddResult(tournamentIndividualResults, tournamentScoreType, null);
			if (tournamentIndividualResults.UserId == profile.UserId)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			TournamentIndividualResults tournamentIndividualResults2 = this._results.FirstOrDefault((TournamentIndividualResults r) => r.UserId == profile.UserId);
			if (tournamentIndividualResults2 != null)
			{
				if (list.Count == 3)
				{
					this._data[this._data.Count - 1].SetLast();
				}
				this.AddResult(tournamentIndividualResults2, tournamentScoreType, null);
			}
		}
		this.SetActive();
	}

	private void AddResult(TournamentIndividualResults place, TournamentScoreType scoringType, int? teamPlace = null)
	{
	}

	private void SetActive()
	{
		base.gameObject.SetActive(true);
	}

	[SerializeField]
	private GameObject _itemTeamPrefab;

	[SerializeField]
	private GameObject _itemPrefab;

	[SerializeField]
	private GameObject _itemsRoot;

	private const int PlacesCount = 3;

	private List<TournamentDetailsResult> _data = new List<TournamentDetailsResult>();

	private Tournament _tournament;

	private UserCompetitionPublic _ugc;

	private List<TournamentTeamResult> _teamTesults;

	private List<TournamentIndividualResults> _results;
}
