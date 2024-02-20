using System;
using ObjectModel;
using UnityEngine;

public class LureBehavior : ChangeHandler
{
	protected override void Unequip()
	{
		if (base.InitRod.Bait.InventoryItem != null)
		{
			if (base.InitRod.Bait.InventoryItem.ParentItem != null && base.InitRod.Bait.InventoryItem.Storage == StoragePlaces.ParentItem && !PhotonConnectionFactory.Instance.ProfileWasRequested)
			{
				PhotonConnectionFactory.Instance.MoveItemOrCombine(base.InitRod.Bait.InventoryItem, null, base.ActiveStorages.storage, true);
			}
			base.InitRod.Bait.InventoryItem = null;
			base.InitRod.Bait.GetComponent<DragMeDoll>().Clear();
		}
	}

	public GameObject Hooks;

	public GameObject Tackle;
}
