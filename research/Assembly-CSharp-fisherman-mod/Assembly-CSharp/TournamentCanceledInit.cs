using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentCanceledInit : MonoBehaviour
{
	private void Awake()
	{
		this._originWndHeight = this._wnd.rect.height;
		this._originMessageHeight = this.MessageText.preferredHeight;
	}

	private void Update()
	{
		if (this._hasEndDate)
		{
			string text = string.Format(ScriptLocalization.Get("EndTournamentTimeText"), UgcConsts.GetYellowTan(string.Format("{0} {1}", "\ue70c", BaseTournamentDetails.GetTimerValue(this._endDate))));
			string text2 = string.Format("{0}\n{1}", this._message, text);
			this.MessageText.text = text2;
		}
	}

	public void Init(TournamentCancelInfo info)
	{
		this.FillData(string.Format(ScriptLocalization.Get("CompetitionCanceledText"), string.Format("<color=#FFDD77FF><b>{0}</b></color>", info.Name)), info.FeeReturned, info.Currency);
	}

	public void Init(UserCompetitionCancellationMessage o)
	{
		UserCompetitionCancellationReason reason = o.Reason;
		switch (reason)
		{
		case UserCompetitionCancellationReason.ReasonPlayerRemovedByHost:
			this.Init(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("UGC_KickedByHost"), TournamentCanceledInit.MessageTypes.Warning);
			return;
		default:
			if (reason != UserCompetitionCancellationReason.ReasonCompetitionReverted)
			{
				this._bg.SetActive(true);
				string text = string.Format("{0} - {1}", string.Format("<color=#FFDD77FF><b>{0}</b></color>", UserCompetitionHelper.GetDefaultName(new UserCompetitionPublic
				{
					HostName = o.HostName,
					Format = o.Format,
					Type = o.Type,
					TemplateName = o.TemplateName
				})), UgcConsts.GetCancellationReasonLoc(o.Reason).Name);
				this.FillData(text, o.Amount, o.Currency);
				return;
			}
			break;
		case UserCompetitionCancellationReason.ReasonPlayerNotReady:
			break;
		}
		string yellowTan = UgcConsts.GetYellowTan(UserCompetitionHelper.GetDefaultName(new UserCompetitionPublic
		{
			HostName = o.HostName,
			Format = o.Format,
			Type = o.Type,
			TemplateName = o.TemplateName
		}));
		this.Init(ScriptLocalization.Get("MessageCaption"), string.Format(UgcConsts.GetCancellationReasonLoc(o.Reason).Name, yellowTan), TournamentCanceledInit.MessageTypes.Warning);
	}

	public void Init(string title, string message, TournamentCanceledInit.MessageTypes type, DateTime endDate)
	{
		this.Init(title, message, type);
		this._message = message;
		this._hasEndDate = true;
		this._endDate = endDate;
	}

	public void Init(string title, string message, TournamentCanceledInit.MessageTypes type)
	{
		if (this._originMessageHeight <= 0f)
		{
			this.Awake();
		}
		Text amountRefundedText = this.AmountRefundedText;
		string empty = string.Empty;
		this.CurrencyRefundedText.text = empty;
		amountRefundedText.text = empty;
		this._bg.SetActive(true);
		TournamentCanceledInit.Message message2 = this.MessagesData.First((TournamentCanceledInit.Message p) => p.Type == type);
		this._refunded.SetActive(false);
		this._title.text = title;
		this.MessageText.text = message;
		this._ico.text = message2.Ico;
		Graphic ico = this._ico;
		Color color = message2.Color;
		this._title.color = color;
		ico.color = color;
		if (this.MessageText.preferredHeight > 800f)
		{
			this.MessageText.enableAutoSizing = true;
			this.MessageText.fontSizeMax = 20f;
			this._wnd.sizeDelta = new Vector2(this._wnd.rect.width, 900f);
			return;
		}
		float num = ((this.MessageText.preferredHeight <= this._originMessageHeight) ? 0f : (this.MessageText.preferredHeight - this._originMessageHeight));
		this._wnd.sizeDelta = new Vector2(this._wnd.rect.width, this._originWndHeight + num);
	}

	private void FillData(string name, int amount, string currency)
	{
		this._title.text = ScriptLocalization.Get("CompetitionCanceledCaption");
		this.MessageText.text = name;
		this.AmountRefundedText.text = amount.ToString();
		this.CurrencyRefundedText.text = MeasuringSystemManager.GetCurrencyIcon(currency);
	}

	[SerializeField]
	private RectTransform _wnd;

	[SerializeField]
	private TextMeshProUGUI _ico;

	[SerializeField]
	private Text _title;

	[SerializeField]
	private GameObject _refunded;

	[SerializeField]
	private GameObject _bg;

	public TextMeshProUGUI MessageText;

	public Text AmountRefundedText;

	public Text CurrencyRefundedText;

	private const string NameFormatting = "<color=#FFDD77FF><b>{0}</b></color>";

	private readonly IList<TournamentCanceledInit.Message> MessagesData = new ReadOnlyCollection<TournamentCanceledInit.Message>(new List<TournamentCanceledInit.Message>
	{
		new TournamentCanceledInit.Message
		{
			Ico = "\ue67f",
			Color = UgcConsts.YellowTan,
			Type = TournamentCanceledInit.MessageTypes.Warning
		},
		new TournamentCanceledInit.Message
		{
			Ico = "\ue782",
			Color = UgcConsts.WinnerColor,
			Type = TournamentCanceledInit.MessageTypes.Ok
		},
		new TournamentCanceledInit.Message
		{
			Ico = "\ue781",
			Color = UgcConsts.GetTeamColor("Red"),
			Type = TournamentCanceledInit.MessageTypes.Error
		}
	});

	private const float MessageTextFontSizeMax = 20f;

	private const float MessageTextPreferredHeightMax = 800f;

	private const float WndHeightMax = 900f;

	private float _originWndHeight;

	private float _originMessageHeight;

	private DateTime _endDate;

	private bool _hasEndDate;

	private string _message;

	public enum MessageTypes : byte
	{
		Ok,
		Warning,
		Error
	}

	private class Message
	{
		public string Ico { get; set; }

		public Color Color { get; set; }

		public TournamentCanceledInit.MessageTypes Type { get; set; }
	}
}
