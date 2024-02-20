using System;
using ObjectModel;
using UnitsNet;
using UnityEngine;

public class DropMeDollRod : DropMeDoll
{
	protected override bool TransferItem(InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously)
	{
		Rod rod = (Rod)dragNDropContent;
		rod.LeaderLength = (float)Length.FromInches(20.0).Meters;
		Transform transform = base.gameObject.transform.Find("LeashLineLength");
		if (transform != null)
		{
			transform.GetComponent<LeashLineController>().SetValue((float)((int)MeasuringSystemManager.LineLeashLength(rod.LeaderLength)));
		}
		if (dragNDropContentPreviously != null)
		{
			if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanSubordinate(dragNDropContentPreviously, dragNDropContent))
			{
				GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
				return false;
			}
			this.SubordinateItem(dragNDropContentPreviously, dragNDropContent);
			base.GetComponent<RodBehavior>().Refresh();
			return true;
		}
		else
		{
			if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(dragNDropContent, null, StoragePlaces.Doll, true))
			{
				GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
				return false;
			}
			rod.Slot = base.GetComponent<RodBehavior>().CurrentSlot();
			this.MoveItemOrCombine(dragNDropContent, null, StoragePlaces.Doll, true);
			base.GetComponent<RodBehavior>().Clear();
			return true;
		}
	}
}
