using System;
using System.Linq;
using Assets.Scripts.UI._2D.Inventory;
using ObjectModel;

public class RodPodHelper
{
	public static bool AnyPodOnGround
	{
		get
		{
			return GameFactory.Player != null && GameFactory.Player.RodPods != null && GameFactory.Player.RodPods.Count > 0;
		}
	}

	public static RodStand FindPodOnDoll()
	{
		return PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem i) => i.Storage == StoragePlaces.Doll && i.ItemSubType == ItemSubTypes.RodStand) as RodStand;
	}

	public static RodStand FindPodInHands()
	{
		if (GameFactory.Player == null || GameFactory.Player.CurrentRodPod == null)
		{
			return null;
		}
		return InventoryHelper.GetFirstItemByItemId(GameFactory.Player.CurrentRodPod.ItemId) as RodStand;
	}

	public static int GetUnusedCount()
	{
		RodStand rodStand = RodPodHelper.FindPodOnDoll();
		if (rodStand != null)
		{
			int num;
			if (GameFactory.Player != null)
			{
				num = rodStand.StandCount - GameFactory.Player.RodPods.Count((RodPodController p) => p.CouldBeTaken);
			}
			else
			{
				num = rodStand.StandCount;
			}
			return num;
		}
		return 0;
	}

	public static RodPodController GetRodPodByPodSlotId(int slotId)
	{
		IOrderedEnumerable<RodPodController> orderedEnumerable = from p in GameFactory.Player.RodPods
			where (p.transform.position - GameFactory.Player.Position).magnitude <= 5f && p.IsValidSlot(slotId)
			orderby (p.transform.position - GameFactory.Player.Position).magnitude
			select p;
		return (orderedEnumerable.Count<RodPodController>() <= 0) ? null : orderedEnumerable.First<RodPodController>();
	}

	public static bool IsFreeAllRodStands
	{
		get
		{
			bool flag;
			if (!(GameFactory.Player == null))
			{
				flag = GameFactory.Player.RodPods.All((RodPodController p) => p.IsFree);
			}
			else
			{
				flag = true;
			}
			return flag;
		}
	}

	public static string RodStandIcon = "\ue70b";
}
