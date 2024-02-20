using System;
using System.Collections;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConverterHandler : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	private void OnDestroy()
	{
		this.UnSubscribe();
	}

	public void Init()
	{
		this.ExchangeText.text = "\ue62c " + this._exchangeValue;
		this.ExchangedResultValue.text = "\ue62b " + 0;
		this.ExchangeRateText.text = string.Format(ScriptLocalization.Get("ExchangeRateText"), Inventory.ExchangeRate());
		ControlsController.ControlsActions.BlockInput(null);
		this.UpdateInfo();
	}

	public void CloseClick()
	{
		base.GetComponent<AlphaFade>().HidePanel();
		ControlsController.ControlsActions.UnBlockInput();
	}

	public void ConvertClick()
	{
		this._messageBox = this._helpers.ShowMessageSelectable(null, ScriptLocalization.Get("ConfirmCaption"), ScriptLocalization.Get("TransactionConfirmText"), ScriptLocalization.Get("AgreeButtonCaption"), ScriptLocalization.Get("DisagreeButtonCaption"), false, true);
		this._messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.ConverterHandler_ConfirmActionCalled;
		this._messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.ConverterHandler_CancelActionCalled;
		this._messageBox.Open();
	}

	private void ConverterHandler_CancelActionCalled(object sender, EventArgs e)
	{
		this._messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled -= this.ConverterHandler_ConfirmActionCalled;
		this._messageBox.GetComponent<AlphaFade>().HideFinished += this.MessageBox_HideFinished;
		this._messageBox.GetComponent<AlphaFade>().HidePanel();
	}

	private void ConverterHandler_ConfirmActionCalled(object sender, EventArgs e)
	{
		this._messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.ConverterHandler_CancelActionCalled;
		this._messageBox.GetComponent<AlphaFade>().HideFinished += this.MessageBox_HideFinished;
		this._messageBox.GetComponent<AlphaFade>().HidePanel();
		this.Subscribe();
		PhotonConnectionFactory.Instance.ExchangeCurrency(this._exchangeValue);
	}

	private void InstanceOnOnCurrencyExchangeFailed(Failure failure)
	{
		this.UnSubscribe();
		this.CloseClick();
	}

	private void Instance_OnCurrencyExchanged(int goldAmount, int silverAmount)
	{
		this.UnSubscribe();
		this.CloseClick();
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

	private void MessageBox_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this._messageBox.GetComponent<AlphaFade>().HideFinished -= this.MessageBox_HideFinished;
		Object.Destroy(this._messageBox.gameObject);
	}

	public void IncClick()
	{
		if (PhotonConnectionFactory.Instance.Profile.GoldCoins > (double)this._exchangeValue)
		{
			this._exchangeValue++;
		}
		this.UpdateInfo();
	}

	public void DecClick()
	{
		if (this._exchangeValue > 0)
		{
			this._exchangeValue--;
		}
		this.UpdateInfo();
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (this._exchangeValue + (int)eventData.scrollDelta.y >= 0 && (double)(this._exchangeValue + (int)eventData.scrollDelta.y) <= PhotonConnectionFactory.Instance.Profile.GoldCoins)
		{
			this._exchangeValue += (int)eventData.scrollDelta.y;
		}
		this.UpdateInfo();
	}

	private void UpdateInfo()
	{
		this.ConvertButton.interactable = this._exchangeValue > 0;
		this.ExchangeText.text = "\ue62c " + this._exchangeValue;
		this.ExchangedResultValue.text = "\ue62b " + (float)this._exchangeValue * Inventory.ExchangeRate();
		this.NewResultValue.text = string.Format("\ue62c {0}     \ue62b {1}", PhotonConnectionFactory.Instance.Profile.GoldCoins - (double)this._exchangeValue, PhotonConnectionFactory.Instance.Profile.SilverCoins + (double)((float)this._exchangeValue * Inventory.ExchangeRate()));
	}

	public void IncOnPointerDown()
	{
		this._stopCoroutine = false;
		this._speed = 1f;
		base.StartCoroutine(this.IncValue());
	}

	public void IncOnPointerUp()
	{
		this._stopCoroutine = true;
	}

	public void DecOnPointerDown()
	{
		this._stopCoroutine = false;
		this._speed = -1f;
		base.StartCoroutine(this.DecValue());
	}

	public void DecOnPointerUp()
	{
		this._stopCoroutine = true;
	}

	private IEnumerator IncValue()
	{
		yield return new WaitForSeconds(0.5f);
		while (!this._stopCoroutine)
		{
			this._speed += 0.01f;
			int newValue = this._exchangeValue + (int)this._speed;
			if (PhotonConnectionFactory.Instance.Profile.GoldCoins > (double)newValue)
			{
				this._exchangeValue = newValue;
			}
			else if (this._exchangeValue != (int)PhotonConnectionFactory.Instance.Profile.GoldCoins)
			{
				this._exchangeValue = (int)PhotonConnectionFactory.Instance.Profile.GoldCoins;
				this._stopCoroutine = true;
			}
			this.UpdateInfo();
			yield return new WaitForEndOfFrame();
		}
		yield break;
	}

	private IEnumerator DecValue()
	{
		yield return new WaitForSeconds(0.5f);
		while (!this._stopCoroutine)
		{
			this._speed -= 0.01f;
			int newValue = this._exchangeValue + (int)this._speed;
			if (newValue > 0)
			{
				this._exchangeValue = newValue;
			}
			else if (newValue != 0)
			{
				this._exchangeValue = 0;
				this._stopCoroutine = true;
			}
			this.UpdateInfo();
			yield return new WaitForEndOfFrame();
		}
		yield break;
	}

	public Text ExchangeRateText;

	public Text ExchangeText;

	public Text ExchangedResultValue;

	public Text NewResultValue;

	public Button IncButton;

	public Button ConvertButton;

	private int _exchangeValue;

	private float _speed;

	private bool _stopCoroutine;

	private MessageBox _messageBox;

	private MenuHelpers _helpers = new MenuHelpers();

	private bool _subscribed;
}
