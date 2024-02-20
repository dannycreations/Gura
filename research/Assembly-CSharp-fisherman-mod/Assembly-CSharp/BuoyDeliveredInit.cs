using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class BuoyDeliveredInit : MessageBoxBase
{
	internal void Init(BuoySetting buoy, bool showCount = false)
	{
		this._buoy = buoy;
		List<BuoySetting> buoyShareRequests = PhotonConnectionFactory.Instance.Profile.BuoyShareRequests;
		this._currentIndex = buoyShareRequests.FindIndex((BuoySetting x) => x == buoy);
		this._buyClick = base.GetComponent<BuyClick>();
		string text = string.Format("<color=#FFDD77>" + buoy.Sender + "</color>", new object[0]);
		this.InfoTitle.text = string.Format(ScriptLocalization.Get("RecievedBuoyText"), text);
		if (showCount || buoyShareRequests.Count > 1)
		{
			this.Title.text = string.Format("<b>{0}</b> <color=#AAAAAA>({1}/{2})</color>", ScriptLocalization.Get("SharedBuoysTitle"), this._currentIndex + 1, buoyShareRequests.Count);
		}
		else
		{
			this.Title.text = string.Format("<b>{0}</b>", ScriptLocalization.Get("BuoyCaption"));
		}
		this.Name.text = (string.IsNullOrEmpty(buoy.Name) ? ScriptLocalization.Get("BuoyCaption") : buoy.Name.ToUpper(CultureInfo.InvariantCulture));
		DateTime dateTime = DateTime.Now;
		if (buoy.CreatedTime != null)
		{
			dateTime = buoy.CreatedTime.Value;
		}
		if (buoy.Fish != null)
		{
			this.FishInfo.Refresh(buoy.Fish, dateTime, buoy.PondId);
		}
		else
		{
			this.FishInfo.SetPanelEmpty();
		}
		bool flag = false;
		if (PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.Friends != null)
		{
			for (int i = 0; i < PhotonConnectionFactory.Instance.Profile.Friends.Count; i++)
			{
				Player player = PhotonConnectionFactory.Instance.Profile.Friends[i];
				if (player.UserName.Equals(buoy.Sender, StringComparison.InvariantCultureIgnoreCase) && player.Status == FriendStatus.Friend)
				{
					flag = true;
					this.Friend.Init(player);
					this.Friend.Status.gameObject.SetActive(player.IsOnline);
					if (player.IsOnline)
					{
						this.Friend.Status.GetComponent<Image>().color = new Color(0.27058825f, 0.6627451f, 0.29803923f);
					}
				}
			}
		}
		if (!flag)
		{
			this.Friend.Username.text = buoy.Sender;
			this.Friend.LevelNumber.gameObject.SetActive(false);
		}
		this.BuySlotButton.SetActive(false);
		bool flag2 = PhotonConnectionFactory.Instance.Profile.Buoys != null && PhotonConnectionFactory.Instance.Profile.Buoys.Count >= PhotonConnectionFactory.Instance.Profile.Inventory.CurrentBuoyCapacity;
		this.BuoyInavailableParent.SetActive(flag2);
		bool flag3 = StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId == buoy.PondId;
		this.PondSpecificTitle.text = (flag3 ? string.Empty : (string.Format(ScriptLocalization.Get("BuoyOnlyOnPondWarning"), "<color=#FFDD77><b>" + CacheLibrary.MapCache.CachedPonds.First((Pond x) => x.PondId == buoy.PondId).Name) + "</b></color>"));
		if (flag3 && flag2)
		{
			this.product = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.TypeId == 7 && x.BuoyExt != null && x.BuoyExt.Value == 1 && PhotonConnectionFactory.Instance.Profile.Inventory.CurrentBuoyCapacity + x.BuoyExt.Value <= Inventory.MaxBuoyCapacity);
			if (this.product != null)
			{
				this.price = (float)this.product.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow);
				this.BuySlotButtonPrice.text = this.price.ToString(CultureInfo.InvariantCulture);
			}
			this.BuySlotButton.SetActive(this.product != null);
		}
		this.AcceptBuoy.gameObject.SetActive(!flag2 && flag3);
		this.RefreshButtonsVisibility();
	}

	public void OnEnable()
	{
		PhotonConnectionFactory.Instance.OnProductBought += this.OnProductBought;
	}

	private void OnDisable()
	{
		MonoBehaviour.print("disabled");
		PhotonConnectionFactory.Instance.OnProductBought -= this.OnProductBought;
	}

	public void SwitchIndex(bool right)
	{
		if (right)
		{
			this._currentIndex++;
		}
		else
		{
			this._currentIndex--;
		}
		if (this._currentIndex < 0)
		{
			this._currentIndex = PhotonConnectionFactory.Instance.Profile.BuoyShareRequests.Count - 1;
		}
		else if (this._currentIndex >= PhotonConnectionFactory.Instance.Profile.BuoyShareRequests.Count)
		{
			this._currentIndex = 0;
		}
		if (PhotonConnectionFactory.Instance.Profile.BuoyShareRequests.Count == 0)
		{
			this.Close();
			return;
		}
		BuoySetting buoySetting = PhotonConnectionFactory.Instance.Profile.BuoyShareRequests[this._currentIndex];
		this.Init(buoySetting, true);
	}

	private void RefreshButtonsVisibility()
	{
		List<BuoySetting> buoyShareRequests = PhotonConnectionFactory.Instance.Profile.BuoyShareRequests;
		if (buoyShareRequests.Count == 0)
		{
			this.Close();
			return;
		}
		this.LeftButton.gameObject.SetActive(buoyShareRequests.Count > 1);
		this.RightButton.gameObject.SetActive(buoyShareRequests.Count > 1);
		this.LeftButton.interactable = this._currentIndex != 0;
		this.RightButton.interactable = this._currentIndex < buoyShareRequests.Count - 1;
	}

	public void BuyClick()
	{
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		this.AlphaFade.CanvasGroup.interactable = false;
		this.messageBox = GUITools.AddChild(gameObject, MessageBoxList.Instance.MessageBoxWithCurrencyPrefab).GetComponent<MessageBox>();
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this.messageBox.GetComponent<MessageBoxSellItem>().Init(string.Format(ScriptLocalization.Get("BuoyBuySlotConfirm"), 1), this.product.ProductCurrency, this.price.ToString());
		this.messageBox.ConfirmButtonText = ScriptLocalization.Get("YesCaption");
		this.messageBox.CancelButtonText = ScriptLocalization.Get("NoCaption");
		this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.BuySlot;
		this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CloaseAndActivateInteractable;
		this.messageBox.Open();
	}

	public void AddClick()
	{
		this.AlphaFade.CanvasGroup.interactable = false;
		PhotonConnectionFactory.Instance.AcceptSharedBuoy(this._buoy.BuoyId);
		PhotonConnectionFactory.Instance.OnBuoyShareAccepted += this.OnBuoyShareAccepted;
		PhotonConnectionFactory.Instance.OnBuoyShareAcceptingFailed += this.OnBuoyShareAcceptingFailed;
	}

	private void OnBuoyShareAccepted()
	{
		GameFactory.Message.ShowMessage(ScriptLocalization.Get("BuoyAccepted"), null, 3f, false);
		this.SwitchIndex(false);
		PhotonConnectionFactory.Instance.OnBuoyShareAccepted -= this.OnBuoyShareAccepted;
		PhotonConnectionFactory.Instance.OnBuoyShareAcceptingFailed -= this.OnBuoyShareAcceptingFailed;
		this.AlphaFade.CanvasGroup.interactable = true;
	}

	private void OnBuoyShareDeclined()
	{
		GameFactory.Message.ShowMessage(ScriptLocalization.Get("BuoyDeclined"), null, 3f, false);
		this.SwitchIndex(false);
		PhotonConnectionFactory.Instance.OnBuoyShareDeclined -= this.OnBuoyShareDeclined;
		PhotonConnectionFactory.Instance.OnBuoyShareDecliningFailed -= this.OnBuoyShareDecliningFailed;
		this.AlphaFade.CanvasGroup.interactable = true;
	}

	private void OnBuoyShareAcceptingFailed(Failure failure)
	{
		GameFactory.Message.ShowMessage(failure.ErrorMessage, null, 3f, false);
		PhotonConnectionFactory.Instance.OnBuoyShareAccepted -= this.OnBuoyShareAccepted;
		PhotonConnectionFactory.Instance.OnBuoyShareAcceptingFailed -= this.OnBuoyShareAcceptingFailed;
		this.AlphaFade.CanvasGroup.interactable = true;
	}

	private void OnBuoyShareDecliningFailed(Failure failure)
	{
		GameFactory.Message.ShowMessage(failure.ErrorMessage, null, 3f, false);
		PhotonConnectionFactory.Instance.OnBuoyShareDeclined -= this.OnBuoyShareDeclined;
		PhotonConnectionFactory.Instance.OnBuoyShareDecliningFailed -= this.OnBuoyShareDecliningFailed;
		this.SwitchIndex(false);
		this.AlphaFade.CanvasGroup.interactable = true;
	}

	public void RemoveClick()
	{
		this.AlphaFade.CanvasGroup.interactable = false;
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		GameObject MessageBox = MenuHelpers.Instance.ShowMessageSelectable(gameObject, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("RemoveBuoyConfirm"), true).gameObject;
		MessageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object ea, EventArgs o)
		{
			if (MessageBox != null)
			{
				MessageBox.GetComponent<MessageBox>().Close();
			}
			PhotonConnectionFactory.Instance.DeclineSharedBuoy(this._buoy.BuoyId);
			PhotonConnectionFactory.Instance.OnBuoyShareDeclined += this.OnBuoyShareDeclined;
			PhotonConnectionFactory.Instance.OnBuoyShareDecliningFailed += this.OnBuoyShareAcceptingFailed;
		};
		MessageBox.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object ea, EventArgs o)
		{
			this.AlphaFade.CanvasGroup.interactable = true;
			if (MessageBox != null)
			{
				MessageBox.GetComponent<MessageBox>().Close();
			}
		};
	}

	private void BuySlot(object sender, EventArgs e)
	{
		this._buyClick.BuyProduct(this.product);
		this.messageBox.Close();
	}

	private void CloaseAndActivateInteractable(object sender, EventArgs e)
	{
		this.AlphaFade.CanvasGroup.interactable = true;
		this.messageBox.Close();
	}

	private void OnProductBought(ProfileProduct product, int count)
	{
		this.AlphaFade.CanvasGroup.interactable = true;
		CompleteMessage.SetBuyingActive(false);
		if (this.product.ProductId == product.ProductId)
		{
			MonoBehaviour.print("BuoyDeliveredInit onBuyProduct");
			MonoBehaviour.print("add click called");
			this.AddClick();
		}
	}

	public Text Title;

	public Text InfoTitle;

	public Text PondSpecificTitle;

	public GameObject BuoyInavailableParent;

	public Text Name;

	public ConcreteTrophyInit FishInfo;

	public FriendsListItemInit Friend;

	public GameObject BuySlotButton;

	public Text BuySlotButtonPrice;

	public Button LeftButton;

	public Button RightButton;

	public Button AcceptBuoy;

	public new AlphaFade AlphaFade;

	private MessageBox messageBox;

	private float price;

	private BuyClick _buyClick;

	private StoreProduct product;

	private BuoySetting _buoy;

	private int _currentIndex;

	private string _friendName;
}
