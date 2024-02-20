using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudTournamentTeamsManager : MonoBehaviour
{
	private void Awake()
	{
		this._isInited = true;
		this._teamsScore[0] = this._firstScore;
		this._teamsScore[1] = this._secondScore;
	}

	public void Init(UserCompetitionPublic t)
	{
		this._ugc = t;
		this.Clear();
	}

	public void UpdateData(List<TournamentTeamResult> results, int totalParticipants)
	{
		this.GotTeamResult(results, totalParticipants);
	}

	private void Clear()
	{
		this._data.ForEach(delegate(RankingsListItemInit p)
		{
			Object.Destroy(p.gameObject);
		});
		this._data.Clear();
	}

	private void GotTeamResult(List<TournamentTeamResult> results, int totalParticipants)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null || this._ugc == null)
		{
			return;
		}
		if (!this._isInited)
		{
			this.Awake();
		}
		List<TournamentIndividualResults> list = new List<TournamentIndividualResults>();
		results = results.OrderBy((TournamentTeamResult p) => p2.Place).ToList<TournamentTeamResult>();
		for (int i = 0; i < results.Count; i++)
		{
			TournamentTeamResult tournamentTeamResult = results[i];
			list.AddRange(tournamentTeamResult.IndividualResults);
			TextMeshProUGUI[] array = this._teamsScore[i];
			this._scoreTeams[i].color = UgcConsts.GetTeamColor(tournamentTeamResult.Team);
			array[0].text = UserCompetitionHelper.GetScoreString(this._ugc.ScoringType, tournamentTeamResult.Score, this._ugc.TotalScoreKind);
			array[1].text = UserCompetitionHelper.GetScoreString(this._ugc.SecondaryScoringType, tournamentTeamResult.SecondaryScore, this._ugc.TotalScoreKind);
			if (i == 0)
			{
				this._cup.color = ((tournamentTeamResult.Place == null || tournamentTeamResult.Score == null) ? new Color(0f, 0f, 0f, 0f) : this._scoreTeams[i].color);
			}
		}
		int count = list.Count;
		TournamentIndividualResults tournamentIndividualResults = list.FirstOrDefault((TournamentIndividualResults r) => r.UserId == profile.UserId);
		list = list.OrderBy((TournamentIndividualResults p) => p2.Rank).Take(5).ToList<TournamentIndividualResults>();
		bool flag = list.Contains(tournamentIndividualResults);
		if (!flag && tournamentIndividualResults != null)
		{
			list.Add(tournamentIndividualResults);
		}
		this.Check4Del(list);
		for (int j = 0; j < list.Count; j++)
		{
			TournamentIndividualResults p2 = list[j];
			RankingsListItemInit rankingsListItemInit = this._data.FirstOrDefault((RankingsListItemInit el) => el.Result.UserId == p2.UserId);
			if (rankingsListItemInit == null)
			{
				this.AddRankingsListItem().Init(p2, this._ugc);
			}
			else
			{
				rankingsListItemInit.UpdateResult(p2);
				rankingsListItemInit.UpdateInfo(p2.Rank.ToString(), this._ugc.ScoringType, p2.Score, this._ugc.SecondaryScoringType, p2.SecondaryScore, this._ugc.TotalScoreKind);
			}
		}
		this._data = this._data.OrderBy((RankingsListItemInit p) => p.Result.Rank).ToList<RankingsListItemInit>();
		for (int k = 0; k < this._data.Count; k++)
		{
			this._data[k].SetSiblingIndex(k);
			this._data[k].SetNonLast();
		}
		if (!flag && tournamentIndividualResults != null)
		{
			int num = this._data.FindIndex((RankingsListItemInit p) => p.Result.UserId == profile.UserId);
			this._data[num - 1].SetLast();
		}
		if (count > 5)
		{
			this._data.Last<RankingsListItemInit>().SetLast();
		}
	}

	private RankingsListItemInit AddRankingsListItem()
	{
		GameObject gameObject = GUITools.AddChild(this._playersRoot, this._playersItemPrefab);
		gameObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
		this._data.Add(gameObject.GetComponent<RankingsListItemInit>());
		return this._data[this._data.Count - 1];
	}

	private void Check4Del(List<TournamentIndividualResults> r)
	{
		int i;
		for (i = 0; i < this._data.Count; i++)
		{
			if (!r.Exists((TournamentIndividualResults p) => p.UserId == this._data[i].Result.UserId))
			{
				this._data[i].Remove();
				this._data[i] = null;
			}
		}
		this._data.RemoveAll((RankingsListItemInit p) => p == null);
	}

	[SerializeField]
	private GameObject _playersRoot;

	[SerializeField]
	private GameObject _playersItemPrefab;

	[SerializeField]
	private TextMeshProUGUI[] _firstScore;

	[SerializeField]
	private TextMeshProUGUI[] _secondScore;

	[SerializeField]
	private Image[] _scoreTeams;

	[SerializeField]
	private TextMeshProUGUI _cup;

	private const int MaxItemsCount = 5;

	private UserCompetitionPublic _ugc;

	private List<RankingsListItemInit> _data = new List<RankingsListItemInit>();

	private TextMeshProUGUI[][] _teamsScore = new TextMeshProUGUI[2][];

	private bool _isInited;
}
