using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RodSetupDeliveredInit : MonoBehaviour
{
	internal void Start()
	{
		PhotonConnectionFactory.Instance.OnProductBought += this.OnProductBought;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnProductBought -= this.OnProductBought;
	}

	internal void Init(RodSetup setup, string friendName)
	{
		this._setup = setup;
		this._buyClick = base.GetComponent<BuyClick>();
		string text = string.Format("<color=#FFDD77FF>" + friendName + "</color>", new object[0]);
		this.Title.text = string.Format(ScriptLocalization.Get("RecievedRodSetupText"), text);
		this.Name.text = setup.Name.ToUpper();
		string text2 = string.Empty;
		foreach (InventoryItem inventoryItem in setup.Items)
		{
			if (text2 != string.Empty)
			{
				text2 += "\n";
			}
			text2 = text2 + "<voffset=0.18em><color=#FFDD77FF><size=-10>\ue788 </size></color></voffset> " + inventoryItem.Name;
		}
		this.Desc.text = text2;
		this._noSlots = PhotonConnectionFactory.Instance.Profile.InventoryRodSetups != null && !PhotonConnectionFactory.Instance.Profile.InventoryRodSetups.Contains(setup);
		this.InavailableParent.SetActive(this._noSlots);
		this.AcceptButton.SetActive(!this._noSlots);
		if (this._noSlots)
		{
			this.product = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.TypeId == 6 && x.RodSetupExt != null && x.RodSetupExt.Value == 1 && PhotonConnectionFactory.Instance.Profile.Inventory.CurrentRodSetupCapacity + x.RodSetupExt.Value <= Inventory.MaxRodSetupCapacity);
			if (this.product != null)
			{
				this.price = (float)this.product.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow);
				this.CostValue.text = this.price.ToString();
			}
			this.BuySlotButton.SetActive(this.product != null);
		}
		bool flag = false;
		if (PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.Friends != null)
		{
			for (int i = 0; i < PhotonConnectionFactory.Instance.Profile.Friends.Count; i++)
			{
				Player player = PhotonConnectionFactory.Instance.Profile.Friends[i];
				if (player.UserName.Equals(friendName, StringComparison.InvariantCultureIgnoreCase) && player.Status == FriendStatus.Friend)
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
			this.Friend.Username.text = friendName;
			this.Friend.LevelNumber.gameObject.SetActive(false);
		}
	}

	public void OpenShop()
	{
		Transform transform = MenuHelpers.Instance.GetFormByName(FormsEnum.TopDashboard).transform.Find("Image");
		IEnumerable<Toggle> enumerable = transform.Find("TopMenuLeft").Find("btnInventory").GetComponent<Toggle>()
			.group.ActiveToggles();
		foreach (Toggle toggle in enumerable)
		{
			toggle.isOn = false;
		}
		transform.Find("TopMenuLeft").Find("btnShop").GetComponent<Toggle>()
			.group.SetAllTogglesOff();
		transform.Find("TopMenuLeft").Find("btnShop").GetComponent<Toggle>()
			.isOn = true;
	}

	public void BuyClick()
	{
		this.messageBox = GUITools.AddChild(InfoMessageController.Instance.gameObject, MessageBoxList.Instance.MessageBoxWithCurrencyPrefab);
		MessageBox component = this.messageBox.GetComponent<MessageBox>();
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		if (MessageFactory.MessageBoxQueue.Contains(component))
		{
			MessageFactory.MessageBoxQueue.Remove(component);
		}
		this.messageBox.GetComponent<MessageBoxSellItem>().Init(ScriptLocalization.Get("RodPresetBuySlotConfirm"), this.product.ProductCurrency, this.price.ToString(CultureInfo.InvariantCulture));
		component.ConfirmButtonText = ScriptLocalization.Get("YesCaption");
		component.CancelButtonText = ScriptLocalization.Get("NoCaption");
		this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.BuySlot;
		this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CloseMessageBox;
		component.Open();
	}

	private void BuySlot(object sender, EventArgs e)
	{
		this._buyClick.BuyProduct(this.product);
		this.CloseMessageBox(sender, e);
	}

	public void Close()
	{
		this.CloseMessageBox(null, null);
		base.GetComponent<AlphaFade>().HidePanel();
	}

	public void DeclinePreset()
	{
		if (this._noSlots)
		{
			this.Close();
			return;
		}
		PhotonConnectionFactory.Instance.RodSetupRemove(this._setup);
		PhotonConnectionFactory.Instance.OnRodSetupRemove += this.OnRodSetupRemoved;
		PhotonConnectionFactory.Instance.OnRodSetupRemoveFailure += this.OnRodSetupRemoveFailure;
	}

	private void OnRodSetupRemoveFailure(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnRodSetupRemove -= this.OnRodSetupRemoved;
		PhotonConnectionFactory.Instance.OnRodSetupRemoveFailure -= this.OnRodSetupRemoveFailure;
		Debug.LogError(failure.ErrorMessage);
		this.Close();
	}

	private void OnRodSetupRemoved(RodSetup setup)
	{
		PhotonConnectionFactory.Instance.OnRodSetupRemove -= this.OnRodSetupRemoved;
		PhotonConnectionFactory.Instance.OnRodSetupRemoveFailure -= this.OnRodSetupAddFailure;
		this.Close();
	}

	private void CloseMessageBox(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.GetComponent<MessageBox>().Close();
		}
	}

	public void OnProductBought(ProfileProduct product, int count)
	{
		if (this.product.ProductId == product.ProductId)
		{
			PhotonConnectionFactory.Instance.RodSetupAdd(this._setup);
			PhotonConnectionFactory.Instance.OnAddRodSetup += this.OnRodSetupAdded;
			PhotonConnectionFactory.Instance.OnAddRodSetupFailure += this.OnRodSetupAddFailure;
		}
	}

	private void OnRodSetupAdded(RodSetup setup)
	{
		PhotonConnectionFactory.Instance.OnAddRodSetup -= this.OnRodSetupAdded;
		PhotonConnectionFactory.Instance.OnAddRodSetupFailure -= this.OnRodSetupAddFailure;
		this.Close();
	}

	private void OnRodSetupAddFailure(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnAddRodSetup -= this.OnRodSetupAdded;
		PhotonConnectionFactory.Instance.OnAddRodSetupFailure -= this.OnRodSetupAddFailure;
		Debug.LogError(failure.ErrorMessage);
		this.CloseMessageBox(null, null);
	}

	public Text Title;

	public Text Name;

	public TextMeshProUGUI Desc;

	public GameObject BuySlotButton;

	public GameObject AcceptButton;

	public GameObject InavailableParent;

	public Text CostValue;

	public FriendsListItemInit Friend;

	private GameObject messageBox;

	private float price;

	private BuyClick _buyClick;

	private RodSetup _setup;

	private StoreProduct product;

	private bool _noSlots;
}
