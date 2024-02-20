using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using TMPro;
using UnityEngine;

public class WindowEntryFee : WindowLevelMinMax
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<WindowEntryFee.CurrencyData> OnSelectedCurrency = delegate(WindowEntryFee.CurrencyData cd)
	{
	};

	public void Init(WindowEntryFee.EntryFeeData d)
	{
		Func<string, Range> GetRange = (string currency) => (!(currency == "SC")) ? d.EntryFeeRangeGold : d.EntryFeeRange;
		Func<string, int> GetMaxAdditional = (string currency) => (!(currency == "SC")) ? d.MaxAdditionalGold : d.MaxAdditional;
		Action<string> CheckCurrency = delegate(string currency)
		{
			this.CheckBtnsInteractable(this.CurrencyIcos.IndexOf(currency), new Range(0, this.CurrencyIcos.Count - 1), this.MinBtns);
		};
		Action<string, int> CheckFee = delegate(string currency, int sum)
		{
			this.CheckBtnsInteractable(sum, GetRange(currency), this.MaxBtns);
		};
		this.FeeCommission.text = UserCompetitionHelper.FeeCommissionExtLoc;
		this.UpdateMaxHint(GetRange(d.Current.Currency));
		this.IfCheckerMinLevel.OnValueChanged += delegate(int v)
		{
			string text = this.CurrencyIcos[v];
			CheckCurrency(text);
			this.CurrencyIco.text = MeasuringSystemManager.GetCurrencyIcon(text);
			this.UpdateMaxHint(GetRange(text));
			this.IfCheckerMaxLevel.SetRange(UserCompetitionHelper.GetFee(text, GetRange(text), GetMaxAdditional(text)), new int?(this.IfCheckerMaxLevel.Value));
			CheckFee(text, this.IfCheckerMaxLevel.Value);
		};
		this.IfCheckerMinLevel.SetRange(new Range(0, this.CurrencyIcos.Count - 1), new int?(this.CurrencyIcos.IndexOf(d.Current.Currency)));
		this.CurrencyIco.text = MeasuringSystemManager.GetCurrencyIcon(d.Current.Currency);
		CheckCurrency(d.Current.Currency);
		this.IfCheckerMaxLevel.OnValueChanged += delegate(int v)
		{
			CheckFee(this.CurrencyIcos[this.IfCheckerMinLevel.Value], this.IfCheckerMaxLevel.Value);
		};
		this.IfCheckerMaxLevel.SetRange(UserCompetitionHelper.GetFee(d.Current.Currency, GetRange(d.Current.Currency), GetMaxAdditional(d.Current.Currency)), new int?((d.Current.EntranceFee == null) ? 0 : ((int)d.Current.EntranceFee.Value)));
		CheckFee(d.Current.Currency, this.IfCheckerMaxLevel.Value);
	}

	public override void MinInc(int v)
	{
		this.IfCheckerMinLevel.Inc(v);
	}

	public override void MaxInc(int v)
	{
		this.IfCheckerMaxLevel.Inc(v);
	}

	protected override void AcceptActionCalled()
	{
		this.OnSelectedCurrency(new WindowEntryFee.CurrencyData
		{
			Currency = this.CurrencyIcos[this.IfCheckerMinLevel.Value],
			EntranceFee = new double?((double)this.IfCheckerMaxLevel.Value)
		});
	}

	protected void UpdateMaxHint(Range entryFee)
	{
		if (this.Hint != null)
		{
			this.Hint.gameObject.SetActive(true);
			if (entryFee.Max > 0)
			{
				this.Hint.text = string.Format(ScriptLocalization.Get("UGC_MaxFee"), entryFee.Max);
			}
		}
	}

	[SerializeField]
	protected TextMeshProUGUI Hint;

	[SerializeField]
	protected TextMeshProUGUI CurrencyIco;

	[SerializeField]
	protected TextMeshProUGUI FeeCommission;

	protected List<string> CurrencyIcos = new List<string> { "SC" };

	public class CurrencyData
	{
		public double? EntranceFee { get; set; }

		public string Currency { get; set; }
	}

	public class EntryFeeData
	{
		public WindowEntryFee.CurrencyData Current { get; set; }

		public Range EntryFeeRange { get; set; }

		public Range EntryFeeRangeGold { get; set; }

		public int MaxAdditional { get; set; }

		public int MaxAdditionalGold { get; set; }
	}
}
