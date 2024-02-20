using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;
using ObjectModel.Tournaments;
using Photon.Interfaces.Tournaments;
using UnityEngine;
using UnityEngine.UI;

public class PondSubMenu : SubMenuFoldoutBase
{
	protected override void Awake()
	{
		base.Awake();
		this._previews[3] = this._competitionPreviews;
		this._previews[1] = this._tournametPreviews;
	}

	public override void SetOpened(bool opened, Action callback = null)
	{
		base.SetOpened(opened, callback);
		this.Init(opened);
	}

	private void OnEnable()
	{
		this.Init(base.Opened);
	}

	private void OnDisable()
	{
		if (this._subscribed)
		{
			this._subscribed = false;
			PhotonConnectionFactory.Instance.OnGotTournaments -= this.Instance_OnGotPondTournaments;
			PhotonConnectionFactory.Instance.OnGotPondStats -= this.OnGotPondStats;
			PhotonConnectionFactory.Instance.OnRegisteredForTournament -= this.RefreshTournamentData;
			PhotonConnectionFactory.Instance.OnRegisteredForTournamentSerie -= this.RefreshTournamentData;
		}
		this.DisablePrewievs();
	}

	private void Update()
	{
		this._timer += Time.deltaTime;
		if (this._timer >= 2f && this._curPond != null)
		{
			this._timer = 0f;
			this._lockEndTime = PondHelper.LockEndTime(this._curPond);
		}
		DateTime? lockEndTime = this._lockEndTime;
		if (lockEndTime != null && this._lockEndTime.Value > TimeHelper.UtcTime())
		{
			this._lockedInText.gameObject.SetActive(true);
			this._lockedInText.text = string.Format(ScriptLocalization.Get("PondLockIn"), this._lockEndTime.Value.GetTimeFinishInValue(false));
		}
		else
		{
			this._lockedInText.gameObject.SetActive(false);
		}
	}

	public void SetPond(Pond currentPond)
	{
		this._curPond = currentPond;
		this.Init(base.Opened);
	}

	private void Init(bool isOpened)
	{
		if (isOpened && this._curPond != null)
		{
			this._timer = 2f;
			if (!this._subscribed)
			{
				this._subscribed = true;
				PhotonConnectionFactory.Instance.OnGotPondStats += this.OnGotPondStats;
				PhotonConnectionFactory.Instance.OnGotTournaments += this.Instance_OnGotPondTournaments;
				PhotonConnectionFactory.Instance.OnRegisteredForTournament += this.RefreshTournamentData;
				PhotonConnectionFactory.Instance.OnRegisteredForTournamentSerie += this.RefreshTournamentData;
			}
			this.RefreshTournamentData();
		}
	}

	private void RefreshTournamentData()
	{
		PhotonConnectionFactory.Instance.GetPondStats(new int?(this._curPond.PondId));
		DateTime dateTime = TimeHelper.UtcTime();
		PhotonConnectionFactory.Instance.GetTournaments(null, new int?(this._curPond.PondId), new DateTime?(dateTime), new DateTime?(dateTime.AddDays(3.0)));
	}

	private void Instance_OnGotPondTournaments(int? kind, List<Tournament> tournaments)
	{
		this.OnGetTournaments(tournaments);
	}

	private void DisablePrewievs()
	{
		this._previews.Values.ToList<CompetitionsPreviewPanel[]>().ForEach(delegate(CompetitionsPreviewPanel[] p)
		{
			this.DisablePrewiev(p);
		});
	}

	private void DisablePrewiev(CompetitionsPreviewPanel[] previews)
	{
		previews.ToList<CompetitionsPreviewPanel>().ForEach(delegate(CompetitionsPreviewPanel p)
		{
			p.SetActive(false);
		});
	}

	private void OnGetTournaments(List<Tournament> tournaments)
	{
		if (ShowPondInfo.Instance.CurrentPond != null)
		{
			using (Dictionary<TournamentKinds, CompetitionsPreviewPanel[]>.Enumerator enumerator = this._previews.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<TournamentKinds, CompetitionsPreviewPanel[]> p = enumerator.Current;
					this.FillList(p.Value, from x in tournaments
						where x.KindId == p.Key
						orderby x.StartDate
						select x);
				}
			}
		}
		else
		{
			this.DisablePrewievs();
		}
	}

	private void OnGotPondStats(IList<PondStat> stats)
	{
		if (stats == null || stats.Count == 0)
		{
			return;
		}
		this._playersOnPond.text = string.Format(ScriptLocalization.Get("PlayersOnPond"), stats.First<PondStat>().Count, stats.First<PondStat>().FriendsCount);
	}

	private void FillList(CompetitionsPreviewPanel[] previews, IOrderedEnumerable<Tournament> tournaments)
	{
		int i = 0;
		using (IEnumerator<Tournament> enumerator = tournaments.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Tournament t = enumerator.Current;
				previews[i].ClearEvents();
				previews[i].OnShowDetails += delegate
				{
					bool hasSeries = TournamentHelper.HasSeries;
					if (!hasSeries || t.SerieInstanceId == null || t.IsRegistered)
					{
						TournamentHelper.ShowingTournamentDetails(t, false);
					}
					else if (hasSeries)
					{
						TournamentHelper.ShowingSerieDetails(TournamentHelper.SeriesInstances.FirstOrDefault((TournamentSerieInstance x) => x.SerieInstanceId == t.SerieInstanceId.Value));
					}
				};
				previews[i].SetActive(true);
				previews[i++].PushTournament(t);
				if (i == previews.Length)
				{
					break;
				}
			}
		}
		while (i < previews.Length)
		{
			previews[i].SetActive(false);
			i++;
		}
	}

	[SerializeField]
	private CompetitionsPreviewPanel[] _tournametPreviews;

	[SerializeField]
	private CompetitionsPreviewPanel[] _competitionPreviews;

	[SerializeField]
	private Text _playersOnPond;

	[SerializeField]
	private Text _lockedInText;

	public const double PondTournamentsDaysCount = 3.0;

	private const float MaxTime = 2f;

	private float _timer = 2f;

	private DateTime? _lockEndTime;

	private bool _subscribed;

	private Dictionary<TournamentKinds, CompetitionsPreviewPanel[]> _previews = new Dictionary<TournamentKinds, CompetitionsPreviewPanel[]>();

	private Pond _curPond;
}
