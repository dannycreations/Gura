using System;
using System.Globalization;
using Assets.Scripts.Common.Managers;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;

public class BoatRentController
{
	public void Subscribe()
	{
		PhotonConnectionFactory.Instance.OnBoatRented += this.Instance_OnBoatRented;
		PhotonConnectionFactory.Instance.OnErrorRentingBoat += this.Instance_OnErrorRentingBoat;
	}

	private void Instance_OnErrorRentingBoat(Failure failure)
	{
		Debug.LogError("Rent a boat failure " + failure.ErrorMessage);
	}

	private void Instance_OnBoatRented()
	{
		Debug.Log("Rent a boat Succes");
	}

	public void Unsubscribe()
	{
		PhotonConnectionFactory.Instance.OnBoatRented -= this.Instance_OnBoatRented;
		PhotonConnectionFactory.Instance.OnErrorRentingBoat -= this.Instance_OnErrorRentingBoat;
	}

	public void ShowExtendRentPopup(BoatDesc boatForRent, Action<BoatDesc, int> onRentABoat, Action<BoatDesc> onRentNotExtended)
	{
		if (!HudTournamentHandler.IsWarningOfEnd)
		{
			this.ShowRentPopup(boatForRent, onRentABoat, onRentNotExtended, true);
		}
	}

	public void ShowRentPopup(BoatDesc boatForRent, Action<BoatDesc, int> onRentABoat)
	{
		if (!HudTournamentHandler.IsWarningOfEnd)
		{
			this.ShowRentPopup(boatForRent, onRentABoat, delegate(BoatDesc d)
			{
			}, false);
		}
	}

	public void ShowBoatFishingLicenseIsMissing(Action onProceed)
	{
		this.onProceed = onProceed;
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		this.messageBox = GUITools.AddChild(gameObject, this.helpers.MessageBoxList.messageBoxThreeSelectablePrefab).GetComponent<MessageBox>();
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this.messageBox.Caption = ScriptLocalization.Get("MessageCaption");
		this.messageBox.Message = ScriptLocalization.Get("BoatFishingProhibitedMessage");
		this.messageBox.ConfirmButtonText = ScriptLocalization.Get("ButtonBuyLicensesLower");
		this.messageBox.CancelButtonText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ScriptLocalization.Get("ProccedCaption").ToLower());
		this.messageBox.ThirdButtonText = ScriptLocalization.Get("CancelButton");
		this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.LoadShop_ActionCalled;
		this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.ProceedFishing_ActionCalled;
		this.messageBox.GetComponent<EventConfirmAction>().ThirdButtonActionCalled += this.CompleteMessage_ActionCalled;
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		this.CloseMessage();
	}

	private void ProceedFishing_ActionCalled(object sender, EventArgs e)
	{
		if (this.onProceed != null)
		{
			this.onProceed();
		}
		this.CloseMessage();
	}

	private void LoadShop_ActionCalled(object sender, EventArgs e)
	{
		this.messageBox.AfterFullyHidden.AddListener(new UnityAction(KeysHandlerAction.LicenseShopHandler));
		this.CloseMessage();
	}

	private void CloseMessage()
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	private void ShowRentPopup(BoatDesc boatForRent, Action<BoatDesc, int> onRentABoat, Action<BoatDesc> onRentNotExtended, bool extend)
	{
		GameObject gameObject = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.RentPopup);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		gameObject.GetComponent<BoatRentHandler>().Init(boatForRent, onRentABoat, onRentNotExtended, extend);
		gameObject.GetComponent<AlphaFade>().FastHidePanel();
		gameObject.SetActive(false);
		MessageFactory.MessageBoxQueue.Enqueue(gameObject.GetComponent<BoatRentHandler>());
	}

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox messageBox;

	private Action onProceed;
}
