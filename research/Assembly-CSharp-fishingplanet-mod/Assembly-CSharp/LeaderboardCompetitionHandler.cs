using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using Photon.Interfaces.LeaderBoards;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LeaderboardCompetitionHandler : MonoBehaviour
{
	internal void Start()
	{
		base.GetComponent<AlphaFade>().ShowFinished += this.LeaderboardCompetitionHandler_ShowFinished;
		this.SelfToggle.onValueChanged.AddListener(new UnityAction<bool>(this.SelfToggleChanged));
	}

	private void LeaderboardCompetitionHandler_ShowFinished(object sender, EventArgs e)
	{
		this.Refresh();
	}

	private void Refresh()
	{
		TopScope topScope = 1;
		PhotonConnectionFactory.Instance.OnGotLeaderboards += this.OnGotLeaderboards;
		PhotonConnectionFactory.Instance.OnGettingLeaderboardsFailed += this.OnGettingLeaderboardsFailed;
		PhotonConnectionFactory.Instance.GetTopTournamentPlayers(topScope, 3);
	}

	private void OnGotLeaderboards(TopLeadersResult result)
	{
		PhotonConnectionFactory.Instance.OnGotLeaderboards -= this.OnGotLeaderboards;
		if (result.TournamentKind != null && result.TournamentKind.Value == 3)
		{
			this.Clear();
			this.FillData(result);
		}
	}

	private void Clear()
	{
		if (this.ContentPanel != null)
		{
			for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
			{
				this.ContentPanel.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
	}

	private void FillData(TopLeadersResult result)
	{
		int num = 0;
		if (result.Tournaments != null)
		{
			int num2 = 0;
			TopTournamentPlayers self = result.Tournaments.FirstOrDefault((TopTournamentPlayers x) => x.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
			IList<TopTournamentPlayers> list = new List<TopTournamentPlayers>();
			if (!this.SelfToggle.isOn || result.Tournaments.Count<TopTournamentPlayers>() <= 100)
			{
				if (result.Tournaments.Count<TopTournamentPlayers>() <= 100)
				{
					list = result.Tournaments.Distinct<TopTournamentPlayers>().ToList<TopTournamentPlayers>();
				}
				else
				{
					list = result.Tournaments.Distinct<TopTournamentPlayers>().ToList<TopTournamentPlayers>().GetRange(0, 100);
				}
			}
			else if (self != null)
			{
				list = result.Tournaments.Where((TopTournamentPlayers x) => x.CompetitionRank > self.CompetitionRank || (x.CompetitionRank <= self.CompetitionRank && x.CompetitionRank > self.CompetitionRank - 20)).Distinct<TopTournamentPlayers>().ToList<TopTournamentPlayers>();
			}
			foreach (TopTournamentPlayers topTournamentPlayers in list.OrderBy((TopTournamentPlayers x) => x.CompetitionRank))
			{
				GameObject gameObject = this.ContentPanel.transform.GetChild(num2).gameObject;
				gameObject.SetActive(true);
				if (num % 2 != 0)
				{
					gameObject.GetComponent<Image>().enabled = true;
				}
				gameObject.GetComponent<ConcreteTopPlayerCompetition>().InitItem(topTournamentPlayers);
				num++;
				num2++;
			}
			if (self != null)
			{
				if (self.CompetitionRank > 100)
				{
					if (this.SelfToggle.GetComponent<ToggleStateChanges>().Disabled)
					{
						this.SelfToggle.GetComponent<ToggleStateChanges>().Enable();
					}
				}
				else
				{
					this.SelfToggle.GetComponent<ToggleStateChanges>().Disable();
				}
				this.SelfRecord.InitItem(self);
			}
			else
			{
				this.SelfRecord.InitItem(null);
				this.SelfToggle.GetComponent<ToggleStateChanges>().Disable();
			}
			if (!this.SelfToggle.isOn || self == null)
			{
				this.ScrollBar.value = 1f;
			}
			else
			{
				float num3 = (float)list.IndexOf(self) * this.ContentPanel.transform.GetChild(num2).GetComponent<RectTransform>().sizeDelta.y;
				this.ContentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, num3);
			}
		}
		this.LoadingText.SetActive(false);
	}

	private void OnGettingLeaderboardsFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGettingLeaderboardsFailed -= this.OnGettingLeaderboardsFailed;
	}

	private void SelfToggleChanged(bool value)
	{
		this.Refresh();
	}

	public Toggle SelfToggle;

	public GameObject ContentPanel;

	public GameObject LoadingText;

	public ConcreteTopPlayerCompetition SelfRecord;

	public Scrollbar ScrollBar;
}
