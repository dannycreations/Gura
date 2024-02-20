using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.PlayerProfile;
using I2.Loc;
using LeaderboardSRIA.ViewsHolders;
using ObjectModel;
using Photon.Interfaces.LeaderBoards;
using TMPro;
using UnityEngine;

namespace Leaderboard
{
	public class PlayersPage : Top100CommonPage
	{
		protected override Top100Proxy CurrentProxy
		{
			get
			{
				return this._proxies[this._curSortOrder];
			}
		}

		private bool IsMyRespond(TopLeadersResult result)
		{
			return result.Kind == 1;
		}

		protected override void Awake()
		{
			base.Awake();
			string text = ScriptLocalization.Get("UGC_FiltersCaption").ToLower();
			this._filters.text = char.ToUpper(text[0]) + text.Substring(1);
			this._captionNumber.text = ScriptLocalization.Get("NumberColumnCaption");
			this._captionName.text = PlayerProfileHelper.UsernameCaption;
			this._captionLevel.text = ScriptLocalization.Get("LevelMenuCaption");
			this._captionRank.text = ScriptLocalization.Get("RankMenuCaption");
			this._captionFishCount.text = ScriptLocalization.Get("FishesColumnCaption");
			this._captionTrophyCount.text = ScriptLocalization.Get("TrophyColumnCaption");
			this._captionUniqueCount.text = ScriptLocalization.Get("UniqueColumnCaption");
			this._captionExperience.text = ((this._curSortOrder != 7) ? ScriptLocalization.Get("ExpCaption") : ScriptLocalization.Get("WeeklyExpCaption"));
			this.selectorSort = Object.Instantiate<ScrollSelectorInit>(this.selectorPrefab, this.selectorParent);
			List<IScrollSelectorElement> list = this._sortingMap.Select((KeyValuePair<string, TopPlayerOrder> x) => new SortSelectorElement
			{
				Sort = x.Value,
				Text = ScriptLocalization.Get(x.Key).ToUpper()
			}).ToList<IScrollSelectorElement>();
			this.selectorSort.Init(ScriptLocalization.Get("ChooseSorting").ToUpper(), list, 0, string.Empty, new Action(this.Refresh));
		}

		private void Refresh()
		{
			this._curSortOrder = (this.selectorSort.GetCurrentValue() as SortSelectorElement).Sort;
			this.RefreshPage();
		}

		protected override void InitProxy()
		{
			this._proxies = new Dictionary<TopPlayerOrder, Top100Proxy>
			{
				{
					2,
					this.AddProxy(2)
				},
				{
					4,
					this.AddProxy(4)
				},
				{
					5,
					this.AddProxy(5)
				},
				{
					6,
					this.AddProxy(6)
				},
				{
					7,
					this.AddProxy(7)
				}
			};
		}

		private Top100Proxy AddProxy(TopPlayerOrder category)
		{
			Top100Proxy top100Proxy = new Top100Proxy(string.Format("PlayersPage({0})", category), new Action(this.SendServerRequest), new ServerProxy<TopLeadersResult>.IsMyRespond(this.IsMyRespond), new int[] { 17, 47 });
			top100Proxy.ERequestFailure += this.OnGetLeaderboardsFailed;
			top100Proxy.ERespond += this.OnGetLeaderboards;
			return top100Proxy;
		}

		private void SendServerRequest()
		{
			base.ShowEmpty(false);
			base.ShowLoading(true);
			this.Displayer.LoadEntriesList(null);
			PhotonConnectionFactory.Instance.GetTopPlayers(this._scope, this._curSortOrder);
		}

		private void OnGetLeaderboards(TopLeadersResult result)
		{
			this._firstLaunch = false;
			base.CancelLoading();
			this._captionExperience.text = ((this._curSortOrder != 7) ? ScriptLocalization.Get("ExpCaption") : ScriptLocalization.Get("WeeklyExpCaption"));
			PlayerItemVH.IsWeeklyExp = this._curSortOrder == 7;
			base.ShowLoading(false);
			base.ShowEmpty(result == null || result.Players == null || result.Players.ToList<TopPlayers>().Count == 0);
			this.Displayer.LoadEntriesList(result.Players.Cast<TopPlayerBase>());
		}

		private void OnDisable()
		{
			base.CancelLoading();
		}

		protected override void OnDestroy()
		{
			base.StopAllCoroutines();
			foreach (Top100Proxy top100Proxy in this._proxies.Values)
			{
				top100Proxy.ERequestFailure -= this.OnGetLeaderboardsFailed;
				top100Proxy.ERespond -= this.OnGetLeaderboards;
			}
			base.OnDestroy();
		}

		[SerializeField]
		private PlayerLeaderboard Displayer;

		public ScrollSelectorInit selectorPrefab;

		public Transform selectorParent;

		private ScrollSelectorInit selectorSort;

		[SerializeField]
		private TopScope _scope;

		[SerializeField]
		private TextMeshProUGUI _filters;

		[SerializeField]
		private TextMeshProUGUI _captionNumber;

		[SerializeField]
		private TextMeshProUGUI _captionName;

		[SerializeField]
		private TextMeshProUGUI _captionLevel;

		[SerializeField]
		private TextMeshProUGUI _captionRank;

		[SerializeField]
		private TextMeshProUGUI _captionExperience;

		[SerializeField]
		private TextMeshProUGUI _captionFishCount;

		[SerializeField]
		private TextMeshProUGUI _captionTrophyCount;

		[SerializeField]
		private TextMeshProUGUI _captionUniqueCount;

		private readonly Dictionary<string, TopPlayerOrder> _sortingMap = new Dictionary<string, TopPlayerOrder>
		{
			{ "WeeklyExpCaption", 7 },
			{ "ExpCaption", 2 },
			{ "FishCaption", 4 },
			{ "TrophyCaptionOld", 5 },
			{ "UniqueCaption", 6 }
		};

		private Dictionary<TopPlayerOrder, Top100Proxy> _proxies;

		private TopPlayerOrder _curSortOrder = 7;
	}
}
