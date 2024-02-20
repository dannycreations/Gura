using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class HudTournamentIndividualManager : MonoBehaviour
{
	public TournamentIndividualResults YourResult { get; private set; }

	public void Init(ProfileTournament t)
	{
		if (this._tournament != t)
		{
			this._tournament = t;
			this.Clear();
		}
	}

	public void UpdateData(List<TournamentIndividualResults> results, int totalParticipants)
	{
		this.GotResult(results, totalParticipants);
	}

	private void Clear()
	{
		this._data.ForEach(delegate(RankingsListItemInit p)
		{
			Object.Destroy(p.gameObject);
		});
		this._data.Clear();
	}

	private void GotResult(List<TournamentIndividualResults> results, int totalParticipants)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null || this._tournament == null)
		{
			return;
		}
		this.YourResult = results.FirstOrDefault((TournamentIndividualResults r) => r.UserId == profile.UserId);
		results = results.OrderBy((TournamentIndividualResults p) => p2.Rank).Take(totalParticipants).ToList<TournamentIndividualResults>();
		bool flag = results.Contains(this.YourResult);
		if (!flag && this.YourResult != null)
		{
			results.Add(this.YourResult);
		}
		this.Check4Del(results);
		for (int i = 0; i < results.Count; i++)
		{
			TournamentIndividualResults p2 = results[i];
			RankingsListItemInit rankingsListItemInit = this._data.FirstOrDefault((RankingsListItemInit el) => el.Result.UserId == p2.UserId);
			if (rankingsListItemInit == null)
			{
				this.AddRankingsListItem().Init(p2, this._tournament);
			}
			else
			{
				rankingsListItemInit.UpdateResult(p2);
				rankingsListItemInit.UpdateInfo(p2.Rank.ToString(), this._tournament.PrimaryScoring.ScoringType, p2.Score, this._tournament.SecondaryScoring.ScoringType, p2.SecondaryScore, this._tournament.PrimaryScoring.TotalScoreKind);
			}
		}
		this._data = this._data.OrderBy((RankingsListItemInit p) => p.Result.Rank).ToList<RankingsListItemInit>();
		for (int j = 0; j < this._data.Count; j++)
		{
			this._data[j].SetSiblingIndex(j);
			this._data[j].SetNonLast();
		}
		if (!flag && this.YourResult != null)
		{
			int num = this._data.FindIndex((RankingsListItemInit p) => p.Result.UserId == profile.UserId);
			this._data[num - 1].SetLast();
		}
		if (this._data.Count > 0)
		{
			this._data.Last<RankingsListItemInit>().SetLastEmpty();
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

	private ProfileTournament _tournament;

	private List<RankingsListItemInit> _data = new List<RankingsListItemInit>();
}
