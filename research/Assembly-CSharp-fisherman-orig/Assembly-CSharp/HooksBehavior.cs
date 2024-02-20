using System;
using ObjectModel;
using UnityEngine;

public class HooksBehavior : ChangeHandler
{
	protected override void Unequip()
	{
		if (base.InitRod.LureHook.InventoryItem != null)
		{
			if (base.InitRod.LureHook.InventoryItem.ParentItem != null && base.InitRod.LureHook.InventoryItem.Storage == StoragePlaces.ParentItem && !PhotonConnectionFactory.Instance.ProfileWasRequested)
			{
				PhotonConnectionFactory.Instance.MoveItemOrCombine(base.InitRod.LureHook.InventoryItem, null, base.ActiveStorages.storage, true);
			}
			base.InitRod.LureHook.InventoryItem = null;
			base.InitRod.LureHook.GetComponent<DragMeDoll>().Clear();
		}
	}

	public GameObject Tackle;

	public GameObject Lure;
}
