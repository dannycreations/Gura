using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using ObjectModel.Tournaments;
using Photon.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class TournamentSerieDetailsMessage : MessageBoxBase, ITournamentDetails
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<MessageBoxEventArgs> CloseOnClick;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> HaventMoneyClickBuy;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ApplyOnClick;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnUnregister = delegate
	{
	};

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(false);
		MessageFactory.MessageBoxQueue.Enqueue(this);
		this.ApplyOnClick += this.TournamentSerieDetailsMessage_ApplyOnClick;
	}

	private void TournamentSerieDetailsMessage_ApplyOnClick(object sender, EventArgs e)
	{
		this.ApplyButton.gameObject.SetActive(false);
		PhotonConnectionFactory.Instance.GetAvailablePonds(StaticUserData.CountryId);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.Unsubscribe();
		this.UnsubscribeEulaSigned();
	}

	protected void Update()
	{
		if (this._currentSerie != null)
		{
			if (this._currentSerie.StartDate < TimeHelper.UtcTime() && this.ApplyButton.interactable)
			{
				this.ApplyButton.interactable = false;
			}
			if (this._currentSerie.StartDate > TimeHelper.UtcTime())
			{
				if (this._currentSerie.RegistrationStart > TimeHelper.UtcTime())
				{
					this.StartTimeBellowLogo.text = string.Format(ScriptLocalization.Get("RegTimeTournamentOpensIn"), BaseTournamentDetails.GetTimerValue(this._currentSerie.RegistrationStart));
				}
				else
				{
					this.StartTimeBellowLogo.text = string.Format(ScriptLocalization.Get("StartTournamentTimeText"), BaseTournamentDetails.GetTimerValue(this._currentSerie.StartDate));
				}
			}
			else if (this._currentSerie.EndDate > TimeHelper.UtcTime())
			{
				this.StartTimeBellowLogo.text = string.Format(ScriptLocalization.Get("EndTournamentTimeText"), BaseTournamentDetails.GetTimerValue(this._currentSerie.EndDate));
			}
		}
	}

	internal void Init(TournamentSerieInstance serie)
	{
		this._currentSerie = serie;
		this.Title.text = this._currentSerie.Name;
		if (this._currentSerie.ImageBID != null)
		{
			this.LogoImageLoadable.Image = this.LogoImage;
			this.LogoImageLoadable.Load(string.Format("Textures/Inventory/{0}", this._currentSerie.ImageBID));
		}
		this.Description.text = string.Format(this._currentSerie.Desc, "<b><color=yellow>", "</color></b>", "\n");
		this.RulesText.text = this._currentSerie.Rules;
		this.TermsText.text = this._currentSerie.Terms;
		this.FeeText.text = this._currentSerie.EntranceFee.ToString();
		this.FeeCurrency.text = MeasuringSystemManager.GetCurrencyIcon(this._currentSerie.Currency);
		this.RewardsText.text = this._currentSerie.Reward;
		this.TournamentSchedule.text = this.ScheduleString(serie);
		this.SetButtonView();
	}

	public void ApplyClick()
	{
		if (!this._currentSerie.Stages.Any((Tournament x) => x.IsRegistered))
		{
			if (this._currentSerie.EntranceFee > PhotonConnectionFactory.Instance.Profile.SilverCoins)
			{
				this._buyMoneyMessageBox = this._menuHelpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("TravelNotEnoughMoney"), true, false, false, null);
				UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
				this._buyMoneyMessageBox.GetComponent<EventAction>().ActionCalled += this.BuyMoneyCompleteMessage_ActionCalled;
			}
			else if (PhotonConnectionFactory.Instance.Profile.IsLatestEulaSigned)
			{
				this.Subscribe();
			}
			else
			{
				this._messageBoxEula = TournamentHelper.ShowingEulaMessage();
				this._messageBoxEula.GetComponent<EventConfirmAction>().CancelActionCalled += this.TournamentSerieDetailsMessage_CancelActionCalled;
				this._messageBoxEula.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.TournamentSerieDetailsMessage_ConfirmActionCalled;
			}
		}
	}

	private void TournamentSerieDetailsMessage_ConfirmActionCalled(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.OnEulaSigned += this.PhotonServer_OnEulaSigned;
		PhotonConnectionFactory.Instance.OnEulaSignFailed += this.PhotonServer_OnEulaSignFailed;
		PhotonConnectionFactory.Instance.SignEula(1);
	}

	private void PhotonServer_OnEulaSignFailed(Failure failure)
	{
		this.UnsubscribeEulaSigned();
		Object.Destroy(this._messageBoxEula);
		this._messageBox = this._menuHelpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("TournamentRegistrationFailed"), true, false, false, null);
		this._messageBox.GetComponent<EventAction>().ActionCalled += this.Confirm_ActionCalled;
	}

	private void PhotonServer_OnEulaSigned()
	{
		this.UnsubscribeEulaSigned();
		Object.Destroy(this._messageBoxEula);
		this.Subscribe();
	}

	private void TournamentSerieDetailsMessage_CancelActionCalled(object sender, EventArgs e)
	{
		Object.Destroy(this._messageBoxEula);
	}

	public void CloseClick()
	{
		this.Close();
		if (this.CloseOnClick != null)
		{
			this.CloseOnClick(this, new MessageBoxEventArgs(base.gameObject));
		}
	}

	private string ScheduleString(TournamentSerieInstance serie)
	{
		DateTime dateTime = serie.Stages.Min((Tournament x) => x.StartDate);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0}: {1} - {2}", ScriptLocalization.Get("RegTimeTournamentInfo"), MeasuringSystemManager.DateTimeString(serie.RegistrationStart.ToLocalTime()), MeasuringSystemManager.DateTimeString(dateTime.ToLocalTime()));
		List<Tournament> list = serie.Stages.OrderBy((Tournament x) => x.StartDate).ToList<Tournament>();
		for (int i = 0; i < serie.Stages.Count<Tournament>(); i++)
		{
			Tournament tournament = list[i];
			stringBuilder.AppendFormat("\n{0}: {1} - {2}", tournament.Name, MeasuringSystemManager.DateTimeString(tournament.StartDate.ToLocalTime()), MeasuringSystemManager.DateTimeString(tournament.EndDate.ToLocalTime()));
		}
		return stringBuilder.ToString();
	}

	private void SetButtonView()
	{
		if (this._currentSerie.Stages.Any((Tournament x) => x.IsRegistered))
		{
			this.ApplyButton.interactable = false;
		}
		else
		{
			this.ApplyButton.interactable = this._currentSerie.RegistrationStart < TimeHelper.UtcTime() && this._currentSerie.StartDate > TimeHelper.UtcTime();
		}
	}

	private void BuyMoneyCompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		if (this._buyMoneyMessageBox != null)
		{
			this._buyMoneyMessageBox.Close();
		}
	}

	private void BuyClick_ThirdButtonActionCalled(object sender, EventArgs e)
	{
		if (this.HaventMoneyClickBuy != null)
		{
			this.HaventMoneyClickBuy(this, new EventArgs());
		}
		if (this._buyMoneyMessageBox != null)
		{
			this._buyMoneyMessageBox.Close();
		}
		ShopMainPageHandler.OpenPremiumShop();
	}

	private void OnRegisterForTournamentSerieFailed(Failure failure)
	{
		ErrorCode errorCode = failure.ErrorCode;
		if (errorCode != 32552 && errorCode != 32554)
		{
			this.Unsubscribe();
			this._messageBox = this._menuHelpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("TournamentRegistrationFailed"), true, false, false, null);
			this._messageBox.GetComponent<EventAction>().ActionCalled += this.Confirm_ActionCalled;
			Debug.LogError(failure.FullErrorInfo);
		}
		else
		{
			bool flag = failure.ErrorCode == 32552;
			UserCompetitionPublic userCompetition = ((TournamentFailure)failure).UserCompetition;
			string yellowTan = UgcConsts.GetYellowTan(UserCompetitionHelper.GetDefaultName(userCompetition));
			if (flag)
			{
				this.Unsubscribe();
				this.CloseClick();
				UserCompetitionFailureHandler.ShowMsgSportEventWait(yellowTan, userCompetition.EndDate);
			}
			else
			{
				UIHelper.ShowYesNo(string.Format(ScriptLocalization.Get("UGC_AlreadySignedToSportEventJoinAnother"), yellowTan), delegate
				{
					PhotonConnectionFactory.Instance.RegisterForTournamentSerie(this._currentSerie.SerieInstanceId, true);
				}, null, "UGC_ApplyAnywayButton", delegate
				{
					this.Unsubscribe();
				}, "NoCaption", null, null, null);
			}
		}
	}

	private void OnRegisteredForTournamentSerie()
	{
		this.Unsubscribe();
		this._messageBox = this._menuHelpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("YouWereRegisteredOnSerieCaption"), true, false, false, null);
		this._messageBox.GetComponent<EventAction>().ActionCalled += this.Confirm_ActionCalled;
		if (this.ApplyOnClick != null)
		{
			this.ApplyOnClick(this, new EventArgs());
		}
	}

	private void Confirm_ActionCalled(object sender, EventArgs e)
	{
		this._messageBox.Close();
	}

	private void Subscribe()
	{
		this._isWaiting = true;
		this.ApplyButton.interactable = false;
		PhotonConnectionFactory.Instance.OnRegisteredForTournamentSerie += this.OnRegisteredForTournamentSerie;
		PhotonConnectionFactory.Instance.OnRegisterForTournamentSerieFailed += this.OnRegisterForTournamentSerieFailed;
		PhotonConnectionFactory.Instance.RegisterForTournamentSerie(this._currentSerie.SerieInstanceId, false);
	}

	private void Unsubscribe()
	{
		PhotonConnectionFactory.Instance.OnRegisteredForTournamentSerie -= this.OnRegisteredForTournamentSerie;
		PhotonConnectionFactory.Instance.OnRegisterForTournamentSerieFailed -= this.OnRegisterForTournamentSerieFailed;
		this._isWaiting = false;
		this.SetButtonView();
	}

	private void UnsubscribeEulaSigned()
	{
		PhotonConnectionFactory.Instance.OnEulaSigned -= this.PhotonServer_OnEulaSigned;
		PhotonConnectionFactory.Instance.OnEulaSignFailed -= this.PhotonServer_OnEulaSignFailed;
	}

	[SerializeField]
	protected BorderedButton UnregBtn;

	public Image LogoImage;

	private ResourcesHelpers.AsyncLoadableImage LogoImageLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public Text Title;

	public Text TournamentSchedule;

	public Text StartTimeBellowLogo;

	public Text Description;

	public Text TermsText;

	public Text RulesText;

	public Text FeeText;

	public Text FeeCurrency;

	public Text RewardsText;

	public Button ApplyButton;

	public GameObject SummaryPanelButton;

	public GameObject RankingsPanel;

	public GameObject ResultsPanel;

	public GameObject ResultsPanelButton;

	private TournamentSerieInstance _currentSerie;

	private MenuHelpers _menuHelpers = new MenuHelpers();

	private MessageBox _buyMoneyMessageBox;

	private MessageBox _messageBox;

	private GameObject _messageBoxEula;

	private bool _isWaiting;
}
