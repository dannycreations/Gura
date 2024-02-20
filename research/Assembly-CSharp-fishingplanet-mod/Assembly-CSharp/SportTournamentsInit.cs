using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using ObjectModel.Tournaments;
using UnityEngine;
using UnityEngine.UI;

public class SportTournamentsInit : MainPageItem
{
	private void Awake()
	{
		PhotonConnectionFactory.Instance.OnProductDelivered += this.Instance_OnProductDelivered;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.Instance_OnProductDelivered;
	}

	protected override void SetHelp()
	{
		if (this.SelectedDate != null)
		{
			this.SelectedDate.text = string.Empty;
		}
		this.Refresh();
		if (this.TournamentsLoading != null)
		{
			this.TournamentsLoading.SetActive(true);
		}
	}

	private void Update()
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		if (this._requiredUpdate && this._grid != null && this._series != null)
		{
			this._requiredUpdate = false;
			this.FillTable();
		}
	}

	public void Refresh()
	{
		this.Clear();
		PhotonConnectionFactory.Instance.OnGotTournamentGrid += this.PhotonServerOnGotTournamentGrid;
		PhotonConnectionFactory.Instance.OnGettingTournamentGridFailed += this.PhotonServerOnGettingTournamentGridFailed;
		PhotonConnectionFactory.Instance.OnGotTournamentSerieInstances += this.PhotonServerOnGotTournamentSeries;
		PhotonConnectionFactory.Instance.OnGettingTournamentSerieInstancesFailed += this.PhotonServerOnGettingTournamentSeriesFailed;
		this._series = null;
		PhotonConnectionFactory.Instance.GetTournamentGrid();
		PhotonConnectionFactory.Instance.GetTournamentSerieInstances();
		this._requiredUpdate = true;
	}

	private void FillTable()
	{
		if (this.TournamentsLoading != null)
		{
			this.TournamentsLoading.SetActive(false);
		}
		List<TournamentSerieInstance> list = this._series.OrderBy((TournamentSerieInstance x) => x.StartDate).ToList<TournamentSerieInstance>();
		for (int i = 0; i < list.Count; i++)
		{
			this.FillSerie(list[i]);
		}
	}

	private void FillSerie(TournamentSerieInstance tournamentSerie)
	{
		SportTournamentsInit.<FillSerie>c__AnonStorey0 <FillSerie>c__AnonStorey = new SportTournamentsInit.<FillSerie>c__AnonStorey0();
		<FillSerie>c__AnonStorey.tournamentSerie = tournamentSerie;
		<FillSerie>c__AnonStorey.rows = (from x in this._grid
			where x.SerieId == <FillSerie>c__AnonStorey.tournamentSerie.SerieId
			group x by x.RowNumber).ToList<IGrouping<int, TournamentGridItem>>();
		int i;
		for (i = 0; i < <FillSerie>c__AnonStorey.rows.Count<IGrouping<int, TournamentGridItem>>(); i++)
		{
			GameObject gameObject = GUITools.AddChild(this.Content, this.RowPrefab);
			this.FillRow(gameObject, (from x in this._grid
				where x.SerieId == <FillSerie>c__AnonStorey.tournamentSerie.SerieId && x.RowNumber == <FillSerie>c__AnonStorey.rows[i].Key
				orderby x.ColumnNumber
				select x).ToList<TournamentGridItem>(), <FillSerie>c__AnonStorey.tournamentSerie);
		}
	}

	private void FillRow(GameObject rowContent, List<TournamentGridItem> grid, TournamentSerieInstance tournamentSerie)
	{
		SportTournamentsInit.<FillRow>c__AnonStorey2 <FillRow>c__AnonStorey = new SportTournamentsInit.<FillRow>c__AnonStorey2();
		<FillRow>c__AnonStorey.grid = grid;
		int i;
		for (i = 0; i < 5; i++)
		{
			GameObject gameObject = GUITools.AddChild(rowContent, this.GetPanelPrefab(<FillRow>c__AnonStorey.grid[i].PanelId.Value));
			gameObject.GetComponent<LayoutElement>().enabled = true;
			SportTournamentInit component = gameObject.GetComponent<SportTournamentInit>();
			if (component != null)
			{
				Tournament tournament = tournamentSerie.Stages.FirstOrDefault((Tournament x) => x.TemplateId == <FillRow>c__AnonStorey.grid[i].TemplateId);
				if (tournament != null)
				{
					component.Init(tournament, tournamentSerie);
				}
			}
			SerieTournamentInit component2 = gameObject.GetComponent<SerieTournamentInit>();
			if (component2 != null)
			{
				TournamentSerieInstance tournamentSerieInstance = this._series.FirstOrDefault((TournamentSerieInstance x) => x.SerieId == <FillRow>c__AnonStorey.grid[i].SerieId);
				component2.ApplyOnClick += this.serieInitScript_ApplyOnClick;
				component2.Init(tournamentSerieInstance);
			}
		}
	}

	private void serieInitScript_ApplyOnClick(object sender, EventArgs e)
	{
		this.Refresh();
	}

	private GameObject GetPanelPrefab(int panelId)
	{
		switch (panelId)
		{
		case 0:
			return this.EmptyPanelPrefab;
		case 1:
			return this.Empty2x2Panel;
		case 2:
			return this.SeriePanelPrefab;
		case 3:
			return this.TournamentUp1x1PanelPrefab;
		case 4:
			return this.Tournament1x0PanelPrefab;
		case 5:
			return this.Tournament1x1PanelPrefab;
		case 6:
			return this.Tournament3x1PanelPrefab;
		case 7:
			return this.Tournament3x3PanelPrefab;
		case 8:
			return this.TournamentDown1x1PanelPrefab;
		default:
			return this.EmptyPanelPrefab;
		}
	}

	private void Clear()
	{
		for (int i = 0; i < this.Content.transform.childCount; i++)
		{
			Object.Destroy(this.Content.transform.GetChild(i).gameObject);
		}
	}

	private void PhotonServerOnGotTournamentGrid(List<TournamentGridItem> grid)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentGrid -= this.PhotonServerOnGotTournamentGrid;
		PhotonConnectionFactory.Instance.OnGettingTournamentGridFailed -= this.PhotonServerOnGettingTournamentGridFailed;
		this._grid = grid;
	}

	private void PhotonServerOnGotTournamentSeries(List<TournamentSerieInstance> series)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentSerieInstances -= this.PhotonServerOnGotTournamentSeries;
		PhotonConnectionFactory.Instance.OnGettingTournamentSeriesFailed -= this.PhotonServerOnGettingTournamentSeriesFailed;
		this._series = series;
	}

	private void PhotonServerOnGettingTournamentGridFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentGrid -= this.PhotonServerOnGotTournamentGrid;
		PhotonConnectionFactory.Instance.OnGettingTournamentGridFailed -= this.PhotonServerOnGettingTournamentGridFailed;
		Debug.LogError(failure.FullErrorInfo);
	}

	private void PhotonServerOnGettingTournamentSeriesFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentSerieInstances -= this.PhotonServerOnGotTournamentSeries;
		PhotonConnectionFactory.Instance.OnGettingTournamentSerieInstancesFailed -= this.PhotonServerOnGettingTournamentSeriesFailed;
		Debug.LogError(failure.FullErrorInfo);
	}

	private void Instance_OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		if (product.PaidPondsUnlocked != null)
		{
			this.Refresh();
		}
	}

	public GameObject RowPrefab;

	public GameObject Content;

	public GameObject EmptyPanelPrefab;

	public GameObject Empty2x2Panel;

	public GameObject SeriePanelPrefab;

	public GameObject TournamentUp1x1PanelPrefab;

	public GameObject Tournament1x0PanelPrefab;

	public GameObject Tournament1x1PanelPrefab;

	public GameObject Tournament3x1PanelPrefab;

	public GameObject Tournament3x3PanelPrefab;

	public GameObject TournamentDown1x1PanelPrefab;

	public GameObject TournamentsLoading;

	public Text SelectedDate;

	private List<TournamentSerieInstance> _series;

	private List<TournamentGridItem> _grid;

	private bool _requiredUpdate;
}
