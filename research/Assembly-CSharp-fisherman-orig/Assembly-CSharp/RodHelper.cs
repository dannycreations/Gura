using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;

public class RodHelper
{
	public static bool IsRodEquipped(Rod rod)
	{
		return PhotonConnectionFactory.Instance.Profile.Inventory.GetRodTemplate(rod) != RodTemplate.UnEquiped;
	}

	public static bool WillBeEquippedWith(Rod rod, InventoryItem itemEquipping)
	{
		return PhotonConnectionFactory.Instance.Profile.Inventory.GetRodTemplateWith(rod, itemEquipping) != RodTemplate.UnEquiped;
	}

	public static Rod FindRodInHands()
	{
		return PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem i) => i.Storage == StoragePlaces.Hands && i.ItemType == ItemTypes.Rod) as Rod;
	}

	public static Rod FindRodInSlot(int slotId, Profile profile = null)
	{
		profile = profile ?? PhotonConnectionFactory.Instance.Profile;
		return profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemType == ItemTypes.Rod && x.Slot == slotId) as Rod;
	}

	public static Rod FindFirstEquippedRod()
	{
		Inventory inventory = PhotonConnectionFactory.Instance.Profile.Inventory;
		return inventory.FirstOrDefault((InventoryItem i) => i.ItemType == ItemTypes.Rod && inventory.GetRodTemplate((Rod)i) != RodTemplate.UnEquiped) as Rod;
	}

	public static int GetSlotCount()
	{
		RodCase rodCase = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemSubType == ItemSubTypes.RodCase) as RodCase;
		return (rodCase == null) ? 1 : (rodCase.RodWithReelCount + 1);
	}

	public static List<Rod> FindAllUsedRods()
	{
		return (from x in PhotonConnectionFactory.Instance.Profile.Inventory
			where x.ItemType == ItemTypes.Rod && (x.Storage == StoragePlaces.Doll || x.Storage == StoragePlaces.Hands)
			select x into i
			select i as Rod).ToList<Rod>();
	}

	public static IEnumerable<InventoryItem> GetEquippableRods()
	{
		Inventory inventory = PhotonConnectionFactory.Instance.Profile.Inventory;
		return inventory.Where((InventoryItem x) => x.ItemType == ItemTypes.Rod && (x.Storage == StoragePlaces.Hands || x.Storage == StoragePlaces.Doll) && inventory.GetRodTemplate((Rod)x) != RodTemplate.UnEquiped);
	}

	public static bool IsInventorySlotOccupiedByRodStand(int slotId)
	{
		return GameFactory.Player != null && (GameFactory.Player.RodPods.Any((RodPodController p) => p.FindSlotByRodSlot(slotId) != -1) || GameFactory.Player.SavedRodPods.Any((RodPodController p) => p.FindSlotByRodSlot(slotId) != -1));
	}

	public static bool HasAnyRonOnStand()
	{
		bool flag = false;
		for (int i = 0; i < RodHelper.GetSlotCount(); i++)
		{
			if (RodHelper.IsInventorySlotOccupiedByRodStand(i + 1))
			{
				flag = true;
				break;
			}
		}
		return flag;
	}

	public static RodPodController GetRodPodByRodSlotId(int slotId)
	{
		return GameFactory.Player.RodPods.FirstOrDefault((RodPodController p) => p.FindSlotByRodSlot(slotId) != -1);
	}

	public static void MoveRodToDoll(Rod rod)
	{
		PhotonConnectionFactory.Instance.MoveItemOrCombine(rod, null, StoragePlaces.Doll, true);
	}

	public static void MoveRodToHands(Rod rod, bool removeOld = true, bool fastChange = false)
	{
		if (rod != null)
		{
			if (fastChange)
			{
				PhotonConnectionFactory.Instance.OnInventoryMoved += RodHelper.Instance_OnInventoryMoved;
				PhotonConnectionFactory.Instance.OnInventoryMoveFailure += RodHelper.Instance_OnInventoryMoveFailure;
			}
			Rod rod2 = RodHelper.FindRodInHands();
			if (rod2 != null && rod2 != rod)
			{
				PhotonConnectionFactory.Instance.SwapRods(rod2, rod);
			}
			else
			{
				PhotonConnectionFactory.Instance.MoveItemOrCombine(rod, null, StoragePlaces.Hands, true);
			}
		}
	}

	public static float? GetTackleWeight(Rod rod, InventoryItem itemBeingEquipped = null)
	{
		TacklePhysicalParams tacklePhysicalParams = RodHelper.GetTacklePhysicalParams(rod, itemBeingEquipped);
		if (tacklePhysicalParams == null)
		{
			return null;
		}
		return new float?(tacklePhysicalParams.TackleMass);
	}

	public static TacklePhysicalParams GetTacklePhysicalParams(Rod rod, InventoryItem itemBeingEquipped = null)
	{
		if (rod == null)
		{
			throw new ArgumentException("Rod is null while getting tackle physical params");
		}
		RodTemplate rodTemplate;
		if (itemBeingEquipped == null)
		{
			rodTemplate = PhotonConnectionFactory.Instance.Profile.Inventory.RodTemplate.MatchedTemplate(rod);
			if (rodTemplate == RodTemplate.UnEquiped)
			{
				rodTemplate = PhotonConnectionFactory.Instance.Profile.Inventory.RodTemplate.MatchedTemplatePartial(rod);
			}
		}
		else
		{
			rodTemplate = PhotonConnectionFactory.Instance.Profile.Inventory.RodTemplate.MatchedTemplateWith(rod, itemBeingEquipped);
			if (rodTemplate == RodTemplate.UnEquiped)
			{
				rodTemplate = PhotonConnectionFactory.Instance.Profile.Inventory.RodTemplate.MatchedTemplatePartialWith(rod, itemBeingEquipped);
			}
		}
		if (rodTemplate == RodTemplate.UnEquiped)
		{
			return null;
		}
		List<InventoryItem> list = PhotonConnectionFactory.Instance.Profile.Inventory.Where(delegate(InventoryItem i)
		{
			Guid? parentItemInstanceId = i.ParentItemInstanceId;
			bool flag = parentItemInstanceId != null;
			Guid? instanceId = rod.InstanceId;
			return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault());
		}).ToList<InventoryItem>();
		if (itemBeingEquipped != null)
		{
			if (itemBeingEquipped is Chum)
			{
				Feeder feeder = list.FirstOrDefault((InventoryItem i) => i is Feeder) as Feeder;
				if (feeder != null)
				{
					InventoryItem inventoryItem = Inventory.CloneItemFull(itemBeingEquipped);
					InventoryItem inventoryItem2 = inventoryItem;
					double? weight = itemBeingEquipped.Weight;
					inventoryItem2.Weight = new double?((double)Math.Min((float)((weight == null) ? 0.0 : weight.Value), feeder.Capacity));
					list.Add(inventoryItem);
				}
			}
			else
			{
				InventoryItem inventoryItem3 = list.FirstOrDefault((InventoryItem x) => x.ItemType == itemBeingEquipped.ItemType);
				if (inventoryItem3 != null)
				{
					list.Remove(inventoryItem3);
				}
				list.Add(itemBeingEquipped);
			}
		}
		RodTackleProxy rodTackleProxy = new RodTackleProxy(rodTemplate, list);
		return new TacklePhysicalParams(rodTackleProxy);
	}

	private static void Instance_OnInventoryMoved()
	{
		PhotonConnectionFactory.Instance.OnInventoryMoved -= RodHelper.Instance_OnInventoryMoved;
		PhotonConnectionFactory.Instance.OnInventoryMoveFailure -= RodHelper.Instance_OnInventoryMoveFailure;
		Rod rod = RodHelper.FindRodInHands();
		PhotonConnectionFactory.Instance.ChangeIndicator(GameIndicatorType.FastRod, (rod == null) ? 0 : rod.ItemId);
	}

	private static void Instance_OnInventoryMoveFailure()
	{
		PhotonConnectionFactory.Instance.OnInventoryMoved -= RodHelper.Instance_OnInventoryMoved;
		PhotonConnectionFactory.Instance.OnInventoryMoveFailure -= RodHelper.Instance_OnInventoryMoveFailure;
	}
}
