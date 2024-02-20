using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.PlayerProfile;
using I2.Loc;
using ObjectModel;
using Photon.Interfaces.LeaderBoards;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Leaderboard
{
	public class CompetitionsPage : Top100CommonPage
	{
		protected override Top100Proxy CurrentProxy
		{
			get
			{
				return this._serverProxy;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			string text = ScriptLocalization.Get("UGC_FiltersCaption").ToLower();
			this._filters.text = char.ToUpper(text[0]) + text.Substring(1);
			string text2 = ScriptLocalization.Get("PerformanceCaption").ToLower();
			this._captionPerformance.text = char.ToUpper(text2[0]) + text2.Substring(1);
			this._captionNumber.text = ScriptLocalization.Get("NumberColumnCaption");
			this._captionName.text = PlayerProfileHelper.UsernameCaption;
			this._captionGamesPlayed.text = ScriptLocalization.Get("Played");
			this._captionLevel.text = ScriptLocalization.Get("LevelMenuCaption");
			this._captionRank.text = ScriptLocalization.Get("RankMenuCaption");
			this._captionWinsCount.text = ScriptLocalization.Get("Win");
			this._captionRating.text = ScriptLocalization.Get("Rating");
			this._scope = 1;
			this.SelfToggle.onValueChanged.AddListener(new UnityAction<bool>(this.SelfToggleChanged));
			this.selectorScope = Object.Instantiate<ScrollSelectorInit>(this.selectorPrefab, this.selectorParent);
			List<IScrollSelectorElement> list = new List<IScrollSelectorElement>
			{
				new LocalScopeSelectorElement
				{
					Scope = LocalSCope.All,
					Text = ScriptLocalization.Get("PlayerTopCaption").ToUpper()
				},
				new LocalScopeSelectorElement
				{
					Scope = LocalSCope.Me,
					Text = ScriptLocalization.Get("MyCaption").ToUpper()
				}
			};
			this.selectorScope.Init(ScriptLocalization.Get("UGC_PlaceResult").ToUpper(), list, 0, "ponds", new Action(this.RefreshPage));
		}

		private new void Start()
		{
			this.SelfRecord.InitItem(null);
		}

		protected override void InitProxy()
		{
			this._serverProxy = new Top100Proxy("CompetitionsPage", new Action(this.SendServerRequest), new ServerProxy<TopLeadersResult>.IsMyRespond(this.IsMyRespond), new int[] { 20, 50 });
			this._serverProxy.ERequestFailure += this.OnGetLeaderboardsFailed;
			this._serverProxy.ERespond += this.OnGetLeaderboards;
		}

		private bool IsMyRespond(TopLeadersResult result)
		{
			return result.TournamentKind != null && result.TournamentKind.Value == 3 && result.Tournaments != null;
		}

		private void SendServerRequest()
		{
			base.ShowEmpty(false);
			base.ShowLoading(true);
			this.Displayer.LoadEntriesList(null);
			PhotonConnectionFactory.Instance.GetTopTournamentPlayers(this._scope, 3);
		}

		private void SelfToggleChanged(bool value)
		{
			this.RefreshPage();
		}

		private TopTournamentPlayers CloneRecord(TopTournamentPlayers record, string userName, int rank)
		{
			return new TopTournamentPlayers
			{
				UserId = record.UserId,
				UserName = userName,
				AvatarBID = record.AvatarBID,
				Level = record.Level,
				Rank = record.Rank,
				Title = record.Title,
				Rating = record.Rating,
				CompetitionRank = rank,
				CompetitionRankChange = record.CompetitionRankChange,
				Played = record.Played,
				Won = record.Won
			};
		}

		private void OnGetLeaderboards(TopLeadersResult result)
		{
			this._firstLaunch = false;
			base.CancelLoading();
			TopTournamentPlayers self = result.Tournaments.FirstOrDefault((TopTournamentPlayers x) => x.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
			List<TopTournamentPlayers> list = new List<TopTournamentPlayers>();
			LocalSCope scope = (this.selectorScope.GetCurrentValue() as LocalScopeSelectorElement).Scope;
			if (scope == LocalSCope.Me && self != null && self.CompetitionRank > 100)
			{
				MonoBehaviour.print("Local scope");
				IEnumerable<TopTournamentPlayers> tournaments = result.Tournaments;
				list.AddRange(tournaments.Where((TopTournamentPlayers x) => x.CompetitionRank <= 50));
				list.Add(null);
				list.AddRange(tournaments.Where((TopTournamentPlayers x) => x.CompetitionRank >= self.CompetitionRank - 20));
				base.ShowEmpty(list.Count == 0);
				this.Displayer.LoadEntriesList(list.Cast<TopPlayerBase>());
				int num = list.IndexOf(self);
				this.Displayer.SelectAndScrollToIndex(num);
			}
			else
			{
				if (result.Tournaments.Count<TopTournamentPlayers>() <= 100)
				{
					list = result.Tournaments.ToList<TopTournamentPlayers>();
				}
				else
				{
					list = result.Tournaments.ToList<TopTournamentPlayers>().GetRange(0, 100);
				}
				base.ShowEmpty(list.Count == 0);
				this.Displayer.LoadEntriesList(list.Cast<TopPlayerBase>());
				this.Displayer.SetMine(self);
			}
		}

		protected override void OnDestroy()
		{
			this._serverProxy.ERequestFailure -= this.OnGetLeaderboardsFailed;
			this._serverProxy.ERespond -= this.OnGetLeaderboards;
			this.SelfToggle.onValueChanged.RemoveAllListeners();
			base.OnDestroy();
		}

		private const int PLAYERS_FROM_TOP100 = 50;

		private const int PLAYERS_BEFORE_MY = 20;

		public ScrollSelectorInit selectorPrefab;

		private ScrollSelectorInit selectorScope;

		public Transform selectorParent;

		[SerializeField]
		private Toggle SelfToggle;

		[SerializeField]
		private ConcreteTopPlayerCompetition SelfRecord;

		[SerializeField]
		private TextMeshProUGUI _filters;

		[SerializeField]
		private TextMeshProUGUI _captionNumber;

		[SerializeField]
		private TextMeshProUGUI _captionPerformance;

		[SerializeField]
		private TextMeshProUGUI _captionName;

		[SerializeField]
		private TextMeshProUGUI _captionLevel;

		[SerializeField]
		private TextMeshProUGUI _captionRank;

		[SerializeField]
		private TextMeshProUGUI _captionGamesPlayed;

		[SerializeField]
		private TextMeshProUGUI _captionWinsCount;

		[SerializeField]
		private TextMeshProUGUI _captionRating;

		public CompetitionsPlayerLeaderboard Displayer;

		private TopScope _scope;

		private List<ConcreteTopPlayerCompetition> _items;

		private Top100Proxy _serverProxy;
	}
}
