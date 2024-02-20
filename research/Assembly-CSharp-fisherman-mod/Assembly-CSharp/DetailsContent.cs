using System;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class DetailsContent : MonoBehaviour
{
	public void Show(InventoryItem inventoryItem)
	{
		this._inventoryItem = inventoryItem;
		if (inventoryItem.Brand != null)
		{
			this.Brand.text = inventoryItem.Brand.Name;
		}
		this.Name.text = inventoryItem.Name;
		this.Description.text = InventoryParamsHelper.ParseDesc(this._inventoryItem);
		this.ImageLdbl.Image = this.InventoryImage;
		this.ImageLdbl.Load((inventoryItem.ThumbnailBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", inventoryItem.ThumbnailBID.ToString()));
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
	}

	public void Buy()
	{
		if (this._inventoryItem == null)
		{
			return;
		}
		double? priceSilver = this._inventoryItem.PriceSilver;
		if (PhotonConnectionFactory.Instance.Profile.SilverCoins >= priceSilver)
		{
			if (PhotonConnectionFactory.Instance.Profile.Inventory.GetDefaultStorage(this._inventoryItem) != null)
			{
				PhotonConnectionFactory.Instance.BuyItem(this._inventoryItem, null);
			}
			else
			{
				this.messageBox = this.helpers.ShowMessageSelectable(ShopMainPageHandler.Instance.ContentUpdater.RootObject, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("CantPlaceItem"), false);
				this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.SendToHome_ActionCalled;
				this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CompleteMessage_ActionCalled;
			}
		}
		else
		{
			this.messageBox = this.helpers.ShowMessage(ShopMainPageHandler.Instance.ContentUpdater.RootObject, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("HaventMoney"), false, false, false, null);
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
		}
	}

	private void BuyClick_ThirdButtonActionCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		ShopMainPageHandler.OpenPremiumShop();
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	private void SendToHome_ActionCalled(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.BuyItemAndSendToBase(base.GetComponent<InventoryItemComponent>().InventoryItem, null);
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	public Text Brand;

	public Text Name;

	public Text Description;

	public Image InventoryImage;

	private ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	private InventoryItem _inventoryItem;

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox messageBox;
}
