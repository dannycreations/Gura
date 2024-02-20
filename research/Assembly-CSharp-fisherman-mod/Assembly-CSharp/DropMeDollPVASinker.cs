using System;
using ObjectModel;

public class DropMeDollPVASinker : DropMeDollTackle
{
	public override bool CanEquipNow(InventoryItem itemToEquip)
	{
		DropMeDollLine component = base.GetComponent<ChangeHandler>().InitRod.Line.GetComponent<DropMeDollLine>();
		if (base.GetComponent<ChangeHandler>().InitRod.Feeder.InventoryItem == null)
		{
			return false;
		}
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
}
