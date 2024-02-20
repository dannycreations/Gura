using System;
using ObjectModel;

public class ReelBehavior : ChangeHandler
{
	protected override void Unequip()
	{
		if (base.InitRod.Reel.InventoryItem != null)
		{
			if (base.InitRod.Reel.InventoryItem.ParentItem != null && base.InitRod.Reel.InventoryItem.Storage == StoragePlaces.ParentItem && !PhotonConnectionFactory.Instance.ProfileWasRequested)
			{
				PhotonConnectionFactory.Instance.MoveItemOrCombine(base.InitRod.Reel.InventoryItem, null, base.ActiveStorages.storage, true);
			}
			base.InitRod.Reel.InventoryItem = null;
			base.InitRod.Reel.GetComponent<DragMeDoll>().Clear();
		}
	}
}
