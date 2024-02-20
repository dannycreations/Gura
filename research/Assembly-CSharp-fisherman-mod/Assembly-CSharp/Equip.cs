using System;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using I2.Loc;
using ObjectModel;
using UnityEngine.EventSystems;

public class Equip : EquipBase
{
	protected override void DblClick(PointerEventData eventData)
	{
		if (eventData.button == null)
		{
			this.EquipItem();
		}
		else
		{
			this.MoveItem();
		}
	}

	public void EquipItem()
	{
		if (this.CheckIfExceed())
		{
			return;
		}
		if (this._iiComponent.InventoryItem is ChumIngredient && InitRods.Instance.IsChumMixing)
		{
			ChumMixing.Instance.Add(this._iiComponent.InventoryItem);
			return;
		}
		DropMeDoll currentRodSlotForSubType = InventoryHelper.GetCurrentRodSlotForSubType(this._iiComponent.InventoryItem.ItemSubType);
		if (currentRodSlotForSubType != null)
		{
			this.DranNDropStartChangeActiveStorage();
			this.DranNDropEnd(currentRodSlotForSubType);
			return;
		}
		ChumBase chumBase = this._iiComponent.InventoryItem as ChumBase;
		if (chumBase != null)
		{
			if (StaticUserData.CurrentPond == null)
			{
				UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("OnPondFailedMesssage"), true, null, false);
			}
			else
			{
				new MenuHelpers().ChumMixing(chumBase, null);
			}
		}
	}

	public bool CheckIfExceed()
	{
		bool flag = PhotonConnectionFactory.Instance.Profile.Inventory.StorageExceededInventory.Contains(this._dragMeComponent.DragNDropContent.InventoryItem);
		if (flag)
		{
			GameFactory.Message.ShowCanNotMove("Can't move items from exceeded storage", base.transform.root.gameObject);
		}
		return flag;
	}

	public void MoveItem()
	{
		if (this.CheckIfExceed())
		{
			return;
		}
		DropMeStorage[] storages = base.Storages;
		for (int i = 0; i < storages.Length; i++)
		{
			if (Array.Exists<int>(storages[i].typeId, (int x) => x == this._dragMeComponent.typeId) && storages[i].storage != this._dragMeComponent.DragNDropContent.Storage)
			{
				this.Move(storages[i]);
				break;
			}
		}
	}
}
