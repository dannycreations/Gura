using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;
using Photon.Interfaces.Tournaments;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoCompetitions : BaseCompetitions
{
	private string ShowingDate
	{
		get
		{
			return string.Format("{0}_{1}", this.CurrentShowingDate.ToShortDateString(), this.CurrentShowingDate.Hour);
		}
	}

	protected Tournament Tournament
	{
		get
		{
			CompetitionItem competitionItem = base.CurCompetition as CompetitionItem;
			return (!(competitionItem != null)) ? null : competitionItem.Tournament;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		TournamentManager instance = TournamentManager.Instance;
		instance.OnRefreshedTournaments = (Action)Delegate.Combine(instance.OnRefreshedTournaments, new Action(this.RefreshedTournaments));
		PhotonConnectionFactory.Instance.OnProductDelivered += this.Instance_OnProductDelivered;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.Instance_OnProductDelivered;
		if (TournamentManager.Instance != null)
		{
			TournamentManager instance = TournamentManager.Instance;
			instance.OnRefreshedTournaments = (Action)Delegate.Remove(instance.OnRefreshedTournaments, new Action(this.RefreshedTournaments));
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.Refresh();
		this._btnJoinLabel.text = ScriptLocalization.Get("ViewInfoTitle").ToUpper();
		PhotonConnectionFactory.Instance.OnGotTournaments += this.PhotonServerOnGotTournaments;
		PhotonConnectionFactory.Instance.OnGettingTournamentsFailed += this.PhotonServerOnGettingTournamentsFailed;
	}

	protected override void OnDisable()
	{
		this.UnsubscribeTournament();
		this.UnsubscribeTournamentWeather(false);
		this.UnsubscribeGotOpenTournaments();
		base.OnDisable();
	}

	private void PhotonServerOnGotTournaments(int? kind, List<Tournament> tournaments)
	{
		if (kind != null && kind.Value == 3)
		{
			List<Tournament> list = new List<Tournament>();
			List<Tournament> list2 = new List<Tournament>();
			for (int i = 0; i < tournaments.Count; i++)
			{
				Tournament t = tournaments[i];
				Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond p) => p.PondId == t.PondId);
				if (pond != null)
				{
					if (pond.PondPaidLocked())
					{
						LogHelper.Log("AutoCompetitions - skip competition /PondPaidLocked/ for pondId:{0} TournamentId:{1}", new object[] { t.PondId, t.TournamentId });
					}
					else if (t.IsEnded || t.IsCanceled || t.IsDone)
					{
						list.Add(t);
					}
					else
					{
						list2.Add(t);
					}
				}
			}
			list2 = list2.OrderBy((Tournament p) => p.StartDate).ToList<Tournament>();
			list2.AddRange(list.OrderBy((Tournament p) => p.StartDate));
			this._tournamentsCache.Keys.Where((string p) => p.StartsWith(this.CurrentShowingDate.ToShortDateString())).ToList<string>().ForEach(delegate(string p)
			{
				this._tournamentsCache.Remove(p);
			});
			this._tournamentsCache[this.ShowingDate] = list2;
			this.InitItems<Tournament, CompetitionItem>(this._tournamentsCache[this.ShowingDate]);
		}
	}

	protected override int GetItemTournamentId<T>(T t)
	{
		Tournament tournament = t as Tournament;
		if (tournament != null)
		{
			return tournament.TournamentId;
		}
		return 0;
	}

	protected override void InitItem<T, TN>(T t, TN item)
	{
		CompetitionItem competitionItem = item as CompetitionItem;
		Tournament tournament = t as Tournament;
		if (tournament != null && competitionItem != null)
		{
			competitionItem.Init(tournament);
		}
	}

	private void Instance_OnGotTournamentWeather(WeatherDesc[] weather)
	{
		this.UnsubscribeTournamentWeather(true);
		TournamentHelper.TournamentWeatherDesc tournamentWeather = TournamentHelper.GetTournamentWeather(this.Tournament, weather);
		if (tournamentWeather != null)
		{
			this.Temp.text = tournamentWeather.WeatherTemperature;
			this.TempIco.text = tournamentWeather.WeatherIcon;
			this.TempIco2.text = tournamentWeather.PressureIcon;
			this.TempWater.text = tournamentWeather.WaterTemperature;
			this.TempWaterIco.text = "\ue73a";
			this.Wind.text = string.Format("{0} {1} {2}", tournamentWeather.WeatherWindDirection, tournamentWeather.WeatherWindPower, tournamentWeather.WeatherWindSuffix);
		}
	}

	protected override void UpdateCurCompetition()
	{
		CompetitionItem competitionItem = base.CurCompetition as CompetitionItem;
		Tournament tournament = ((!(competitionItem != null)) ? null : competitionItem.Tournament);
		if (tournament != null)
		{
			this.Name.text = tournament.Name.ToUpper();
			this.ImageLdbl.Image = this.Image;
			this.ImageLdbl.Load((tournament.ImageBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", tournament.ImageBID.Value));
			int num = (tournament.InGameDuration * 60 + tournament.InGameDurationMinute) / 4;
			this._scoringTime.text = string.Format("{0} {1}", num, ScriptLocalization.Get("MinutesCaption"));
			DateTime dateTime = tournament.StartDate.ToLocalTime();
			DateTime dateTime2 = tournament.EndDate.ToLocalTime();
			string text = MeasuringSystemManager.TimeString(dateTime);
			string text2 = MeasuringSystemManager.TimeString(dateTime2);
			if (dateTime.Day == dateTime2.Day)
			{
				this._startTime.text = string.Format("{0} {1} - {2}", MeasuringSystemManager.DateTimeShortWithoutYear(dateTime), text, text2);
			}
			else
			{
				this._startTime.text = string.Format("{0} {1} - {2} {3}", new object[]
				{
					MeasuringSystemManager.DateTimeShortWithoutYear(dateTime),
					text,
					MeasuringSystemManager.DateTimeShortWithoutYear(dateTime2),
					text2
				});
			}
			this._rules.text = tournament.Rules;
			bool flag = (tournament.FreeForPremium && PhotonConnectionFactory.Instance.Profile.HasPremium) || tournament.EntranceFee <= 0.0;
			this.EntryCurrency.gameObject.SetActive(!flag);
			if (flag)
			{
				this.EntrySum.text = ScriptLocalization.Get("TournamentFeeFreeCaption");
			}
			else
			{
				this.EntrySum.text = tournament.EntranceFee.ToString();
				this.EntryCurrency.text = MeasuringSystemManager.GetCurrencyIcon(tournament.Currency);
			}
			PhotonConnectionFactory.Instance.OnGotTournamentWeather += this.Instance_OnGotTournamentWeather;
			PhotonConnectionFactory.Instance.OnGettingTournamentWeatherFailed += this.Instance_OnGettingTournamentWeatherFailed;
			PhotonConnectionFactory.Instance.GetTournamentWeather(tournament.TournamentId);
		}
	}

	protected override void Localize()
	{
		base.Localize();
		this._dateCaption.text = ScriptLocalization.Get("DateCaption");
		this._regTimeCaption.text = ScriptLocalization.Get("RegTimeTournamentInfo");
		this._competitionTimeCaption.text = ScriptLocalization.Get("StartTimeTournamentCaption");
		this._scoringTimeCaption.text = ScriptLocalization.Get("EntryTimeTournamentCaption");
		this._weatherForecastCaption.text = ScriptLocalization.Get("TournamentWeatherForecast");
		this._entryFeeCaption.text = ScriptLocalization.Get("EntryFeeTournamentCaption");
		this._rulesCaption.text = ScriptLocalization.Get("RulesAndRestrictionsTournamentCaption");
	}

	public override void Join()
	{
		Tournament t = this.Tournament;
		if (t != null)
		{
			GameObject gameObject = TournamentHelper.ShowingTournamentDetails(t, false);
			ITournamentDetails component = gameObject.GetComponent<ITournamentDetails>();
			component.ApplyOnClick += delegate(object sender, EventArgs e)
			{
				this.RefreshTournament(t.TournamentId);
			};
			component.OnUnregister += delegate
			{
				this.RefreshTournament(t.TournamentId);
			};
		}
	}

	private void RefreshTournament(int tournamentId)
	{
		PhotonConnectionFactory.Instance.OnGettingTournamentFailed += this.Instance_OnGettingTournamentFailed;
		PhotonConnectionFactory.Instance.OnGotTournament += this.Instance_OnGotTournament;
		PhotonConnectionFactory.Instance.GetTournament(tournamentId);
	}

	private void Instance_OnGotTournament(Tournament t)
	{
		this.UnsubscribeTournament();
		if (t != null && this.Data.ContainsKey(t.TournamentId))
		{
			((CompetitionItem)this.Data[t.TournamentId]).Init(t);
		}
	}

	private void PhotonServerOnGettingTournamentsFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	private void Instance_OnGettingTournamentFailed(Failure failure)
	{
		this.UnsubscribeTournament();
		Debug.LogError(failure.FullErrorInfo);
	}

	private void Instance_OnGettingTournamentWeatherFailed(Failure failure)
	{
		this.UnsubscribeTournamentWeather(true);
		Debug.LogError(failure.FullErrorInfo);
	}

	private void UnsubscribeGotOpenTournaments()
	{
		PhotonConnectionFactory.Instance.OnGotTournaments -= this.PhotonServerOnGotTournaments;
		PhotonConnectionFactory.Instance.OnGettingTournamentsFailed -= this.PhotonServerOnGettingTournamentsFailed;
	}

	private void UnsubscribeTournament()
	{
		PhotonConnectionFactory.Instance.OnGettingTournamentFailed -= this.Instance_OnGettingTournamentFailed;
		PhotonConnectionFactory.Instance.OnGotTournament -= this.Instance_OnGotTournament;
	}

	private void UnsubscribeTournamentWeather(bool playAnim = true)
	{
		if (playAnim)
		{
			this.SetActiveRightMenu(true);
		}
		PhotonConnectionFactory.Instance.OnGotTournamentWeather -= this.Instance_OnGotTournamentWeather;
		PhotonConnectionFactory.Instance.OnGettingTournamentWeatherFailed -= this.Instance_OnGettingTournamentWeatherFailed;
	}

	protected override void Refresh()
	{
		if (!this.IsInited)
		{
			this.IsInited = true;
			this.CurrentShowingDate = TimeHelper.UtcTime();
		}
		else
		{
			DateTime dateTime = TimeHelper.UtcTime();
			if (dateTime.Year == this.CurrentShowingDate.Year && dateTime.Month == this.CurrentShowingDate.Month && dateTime.Day == this.CurrentShowingDate.Day && dateTime.Hour != this.CurrentShowingDate.Hour)
			{
				this.CurrentShowingDate = dateTime;
			}
		}
		this.UpdateSelectedDt();
		if (this._tournamentsCache.ContainsKey(this.ShowingDate))
		{
			List<Tournament> list = this._tournamentsCache[this.ShowingDate];
			if (list.Count == 0)
			{
				this.TournamentId = null;
				this.SetActiveRightMenu(false);
			}
			this.InitItems<Tournament, CompetitionItem>(list);
		}
		else
		{
			PhotonConnectionFactory.Instance.GetTournaments(new TournamentKinds?(3), null, new DateTime?(this.CurrentShowingDate.StartOfDay()), new DateTime?(this.CurrentShowingDate.EndOfDay().AddTicks(2L)));
		}
	}

	private void RefreshedTournaments()
	{
		this._tournamentsCache.Clear();
	}

	private void Instance_OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		if (product.PaidPondsUnlocked != null)
		{
			this._tournamentsCache.Clear();
			this.Refresh();
		}
	}

	[Space(5f)]
	[SerializeField]
	private TextMeshProUGUI _dateCaption;

	[SerializeField]
	private TextMeshProUGUI _regTimeCaption;

	[Space(3f)]
	[SerializeField]
	private TextMeshProUGUI _competitionTimeCaption;

	[SerializeField]
	private TextMeshProUGUI _scoringTimeCaption;

	[SerializeField]
	private TextMeshProUGUI _weatherForecastCaption;

	[SerializeField]
	private TextMeshProUGUI _entryFeeCaption;

	[SerializeField]
	private TextMeshProUGUI _rulesCaption;

	[Space(5f)]
	[SerializeField]
	private TextMeshProUGUI _startTime;

	[SerializeField]
	private TextMeshProUGUI _endTime;

	[SerializeField]
	private TextMeshProUGUI _scoringTime;

	[SerializeField]
	private TextMeshProUGUI _rules;

	[Space(5f)]
	[SerializeField]
	private Text _btnJoinLabel;

	private readonly Dictionary<string, List<Tournament>> _tournamentsCache = new Dictionary<string, List<Tournament>>();
}
