using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using Photon.Interfaces.Tournaments;
using UnityEngine;
using UnityEngine.UI;

public class ScheduleTournamentsHandler : ActivityStateControlled
{
	private string ShowingDate
	{
		get
		{
			return this._currentShowingDate.ToShortDateString();
		}
	}

	protected override void SetHelp()
	{
		this._tournamentsCache.Clear();
		PhotonConnectionFactory.Instance.CaptureActionInStats("CompetitionSchedule", null, null, null);
		this.SelectedDate.gameObject.SetActive(true);
		PhotonConnectionFactory.Instance.OnGotTournaments += this.PhotonServerOnGotTournaments;
		PhotonConnectionFactory.Instance.OnGettingTournamentsFailed += this.PhotonServerOnGettingTournamentsFailed;
		this.Refresh();
	}

	protected override void HideHelp()
	{
		PhotonConnectionFactory.Instance.OnGotTournaments -= this.PhotonServerOnGotTournaments;
		PhotonConnectionFactory.Instance.OnGettingTournamentsFailed -= this.PhotonServerOnGettingTournamentsFailed;
		this.SelectedDate.gameObject.SetActive(false);
		this.Waiting(false);
	}

	private void Refresh()
	{
		if (!this._isInited)
		{
			this._isInited = true;
			this._currentShowingDate = TimeHelper.UtcTime();
		}
		this.ChangeDayText();
		if (this._tournamentsCache.ContainsKey(this.ShowingDate))
		{
			this.Show(this._tournamentsCache[this.ShowingDate]);
		}
		else
		{
			this.Waiting(true);
			PhotonConnectionFactory.Instance.GetTournaments(new TournamentKinds?(3), null, new DateTime?(this._currentShowingDate.StartOfDay()), new DateTime?(this._currentShowingDate.EndOfDay().AddTicks(2L)));
		}
	}

	private void PhotonServerOnGotTournaments(int? kind, List<Tournament> tournaments)
	{
		this.Waiting(false);
		this.Show(tournaments);
	}

	private void Show(List<Tournament> dayTournaments)
	{
		ScheduleTournamentsHandler.<Show>c__AnonStorey2 <Show>c__AnonStorey = new ScheduleTournamentsHandler.<Show>c__AnonStorey2();
		<Show>c__AnonStorey.$this = this;
		this._tournamentsCache[this.ShowingDate] = dayTournaments;
		this.Clear();
		<Show>c__AnonStorey.pondList = (from p in dayTournaments
			where CacheLibrary.MapCache.CachedPonds.Any((Pond pond) => pond.PondId == p.PondId)
			select p into x
			orderby CacheLibrary.MapCache.CachedPonds.First((Pond y) => y.PondId == x.PondId).OriginalMinLevel
			select x.PondId).Distinct<int>().ToList<int>();
		int i;
		for (i = 0; i < <Show>c__AnonStorey.pondList.Count; i++)
		{
			GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.PondSchedule);
			List<Tournament> list = dayTournaments.Where((Tournament x) => x.PondId == <Show>c__AnonStorey.pondList[i] && x.StartDate.ToLocalTime().Date == <Show>c__AnonStorey.$this._currentShowingDate.ToLocalTime().Date).ToList<Tournament>();
			gameObject.GetComponent<SchedPondInit>().Init(list, <Show>c__AnonStorey.pondList[i], !ScheduleTournamentsHandler.IsOdd(i + 1));
		}
	}

	public void NextDayClick()
	{
		this._currentShowingDate = this._currentShowingDate.Add(TimeSpan.FromDays(1.0));
		this.Refresh();
	}

	public void PrevsDayClick()
	{
		this._currentShowingDate = this._currentShowingDate.Subtract(TimeSpan.FromDays(1.0));
		this.Refresh();
	}

	private void ChangeDayText()
	{
		this.SelectedDate.text = string.Format("{0}, {1}", MeasuringSystemManager.GetFullDayCaption(this._currentShowingDate), MeasuringSystemManager.DateTimeShortString(this._currentShowingDate));
		this.NextDayText.text = string.Format("{0} \ue62e", MeasuringSystemManager.GetShortDayCaption(this._currentShowingDate.Add(TimeSpan.FromDays(1.0))));
		try
		{
			this.PrevsDayText.text = string.Format("\ue632 {0}", MeasuringSystemManager.GetShortDayCaption(this._currentShowingDate.Subtract(TimeSpan.FromDays(1.0))));
		}
		catch (Exception)
		{
		}
		if ((this._currentShowingDate - DateTime.Now).TotalDays >= 6.0)
		{
			this.NextDayText.text = string.Empty;
			this.NextDayText.transform.parent.GetComponent<Button>().interactable = false;
		}
		else
		{
			this.NextDayText.transform.parent.GetComponent<Button>().interactable = true;
		}
		if ((DateTime.Now - this._currentShowingDate).TotalDays >= 6.0)
		{
			this.PrevsDayText.text = string.Empty;
			this.PrevsDayText.transform.parent.GetComponent<Button>().interactable = false;
		}
		else
		{
			this.PrevsDayText.transform.parent.GetComponent<Button>().interactable = true;
		}
	}

	private void Clear()
	{
		for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.ContentPanel.transform.GetChild(i).gameObject);
		}
	}

	private static bool IsOdd(int value)
	{
		return value % 2 != 0;
	}

	private void Waiting(bool flag)
	{
		if (this.TournamentsLoading != null)
		{
			this.TournamentsLoading.SetActive(flag);
		}
	}

	private void PhotonServerOnGettingTournamentsFailed(Failure failure)
	{
		this.Waiting(false);
		Debug.LogError(failure.FullErrorInfo);
	}

	public GameObject PondSchedule;

	public GameObject ContentPanel;

	public Scrollbar ContentScrollbar;

	public Text NextDayText;

	public Text PrevsDayText;

	public Text SelectedDate;

	public GameObject TournamentsLoading;

	private DateTime _currentShowingDate;

	private bool _isInited;

	private readonly Dictionary<string, List<Tournament>> _tournamentsCache = new Dictionary<string, List<Tournament>>();
}
