using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitRods : ActivityStateControlled, IChangeActiveRod
{
	public bool IsOtherPlayer
	{
		get
		{
			return this._isOtherPlayer;
		}
	}

	[HideInInspector]
	public InitRod ActiveRod { get; set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action ChangedActiveRod = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OutfitOrRodOrChumMixingSwitched = delegate
	{
	};

	public bool IsChumMixing
	{
		get
		{
			return this.ChumMixToggle.Toggle.isOn;
		}
	}

	private void Awake()
	{
		if (this._rodToggles == null)
		{
			this._rodToggles = new List<RodToggle>();
		}
		this.InitToggleLogic(this.BodyToggle, false);
		if (!this._isOtherPlayer && this.ChumMixToggle != null)
		{
			this.InitToggleLogic(this.ChumMixToggle, false);
		}
		this.BodyToggle.Toggle.group = this.ToggleGroup;
		if (this.ChumMixToggle != null)
		{
			this.ChumMixToggle.Toggle.group = this.ToggleGroup;
		}
		this.ToggleGroup.allowSwitchOff = true;
		this.BodyToggle.Toggle.isOn = false;
		if (this.ChumMixToggle != null)
		{
			this.ChumMixToggle.Toggle.isOn = false;
		}
		this.ActiveRod = this.RodView;
		if (!this._isOtherPlayer)
		{
			if (InitRods.Instance == null)
			{
				InitRods.Instance = this;
			}
			this._profile = this._profile ?? PhotonConnectionFactory.Instance.Profile;
			this.Subscribe();
			DropMeDoll[] componentsInChildren = this.BodyToggle.ContentToEnable.transform.GetComponentsInChildren<DropMeDoll>(true);
			if (componentsInChildren != null)
			{
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i] as DropMeDollChumHands == null)
					{
						componentsInChildren[i].VerifyRegisteredForFastEquip();
					}
				}
			}
			if (this.BodyView != null)
			{
				if (!this.BodyView.Subscribed)
				{
					this.BodyView.Subscribe();
				}
				this.BodyView.Setup(PhotonConnectionFactory.Instance.Profile, false);
			}
			this.Refresh(null);
		}
		else
		{
			this.ToggleGroup.allowSwitchOff = false;
		}
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		CircleNavigation component = base.GetComponent<CircleNavigation>();
		if (component != null)
		{
			component.SetSelectablesInteractable(type == InputModuleManager.InputType.Mouse);
		}
	}

	private void InitToggleLogic(RodToggle newToggle, bool isRod = true)
	{
		newToggle.Toggle.onValueChanged.AddListener(delegate(bool x)
		{
			if (newToggle.ContentToEnable.activeSelf != x)
			{
				newToggle.ContentToEnable.SetActive(x);
			}
			if (this.soundEffects != null)
			{
				this.soundEffects.OnSetToogle(x);
			}
			if (x)
			{
				this.bodyToggleCalledLast = false;
				if (isRod)
				{
					this.ActiveRod.Setup(newToggle.SlotId, this._profile);
					this.ChangedActiveRod();
					if (this.currentSlot != newToggle.SlotId)
					{
						this.currentSlot = newToggle.SlotId;
						if (!this._isOtherPlayer)
						{
							PhotonConnectionFactory.Instance.ChangeSelectedElement(GameElementType.RodSlot, "PD_Rod_" + newToggle.SlotId, null);
							this.SetToggleColor(newToggle);
						}
					}
				}
				else if (newToggle == this.BodyToggle)
				{
					if (!this._isOtherPlayer)
					{
						PhotonConnectionFactory.Instance.ChangeSelectedElement(GameElementType.RodSlot, "PD_Body", null);
					}
					this.bodyToggleCalledLast = true;
				}
				if (!this._isOtherPlayer)
				{
					this.RefreshInteractabilityForSlot(newToggle.SlotId, isRod);
				}
				this.OutfitOrRodOrChumMixingSwitched();
			}
		});
	}

	private RodToggle AddToggle(int slotId)
	{
		RodToggle rodToggle;
		if (slotId == 1)
		{
			rodToggle = this.FirstToggle;
		}
		else
		{
			if (this.FirstToggle.Toggle.isOn)
			{
				this.FirstToggle.Toggle.isOn = false;
			}
			bool flag = this._rodToggles != null && this._rodToggles.Count >= slotId;
			rodToggle = ((!flag) ? Object.Instantiate<RodToggle>(this.FirstToggle, this.TogglesParent) : this._rodToggles[slotId - 1]);
		}
		if (!this._rodToggles.Contains(rodToggle))
		{
			this._rodToggles.Add(rodToggle);
		}
		string text = "PD_Rod_" + slotId;
		rodToggle.name = "tglRod" + slotId;
		rodToggle.SlotId = slotId;
		rodToggle.Label.text = slotId.ToString();
		rodToggle.HintId.SetElementId(text, null, null);
		rodToggle.Toggle.group = this.ToggleGroup;
		rodToggle.ContentToEnable = this.RodView.gameObject;
		this.InitToggleLogic(rodToggle, true);
		return rodToggle;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (!this._isOtherPlayer)
		{
			InitRods.Instance = null;
			this.Unsubscribe();
			InitRods.DropMeComponents.Clear();
			if (this.BodyView != null && this.BodyView.Subscribed)
			{
				this.BodyView.Unsubscribe();
			}
		}
	}

	internal void Subscribe()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
		PhotonConnectionFactory.Instance.OnActiveQuiverTipSet += this.OnInventoryUpdated;
		PhotonConnectionFactory.Instance.OnSetActiveQuiverTipFailed += this.OnFailure;
		PhotonConnectionFactory.Instance.OnInventoryUpdateCancelled += this.Instance_OnInventoryUpdateCancelled;
		MenuPrefabsList menuPrefabsList = MenuHelpers.Instance.MenuPrefabsList;
		menuPrefabsList.HideCalled = (Action)Delegate.Combine(menuPrefabsList.HideCalled, new Action(this.HideCalled));
	}

	internal void Unsubscribe()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
		PhotonConnectionFactory.Instance.OnActiveQuiverTipSet -= this.OnInventoryUpdated;
		PhotonConnectionFactory.Instance.OnSetActiveQuiverTipFailed -= this.OnFailure;
		PhotonConnectionFactory.Instance.OnInventoryUpdateCancelled -= this.Instance_OnInventoryUpdateCancelled;
		MenuPrefabsList menuPrefabsList = MenuHelpers.Instance.MenuPrefabsList;
		menuPrefabsList.HideCalled = (Action)Delegate.Remove(menuPrefabsList.HideCalled, new Action(this.HideCalled));
	}

	private void HideCalled()
	{
		this._shouldResetRod = true;
	}

	private void OnInventoryUpdated()
	{
		if (base.ShouldUpdate())
		{
			this.SmartRefresh(false);
		}
	}

	private void OnFailure(Failure failure)
	{
		Debug.Log(failure.ErrorMessage);
		this.OnInventoryUpdated();
	}

	private void Instance_OnInventoryUpdateCancelled()
	{
		this.OnInventoryUpdated();
	}

	protected override void SetHelp()
	{
		this.SmartRefresh(this._shouldResetRod);
		if (this._shouldResetRod)
		{
			this._shouldResetRod = false;
		}
		if (this._isOtherPlayer)
		{
			this.OnInputTypeChanged(SettingsManager.InputType);
			InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		}
	}

	private void RefreshInteractabilityForSlot(int slot, bool isRod)
	{
		bool flag = !isRod || !RodHelper.IsInventorySlotOccupiedByRodStand(slot);
		this.RodInteractabilityCanvasGroup.interactable = flag;
		this.RodInteractabilityCanvasGroup.blocksRaycasts = flag;
		this.RodInteractabilityCanvasGroup.alpha = ((!flag) ? 0.1f : 1f);
		string text = string.Empty;
		if (!flag)
		{
			RodPodController rodPodByRodSlotId = RodHelper.GetRodPodByRodSlotId(slot);
			text = ((!(rodPodByRodSlotId != null) || rodPodByRodSlotId.Item == null) ? ScriptLocalization.Get("RodStandsCaption") : ((rodPodByRodSlotId.Item.ItemType != ItemTypes.Boat) ? InventoryHelper.GetFirstItemByItemId(rodPodByRodSlotId.ItemId).Name : rodPodByRodSlotId.Item.Name));
		}
		this.RodLocationDescription.text = ((!flag) ? string.Format(ScriptLocalization.Get("RodLocationLabel"), text) : string.Empty);
	}

	private void SmartRefresh(bool resetCurrent = false)
	{
		if (!this._isOtherPlayer)
		{
			object obj;
			if ((obj = ((!(GameFactory.Player != null)) ? null : GameFactory.Player.RequestedRod)) == null)
			{
				obj = this._profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Hands && x.ItemType == ItemTypes.Rod);
			}
			InventoryItem inventoryItem = obj;
			if (inventoryItem != null && this.currentSlot != 0 && resetCurrent)
			{
				this.currentSlot = inventoryItem.Slot;
			}
			if (this.currentSlot == 0 || (inventoryItem != null && inventoryItem != this.ActiveRod.Rod.InventoryItem) || this.maxRodsCount != RodHelper.GetSlotCount())
			{
				this.Refresh(null);
			}
			else
			{
				bool isOn = this.BodyToggle.Toggle.isOn;
				bool flag = this.ChumMixToggle != null && this.ChumMixToggle.Toggle.isOn;
				if (!isOn && !flag)
				{
					this.ActiveRod.Setup(this.currentSlot, this._profile);
					if (!this.BodyToggle.Toggle.isOn && !this.ChumMixToggle.Toggle.isOn)
					{
						this.RefreshInteractabilityForSlot(this.currentSlot, true);
					}
					RodToggle rodToggle = this._rodToggles.FirstOrDefault((RodToggle x) => x.SlotId == this.currentSlot);
					if (rodToggle != null)
					{
						this.SetToggleColor(rodToggle);
					}
				}
			}
		}
	}

	protected override void HideHelp()
	{
		if (this._isOtherPlayer)
		{
			InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		}
		if (!this._isOtherPlayer && PhotonConnectionFactory.Instance.IsConnectedToGameServer)
		{
			if (this.bodyToggleCalledLast && GameFactory.Player != null)
			{
				Chum chum = FeederHelper.FindPreparedChumOnDoll();
				Chum chum2 = FeederHelper.FindPreparedChumInHand();
				if (chum != null || chum2 != null)
				{
					if (chum2 == null)
					{
						PhotonConnectionFactory.Instance.MoveItemOrCombine(chum, null, StoragePlaces.Hands, true);
					}
					GameFactory.Player.IsHandThrowMode = true;
				}
			}
			else
			{
				Rod rodInSlot = PhotonConnectionFactory.Instance.Profile.Inventory.GetRodInSlot(this.currentSlot);
				if (rodInSlot == null || !RodHelper.IsRodEquipped(rodInSlot))
				{
					return;
				}
				if (GameFactory.Player != null && GameFactory.Player.State != null)
				{
					GameFactory.Player.TryToTakeRodFromSlot(this.currentSlot, false);
				}
				else
				{
					RodHelper.MoveRodToHands(rodInSlot, true, false);
				}
			}
		}
	}

	private void SetToggleColor(RodToggle toggle)
	{
		Rod rod = this._profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemType == ItemTypes.Rod && x.Slot == toggle.SlotId) as Rod;
		if (this._isOtherPlayer)
		{
			toggle.Label.color = this.DefaultColor;
			return;
		}
		bool flag = RodHelper.IsInventorySlotOccupiedByRodStand(toggle.SlotId);
		if (toggle.StatusIcon != null)
		{
			toggle.StatusIcon.gameObject.SetActive(flag);
		}
		if (toggle.StatusIcon != null && flag)
		{
			toggle.StatusIcon.text = this.OnRodPodStatus;
		}
		if (rod == null)
		{
			toggle.Label.color = this.DefaultColor;
		}
		else
		{
			RodTemplate rodTemplate = this._profile.Inventory.GetRodTemplate(rod);
			toggle.Label.color = ((rodTemplate != RodTemplate.UnEquiped) ? (flag ? this.UnavailableColor : this.EquipedColor) : this.UnEquipedColor);
		}
	}

	public void Refresh(Profile profile = null)
	{
		this._profile = profile ?? PhotonConnectionFactory.Instance.Profile;
		if (this._rodToggles == null)
		{
			this.Awake();
		}
		bool isOn = this.BodyToggle.Toggle.isOn;
		bool flag = this.ChumMixToggle != null && this.ChumMixToggle.Toggle.isOn;
		this.ToggleGroup.allowSwitchOff = true;
		int i;
		for (i = 0; i < this._rodToggles.Count; i++)
		{
			this._rodToggles[i].Toggle.isOn = false;
		}
		i = 0;
		this.maxRodsCount = this._profile.Inventory.RodSlotsCount;
		while (i < this.maxRodsCount)
		{
			int num = i + 1;
			RodToggle rodToggle;
			if (i < this._rodToggles.Count)
			{
				rodToggle = this._rodToggles[i];
			}
			else
			{
				rodToggle = this.AddToggle(num);
			}
			if (!rodToggle.gameObject.activeSelf)
			{
				rodToggle.gameObject.SetActive(true);
			}
			this.SetToggleColor(rodToggle);
			i++;
		}
		if (this.ChumMixToggle != null)
		{
			this.ChumMixToggle.transform.SetAsLastSibling();
		}
		while (i < this._rodToggles.Count)
		{
			this._rodToggles[i++].gameObject.SetActive(false);
		}
		if (this.currentSlot == 0)
		{
			InventoryItem inventoryItem = this._profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Hands && x.ItemType == ItemTypes.Rod);
			int num2 = ((inventoryItem == null) ? 1 : inventoryItem.Slot);
			this._rodToggles[num2 - 1].Toggle.isOn = true;
		}
		else if (isOn)
		{
			this.BodyToggle.Toggle.isOn = true;
		}
		else if (flag)
		{
			this.ChumMixToggle.Toggle.isOn = true;
		}
		else
		{
			this.currentSlot = Mathf.Clamp(this.currentSlot, 1, this.maxRodsCount);
			this._rodToggles[this.currentSlot - 1].Toggle.isOn = true;
		}
		this.ToggleGroup.allowSwitchOff = false;
		this.ChangedListener.OnTransformChildrenChanged();
	}

	public Transform TogglesParent;

	public ToggleGroup ToggleGroup;

	public RodToggle BodyToggle;

	public RodToggle FirstToggle;

	public RodToggle ChumMixToggle;

	public InitOutfit BodyView;

	public InitRod RodView;

	[SerializeField]
	private PlayButtonEffect soundEffects;

	[SerializeField]
	private ChildrenChangedListener ChangedListener;

	public Color EquipedColor;

	public Color UnEquipedColor;

	public Color UnavailableColor;

	public Color DefaultColor;

	public CanvasGroup RodInteractabilityCanvasGroup;

	public TextMeshProUGUI RodLocationDescription;

	private List<RodToggle> _rodToggles;

	[SerializeField]
	private bool _isOtherPlayer;

	private string OnRodPodStatus = "\ue70b";

	private Profile _profile;

	public static InitRods Instance;

	public static List<DropMeDoll> DropMeComponents = new List<DropMeDoll>();

	private int currentSlot;

	private bool bodyToggleCalledLast;

	private bool _shouldResetRod = true;

	private int maxRodsCount = 1;
}
