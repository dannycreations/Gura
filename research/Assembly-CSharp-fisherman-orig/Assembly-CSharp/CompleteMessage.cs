using System;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Common;
using I2.Loc;
using ObjectModel;
using UnityEngine;

public class CompleteMessage : MonoBehaviour
{
	public static bool IsBuyingActive
	{
		get
		{
			return CompleteMessage._isBuyingActive;
		}
	}

	private void Awake()
	{
		CompleteMessage.WaitOp = new WaitingOperation();
		CompleteMessage.WaitOp.OnStopWaiting += this._wOp_OnStopWaiting;
	}

	internal void Start()
	{
		PhotonConnectionFactory.Instance.OnItemBought += this.OnItemBought;
		PhotonConnectionFactory.Instance.OnTransactionFailed += this.OnTransactionFailed;
		PhotonConnectionFactory.Instance.OnLicenseBought += this.OnLicenseBought;
		PhotonConnectionFactory.Instance.OnProductBought += this.OnProductBought;
		PhotonConnectionFactory.Instance.OnBuyProductFailed += this.Instance_OnBuyProductFailed;
	}

	internal void OnDestroy()
	{
		CompleteMessage.WaitOp.OnStopWaiting -= this._wOp_OnStopWaiting;
		PhotonConnectionFactory.Instance.OnBuyProductFailed -= this.Instance_OnBuyProductFailed;
		PhotonConnectionFactory.Instance.OnItemBought -= this.OnItemBought;
		PhotonConnectionFactory.Instance.OnTransactionFailed -= this.OnTransactionFailed;
		PhotonConnectionFactory.Instance.OnLicenseBought -= this.OnLicenseBought;
		PhotonConnectionFactory.Instance.OnProductBought -= this.OnProductBought;
	}

	internal void Update()
	{
		if (CompleteMessage._shouldWait)
		{
			CompleteMessage.WaitOp.Update();
		}
	}

	public static void SetBuyingActive(bool flag)
	{
		CompleteMessage._isBuyingActive = flag;
		if (CompleteMessage._isBuyingActive)
		{
			CompleteMessage._shouldWait = true;
		}
		if (!CompleteMessage._isBuyingActive)
		{
			CompleteMessage.StopWaiting(true);
		}
	}

	private void OnItemBought(InventoryItem itemBought)
	{
		CompleteMessage.StopWaiting(false);
		if (!base.enabled)
		{
			CompleteMessage.SetBuyingActive(false);
			return;
		}
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.PurcahseClip, SettingsManager.InterfaceVolume);
		this.IsClosedMessage = false;
		this.messageBox = this.helpers.ShowMessage(base.gameObject, ScriptLocalization.Get("ShopCongratulationCaption"), string.Format(ScriptLocalization.Get("ShopCongratulationMesssage"), itemBought.Name, "\n" + ScriptLocalization.Get(NameResolvingHelpers.GetStorageName(itemBought.Storage))), false, false, false, null);
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
	}

	private void OnLicenseBought(PlayerLicense license)
	{
		CompleteMessage.StopWaiting(false);
		if (!base.enabled)
		{
			CompleteMessage.SetBuyingActive(false);
			return;
		}
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.PurcahseClip, SettingsManager.InterfaceVolume);
		this.IsClosedMessage = false;
		this.messageBox = this.helpers.ShowMessage(base.gameObject, ScriptLocalization.Get("ShopCongratulationCaption"), string.Format(ScriptLocalization.Get("ShopLicenseCongratulationMesssage"), license.Name), false, false, false, null);
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
	}

	private void OnProductBought(ProfileProduct product, int count)
	{
		CompleteMessage.StopWaiting(false);
		if (!base.enabled)
		{
			CompleteMessage.SetBuyingActive(false);
			return;
		}
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.PurcahseClip, SettingsManager.InterfaceVolume);
		this.IsClosedMessage = false;
		if (product.TypeId == 5)
		{
			if (product.InventoryExt != null)
			{
				this.messageBox = this.helpers.ShowMessage(base.gameObject, ScriptLocalization.Get("CongratulationsCaption"), string.Format(ScriptLocalization.Get("StorageBoxPurchasedText"), product.InventoryExt.Value), false, false, false, null);
				this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
			}
		}
		else if (product.TypeId == 6)
		{
			if (product.RodSetupExt != null)
			{
				this.messageBox = this.helpers.ShowMessage(base.gameObject, ScriptLocalization.Get("CongratulationsCaption"), string.Format(ScriptLocalization.Get("RodPresetSlotPurchasedText"), product.RodSetupExt.Value), false, false, false, null);
				this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
			}
		}
		else if (product.TypeId == 7)
		{
			if (product.BuoyExt != null)
			{
				this.messageBox = this.helpers.ShowMessage(base.gameObject, ScriptLocalization.Get("CongratulationsCaption"), string.Format(ScriptLocalization.Get("BuoysPurchasedText"), product.BuoyExt.Value), false, false, false, null);
				this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
			}
		}
		else
		{
			this.messageBox = this.helpers.ShowMessage(base.gameObject, ScriptLocalization.Get("ShopCongratulationCaption"), string.Format(ScriptLocalization.Get("ShopLicenseCongratulationMesssage"), product.Name), false, false, false, null);
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
		}
		this.messageBox.OnPriority = true;
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		CompleteMessage.SetBuyingActive(false);
		if (!base.enabled)
		{
			return;
		}
		if (this.messageBox != null)
		{
			this.messageBox.Close();
			this.IsClosedMessage = true;
		}
	}

	private void OnTransactionFailed(TransactionFailure failure)
	{
		this.Instance_OnBuyProductFailed(failure);
	}

	private void Instance_OnBuyProductFailed(Failure failure)
	{
		CompleteMessage.StopWaiting(false);
		if (!base.enabled)
		{
			CompleteMessage.SetBuyingActive(false);
			return;
		}
		Debug.Log(failure.ErrorMessage);
		this.messageBox = this.helpers.ShowMessage(base.gameObject, ScriptLocalization.Get("MessageCaption"), failure.ErrorMessage, false, false, false, null);
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
	}

	public static void StopWaiting(bool callback = true)
	{
		CompleteMessage._shouldWait = false;
		CompleteMessage.WaitOp.StopWaiting(callback);
	}

	private void _wOp_OnStopWaiting()
	{
		CompleteMessage._isBuyingActive = false;
	}

	public bool IsClosedMessage;

	private MenuHelpers helpers = new MenuHelpers();

	private static bool _isBuyingActive;

	private static bool _shouldWait;

	private MessageBox messageBox;

	protected static WaitingOperation WaitOp;
}
