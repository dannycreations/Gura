using System;
using UnityEngine;

public class RodBagBehavior : ChangeHandler
{
	public override void OnChange()
	{
		MonoBehaviour.print("RodBag OnChange : " + base.InventoryItemComponent.name);
		if (InitRods.Instance != null)
		{
			InitRods.Instance.Refresh(null);
		}
	}
}
