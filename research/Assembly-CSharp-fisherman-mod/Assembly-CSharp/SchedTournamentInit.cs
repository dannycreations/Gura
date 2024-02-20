using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class SchedTournamentInit : TournamentInitBase
{
	internal void Awake()
	{
		this.ApplyText.SetActive(false);
	}

	private void OnDestroy()
	{
		if (this.subscribedForResults)
		{
			this.ResultsUpdateUnsubscribe();
		}
		if (this.subscribedForInfoUpdate)
		{
			this.InfoUpdateUnsubscribe();
		}
	}

	public void Init(Tournament tournament)
	{
		if (this.TournamentPanel != null)
		{
			this.TournamentPanel.SetActive(true);
		}
		this._currentTournament = tournament;
		if (this._currentTournament.LogoBID != null)
		{
			this.ImageLdbl.Image = this.Image;
			this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", this._currentTournament.LogoBID));
		}
		string text = string.Empty;
		if (tournament.IsEnded && !tournament.IsRegistered)
		{
			text = ScriptLocalization.Get("FinishedCaption");
		}
		if (tournament.RegistrationStart > PhotonConnectionFactory.Instance.ServerUtcNow)
		{
			text = string.Format(ScriptLocalization.Get("RegTimeText"), MeasuringSystemManager.TimeString(tournament.RegistrationStart.ToLocalTime())) + "\n" + string.Format(ScriptLocalization.Get("StartTimeText"), MeasuringSystemManager.TimeString(tournament.StartDate.ToLocalTime()));
		}
		if (tournament.RegistrationStart < PhotonConnectionFactory.Instance.ServerUtcNow && tournament.StartDate > PhotonConnectionFactory.Instance.ServerUtcNow && !tournament.IsRegistered)
		{
			text = string.Format(ScriptLocalization.Get("RegTimeText"), "<color=#f4da00ff><b>" + ScriptLocalization.Get("OpenCaption") + "</b></color>") + "\n" + string.Format(ScriptLocalization.Get("StartTimeText"), MeasuringSystemManager.TimeString(tournament.StartDate.ToLocalTime()));
		}
		if (tournament.RegistrationStart < PhotonConnectionFactory.Instance.ServerUtcNow && tournament.StartDate > PhotonConnectionFactory.Instance.ServerUtcNow && tournament.IsRegistered)
		{
			text = "<color=#f4da00ff><b>" + ScriptLocalization.Get("TournamentSignedCaption") + "</b></color>\n" + string.Format(ScriptLocalization.Get("StartTimeText"), MeasuringSystemManager.TimeString(tournament.StartDate.ToLocalTime()));
			this.InfoImage.color = this.color1;
		}
		if (tournament.RegistrationStart < PhotonConnectionFactory.Instance.ServerUtcNow && tournament.StartDate < PhotonConnectionFactory.Instance.ServerUtcNow && !tournament.IsRegistered && !tournament.IsEnded)
		{
			text = "<color=red><b>" + ScriptLocalization.Get("RegClosedCaption") + "</b></color>\n" + ScriptLocalization.Get("RunningStatusText");
		}
		if (tournament.RegistrationStart < PhotonConnectionFactory.Instance.ServerUtcNow && tournament.StartDate < PhotonConnectionFactory.Instance.ServerUtcNow && tournament.IsRegistered && !tournament.IsDone && !tournament.IsEnded)
		{
			text = "<color=white><b>" + ScriptLocalization.Get("StartedCaption") + "</b></color>\n" + string.Format(ScriptLocalization.Get("EndTimeText"), MeasuringSystemManager.TimeString(tournament.EndDate.ToLocalTime()));
			this.InfoImage.color = this.color1;
		}
		if (tournament.RegistrationStart < PhotonConnectionFactory.Instance.ServerUtcNow && tournament.StartDate < PhotonConnectionFactory.Instance.ServerUtcNow && tournament.IsRegistered && tournament.IsDone && !tournament.IsEnded)
		{
			text = "<color=white><b>" + ScriptLocalization.Get("CompletedCaption") + "</b></color>\n" + string.Format(ScriptLocalization.Get("EndTimeText"), MeasuringSystemManager.TimeString(tournament.EndDate.ToLocalTime()));
			this.InfoImage.color = this.color2;
		}
		if (tournament.IsEnded && tournament.IsRegistered)
		{
			this.ResultsUpdateSubscribe();
			PhotonConnectionFactory.Instance.GetFinalTournamentResult(tournament.TournamentId);
		}
		else
		{
			this.InfoText.text = text;
		}
	}

	private void ResultsUpdateSubscribe()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentFinalResult += this.OnGotTournamentFinalResult1;
		this.subscribedForResults = true;
	}

	private void ResultsUpdateUnsubscribe()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentFinalResult -= this.OnGotTournamentFinalResult1;
		this.subscribedForResults = false;
	}

	private void InfoUpdateUnsubscribe()
	{
		PhotonConnectionFactory.Instance.OnGotTournament -= this.PhotonServerOnGotTournament;
		PhotonConnectionFactory.Instance.OnGettingTournamentFailed -= this.InstanceOnGettingTournamentFailed;
		this.subscribedForInfoUpdate = false;
	}

	private void InfoUpdateSubscribe()
	{
		PhotonConnectionFactory.Instance.OnGotTournament += this.PhotonServerOnGotTournament;
		PhotonConnectionFactory.Instance.OnGettingTournamentFailed += this.InstanceOnGettingTournamentFailed;
		this.subscribedForInfoUpdate = true;
	}

	private void OnGotTournamentFinalResult1(List<TournamentIndividualResults> results, int num)
	{
		if (results != null && results.Any<TournamentIndividualResults>() && this._currentTournament != null && results[0].TournamentId == this._currentTournament.TournamentId)
		{
			this.ResultsUpdateUnsubscribe();
			this.InfoImage.color = this.color3;
			TournamentIndividualResults tournamentIndividualResults = results.FirstOrDefault((TournamentIndividualResults r) => r.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
			if (tournamentIndividualResults == null)
			{
				this.InfoText.text = ScriptLocalization.Get("YourResultCaption") + " " + ScriptLocalization.Get("None");
				return;
			}
			if (tournamentIndividualResults.Place < 4)
			{
				this.Icon.SetActive(true);
				this.InfoText.text = ScriptLocalization.Get("YourResultCaption") + " " + tournamentIndividualResults.Place;
			}
			else
			{
				Text infoText = this.InfoText;
				string text = ScriptLocalization.Get("YourResultCaption");
				string text2 = " ";
				int? place = tournamentIndividualResults.Place;
				infoText.text = text + text2 + ((place == null) ? "-" : tournamentIndividualResults.Place.Value.ToString());
			}
		}
	}

	public override void OnClick()
	{
		PhotonConnectionFactory.Instance.CaptureActionInStats("TournamentDetailsSchedule", this._currentTournament.TournamentId.ToString(), null, null);
		this._messageBoxInfo = TournamentHelper.ShowingTournamentDetails(this._currentTournament, false);
		this._messageBoxInfo.GetComponent<TournamentDetailsMessage>().HaventMoneyClickBuy += this.SchedTournamentInit_HaventMoneyClickBuy;
		this._messageBoxInfo.GetComponent<TournamentDetailsMessage>().CloseOnClick += this.TournamentDetails_CloseOnClick;
	}

	private void SchedTournamentInit_HaventMoneyClickBuy(object sender, EventArgs e)
	{
		this._messageBoxInfo.GetComponent<TournamentDetailsMessage>().CloseClick();
	}

	private void TournamentDetails_CloseOnClick(object sender, MessageBoxEventArgs e)
	{
		this.InfoUpdateSubscribe();
		PhotonConnectionFactory.Instance.GetTournament(this._currentTournament.TournamentId);
	}

	private void InstanceOnGettingTournamentFailed(Failure failure)
	{
		this.InfoUpdateUnsubscribe();
		Debug.LogError(failure.FullErrorInfo);
	}

	private void PhotonServerOnGotTournament(Tournament tournament)
	{
		this.InfoUpdateUnsubscribe();
		this.Init(tournament);
	}

	public GameObject TournamentPanel;

	public Text InfoText;

	public GameObject Icon;

	public Color color1;

	public Color color2;

	public Color color3;

	protected GameObject _messageBoxInfo;

	private bool subscribedForResults;

	private bool subscribedForInfoUpdate;
}
