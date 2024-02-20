using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GiftsPopupInit : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	public void Init(Player player)
	{
		this._player = player;
		string text = string.Format("<color=#c3986d>" + this._player.UserName + "</color>", new object[0]);
		this.TitleText.text = string.Format(ScriptLocalization.Get("SelectGiftText"), text);
		List<InventoryItem> list = (from x in PhotonConnectionFactory.Instance.Profile.Inventory
			where x.IsGiftable && x.Storage != StoragePlaces.ParentItem
			orderby x.Name
			select x).ToList<InventoryItem>();
		if (list.Count > 0)
		{
			this.SetSizeForContent(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				this.AddGiftItem(list[i], i);
			}
		}
		else
		{
			this.NoGiftableItemsText.gameObject.SetActive(true);
			this.GiftItemQuantityCounter.text = "0";
			this.SendButton.text = ScriptLocalization.Get("SendButtonLowerCaption").ToUpper(CultureInfo.InvariantCulture) + " (0)";
			this.SendButton.gameObject.transform.parent.GetComponent<Button>().interactable = false;
			this.IncQuantityButton.interactable = false;
			this.DecQuantityButton.interactable = false;
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
		}
		base.GetComponent<AlphaFade>().OnHide.AddListener(delegate
		{
			Object.Destroy(base.gameObject);
		});
	}

	public void AddGiftItem(InventoryItem giftItem, int number)
	{
		GameObject gameObject = GUITools.AddChild(this.GiftListContent, this.GiftItemPrefab);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(35f, 35f, 0f);
		gameObject.GetComponent<GiftItemInit>().Init(giftItem, this.GiftListContent);
		gameObject.GetComponent<GiftItemInit>().OnSelectedItemChange += this.SetCurrentItem;
		if (number == 0)
		{
			gameObject.GetComponent<Toggle>().isOn = true;
		}
	}

	private void SetCurrentItem(InventoryItem item)
	{
		this._currentSelectedItem = item;
		this.GiftItemQuantityCounter.text = "1";
		this.userSetCount = 1;
		this.SendButton.text = ScriptLocalization.Get("SendButtonLowerCaption").ToUpper(CultureInfo.InvariantCulture) + " (" + this.userSetCount.ToString() + ")";
	}

	public void SendGiftItem()
	{
		if (this._currentSelectedItem != null)
		{
			PhotonConnectionFactory.Instance.MakeGift(this._player, this._currentSelectedItem, int.Parse(this.GiftItemQuantityCounter.text));
			PhotonConnectionFactory.Instance.OnGiftMade += this.OnGiftMadeAction;
			PhotonConnectionFactory.Instance.OnMakingGiftFailed += this.OnMakingGiftFailedAction;
		}
	}

	private void OnGiftMadeAction()
	{
		PhotonConnectionFactory.Instance.OnGiftMade -= this.OnGiftMadeAction;
		PhotonConnectionFactory.Instance.OnMakingGiftFailed -= this.OnMakingGiftFailedAction;
		base.GetComponent<AlphaFade>().HidePanel();
	}

	private void OnMakingGiftFailedAction(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGiftMade -= this.OnGiftMadeAction;
		PhotonConnectionFactory.Instance.OnMakingGiftFailed -= this.OnMakingGiftFailedAction;
		base.GetComponent<AlphaFade>().HidePanel();
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		this.messageBox = GUITools.AddChild(gameObject, this.MessageBoxPrefab).GetComponent<MessageBox>();
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.Message = string.Format("{0} <b>{1}</b>", ScriptLocalization.Get("SendFailedText"), this._player.UserName);
		this.messageBox.CancelButtonText = ScriptLocalization.Get("CloseButton");
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CloseMessageBox;
	}

	public void CloseGiftsPopup()
	{
		base.GetComponent<AlphaFade>().HidePanel();
	}

	private void SetSizeForContent(int giftItemsCount)
	{
		RectTransform component = this.GiftListContent.GetComponent<RectTransform>();
		float num = this.GiftItemPrefab.GetComponent<RectTransform>().rect.width * (float)giftItemsCount + component.GetComponent<HorizontalLayoutGroup>().spacing * (float)(giftItemsCount - 1);
		component.sizeDelta = new Vector2(num, component.sizeDelta.y);
		if (component.sizeDelta.x > this.GiftList.GetComponent<RectTransform>().rect.width)
		{
			this.GiftListScrollbar.gameObject.SetActive(true);
		}
		else
		{
			this.GiftListScrollbar.gameObject.SetActive(false);
		}
	}

	public void Increase()
	{
		if (this._currentSelectedItem.Count > this.userSetCount)
		{
			this.userSetCount++;
			this.GiftItemQuantityCounter.text = this.userSetCount.ToString();
			this.SendButton.text = ScriptLocalization.Get("SendButtonLowerCaption").ToUpper(CultureInfo.InvariantCulture) + " (" + this.userSetCount.ToString() + ")";
		}
	}

	public void Decrease()
	{
		if (this.userSetCount > 1)
		{
			this.userSetCount--;
			this.GiftItemQuantityCounter.text = this.userSetCount.ToString();
			this.SendButton.text = ScriptLocalization.Get("SendButtonLowerCaption").ToUpper(CultureInfo.InvariantCulture) + " (" + this.userSetCount.ToString() + ")";
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		int num = (int)eventData.scrollDelta.y;
		if (num > 0)
		{
			this.Increase();
		}
		else
		{
			this.Decrease();
		}
	}

	private void CloseMessageBox(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	public GameObject GiftItemPrefab;

	public GameObject GiftList;

	public GameObject GiftListContent;

	public Text TitleText;

	public GameObject GiftListScrollbar;

	public Text GiftItemQuantityCounter;

	public Text SendButton;

	public Button IncQuantityButton;

	public Button DecQuantityButton;

	public GameObject MessageBoxPrefab;

	public Text NoGiftableItemsText;

	private Player _player;

	private InventoryItem _currentSelectedItem;

	private int userSetCount;

	private MessageBox messageBox;

	private MenuHelpers helpers = new MenuHelpers();
}
