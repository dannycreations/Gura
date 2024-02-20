using System;
using ObjectModel;
using UnityEngine;

public class TackleBehavior : ChangeHandler
{
	protected override void Unequip()
	{
		if (base.InitRod.Tackle.InventoryItem != null)
		{
			if (base.InitRod.Tackle.InventoryItem.ParentItem != null && base.InitRod.Tackle.InventoryItem.Storage == StoragePlaces.ParentItem && !PhotonConnectionFactory.Instance.ProfileWasRequested)
			{
				PhotonConnectionFactory.Instance.MoveItemOrCombine(base.InitRod.Tackle.InventoryItem, null, base.ActiveStorages.storage, true);
			}
			base.InitRod.Tackle.InventoryItem = null;
			base.InitRod.Tackle.GetComponent<DragMeDoll>().Clear();
		}
	}

	public GameObject Hooks;

	public GameObject Lure;
}
