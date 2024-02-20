using System;
using System.Globalization;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyConverter : CutControllerBase
{
	protected override void OnDestroy()
	{
		this.UnSubscribe();
		base.OnDestroy();
	}

	protected override void Update()
	{
		if (Math.Abs(this.Value - this.SliderControl.value) > 0.01f)
		{
			this.Value = this.SliderControl.value;
			this.UpdateInfo();
		}
		base.Update();
	}

	public override void Accept()
	{
		if (!this.Alpha.IsHiding)
		{
			UIHelper.ShowYesNo(ScriptLocalization.Get("TransactionConfirmText"), delegate
			{
				if (!this._subscribed)
				{
					this.Subscribe();
					PhotonConnectionFactory.Instance.ExchangeCurrency((int)this.Value);
				}
			}, ScriptLocalization.Get("ConfirmCaption"), "AgreeButtonCaption", delegate
			{
				this.UnSubscribe();
			}, "DisagreeButtonCaption", null, null, null);
		}
	}

	public void Init()
	{
		for (int i = 0; i < this._balanceTitles.Length; i++)
		{
			this._balanceTitles[i].text = ScriptLocalization.Get("PremShop_Balance");
		}
		float num = Inventory.ExchangeRate();
		this._rateValue.text = string.Format(ScriptLocalization.Get("ExchangeRateText"), num.ToString(CultureInfo.InvariantCulture)).ToUpper();
		this.SliderControl.maxValue = (float)PhotonConnectionFactory.Instance.Profile.GoldCoins;
		this.SliderControl.minValue = 0f;
		this.SliderControl.value = 0f;
		this.UpdateInfo();
		base.Open();
	}

	private void UpdateInfo()
	{
		this.OkBtn.interactable = this.Value > 0f;
		float num = Inventory.ExchangeRate();
		double goldCoins = PhotonConnectionFactory.Instance.Profile.GoldCoins;
		double silverCoins = PhotonConnectionFactory.Instance.Profile.SilverCoins;
		this.BaseLength.text = this.Value.ToString(CultureInfo.InvariantCulture);
		this.TargetLength.text = (this.Value * num).ToString(CultureInfo.InvariantCulture);
		this._balanceBaseValue.text = (goldCoins - (double)this.Value).ToString(CultureInfo.InvariantCulture);
		this._balanceTargetValue.text = (silverCoins + (double)(this.Value * num)).ToString(CultureInfo.InvariantCulture);
	}

	private void InstanceOnOnCurrencyExchangeFailed(Failure failure)
	{
		this.UnSubscribe();
		this.Alpha.HidePanel();
	}

	private void Instance_OnCurrencyExchanged(int goldAmount, int silverAmount)
	{
		this.UnSubscribe();
		UIAudioSourceListener.Instance.Purcahse();
		UIHelper.ShowMessage(ScriptLocalization.Get("ShopCongratulationCaption"), string.Format(ScriptLocalization.Get("ShopCongratulationMesssage"), UgcConsts.GetYellowTan(string.Format("{0} {1}", silverAmount, MeasuringSystemManager.GetCurrencyIcon("SC"))), string.Empty), true, delegate
		{
			this.Alpha.HidePanel();
		}, false);
	}

	private void Subscribe()
	{
		if (!this._subscribed)
		{
			this._subscribed = true;
			PhotonConnectionFactory.Instance.OnCurrencyExchanged += this.Instance_OnCurrencyExchanged;
			PhotonConnectionFactory.Instance.OnCurrencyExchangeFailed += this.InstanceOnOnCurrencyExchangeFailed;
		}
	}

	private void UnSubscribe()
	{
		if (this._subscribed)
		{
			this._subscribed = false;
			PhotonConnectionFactory.Instance.OnCurrencyExchanged -= this.Instance_OnCurrencyExchanged;
			PhotonConnectionFactory.Instance.OnCurrencyExchangeFailed -= this.InstanceOnOnCurrencyExchangeFailed;
		}
	}

	[SerializeField]
	private TextMeshProUGUI[] _balanceTitles;

	[SerializeField]
	private TextMeshProUGUI _balanceBaseValue;

	[SerializeField]
	private TextMeshProUGUI _balanceTargetValue;

	[SerializeField]
	private Text _rateValue;

	protected float Value;

	private bool _subscribed;
}
