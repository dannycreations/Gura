using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuyClick : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	public void Buy()
	{
		InventoryItemComponent component = base.GetComponent<InventoryItemComponent>();
		if (component == null || component.InventoryItem == null)
		{
			return;
		}
		if (component.InventoryItem.PriceGold != null && component.InventoryItem.PriceGold > 0.0)
		{
			double? priceGold = component.InventoryItem.PriceGold;
			if (PhotonConnectionFactory.Instance.Profile.GoldCoins >= priceGold)
			{
				goto IL_169;
			}
		}
		if (component.InventoryItem.PriceGold == null || component.InventoryItem.PriceGold <= 0.0)
		{
			double? priceSilver = component.InventoryItem.PriceSilver;
			if (PhotonConnectionFactory.Instance.Profile.SilverCoins >= priceSilver)
			{
				goto IL_169;
			}
		}
		string text = ((component.InventoryItem.PriceGold == null || !(component.InventoryItem.PriceGold > 0.0)) ? ScriptLocalization.Get("TravelNotEnoughMoney") : ScriptLocalization.Get("HaventMoney"));
		this.messageBox = this.helpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), text, false, false, false, null);
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
		return;
		IL_169:
		if (component.InventoryItem.PriceGold == null)
		{
			double? priceSilver2 = component.InventoryItem.PriceSilver;
			if (((priceSilver2 == null) ? null : new double?(PhotonConnectionFactory.Instance.Profile.SilverCoins - priceSilver2.GetValueOrDefault())) <= (double)ShopMainPageHandler.MinTravelCost)
			{
				MessageBox messageBoxCantTravel = this.helpers.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), (StaticUserData.CurrentPond != null) ? ScriptLocalization.Get("YouCantMoneyForStayOnPond") : ScriptLocalization.Get("YouCantTravel"), false);
				messageBoxCantTravel.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object sender, EventArgs args)
				{
					messageBoxCantTravel.Close();
					this.BuyInventoryItem(component);
				};
				messageBoxCantTravel.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object sender, EventArgs args)
				{
					messageBoxCantTravel.Close();
				};
				messageBoxCantTravel.GetComponent<EventConfirmAction>().ThirdButtonActionCalled += this.BuyClick_ThirdButtonActionCalled;
				return;
			}
		}
		this.BuyInventoryItem(component);
	}

	private void BuyClick_ThirdButtonActionCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		ShopMainPageHandler.OpenPremiumShop();
	}

	private void BuyInventoryItem(InventoryItemComponent component)
	{
		if (PhotonConnectionFactory.Instance.Profile.CanBuyItemWithoutSendingToStorage(component.InventoryItem))
		{
			if (!CompleteMessage.IsBuyingActive)
			{
				CompleteMessage.SetBuyingActive(true);
				PhotonConnectionFactory.Instance.BuyItem(component.InventoryItem, null);
				if (component.InventoryItem.PriceGold != null)
				{
					AnalyticsFacade.WriteSpentGold(component.InventoryItem, (int)component.InventoryItem.PriceGold.Value, (StaticUserData.CurrentPond == null) ? AnalyticsFacade.ShopLocation.GlobalShop : AnalyticsFacade.ShopLocation.LocalShop);
				}
				else
				{
					AnalyticsFacade.WriteSpentSilver(component.InventoryItem, (int)component.InventoryItem.PriceSilver.Value, (StaticUserData.CurrentPond == null) ? AnalyticsFacade.ShopLocation.GlobalShop : AnalyticsFacade.ShopLocation.LocalShop);
				}
			}
		}
		else if (PhotonConnectionFactory.Instance.Profile.Inventory.IsStorageOverloaded)
		{
			this.messageBox = this.helpers.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("CantBuyWithOverloadedStorage"), "\n"), ScriptLocalization.Get("ExpandButtonTitle"), ScriptLocalization.Get("CloseButton"), false, false);
			this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.ExpandWindowCalled;
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CompleteMessage_ActionCalled;
		}
		else
		{
			this.messageBox = this.helpers.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("CantPlaceItem"), false);
			this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.SendToHome_ActionCalled;
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CompleteMessage_ActionCalled;
		}
	}

	public void BuyLicence()
	{
		LicenceItemShop component = base.GetComponent<LicenceItemShop>();
		if (component == null || component.Licence == null)
		{
			return;
		}
		if (component.Border != null && !component.Border.isOn)
		{
			component.Border.isOn = true;
		}
		if (PhotonConnectionFactory.Instance.Profile.ActiveLicenses.Where((PlayerLicense x) => x.LicenseId == component.Licence.LicenseId && !x.CanExpire).Any<PlayerLicense>())
		{
			this.messageBox = this.helpers.ShowMessage(base.gameObject, ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("YouHaveUnlimLicenseText"), component.Licence.Name), false, false, false, null);
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
			return;
		}
		if ((component.Currency == "SC" && PhotonConnectionFactory.Instance.Profile.SilverCoins >= (double)component.Cost) || (component.Currency == "GC" && PhotonConnectionFactory.Instance.Profile.GoldCoins >= (double)component.Cost))
		{
			if (component.Currency == "SC" && PhotonConnectionFactory.Instance.Profile.SilverCoins - (double)component.Cost <= (double)ShopMainPageHandler.MinTravelCost)
			{
				MessageBox messageBoxCantTravel = this.helpers.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), (StaticUserData.CurrentPond != null) ? ScriptLocalization.Get("YouCantMoneyForStayOnPond") : ScriptLocalization.Get("YouCantTravel"), false);
				messageBoxCantTravel.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object sender, EventArgs args)
				{
					messageBoxCantTravel.Close();
					PhotonConnectionFactory.Instance.BuyLicense(component.Licence, component.Term);
				};
				messageBoxCantTravel.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object sender, EventArgs args)
				{
					messageBoxCantTravel.Close();
				};
				messageBoxCantTravel.GetComponent<EventConfirmAction>().ThirdButtonActionCalled += this.BuyClick_ThirdButtonActionCalled;
			}
			else if (!CompleteMessage.IsBuyingActive)
			{
				CompleteMessage.SetBuyingActive(true);
				PhotonConnectionFactory.Instance.BuyLicense(component.Licence, component.Term);
				AnalyticsFacade.WriteSpentSilver(component.Licence, (int)component.Cost, (StaticUserData.CurrentPond == null) ? AnalyticsFacade.ShopLocation.GlobalShop : AnalyticsFacade.ShopLocation.LocalShop);
			}
		}
		else
		{
			string text = ((!(component.Currency == "GC")) ? ScriptLocalization.Get("TravelNotEnoughMoney") : ScriptLocalization.Get("HaventMoney"));
			this.messageBox = this.helpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), text, false, false, false, null);
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
		}
	}

	public void BuyProduct()
	{
		StorageBoxItem component = base.GetComponent<StorageBoxItem>();
		if (component == null || component.Product == null)
		{
			return;
		}
		if ((component.Product.TypeId == 5 && component.Product.InventoryExt != null) || (component.Product.TypeId == 7 && component.Product.BuoyExt != null) || (component.Product.TypeId == 6 && component.Product.RodSetupExt != null))
		{
			if (this.IsFastBuy)
			{
				this.BuyProduct(component.Product);
			}
			else
			{
				GameObject gameObject = InfoMessageController.Instance.gameObject;
				GameObject tempMessageBox = GUITools.AddChild(gameObject, MessageBoxList.Instance.MessageBoxWithCurrencyPrefab);
				tempMessageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
				tempMessageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
				string text = string.Empty;
				int typeId = component.Product.TypeId;
				if (typeId != 5)
				{
					if (typeId != 7)
					{
						if (typeId == 6)
						{
							text = string.Format(ScriptLocalization.Get("RodPresetAmountBuySlotConfirm"), component.Product.RodSetupExt.Value);
						}
					}
					else
					{
						text = string.Format(ScriptLocalization.Get("BuoyBuySlotConfirm"), component.Product.BuoyExt.Value);
					}
				}
				else
				{
					text = string.Format(ScriptLocalization.Get("StorageBoxBuySlotConfirm"), component.Product.InventoryExt.Value);
				}
				double price = component.Product.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow);
				tempMessageBox.GetComponent<MessageBoxSellItem>().Init(text, component.Product.ProductCurrency, price.ToString(CultureInfo.InvariantCulture));
				tempMessageBox.GetComponent<MessageBox>().ConfirmButtonText = ScriptLocalization.Get("YesCaption");
				tempMessageBox.GetComponent<MessageBox>().CancelButtonText = ScriptLocalization.Get("NoCaption");
				tempMessageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate
				{
					tempMessageBox.GetComponent<MessageBox>().Close();
					this.BuyProduct(component.Product);
				};
				tempMessageBox.GetComponent<EventConfirmAction>().CancelActionCalled += delegate
				{
					tempMessageBox.GetComponent<MessageBox>().Close();
				};
				tempMessageBox.GetComponent<MessageBox>().Open();
				if (MessageFactory.MessageBoxQueue.Contains(tempMessageBox.GetComponent<MessageBox>()))
				{
					MessageFactory.MessageBoxQueue.Remove(tempMessageBox.GetComponent<MessageBox>());
				}
			}
		}
		else
		{
			this.BuyProduct(component.Product);
		}
	}

	public void BuyProduct(StoreProduct product)
	{
		BuyClick.Buy(product);
	}

	public static void Buy(StoreProduct product)
	{
		if (CompleteMessage.IsBuyingActive)
		{
			return;
		}
		CompleteMessage.SetBuyingActive(true);
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		double price = product.GetPrice(TimeHelper.UtcTime());
		if ((product.ProductCurrency == "SC" && profile.SilverCoins >= price) || (product.ProductCurrency == "GC" && profile.GoldCoins >= price))
		{
			PhotonConnectionFactory.Instance.BuyProduct(product, null);
		}
		else
		{
			CompleteMessage.StopWaiting(false);
			UIAudioSourceListener.Instance.Fail();
			UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), (!(product.ProductCurrency == "GC")) ? ScriptLocalization.Get("TravelNotEnoughMoney") : ScriptLocalization.Get("HaventMoney"), true, delegate
			{
				CompleteMessage.SetBuyingActive(false);
			}, StaticUserData.CurrentPond != null && GameFactory.Player != null && GameFactory.Player.State == typeof(ShowMap));
		}
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
		if (!CompleteMessage.IsBuyingActive)
		{
			InventoryItemComponent component = base.GetComponent<InventoryItemComponent>();
			if (component != null && component.InventoryItem != null)
			{
				CompleteMessage.SetBuyingActive(true);
				PhotonConnectionFactory.Instance.BuyItemAndSendToBase(component.InventoryItem, null);
			}
		}
		this.CompleteMessage_ActionCalled(sender, e);
	}

	private void ExpandWindowCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		base.StartCoroutine(this.WaitAndShowExpand());
	}

	private IEnumerator WaitAndShowExpand()
	{
		yield return new WaitForSeconds(0.3f);
		MenuHelpers.Instance.ShowBuyProductsOfTypeWindow(null, ScriptLocalization.Get("StorageBoxPopupLabel"), 5);
		yield break;
	}

	public void OnScroll(PointerEventData eventData)
	{
		ScrollInit componentInParent = base.GetComponentInParent<ScrollInit>();
		if (componentInParent != null)
		{
			componentInParent.OnScroll(eventData);
		}
	}

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox messageBox;

	public bool IsFastBuy;
}
