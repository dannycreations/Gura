using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Inventory;
using DG.Tweening;
using I2.Loc;
using ObjectModel;
using ObjectModel.Common;
using UnityEngine;

public class EquipmentChangeHandler : MonoBehaviour
{
	public bool IsBusy
	{
		get
		{
			return this._lureOpenned || this._rodOpenned;
		}
	}

	public void Init(EquipmentInGamePanel baitPanel, EquipmentInGamePanel rodPanel, CanvasGroup[] canvases)
	{
		this._baitPanel = baitPanel;
		this._rodPanel = rodPanel;
		this._canvases = canvases;
		this._previousValues = new float[this._canvases.Length];
		this._tweenSequence = new Tweener[this._canvases.Length];
		this._isActive = true;
	}

	private void OnEnable()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.Instance_OnInventoryUpdated;
	}

	private void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.Instance_OnInventoryUpdated;
		if (this._rodOpenned || this._lureOpenned)
		{
			this._rodOpenned = (this._lureOpenned = false);
			this._rodPanel.HidePanel();
			this._baitPanel.HidePanel();
			for (int i = 0; i < this._canvases.Length; i++)
			{
				if (this._tweenSequence[i] != null)
				{
					TweenExtensions.Kill(this._tweenSequence[i], false);
				}
				this._canvases[i].alpha = this._previousValues[i];
			}
		}
	}

	private void Instance_OnInventoryUpdated()
	{
		if (this.baitID == -1)
		{
			return;
		}
		if (GameFactory.Player.IsHandThrowMode)
		{
			this.baitID = -1;
			return;
		}
		InventoryItem inventoryItem = PhotonConnectionFactory.Instance.Profile.Inventory.First((InventoryItem x) => x.Storage == StoragePlaces.Hands && x.ItemType == ItemTypes.Rod);
		StaticUserData.RodInHand.RefreshRod(inventoryItem.InstanceId);
		if (this.baitID != -1)
		{
			InventoryItem curBait = this.CurBait;
			int num = ((curBait == null) ? (-1) : curBait.ItemId);
			if (StaticUserData.RodInHand.HasAssembledRod && this.baitID == num)
			{
				this.baitID = -1;
				GameFactory.Player.ReplaceBaitInCurrentRod();
				return;
			}
			num = ((StaticUserData.RodInHand.Chum == null) ? (-1) : StaticUserData.RodInHand.Chum.ItemId);
			if (StaticUserData.RodInHand.HasAssembledRod && this.baitID == num)
			{
				this.baitID = -1;
			}
		}
	}

	private InventoryItem CurBait
	{
		get
		{
			return (!GameFactory.Player.IsHandThrowMode) ? ((StaticUserData.RodInHand.RodTemplate != RodTemplate.Lure) ? StaticUserData.RodInHand.Bait : StaticUserData.RodInHand.Hook) : FeederHelper.FindPreparedChumInHand();
		}
	}

	private InventoryItem CurChum
	{
		get
		{
			return StaticUserData.RodInHand.Chum;
		}
	}

	private void Update()
	{
		if (!this._isActive)
		{
			return;
		}
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			return;
		}
		bool flag = !GameFactory.Player.IsSailing || (GameFactory.Player.IsSailing && GameFactory.Player.IsBoatFishing);
		if (GameFactory.Player.IsReadyForRod && flag && ControlsController.ControlsActions.LurePanel.WasPressed && this.baitID == -1 && !this._rodOpenned && !ShowHudElements.Instance.IsCatchedWindowActive && (GameFactory.Player.State == typeof(PlayerIdle) || GameFactory.Player.State == typeof(PlayerIdlePitch) || GameFactory.Player.State == typeof(HandIdle)))
		{
			this._ready = true;
			InventoryItem bait = this.CurBait;
			ItemTypes baitType = bait.ItemType;
			int baitId = bait.ItemId;
			InventoryItem[] items = PhotonConnectionFactory.Instance.Profile.Inventory.Items;
			InventoryItem inventoryItem = items.FirstOrDefault((InventoryItem item) => item.Storage == StoragePlaces.Equipment && item.ItemType == baitType && item.ItemId == baitId);
			if (GameFactory.Player.IsHandThrowMode)
			{
				this._fittingBaits = items.Where((InventoryItem item) => item.Storage == StoragePlaces.Equipment && item.ItemType == ItemTypes.Chum && item.Amount >= Inventory.ChumHandCapacity && !(item as Chum).IsExpired && item.IsHidden != true && !InventoryHelper.IsBlocked2EquipChumHands(item as Chum)).ToList<InventoryItem>();
			}
			else
			{
				Sinker sinker = StaticUserData.RodInHand.Sinker;
				List<InventoryItem> list;
				if (inventoryItem == null)
				{
					list = (from item in items
						where (item.Storage == StoragePlaces.Equipment && item.ItemType == baitType && !InventoryHelper.IsBlocked2Equip(StaticUserData.RodInHand.Rod, item, false)) || item == bait
						select item into b
						orderby b.ItemSubType
						select b).ThenBy((InventoryItem x) => x.Name).ToList<InventoryItem>();
				}
				else
				{
					list = (from item in items
						where item.Storage == StoragePlaces.Equipment && item.ItemType == baitType && !InventoryHelper.IsBlocked2Equip(StaticUserData.RodInHand.Rod, item, false)
						select item into b
						orderby b.ItemSubType
						select b).ThenBy((InventoryItem x) => x.Name).ToList<InventoryItem>();
				}
				this._fittingBaits = list;
				if (sinker != null)
				{
					this._fittingBaits.AddRange((from item in items
						where item.Storage == StoragePlaces.Equipment && item.ItemType == ItemTypes.Chum && !(item as Chum).IsExpired
						select item into b
						orderby b.ItemSubType
						select b).ThenBy((InventoryItem x) => x.Name).ToList<InventoryItem>());
				}
				this._fittingBaits = this._fittingBaits.Where((InventoryItem p) => !(p is Chum) && !(p is ChumIngredient)).ToList<InventoryItem>();
			}
			List<SlotContent> list2 = new List<SlotContent>(this._fittingBaits.Count);
			int num = 0;
			for (int j = 0; j < this._fittingBaits.Count; j++)
			{
				InventoryItem inventoryItem2 = this._fittingBaits[j];
				int num2 = inventoryItem2.Count;
				string text = num2.ToString();
				if (baitType == ItemTypes.Chum)
				{
					Guid? instanceId = inventoryItem2.InstanceId;
					bool flag2 = instanceId != null;
					Guid? instanceId2 = bait.InstanceId;
					if (flag2 == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault()))
					{
						goto IL_40B;
					}
					Guid? instanceId3 = inventoryItem2.InstanceId;
					bool flag3 = instanceId3 != null;
					Guid? splitFromInstanceId = (bait as Chum).SplitFromInstanceId;
					if (flag3 == (splitFromInstanceId != null) && (instanceId3 == null || instanceId3.GetValueOrDefault() == splitFromInstanceId.GetValueOrDefault()))
					{
						goto IL_40B;
					}
					IL_40F:
					text = Math.Round((double)MeasuringSystemManager.FishWeight(inventoryItem2.Amount), 3, MidpointRounding.AwayFromZero).ToString();
					goto IL_45B;
					IL_40B:
					num = j;
					goto IL_40F;
				}
				if (inventoryItem2.ItemId == baitId)
				{
					num = j;
					if (inventoryItem != null)
					{
						num2++;
					}
				}
				IL_45B:
				List<SlotContent> list3 = list2;
				SlotContent slotContent = new SlotContent();
				slotContent.Count = text;
				slotContent.Name = inventoryItem2.Name;
				slotContent.ThumbnailId = inventoryItem2.ThumbnailBID;
				slotContent.Available = true;
				slotContent.Value = inventoryItem2.ItemId;
				SlotContent slotContent2 = slotContent;
				Guid? instanceId4 = inventoryItem2.InstanceId;
				slotContent2.InstanceValue = ((instanceId4 == null) ? Guid.Empty : instanceId4.Value);
				list3.Add(slotContent);
			}
			ControlsActions controlsActions = ControlsController.ControlsActions;
			this._baitPanel.InitPanel(list2, num, controlsActions.UpLure, controlsActions.DownLure, controlsActions.UpLureAdditional, controlsActions.DownLureAdditional, this.deltaSizeLure, this.posLure);
			this.BlockFPS(true);
			this._lureOpenned = true;
		}
		if ((!GameFactory.Player.IsReadyForRod || ControlsController.ControlsActions.LurePanel.WasReleased) && this._ready && this._lureOpenned)
		{
			if (GameFactory.Player.IsHandThrowMode)
			{
				Guid selectedSlotInstanceValue = this._baitPanel.GetSelectedSlotInstanceValue();
				if (selectedSlotInstanceValue != this.CurBait.InstanceId && selectedSlotInstanceValue != (this.CurBait as Chum).SplitFromInstanceId && (this.CurChum == null || selectedSlotInstanceValue != this.CurChum.InstanceId))
				{
					this.SetBaitOrChum(selectedSlotInstanceValue);
				}
				this._baitPanel.HidePanel();
				this.BlockFPS(false);
				this._lureOpenned = false;
			}
			else
			{
				int selectedSlotValue = this._baitPanel.GetSelectedSlotValue();
				if (selectedSlotValue != this.CurBait.ItemId && (this.CurChum == null || selectedSlotValue != this.CurChum.ItemId))
				{
					this.SetBaitOrChum(selectedSlotValue);
				}
				this._baitPanel.HidePanel();
				this.BlockFPS(false);
				this._lureOpenned = false;
			}
		}
		if (GameFactory.Player.IsReadyForRod && flag && ControlsController.ControlsActions.RodPanel.WasPressed && this.baitID == -1 && !this._lureOpenned && !ShowHudElements.Instance.IsCatchedWindowActive)
		{
			List<InventoryItem> list4 = (from i in RodHelper.GetEquippableRods()
				orderby i.Slot
				select i).ToList<InventoryItem>();
			if (!GameFactory.Player.IsBoatFishing)
			{
				list4.AddRange((from item in PhotonConnectionFactory.Instance.Profile.Inventory.Items
					where item.Storage == StoragePlaces.Doll && item.ItemSubType == ItemSubTypes.RodStand
					select item into x
					orderby x.Name
					select x).ToList<InventoryItem>());
				list4.AddRange((from item in PhotonConnectionFactory.Instance.Profile.Inventory.Items
					where item.Storage == StoragePlaces.Equipment && item.ItemSubType == ItemSubTypes.Firework
					select item into b
					orderby b.ItemSubType
					select b).ThenBy((InventoryItem x) => x.Name).ToList<InventoryItem>());
			}
			if (list4.Count == 0)
			{
				return;
			}
			this._ready = true;
			this._rodsAndTools = list4;
			List<SlotContent> list5 = this._rodsAndTools.Select((InventoryItem item) => new SlotContent
			{
				Count = ((item.ItemSubType != ItemSubTypes.Firework) ? ((item.ItemSubType != ItemSubTypes.RodStand) ? ((!RodHelper.IsInventorySlotOccupiedByRodStand(item.Slot)) ? item.Slot.ToString() : RodPodHelper.RodStandIcon) : "9") : item.Count.ToString()),
				Name = item.Name,
				ThumbnailId = item.ThumbnailBID,
				Available = ((!(item is Rod) || !RodHelper.IsInventorySlotOccupiedByRodStand(item.Slot)) && (item.ItemSubType != ItemSubTypes.RodStand || RodPodHelper.GetUnusedCount() != 0)),
				Value = ((item.ItemSubType != ItemSubTypes.Firework && item.ItemSubType != ItemSubTypes.RodStand) ? item.Slot : item.ItemId)
			}).ToList<SlotContent>();
			Chum chum = FeederHelper.FindPreparedChumOnDoll() ?? FeederHelper.FindPreparedChumInHand();
			if (chum != null)
			{
				list5.Add(new SlotContent
				{
					Count = "8",
					Available = !chum.IsExpired,
					Name = ScriptLocalization.Get("HandCastCaption") + "<size=36>\ue795</size>",
					ThumbnailId = chum.ThumbnailBID,
					Value = 8
				});
			}
			list5.Add(new SlotContent
			{
				Count = "0",
				Available = true,
				Name = ScriptLocalization.Get("EmptyHandsCaption") + "<size=36>\ue795</size>",
				ThumbnailId = null,
				Value = 0
			});
			int num3 = 0;
			RodStand rodStand = RodPodHelper.FindPodInHands();
			if (GameFactory.Player.IsEmptyHandsMode)
			{
				num3 = Mathf.Max(0, list5.Count - 1);
			}
			else if (rodStand != null)
			{
				num3 = list4.IndexOf(rodStand);
			}
			else if (GameFactory.Player.CurFirework != null)
			{
				num3 = list4.IndexOf(GameFactory.Player.CurFirework.Item);
			}
			else if (GameFactory.Player.IsHandThrowMode)
			{
				num3 = Mathf.Max(0, list5.Count - 2);
			}
			else if (StaticUserData.RodInHand.Rod != null)
			{
				num3 = list4.IndexOf(StaticUserData.RodInHand.Rod);
			}
			ControlsActions controlsActions2 = ControlsController.ControlsActions;
			this._rodPanel.InitPanel(list5, num3, controlsActions2.UpRod, controlsActions2.DownRod, controlsActions2.UpRodAdditional, controlsActions2.DownRodAdditional, this.deltaSizeRod, this.posRod);
			this.BlockFPS(true);
			this._rodOpenned = true;
		}
		if ((!GameFactory.Player.IsReadyForRod || ControlsController.ControlsActions.RodPanel.WasReleased) && this._ready && this._rodOpenned)
		{
			this._rodOpenned = false;
			GameFactory.Player.IsDrawRodPodRequest = false;
			this.BlockFPS(false);
			int selectedSlotValue2 = this._rodPanel.GetSelectedSlotValue();
			this.SetRod(selectedSlotValue2);
			this._rodPanel.HidePanel();
		}
		if (this._baitPanel.IsActive)
		{
			this._baitPanel.UpdatePanel();
		}
		if (this._rodPanel.IsActive)
		{
			this._rodPanel.UpdatePanel();
		}
	}

	private void BlockFPS(bool block)
	{
		if (block)
		{
			ControlsController.ControlsActions.BlockAxis();
			ControlsController.ControlsActions.BlockMouseButtons(true, true, true, true);
			for (int i = 0; i < this._canvases.Length; i++)
			{
				if (this._tweenSequence[i] == null)
				{
					this._previousValues[i] = this._canvases[i].alpha;
				}
				if (this._tweenSequence[i] != null)
				{
					TweenExtensions.Kill(this._tweenSequence[i], false);
				}
				this._tweenSequence[i] = ShortcutExtensions.DOFade(this._canvases[i], 0f, this._duration);
			}
		}
		else
		{
			ControlsController.ControlsActions.UnBlockAxis();
			ControlsController.ControlsActions.UnBlockInput();
			for (int j = 0; j < this._canvases.Length; j++)
			{
				if (this._tweenSequence[j] != null)
				{
					TweenExtensions.Kill(this._tweenSequence[j], false);
				}
				this._tweenSequence[j] = ShortcutExtensions.DOFade(this._canvases[j], this._previousValues[j], this._duration);
			}
		}
	}

	private void SetRod(int slotID)
	{
		if (slotID == 8)
		{
			Chum chum = FeederHelper.FindPreparedChumOnDoll();
			Chum chum2 = FeederHelper.FindPreparedChumInHand();
			if (chum != null || chum2 != null)
			{
				if (!GameFactory.Player.IsHandThrowMode)
				{
					GameFactory.Player.IsHandThrowMode = true;
				}
			}
			else
			{
				GameFactory.Player.IsHandThrowMode = false;
			}
			return;
		}
		if (slotID == 0)
		{
			GameFactory.Player.IsEmptyHandsMode = true;
			return;
		}
		InventoryItem inventoryItem = this._rodsAndTools.FirstOrDefault((InventoryItem item) => item.ItemId == slotID);
		if (inventoryItem != null)
		{
			if (inventoryItem.ItemSubType == ItemSubTypes.RodStand)
			{
				GameFactory.Player.IsDrawRodPodRequest = true;
			}
			else if (GameFactory.Player.CurFirework == null || GameFactory.Player.CurFirework.Item.ItemId != inventoryItem.ItemId)
			{
				GameFactory.Player.ReplaceFireWork(inventoryItem);
			}
		}
		else if (GameFactory.Player.IsEmptyHandsMode || GameFactory.Player.State == typeof(PlayerEmpty) || GameFactory.Player.CurFirework != null || GameFactory.Player.IsHandThrowMode || GameFactory.Player.IsWithRodPodMode || StaticUserData.RodInHand.Rod == null || StaticUserData.RodInHand.Rod.Slot != slotID || (GameFactory.Player.IsSailing && !GameFactory.Player.IsBoatFishing))
		{
			GameFactory.Player.TryToTakeRodFromSlot(slotID, false);
		}
	}

	private void SetBaitOrChum(int slotID)
	{
		if (this._fittingBaits == null)
		{
			return;
		}
		InventoryItem inventoryItem = this._fittingBaits.FirstOrDefault((InventoryItem item) => item.ItemId == slotID);
		if (inventoryItem == null)
		{
			return;
		}
		InventoryItem inventoryItem2 = ((StaticUserData.RodInHand.Bait == null) ? StaticUserData.RodInHand.Hook : StaticUserData.RodInHand.Bait);
		Rod rod = PhotonConnectionFactory.Instance.Profile.Inventory.First((InventoryItem x) => x.Storage == StoragePlaces.Hands && x.ItemType == ItemTypes.Rod) as Rod;
		Reel reel = StaticUserData.RodInHand.Reel;
		Line line = StaticUserData.RodInHand.Line;
		float? tackleWeight = RodHelper.GetTackleWeight(rod, inventoryItem);
		if (tackleWeight == null)
		{
			Debug.Log("Current tackle weight is null, skipping weight checks!");
			return;
		}
		Debug.Log("Current tackle weight is: " + tackleWeight);
		if (inventoryItem is JigHead || inventoryItem is Lure || inventoryItem is JigBait)
		{
			if (tackleWeight >= rod.CastWeightMin && tackleWeight <= rod.CastWeightMax && tackleWeight >= reel.CastWeightMin && !RodCaster.CanBreakLine(tackleWeight.Value, line.MaxLoad))
			{
				GameFactory.Message.ShowTackleWeightOptimal(base.transform.root.gameObject);
			}
			else if (RodCaster.CanBreakLine(tackleWeight.Value, line.MaxLoad))
			{
				GameFactory.Message.ShowTackleHavyForLine(base.transform.root.gameObject);
			}
			else if (tackleWeight < rod.CastWeightMin)
			{
				GameFactory.Message.ShowTackleLightweightForRods(base.transform.root.gameObject);
			}
			else if (RodCaster.CanBreakRod(tackleWeight.Value, rod.CastWeightMax))
			{
				GameFactory.Message.ShowTackleCanBreakRod(base.transform.root.gameObject);
			}
			else if (tackleWeight > rod.CastWeightMax)
			{
				GameFactory.Message.ShowTackleHavyForRods(base.transform.root.gameObject);
			}
			else if (tackleWeight < reel.CastWeightMin)
			{
				GameFactory.Message.ShowTackleLightweightForReel(base.transform.root.gameObject);
			}
		}
		else if (inventoryItem is Bait)
		{
			if (line != null && RodCaster.CanBreakLine(tackleWeight.Value, line.MaxLoad))
			{
				GameFactory.Message.ShowTackleHavyForLine(base.transform.root.gameObject);
			}
			else if (inventoryItem is Bait && !this.CheckBobberFloating(inventoryItem))
			{
				GameFactory.Message.ShowTackleHavyForBobber(base.transform.root.gameObject);
			}
		}
		else if (inventoryItem is Chum)
		{
			PhotonConnectionFactory.Instance.OnInventoryMoved += this.Instance_OnInventoryMoved;
			PhotonConnectionFactory.Instance.OnInventoryMoveFailure += this.Instance_OnInventoryMoveFailure;
			StaticUserData.RodInHand.RefreshRod(rod.InstanceId);
			inventoryItem2 = StaticUserData.RodInHand.Chum;
			this.MakeSplit(rod, inventoryItem, inventoryItem2, 0f);
			this.baitID = inventoryItem.ItemId;
			return;
		}
		if (inventoryItem.IsUnstockable)
		{
			if (inventoryItem2 != null)
			{
				if (PhotonConnectionFactory.Instance.Profile.Inventory.CanReplace(inventoryItem2, inventoryItem))
				{
					PhotonConnectionFactory.Instance.OnInventoryMoved += this.Instance_OnInventoryMoved;
					PhotonConnectionFactory.Instance.OnInventoryMoveFailure += this.Instance_OnInventoryMoveFailure;
					PhotonConnectionFactory.Instance.ReplaceItem(inventoryItem2, inventoryItem);
				}
				else
				{
					GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
				}
			}
			else if (PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(inventoryItem, rod, StoragePlaces.ParentItem, true))
			{
				PhotonConnectionFactory.Instance.OnInventoryMoved += this.Instance_OnInventoryMoved;
				PhotonConnectionFactory.Instance.OnInventoryMoveFailure += this.Instance_OnInventoryMoveFailure;
				PhotonConnectionFactory.Instance.MoveItemOrCombine(inventoryItem, rod, StoragePlaces.ParentItem, true);
			}
			else
			{
				GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
			}
		}
		else
		{
			PhotonConnectionFactory.Instance.OnInventoryMoved += this.Instance_OnInventoryMoved;
			PhotonConnectionFactory.Instance.OnInventoryMoveFailure += this.Instance_OnInventoryMoveFailure;
			this.MakeSplit(rod, inventoryItem, inventoryItem2, 1);
		}
		this.baitID = inventoryItem.ItemId;
	}

	private void SetBaitOrChum(Guid slotID)
	{
		if (this._fittingBaits == null)
		{
			return;
		}
		InventoryItem inventoryItem = this._fittingBaits.FirstOrDefault((InventoryItem item) => item.InstanceId == slotID);
		if (inventoryItem == null)
		{
			return;
		}
		if (inventoryItem is Chum && GameFactory.Player.IsHandThrowMode)
		{
			Chum chum = FeederHelper.FindPreparedChumOnDoll();
			Chum chum2 = FeederHelper.FindPreparedChumInHand();
			this.baitID = inventoryItem.ItemId;
			this.MakeSplit(null, inventoryItem, chum2 ?? chum, Inventory.ChumHandCapacity);
		}
	}

	private void Instance_OnInventoryMoved()
	{
		PhotonConnectionFactory.Instance.OnInventoryMoved -= this.Instance_OnInventoryMoved;
		PhotonConnectionFactory.Instance.OnInventoryMoveFailure -= this.Instance_OnInventoryMoveFailure;
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			return;
		}
		if (GameFactory.Player.IsHandThrowMode)
		{
			this.baitID = -1;
		}
		PhotonConnectionFactory.Instance.ChangeIndicator(GameIndicatorType.FastBait, this.baitID);
	}

	private void Instance_OnInventoryMoveFailure()
	{
		if (GameFactory.Player.IsHandThrowMode)
		{
			this.baitID = -1;
		}
		PhotonConnectionFactory.Instance.OnInventoryMoved -= this.Instance_OnInventoryMoved;
		PhotonConnectionFactory.Instance.OnInventoryMoveFailure -= this.Instance_OnInventoryMoveFailure;
	}

	protected bool CheckBobberFloating(InventoryItem itemEquipped)
	{
		Bobber bobber = StaticUserData.RodInHand.Bobber;
		Bait bait = itemEquipped as Bait;
		return bobber == null || bobber.Weight == null || bait == null || bait.Weight == null || BobberBuoyancyCalculator.IsBobberFloating(bobber.Buoyancy, (float)bobber.Weight.Value, bobber.SinkerMass, (float)bait.Weight.Value);
	}

	protected bool MakeSplit(InventoryItem parent, InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously, int count)
	{
		if ((dragNDropContentPreviously == null || PhotonConnectionFactory.Instance.Profile.Inventory.CanMoveOrCombineItem(dragNDropContentPreviously, null, dragNDropContent.Storage)) && PhotonConnectionFactory.Instance.Profile.Inventory.CanSplit(dragNDropContent, count) && PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(dragNDropContent, parent, StoragePlaces.ParentItem, false))
		{
			if (dragNDropContentPreviously != null)
			{
				PhotonConnectionFactory.Instance.SplitItemAndReplace(dragNDropContentPreviously, dragNDropContent, count);
			}
			else
			{
				PhotonConnectionFactory.Instance.SplitItem(dragNDropContent, parent, count, StoragePlaces.ParentItem);
			}
			return true;
		}
		GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
		return false;
	}

	protected bool MakeSplit(InventoryItem parent, InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously, float amount)
	{
		if (dragNDropContentPreviously == null && PhotonConnectionFactory.Instance.Profile.Inventory.CanSplit(dragNDropContent, amount) && PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(dragNDropContent, parent, (parent != null) ? StoragePlaces.ParentItem : StoragePlaces.Doll, false))
		{
			PhotonConnectionFactory.Instance.OnInventoryMoved += this.Instance_OnInventoryMoved;
			PhotonConnectionFactory.Instance.OnInventoryMoveFailure += this.Instance_OnInventoryMoveFailure;
			PhotonConnectionFactory.Instance.SplitItem(dragNDropContent, parent, amount, (parent != null) ? StoragePlaces.ParentItem : StoragePlaces.Doll);
			return true;
		}
		if (dragNDropContentPreviously != null && PhotonConnectionFactory.Instance.Profile.Inventory.CanSplit(dragNDropContent, amount) && PhotonConnectionFactory.Instance.Profile.Inventory.CanReplace(dragNDropContentPreviously, dragNDropContent))
		{
			PhotonConnectionFactory.Instance.OnInventoryMoved += this.Instance_OnInventoryMoved;
			PhotonConnectionFactory.Instance.OnInventoryMoveFailure += this.Instance_OnInventoryMoveFailure;
			PhotonConnectionFactory.Instance.SplitItemAndReplace(dragNDropContentPreviously, dragNDropContent, amount);
			return true;
		}
		GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
		return false;
	}

	private bool _isActive;

	private EquipmentInGamePanel _baitPanel;

	private EquipmentInGamePanel _rodPanel;

	private CanvasGroup[] _canvases;

	private float[] _previousValues;

	private Tweener[] _tweenSequence;

	private List<InventoryItem> _fittingBaits;

	private List<InventoryItem> _rodsAndTools;

	private CanvasGroup[] canvases;

	private float _duration = 0.175f;

	private int baitID = -1;

	private Guid baitInstanceID = Guid.Empty;

	private bool _lureOpenned;

	private bool _ready;

	private bool _rodOpenned;

	private Vector2 deltaSizeRod = new Vector2(75f, 75f);

	private Vector2 deltaSizeLure = new Vector2(85f, 85f);

	private Vector2 posRod = new Vector2(-17.3f, -17.4f);

	private Vector2 posLure = new Vector2(-12.8f, -13.1f);
}
