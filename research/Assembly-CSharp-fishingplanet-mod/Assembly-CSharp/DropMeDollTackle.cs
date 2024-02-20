using System;
using Assets.Scripts.UI._2D.Inventory;
using ObjectModel;
using ObjectModel.Common;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropMeDollTackle : DropMeDoll, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	protected override bool TransferItem(InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously)
	{
		if (!this.CanEquipNow(dragNDropContent))
		{
			return false;
		}
		if (this.rod.InventoryItem != null)
		{
			if (InventoryHelper.IsBlocked2Equip(this.rod.InventoryItem, dragNDropContent, false))
			{
				return false;
			}
			Reel reel = (Reel)base.GetComponent<ChangeHandler>().InitRod.Reel.InventoryItem;
			this.CheckWeightsAndSpawnHelpMessages(dragNDropContent);
			if (dragNDropContent.IsUnstockable && !dragNDropContent.IsStockableByAmount)
			{
				if (dragNDropContentPreviously != null)
				{
					if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanReplace(dragNDropContentPreviously, dragNDropContent))
					{
						GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
						return false;
					}
					PhotonConnectionFactory.Instance.ReplaceItem(dragNDropContentPreviously, dragNDropContent);
				}
				else
				{
					if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(dragNDropContent, this.rod.InventoryItem, StoragePlaces.ParentItem, true))
					{
						GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
						return false;
					}
					this.MoveItemOrCombine(dragNDropContent, this.rod.InventoryItem, StoragePlaces.ParentItem, true);
				}
			}
			else if (dragNDropContent is Line)
			{
				int capacityLineOnReel = DropMeDollTackle.GetCapacityLineOnReel(dragNDropContent, reel);
				if (!base.MakeSplit(this.rod.InventoryItem, dragNDropContent, dragNDropContentPreviously, (float)capacityLineOnReel))
				{
					return false;
				}
			}
			else if (dragNDropContent is Chum)
			{
				float chumCapacity = InitRods.Instance.ActiveRod.GetFeeder().ChumCapacity;
				if (!base.MakeSplit(this.rod.InventoryItem, dragNDropContent, dragNDropContentPreviously, chumCapacity))
				{
					return false;
				}
			}
			else if (!base.MakeSplit(this.rod.InventoryItem, dragNDropContent, dragNDropContentPreviously, 1))
			{
				return false;
			}
			base.GetComponent<ChangeHandler>().Refresh();
		}
		return true;
	}

	protected bool CheckBobberFloating(InventoryItem itemEquipped)
	{
		InitRod initRod = base.GetComponent<ChangeHandler>().InitRod;
		if (initRod.Tackle == null)
		{
			return true;
		}
		Bobber bobber;
		if (itemEquipped is Bobber)
		{
			bobber = itemEquipped as Bobber;
		}
		else
		{
			bobber = initRod.Tackle.InventoryItem as Bobber;
		}
		Bait bait;
		if (itemEquipped is Bait)
		{
			bait = itemEquipped as Bait;
		}
		else
		{
			bait = initRod.Bait.InventoryItem as Bait;
		}
		return bobber == null || bobber.Weight == null || bait == null || bait.Weight == null || BobberBuoyancyCalculator.IsBobberFloating(bobber.Buoyancy, (float)bobber.Weight.Value, bobber.SinkerMass, (float)bait.Weight.Value);
	}

	protected void CheckWeightsAndSpawnHelpMessages(InventoryItem itemBeingEquipped)
	{
		Rod rod = (Rod)this.rod.InventoryItem;
		Reel reel = (Reel)base.GetComponent<ChangeHandler>().InitRod.Reel.InventoryItem;
		Line line = (Line)base.GetComponent<ChangeHandler>().InitRod.Line.InventoryItem;
		float? tackleWeight = RodHelper.GetTackleWeight(rod, itemBeingEquipped);
		if (tackleWeight == null)
		{
			Debug.Log("Current tackle weight is null, skipping weight checks!");
			return;
		}
		Debug.Log("Current tackle weight is: " + tackleWeight);
		bool flag = RodHelper.WillBeEquippedWith(rod, itemBeingEquipped);
		bool flag2 = itemBeingEquipped is JigHead || itemBeingEquipped is Lure || itemBeingEquipped is JigBait || itemBeingEquipped is Feeder || itemBeingEquipped is Sinker || rod is FeederRod || rod is BottomRod;
		if (flag && flag2 && tackleWeight >= rod.CastWeightMin && tackleWeight <= rod.CastWeightMax && tackleWeight >= reel.CastWeightMin && !RodCaster.CanBreakLine(tackleWeight.Value, line.MaxLoad))
		{
			GameFactory.Message.ShowTackleWeightOptimal(base.transform.root.gameObject);
		}
		else if (line == null)
		{
			GameFactory.Message.ShowNoFishingLine(base.transform.root.gameObject);
		}
		else if (RodCaster.CanBreakLine(tackleWeight.Value, line.MaxLoad))
		{
			GameFactory.Message.ShowTackleHavyForLine(base.transform.root.gameObject);
		}
		else if (flag2 && tackleWeight < rod.CastWeightMin)
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
		else if (flag2 && tackleWeight < reel.CastWeightMin)
		{
			GameFactory.Message.ShowTackleLightweightForReel(base.transform.root.gameObject);
		}
		else if ((itemBeingEquipped is Bobber || itemBeingEquipped is Bait) && !this.CheckBobberFloating(itemBeingEquipped))
		{
			GameFactory.Message.ShowTackleHavyForBobber(base.transform.root.gameObject);
		}
	}

	protected static int GetCapacityLineOnReel(InventoryItem dragNDropContent, InventoryItem reel)
	{
		int num = 1;
		if (reel != null)
		{
			Line line = (Line)dragNDropContent;
			Reel reel2 = (Reel)reel;
			num = (int)((double)reel2.LineCapacity * (0.1 / (double)line.Thickness));
			double? length = line.Length;
			if ((double)num > length)
			{
				num = (int)line.Length.Value;
			}
			if (Math.Abs(line.Thickness) < 0.001f)
			{
				num = 1;
			}
		}
		return num;
	}

	protected override void Highlight()
	{
		if (this.rod.InventoryItem != null)
		{
			base.Highlight();
		}
	}

	public override bool CanEquipNow(InventoryItem itemToEquip)
	{
		DropMeDollLine component = base.GetComponent<ChangeHandler>().InitRod.Line.GetComponent<DropMeDollLine>();
		if (!component.CanEquipNow(itemToEquip))
		{
			return false;
		}
		if ((Line)base.GetComponent<ChangeHandler>().InitRod.Line.InventoryItem == null)
		{
			GameFactory.Message.ShowLineMustBeSetup(base.transform.root.gameObject);
			return false;
		}
		return true;
	}

	public InventoryItemDollComponent rod;
}
