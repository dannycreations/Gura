using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;
using ObjectModel.Tournaments;
using UnityEngine;
using UnityEngine.UI;

public class SportTournamentInit : TournamentInitBase
{
	public void Init(Tournament tournament, TournamentSerieInstance serie)
	{
		this._currentTournament = tournament;
		this.Name.text = this._currentTournament.Name;
		if (this._currentTournament.LogoBID != null)
		{
			this.ImageLdbl.Image = this.Image;
			this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", this._currentTournament.LogoBID.Value));
		}
		this.StartTime.text = string.Format(ScriptLocalization.Get("StartTimeText"), MeasuringSystemManager.DateTimeString(this._currentTournament.StartDate.ToLocalTime()));
		this.SetBlockedTournament(this._currentTournament, serie);
	}

	public override void OnClick()
	{
		this._messageBoxInfo = TournamentHelper.ShowingTournamentDetails(this._currentTournament, false);
		this._messageBoxInfo.GetComponent<TournamentDetailsMessage>().HaventMoneyClickBuy += this.SchedTournamentInit_HaventMoneyClickBuy;
		this._messageBoxInfo.GetComponent<TournamentDetailsMessage>().CloseOnClick += this.TournamentDetails_CloseOnClick;
	}

	private void SetBlockedTournament(Tournament tournament, TournamentSerieInstance serie)
	{
		this.ClosedPanel.SetActive(false);
		if (!tournament.IsRegistered)
		{
			this.ClosedPanel.SetActive(true);
			this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue64a";
			if (serie.Stages.Any(delegate(Tournament x)
			{
				int? stageTypeId = x.StageTypeId;
				bool flag = stageTypeId != null;
				int? stageTypeId2 = tournament.StageTypeId;
				return (flag & (stageTypeId2 != null)) && stageTypeId.GetValueOrDefault() > stageTypeId2.GetValueOrDefault() && x.IsRegistered;
			}))
			{
				this.StartTime.text = string.Format("{0} {1}", ScriptLocalization.Get("QualifiedStatusText"), tournament.Place);
			}
		}
		if (tournament.IsEnded)
		{
			this.ClosedPanel.SetActive(true);
			this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue64a";
			this.StartTime.text = ScriptLocalization.Get("FinishedCaption");
			if (tournament.Place != null)
			{
				this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue61e";
				this.StartTime.text = string.Format("{0} {1}", ScriptLocalization.Get("YourResultCaption"), tournament.Place);
				if (tournament.Place <= tournament.WinnersCountAdmited2NextStage)
				{
					this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue64e";
					this.StartTime.text = ScriptLocalization.Get("QualifiedStatusText");
					if (tournament.Place == 1)
					{
						this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue64b";
						this.StartTime.text = ScriptLocalization.Get("QualifiedStatusText");
					}
					if (tournament.Place == 2)
					{
						this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue64c";
						this.StartTime.text = ScriptLocalization.Get("QualifiedStatusText");
					}
					if (tournament.Place == 3)
					{
						this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue64d";
						this.StartTime.text = ScriptLocalization.Get("QualifiedStatusText");
					}
				}
				if (tournament.StageTypeId == 3)
				{
					this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue64e";
					if (tournament.Place == 1)
					{
						this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue64b";
					}
					if (tournament.Place == 2)
					{
						this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue64c";
					}
					if (tournament.Place == 3)
					{
						this.ClosedPanel.transform.GetComponentInChildren<Text>().text = "\ue64d";
					}
				}
				if (tournament.IsDisqualified)
				{
					this.StartTime.text = ScriptLocalization.Get("DisqualifiedText");
				}
			}
		}
	}

	public override void PointerEnter()
	{
		this.ApplyText.SetActive(true);
	}

	private void SchedTournamentInit_HaventMoneyClickBuy(object sender, EventArgs e)
	{
		this._messageBoxInfo.GetComponent<TournamentDetailsMessage>().HaventMoneyClickBuy -= this.SchedTournamentInit_HaventMoneyClickBuy;
		this._messageBoxInfo.GetComponent<TournamentDetailsMessage>().CloseClick();
	}

	private void TournamentDetails_CloseOnClick(object sender, MessageBoxEventArgs e)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentSeries += this.PhotonServerOnGotTournamentSeries;
		PhotonConnectionFactory.Instance.OnGettingTournamentSeriesFailed += this.InstanceOnGettingTournamentSeriesFailed;
		PhotonConnectionFactory.Instance.GetTournamentSeries();
	}

	private void InstanceOnGettingTournamentSeriesFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentSeries -= this.PhotonServerOnGotTournamentSeries;
		PhotonConnectionFactory.Instance.OnGettingTournamentSeriesFailed -= this.InstanceOnGettingTournamentSeriesFailed;
		Debug.LogError(failure.FullErrorInfo);
	}

	private void PhotonServerOnGotTournamentSeries(List<TournamentSerie> series)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentSeries -= this.PhotonServerOnGotTournamentSeries;
		PhotonConnectionFactory.Instance.OnGettingTournamentSeriesFailed -= this.InstanceOnGettingTournamentSeriesFailed;
	}

	public GameObject ClosedPanel;

	public Text Name;

	protected GameObject _messageBoxInfo;
}
