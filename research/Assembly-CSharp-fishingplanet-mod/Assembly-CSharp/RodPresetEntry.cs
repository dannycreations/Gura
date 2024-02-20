using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using I2.Loc;
using JetBrains.Annotations;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class RodPresetEntry : MonoBehaviour
{
	private void Awake()
	{
		this._initialSize = ((RectTransform)this.PresetName.transform).sizeDelta;
	}

	public void NavigateInside(bool toggleValue)
	{
		MonoBehaviour.print("navigate inside called on: " + base.name);
		this.Border.isOn = true;
	}

	public void SetEmpty()
	{
		this.PresetName.gameObject.SetActive(false);
		this.SaveCurrentContent.SetActive(false);
		this.WorkingContent.SetActive(false);
		this.BuyContent.SetActive(false);
		this.EditContent.SetActive(false);
		this.EditButton.SetActive(false);
		this.EmptyContent.SetActive(true);
		this.isBuyButton = false;
		this._setup = null;
		this._product = null;
		((RectTransform)this.PresetName.transform).sizeDelta = this._initialSize;
	}

	public void SetBuySlot()
	{
		StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.TypeId == 6 && x.RodSetupExt != null && x.RodSetupExt.Value == 1);
		this.PresetName.gameObject.SetActive(false);
		this.SaveCurrentContent.SetActive(false);
		this.WorkingContent.SetActive(false);
		this.BuyContent.SetActive(true);
		this.EditContent.SetActive(false);
		this.EditButton.SetActive(false);
		this.EmptyContent.SetActive(false);
		this.isBuyButton = true;
		if (storeProduct == null)
		{
			Debug.LogError("no rod preset extension product found");
			return;
		}
		this.price = (float)storeProduct.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow);
		this.currency = storeProduct.ProductCurrency;
		this.SlotCurrency.text = ((!(this.currency != "GC")) ? "\ue62c" : "\ue62b");
		this.SlotPrice.text = this.price.ToString();
		this._buyClick = base.GetComponent<BuyClick>();
		this._product = storeProduct;
	}

	public void Init(RodSetup setup)
	{
		this.isSaveMode = false;
		this.EmptyContent.SetActive(false);
		this.WorkingContent.SetActive(false);
		this.BuyContent.SetActive(false);
		this.EditContent.SetActive(false);
		this.EditButton.SetActive(false);
		this.SaveCurrentContent.SetActive(false);
		this.PresetName.gameObject.SetActive(true);
		this._setup = setup;
		this.PresetName.text = this._setup.Name;
		this.isBuyButton = false;
		((RectTransform)this.PresetName.transform).sizeDelta = this._initialSize + new Vector2(250f, 0f);
	}

	private void Update()
	{
		if (this.isEdit && ControlsController.ControlsActions.UISubmit.WasPressed)
		{
			this.AcceptRename();
		}
	}

	public void Rename()
	{
		this.WorkingContent.SetActive(false);
		this.EditButton.SetActive(false);
		this.EditContent.SetActive(true);
		this.PresetName.gameObject.SetActive(true);
		this.PresetName.interactable = true;
		this.PresetName.Select();
		this.isEdit = true;
		((RectTransform)this.PresetName.transform).sizeDelta = this._initialSize + new Vector2(250f, 0f);
	}

	public void AcceptRename()
	{
		this.EditButton.SetActive(true);
		this.EditContent.SetActive(false);
		string text = AbusiveWords.ReplaceAbusiveWords(this.PresetName.text);
		this.PresetName.interactable = false;
		this.WorkingContent.SetActive(true);
		((RectTransform)this.PresetName.transform).sizeDelta = this._initialSize;
		this.isEdit = false;
		this.OnDeselect();
		if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanRenameSetup(this._setup, text))
		{
			GameFactory.Message.ShowMessage(ScriptLocalization.Get("CantRenameRodSetup") + PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject, 2f, false);
			this.CancelRename();
			return;
		}
		PhotonConnectionFactory.Instance.RodSetupRename(this._setup, text);
		GameFactory.Message.ShowMessage(ScriptLocalization.Get("RodPresetRenamedTooltip"), base.transform.root.gameObject, 2f, false);
	}

	public void CancelRename()
	{
		this.EditButton.SetActive(true);
		this.EditContent.SetActive(false);
		this.PresetName.text = this._setup.Name;
		this.PresetName.interactable = false;
		this.WorkingContent.SetActive(true);
		((RectTransform)this.PresetName.transform).sizeDelta = this._initialSize;
		this.NavigateInside(true);
		this.isEdit = false;
		this.OnDeselect();
	}

	private string GetName(InventoryItem rod)
	{
		int num = 1;
		int num2 = 1;
		int num3 = 1;
		int num4 = 1;
		int num5 = 1;
		int num6 = 1;
		int num7 = 1;
		int num8 = 1;
		foreach (RodSetup rodSetup in PhotonConnectionFactory.Instance.Profile.InventoryRodSetups)
		{
			ItemSubTypes itemSubType = rodSetup.Rod.ItemSubType;
			switch (itemSubType)
			{
			case ItemSubTypes.TelescopicRod:
				num2++;
				break;
			case ItemSubTypes.MatchRod:
				num7++;
				break;
			case ItemSubTypes.SpinningRod:
				num3++;
				break;
			case ItemSubTypes.CastingRod:
				num++;
				break;
			case ItemSubTypes.FeederRod:
				num4++;
				break;
			case ItemSubTypes.BottomRod:
				num5++;
				break;
			default:
				if (itemSubType != ItemSubTypes.CarpRod)
				{
					if (itemSubType == ItemSubTypes.SpodRod)
					{
						num8++;
					}
				}
				else
				{
					num6++;
				}
				break;
			}
		}
		int num9 = 0;
		ItemSubTypes itemSubType2 = rod.ItemSubType;
		switch (itemSubType2)
		{
		case ItemSubTypes.TelescopicRod:
			num9 = num2;
			break;
		case ItemSubTypes.MatchRod:
			num9 = num7;
			break;
		case ItemSubTypes.SpinningRod:
			num9 = num3;
			break;
		case ItemSubTypes.CastingRod:
			num9 = num;
			break;
		case ItemSubTypes.FeederRod:
			num9 = num4;
			break;
		case ItemSubTypes.BottomRod:
			num9 = num5;
			break;
		default:
			if (itemSubType2 != ItemSubTypes.CarpRod)
			{
				if (itemSubType2 == ItemSubTypes.SpodRod)
				{
					num9 = num8;
				}
			}
			else
			{
				num9 = num6;
			}
			break;
		}
		return string.Format("{0} Preset #{1}", Inventory.GetRodSubtypeName(rod), num9);
	}

	public void OnSelect()
	{
		if (RodPresetEntry.LastSelected == this && this.isEdit)
		{
			this.CancelRename();
		}
		if (this._setup == null && !this.isBuyButton)
		{
			this.EmptyContent.SetActive(false);
			this.SaveCurrentContent.SetActive(true);
			this.isPreSaveMode = true;
			this.Preview.CopyFromActiveRod();
		}
		else if (this._setup != null)
		{
			this.Preview.Show(this._setup);
			this.EditButton.SetActive(true);
			this.WorkingContent.SetActive(true);
			((RectTransform)this.PresetName.transform).sizeDelta = this._initialSize;
		}
		if (RodPresetEntry.LastSelected != this && RodPresetEntry.LastSelected != null)
		{
			RodPresetEntry.LastSelected.OnDeselect();
		}
		RodPresetEntry.LastSelected = this;
	}

	public void OnClick()
	{
		if (this.isSaveMode)
		{
			this.SaveCurrent();
			this.isSaveMode = false;
		}
		if (this.isPreSaveMode)
		{
			this.isSaveMode = true;
			this.isPreSaveMode = false;
		}
		if (this.isBuyButton)
		{
			this.BuyClick();
		}
	}

	[UsedImplicitly]
	private void CheckAndHide()
	{
		if (this.Border.isOn)
		{
			return;
		}
		if (this._setup == null && !this.isBuyButton)
		{
			this.SetEmpty();
		}
		this.SaveCurrentContent.SetActive(false);
		this.EditButton.SetActive(false);
		this.WorkingContent.SetActive(false);
		((RectTransform)this.PresetName.transform).sizeDelta = this._initialSize + new Vector2(250f, 0f);
	}

	public void OnDeselect()
	{
		this.Border.isOn = false;
		this.isSaveMode = false;
		this.isPreSaveMode = false;
		this.CheckAndHide();
		if (this.isEdit)
		{
			this.CancelRename();
		}
	}

	public bool CanSave()
	{
		InitRod activeRod = this.Preview.TackleContent.ActiveRod;
		List<InventoryItem> list = new List<InventoryItem>();
		if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanSaveNewSetup(activeRod.SlotId, "name"))
		{
			GameFactory.Message.KillLastMessage();
			GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
			return false;
		}
		if (InventoryHelper.MatchedTemplate(activeRod.Rod.InventoryItem as Rod) == RodTemplate.UnEquiped)
		{
			this.messageBox = MenuHelpers.Instance.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("RodPresetSaveNotFullyEquipped"), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, false).gameObject;
			this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.SaveAnyway;
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CloseMessageBox;
			this.messageBox.GetComponent<AlphaFade>().ShowPanel();
			return false;
		}
		return true;
	}

	public void CanEquip()
	{
		InitRod activeRod = this.Preview.TackleContent.ActiveRod;
		List<InventoryItem> items = this._setup.Items;
		bool flag = true;
		string text = string.Empty;
		List<InventoryItem> missingItems = this._setup.GetMissingItems(activeRod.SlotId);
		foreach (InventoryItem inventoryItem in missingItems)
		{
			flag = false;
			if (text != string.Empty)
			{
				text += ", ";
			}
			text += inventoryItem.Name;
		}
		if (!flag)
		{
			List<InventoryItem> itemsInHands = new List<InventoryItem>();
			foreach (InventoryItem inventoryItem2 in PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem i) => i is Rod && (i.Storage == StoragePlaces.Doll || i.Storage == StoragePlaces.Hands) && i.Slot != activeRod.SlotId))
			{
				itemsInHands.Add(inventoryItem2);
				itemsInHands.AddRange(PhotonConnectionFactory.Instance.Profile.Inventory.GetRodEquipment(inventoryItem2 as Rod));
			}
			if (missingItems.All((InventoryItem x) => itemsInHands.FirstOrDefault((InventoryItem y) => y.ItemId == x.ItemId) != null))
			{
				this.messageBox = MenuHelpers.Instance.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("RodSetupCantEquipMissingItems"), false, false, false, null).gameObject;
				this.messageBox.GetComponent<EventAction>().ActionCalled += this.CloseMessageBox;
				this.messageBox.GetComponent<MessageBox>().Open();
			}
			else
			{
				this.messageBox = MenuHelpers.Instance.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("RodPresetItemsUnavailable"), text), ScriptLocalization.Get("ShopButtonPopup"), ScriptLocalization.Get("CancelButton"), false, false).gameObject;
				this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.OpenShop;
				this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CloseMessageBox;
				this.messageBox.GetComponent<MessageBox>().Open();
			}
			return;
		}
		if (activeRod.Rod.InventoryItem != null)
		{
			this.messageBox = MenuHelpers.Instance.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("RodPresetEquipConfirm"), this._setup.Name), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, false).gameObject;
			this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.EquipAnyway;
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CloseMessageBox;
			this.messageBox.GetComponent<MessageBox>().Open();
		}
		else
		{
			this.EquipAnyway(null, null);
		}
	}

	public void CanRemove()
	{
		if (this._setup != null)
		{
			this.messageBox = MenuHelpers.Instance.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("RodPresetClearConfirm"), this._setup.Name), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, false).gameObject;
			this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.RemoveAnyway;
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CloseMessageBox;
			this.messageBox.GetComponent<MessageBox>().Open();
		}
	}

	private void OpenShop(object sender, EventArgs e)
	{
		this.CloseMessageBox(sender, e);
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

	private void EquipAnyway(object sender, EventArgs e)
	{
		this.CloseMessageBox(sender, e);
		if (PhotonConnectionFactory.Instance.Profile.Inventory.CanEquipSetup(this._setup, this.Preview.TackleContent.ActiveRod.SlotId))
		{
			PhotonConnectionFactory.Instance.RodSetupEquip(this._setup, this.Preview.TackleContent.ActiveRod.SlotId);
			this.Preview.Show(this._setup);
		}
		else
		{
			GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
		}
	}

	private void SaveAnyway(object sender, EventArgs e)
	{
		this.PerformSave();
		this.CloseMessageBox(sender, e);
	}

	private void RemoveAnyway(object sender, EventArgs e)
	{
		this.RemovePreset();
		this.CloseMessageBox(sender, e);
	}

	private void CloseMessageBox(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.GetComponent<MessageBox>().Close();
		}
	}

	private void RemovePreset()
	{
		PhotonConnectionFactory.Instance.RodSetupRemove(this._setup);
		this.Preview.Reset(false);
		this.SetEmpty();
	}

	private void PerformSave()
	{
		this.Preview.CopyFromActiveRod();
		string text = "Rod Preset";
		if (this.Preview.TackleContent.ActiveRod != null)
		{
			text = this.GetName(this.Preview.TackleContent.ActiveRod.Rod.InventoryItem);
		}
		this.SetEmpty();
		this.EmptyContent.SetActive(false);
		this.PresetName.gameObject.SetActive(true);
		this.PresetName.text = text;
		PhotonConnectionFactory.Instance.RodSetupSaveNew(this.Preview.TackleContent.ActiveRod.SlotId, text);
	}

	public void SaveCurrent()
	{
		if (this.CanSave())
		{
			this.PerformSave();
		}
		this.SetEmpty();
	}

	public void Equip(bool val)
	{
		this.CanEquip();
	}

	public void RemovePreset(bool val)
	{
		this.CanRemove();
	}

	public void SharePreset(bool val)
	{
		ShareRodPresetFriendList friendsList = GUITools.AddChild(InfoMessageController.Instance.gameObject, MessageBoxList.Instance.SelectFriendsWindow).GetComponent<ShareRodPresetFriendList>();
		friendsList.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		friendsList.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		friendsList.Init(this._setup);
		friendsList.GetComponent<EventConfirmAction>().CancelActionCalled += delegate
		{
			friendsList.Close();
		};
		ShareRodPresetFriendList friendsList3 = friendsList;
		friendsList3.ConfirmAction = (Action<string>)Delegate.Combine(friendsList3.ConfirmAction, new Action<string>(this.Share));
		ShareRodPresetFriendList friendsList2 = friendsList;
		friendsList2.ConfirmAction = (Action<string>)Delegate.Combine(friendsList2.ConfirmAction, new Action<string>(delegate
		{
			friendsList.Close();
		}));
	}

	private void Share(string friend)
	{
		PhotonConnectionFactory.Instance.RodSetupShare(this._setup, friend);
	}

	private void BuySlot(object sender, EventArgs e)
	{
		MonoBehaviour.print("buy slot called");
		this._buyClick.BuyProduct(this._product);
		this.CloseMessageBox(sender, e);
	}

	public void BuyClick()
	{
		this.messageBox = GUITools.AddChild(InfoMessageController.Instance.gameObject, MessageBoxList.Instance.MessageBoxWithCurrencyPrefab);
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this.messageBox.GetComponent<MessageBoxSellItem>().Init(ScriptLocalization.Get("RodPresetBuySlotConfirm"), this.currency, this.price.ToString(CultureInfo.InvariantCulture));
		this.messageBox.GetComponent<MessageBox>().ConfirmButtonText = ScriptLocalization.Get("YesCaption");
		this.messageBox.GetComponent<MessageBox>().CancelButtonText = ScriptLocalization.Get("NoCaption");
		this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.BuySlot;
		this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CloseMessageBox;
		this.messageBox.GetComponent<MessageBox>().Open();
	}

	public InputField PresetName;

	public RodPresetPreview Preview;

	public GameObject EmptyContent;

	public GameObject SaveCurrentContent;

	public GameObject BuyContent;

	public GameObject WorkingContent;

	public GameObject EditContent;

	public GameObject EditButton;

	public Toggle Border;

	public Text SlotCurrency;

	public Text SlotPrice;

	private float price;

	private string currency;

	public static RodPresetEntry LastSelected;

	private RodSetup _setup;

	private bool isBuyButton;

	private bool isEdit;

	private GameObject messageBox;

	private BuyClick _buyClick;

	private StoreProduct _product;

	private Vector2 _initialSize;

	private const float widthAddition = 250f;

	private bool isPreSaveMode;

	private bool isSaveMode;
}
