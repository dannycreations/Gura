using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class TournamentRankingsInitNew : MonoBehaviour
{
	public void Init(UserCompetitionPublic t, List<TournamentIndividualResults> results, int num, int ownerIndex)
	{
		this._ugc = t;
		this.ShowResults(results, num, ownerIndex);
	}

	public void Init(Tournament t, List<TournamentIndividualResults> results, int num, int ownerIndex)
	{
		this._currentTournament = t;
		this.ShowResults(results, num, ownerIndex);
	}

	public void FillBlankRankingsList(Tournament t)
	{
		this._currentTournament = t;
		this.AddRankings(this.ResultPositionsCount);
	}

	public void Init(UserCompetitionPublic ugc, List<TournamentTeamResult> results, int totalParticipants)
	{
		List<TournamentIndividualResults> iResult = new List<TournamentIndividualResults>();
		List<TournamentIndividualResults> iResultEmpty = new List<TournamentIndividualResults>();
		results.ForEach(delegate(TournamentTeamResult p)
		{
			this.SortIndividualResults(p.IndividualResults.ToList<TournamentIndividualResults>(), iResult, iResultEmpty);
		});
		List<TournamentIndividualResults> list = iResult.OrderBy((TournamentIndividualResults r) => r.Rank).ToList<TournamentIndividualResults>();
		list.AddRange(iResultEmpty);
		int num = list.FindIndex((TournamentIndividualResults r) => r.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
		this.Init(ugc, list, totalParticipants, num);
	}

	public void Init(Tournament tournament, List<TournamentTeamResult> results, int totalParticipants)
	{
	}

	private void AddRankings(int resultPositionsCount)
	{
		for (int i = 0; i < resultPositionsCount; i++)
		{
			this.AddRankingsListItem().Init(i + 1);
		}
		this.SetLast();
	}

	private int ResultPositionsCount
	{
		get
		{
			if (this._currentTournament == null)
			{
				return 10;
			}
			return (this._currentTournament.WinnersCountAdmited2NextStage <= 0) ? 10 : this._currentTournament.WinnersCountAdmited2NextStage;
		}
	}

	private void ShowResults(List<TournamentIndividualResults> sortedResults, int num, int ownerIndex)
	{
		if (sortedResults.Count == 0)
		{
			this.FillBlankRankingsList(this._currentTournament);
			return;
		}
		List<TournamentIndividualResults> list = new List<TournamentIndividualResults>();
		List<TournamentIndividualResults> list2 = new List<TournamentIndividualResults>();
		this.SortIndividualResults(sortedResults, list, list2);
		List<TournamentIndividualResults> list3 = list.OrderBy((TournamentIndividualResults r) => r.Rank).ToList<TournamentIndividualResults>();
		list3.AddRange(list2);
		ownerIndex = list3.FindIndex((TournamentIndividualResults p) => p.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
		int count = list3.Count;
		int num2 = Math.Min(this.ResultPositionsCount, list3.Count);
		LogHelper.Log("___kocha count:{0} resultPositionsCount:{1} ownerIndex:{2}", new object[] { count, num2, ownerIndex });
		for (int i = 0; i < num2; i++)
		{
			if (this._currentTournament != null)
			{
				this.AddRankingsListItem().Init(list3[i], this._currentTournament);
			}
			else
			{
				this.AddRankingsListItem().Init(list3[i], this._ugc);
			}
		}
		if (count > num2)
		{
			this.SetLast();
		}
		if (ownerIndex != -1)
		{
			TournamentIndividualResults tournamentIndividualResults = list3[ownerIndex];
			if (ownerIndex > num2)
			{
				if (this._currentTournament != null)
				{
					this.AddRankingsListItem().Init(tournamentIndividualResults, this._currentTournament);
				}
				else
				{
					this.AddRankingsListItem().Init(tournamentIndividualResults, this._ugc);
				}
			}
		}
	}

	private RankingsListItemInit AddRankingsListItem()
	{
		bool flag = this._ugc != null && this._ugc.Format == UserCompetitionFormat.Team;
		GameObject gameObject = GUITools.AddChild((!flag) ? this.RankingsListContent : this.RankingsListContentTeam, (!flag) ? this.RankingsListItemPrefab : this.RankingsListItemTeamPrefab);
		gameObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
		this._data.Add(gameObject.GetComponent<RankingsListItemInit>());
		return this._data[this._data.Count - 1];
	}

	private void SetLast()
	{
		this._data[this._data.Count - 1].SetLast();
	}

	private void SortIndividualResults(List<TournamentIndividualResults> results, List<TournamentIndividualResults> iResult, List<TournamentIndividualResults> iResultEmpty)
	{
		for (int i = 0; i < results.Count; i++)
		{
			TournamentIndividualResults tournamentIndividualResults = results[i];
			LogHelper.Log("___kocha IndividualResult Name:{0} Rank:{1} Score:{2} SecondaryScore:{3}", new object[] { tournamentIndividualResults.Name, tournamentIndividualResults.Rank, tournamentIndividualResults.Score, tournamentIndividualResults.SecondaryScore });
			if (tournamentIndividualResults.Rank != null && tournamentIndividualResults.Rank.Value > 0)
			{
				iResult.Add(tournamentIndividualResults);
			}
			else
			{
				iResultEmpty.Add(tournamentIndividualResults);
			}
		}
	}

	[SerializeField]
	private GameObject RankingsListItemPrefab;

	[SerializeField]
	private GameObject RankingsListItemTeamPrefab;

	[SerializeField]
	private GameObject RankingsListContent;

	[SerializeField]
	private GameObject RankingsListContentTeam;

	private Tournament _currentTournament;

	private UserCompetitionPublic _ugc;

	private List<RankingsListItemInit> _data = new List<RankingsListItemInit>();

	private const int WinnersCount = 10;
}
