using System;
using UnityEngine;

public class ChangeHandler : MonoBehaviour
{
	protected InventoryItemComponent InventoryItemComponent
	{
		get
		{
			if (this._inventoryItem == null)
			{
				this._inventoryItem = base.GetComponent<InventoryItemComponent>();
			}
			return this._inventoryItem;
		}
	}

	protected ActiveStorage ActiveStorages
	{
		get
		{
			if (this._activeStorage == null)
			{
				this._activeStorage = this.InitRod.activeStorage;
			}
			return this._activeStorage;
		}
	}

	internal InitRod InitRod
	{
		get
		{
			if (InitRods.Instance != null && InitRods.Instance.ActiveRod != null)
			{
				return InitRods.Instance.ActiveRod;
			}
			InitRods initRods = Object.FindObjectOfType<InitRods>();
			return (!(initRods == null)) ? initRods.ActiveRod : null;
		}
	}

	internal int CurrentSlot()
	{
		return this.InitRod.SlotId;
	}

	public virtual void OnChange()
	{
	}

	protected virtual void Unequip()
	{
	}

	internal virtual void Refresh()
	{
	}

	private ActiveStorage _activeStorage;

	private InventoryItemComponent _inventoryItem;
}
