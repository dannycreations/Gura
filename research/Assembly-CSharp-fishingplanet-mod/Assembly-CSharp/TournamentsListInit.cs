using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TournamentsListInit : ActivityStateControlled
{
	protected override void Start()
	{
		base.Start();
		this._appliedLayoutElement = this.AppliedPanel.GetComponent<LayoutElement>();
		this._availableLayoutElement = this.AvailablePanel.GetComponent<LayoutElement>();
		this._minContentHeight = this.ContentRectTransform.rect.height;
		this.Scrollbar.value = 1f;
		PhotonConnectionFactory.Instance.OnGotMyTournaments += this.PhotonServerOnGotMyTournaments;
		PhotonConnectionFactory.Instance.OnGettingMyTournamentsFailed += this.PhotonServerOnGettingMyTournamentsFailed;
		PhotonConnectionFactory.Instance.OnGotOpenTournaments += this.PhotonServerOnGotOpenTournaments;
		PhotonConnectionFactory.Instance.OnGettingOpenTournamentsFailed += this.PhotonServerOnGettingOpenTournamentsFailed;
		this.DetailsPanel.ApplyOnClick += this.DetailsPanel_ApplyOnClick;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnGotMyTournaments -= this.PhotonServerOnGotMyTournaments;
		PhotonConnectionFactory.Instance.OnGettingMyTournamentsFailed -= this.PhotonServerOnGettingMyTournamentsFailed;
		PhotonConnectionFactory.Instance.OnGotOpenTournaments -= this.PhotonServerOnGotOpenTournaments;
		PhotonConnectionFactory.Instance.OnGettingOpenTournamentsFailed -= this.PhotonServerOnGettingOpenTournamentsFailed;
	}

	internal void Update()
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		if (!this._listInited && this._openTournaments != null && this._myTournaments != null)
		{
			this._listInited = true;
			this.TournamentListLoading.SetActive(false);
			this.InitApplied(this._myTournaments.Where((Tournament x) => !x.IsEnded).ToList<Tournament>());
			this.InitAvailable(this._openTournaments.Where((Tournament x) => x.SerieId == null).ToList<Tournament>());
			this.SetActivateFirstTournament();
		}
		float num = this._appliedLayoutElement.preferredHeight + this._availableLayoutElement.preferredHeight;
		num = Mathf.Max(num, this._minContentHeight);
		if (Math.Abs(num - this.ContentRectTransform.rect.height) > 0.5f)
		{
		}
	}

	protected override void SetHelp()
	{
		UIStatsCollector.ChangeGameScreen(GameScreenType.Schedule, GameScreenTabType.Undefined, null, null, null, null, null);
		this.Refresh();
		PhotonConnectionFactory.Instance.CaptureActionInStats("TournamentDashboard", null, null, null);
	}

	public void Refresh()
	{
		if (this.AvailableContentPanel.transform.childCount == 0)
		{
			this.TournamentListLoading.SetActive(true);
		}
		this._openTournaments = null;
		this._myTournaments = null;
		this._listInited = false;
		this._firstToggleInList = null;
		PhotonConnectionFactory.Instance.GetMyTournaments(null);
		PhotonConnectionFactory.Instance.GetOpenTournaments(null, null);
	}

	private void InitAvailable(List<Tournament> tournaments)
	{
		List<Tournament> list = tournaments.OrderBy((Tournament x) => x.StartDate).ToList<Tournament>();
		this.ClearAvailable();
		this.AvailableToggle.isOn = true;
		float num = (float)(this.AvailableContentPanel.GetComponent<VerticalLayoutGroup>().padding.top + this.AvailableContentPanel.GetComponent<VerticalLayoutGroup>().padding.bottom) + this.AvailableContentPanel.GetComponent<VerticalLayoutGroup>().spacing * (float)(list.Count - 1) + this.TournamentListItemPrefab.GetComponent<LayoutElement>().preferredHeight * (float)list.Count;
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = GUITools.AddChild(this.AvailableContentPanel, this.TournamentListItemPrefab);
			gameObject.GetComponent<TournamentListItemHandler>().Init(list[i], TournamentsListInit.IsOdd(i + 1));
			gameObject.GetComponent<TournamentListItemHandler>().Toggle.group = base.GetComponent<ToggleGroup>();
			gameObject.GetComponent<TournamentListItemHandler>().IsSelectedItem += this.TournamentsListInit_IsSelectedItem;
			if (this._firstToggleInList == null)
			{
				this._firstToggleInList = gameObject.GetComponent<TournamentListItemHandler>().Toggle;
			}
		}
		this.AvailablePanel.GetComponent<LayoutElement>().preferredHeight = 50f + num;
		this.ContentRectTransform.anchoredPosition = new Vector3(0f, 0f, 0f);
	}

	private void InitApplied(List<Tournament> tournaments)
	{
		this.ClearApplied();
		List<Tournament> list = tournaments.OrderBy((Tournament x) => x.StartDate).ToList<Tournament>();
		this.AppliedToggle.isOn = true;
		float num = (float)(this.AppliedContentPanel.GetComponent<VerticalLayoutGroup>().padding.top + this.AppliedContentPanel.GetComponent<VerticalLayoutGroup>().padding.bottom) + this.AppliedContentPanel.GetComponent<VerticalLayoutGroup>().spacing * (float)(list.Count - 1) + this.TournamentListItemPrefab.GetComponent<LayoutElement>().preferredHeight * (float)list.Count;
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = GUITools.AddChild(this.AppliedContentPanel, this.TournamentListItemPrefab);
			gameObject.GetComponent<TournamentListItemHandler>().Init(list[i], TournamentsListInit.IsOdd(i + 1));
			gameObject.GetComponent<TournamentListItemHandler>().Toggle.group = base.GetComponent<ToggleGroup>();
			gameObject.GetComponent<TournamentListItemHandler>().IsSelectedItem += this.TournamentsListInit_IsSelectedItem;
			if (this._firstToggleInList == null)
			{
				this._firstToggleInList = gameObject.GetComponent<TournamentListItemHandler>().Toggle;
			}
		}
		this.AppliedPanel.GetComponent<LayoutElement>().preferredHeight = 50f + num;
		this.ContentRectTransform.anchoredPosition = new Vector3(0f, 0f, 0f);
	}

	private void SetActivateFirstTournament()
	{
		if (this._firstToggleInList != null)
		{
			bool mute = PlayButtonEffect.Mute;
			PlayButtonEffect.Mute = true;
			this._firstToggleInList.isOn = true;
			PlayButtonEffect.Mute = mute;
		}
	}

	private void TournamentsListInit_IsSelectedItem(object sender, TournamentEventArgs e)
	{
		this.DetailsPanel.Init(e.Tournament);
		PhotonConnectionFactory.Instance.CaptureActionInStats("TournamentDetailsDashboard", e.Tournament.TournamentId.ToString(), null, null);
	}

	private void DetailsPanel_ApplyOnClick(object sender, EventArgs e)
	{
		this.Refresh();
	}

	private RectTransform ContentRectTransform
	{
		get
		{
			RectTransform rectTransform;
			if ((rectTransform = this._contentRectTransform) == null)
			{
				rectTransform = (this._contentRectTransform = this.ContentPanel.GetComponent<RectTransform>());
			}
			return rectTransform;
		}
	}

	public void OnRollupAvailable()
	{
		this.AvailableContentPanel.gameObject.SetActive(this.AvailableToggle.isOn);
		this.AvailablePanel.GetComponent<LayoutElement>().preferredHeight = 50f + ((!this.AvailableToggle.isOn) ? 0f : this.AvailableContentPanel.GetComponent<RectTransform>().rect.height);
	}

	public void OnRollupApplied()
	{
		this.AppliedContentPanel.gameObject.SetActive(this.AppliedToggle.isOn);
		this.AppliedPanel.GetComponent<LayoutElement>().preferredHeight = 50f + ((!this.AppliedToggle.isOn) ? 0f : this.AppliedContentPanel.GetComponent<RectTransform>().rect.height);
	}

	private void ClearAvailable()
	{
		for (int i = 0; i < this.AvailableContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.AvailableContentPanel.transform.GetChild(i).gameObject);
		}
	}

	private void ClearApplied()
	{
		for (int i = 0; i < this.AppliedContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.AppliedContentPanel.transform.GetChild(i).gameObject);
		}
	}

	private static bool IsOdd(int value)
	{
		return value % 2 != 0;
	}

	private void SetSizeForContent(float height, GameObject panel)
	{
		RectTransform component = panel.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.sizeDelta.x, height);
		this.Scrollbar.gameObject.SetActive(this.ContentRectTransform.rect.height - 1f > this._minContentHeight);
	}

	private void PhotonServerOnGotOpenTournaments(List<Tournament> tournaments)
	{
		this._openTournaments = tournaments.Where((Tournament x) => !x.IsRegistered).ToList<Tournament>();
		this._listInited = false;
	}

	private void PhotonServerOnGotMyTournaments(List<Tournament> tournaments)
	{
		this._myTournaments = tournaments;
		this._listInited = false;
	}

	private void PhotonServerOnGettingMyTournamentsFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	private void PhotonServerOnGettingOpenTournamentsFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	public GameObject TournamentListItemPrefab;

	public GameObject ContentPanel;

	public Scrollbar Scrollbar;

	public GameObject AppliedPanel;

	public GameObject AvailablePanel;

	public GameObject AppliedContentPanel;

	public GameObject AvailableContentPanel;

	public Toggle AppliedToggle;

	public Toggle AvailableToggle;

	public ActiveTournamentDetails DetailsPanel;

	public TournamentRankingsInit RankingsPanel;

	public GameObject TournamentListLoading;

	private LayoutElement _appliedLayoutElement;

	private LayoutElement _availableLayoutElement;

	private RectTransform _contentRectTransform;

	private float _minContentHeight;

	private Toggle _firstToggleInList;

	private List<Tournament> _openTournaments;

	private List<Tournament> _myTournaments;

	private bool _listInited;
}
