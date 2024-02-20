using System;
using System.Globalization;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class LineBehavior : ChangeHandler
{
	public override void OnChange()
	{
		if (base.InventoryItemComponent.InventoryItem != null)
		{
			this.CountPanel.SetActive(true);
			this.Length.text = ((int)MeasuringSystemManager.LineLength((float)base.InventoryItemComponent.InventoryItem.Length.Value)).ToString(CultureInfo.InvariantCulture);
		}
		else
		{
			this.CountPanel.SetActive(false);
		}
	}

	private void Update()
	{
		if (base.InventoryItemComponent.InventoryItem == null && this.CountPanel.activeSelf)
		{
			this.CountPanel.SetActive(false);
		}
	}

	protected override void Unequip()
	{
		if (base.InitRod.Line.InventoryItem != null)
		{
			if (base.InitRod.Line.InventoryItem.ParentItem != null && base.InitRod.Line.InventoryItem.Storage == StoragePlaces.ParentItem && !PhotonConnectionFactory.Instance.ProfileWasRequested)
			{
				PhotonConnectionFactory.Instance.MoveItemOrCombine(base.InitRod.Line.InventoryItem, null, base.ActiveStorages.storage, true);
			}
			base.InitRod.Line.InventoryItem = null;
			base.InitRod.Line.GetComponent<DragMeDoll>().Clear();
		}
	}

	public GameObject CountPanel;

	public Text Length;
}
