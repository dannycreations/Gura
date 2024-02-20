using System;
using Assets.Scripts.UI._2D.Inventory;
using ObjectModel;
using UnityEngine;

public class DropMeDollLine : DropMeDollTackle
{
	protected override bool TransferItem(InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously)
	{
		if (!this.CanEquipNow(dragNDropContent))
		{
			return false;
		}
		this._dragNDropContentInst = dragNDropContent;
		this._dragNDropContentPreviously = dragNDropContentPreviously;
		if (this.rod.InventoryItem != null)
		{
			if (InventoryHelper.IsBlocked2Equip(this.rod.InventoryItem, dragNDropContent, false))
			{
				return false;
			}
			if (((Rod)this.rod.InventoryItem).MaxLoad < ((Line)dragNDropContent).MaxLoad)
			{
				GameFactory.Message.ShowLineDoesntMatchRod(base.transform.root.gameObject);
			}
			InitRod initRod = base.GetComponent<ChangeHandler>().InitRod;
			if (initRod == null || initRod.Reel == null || !(initRod.Reel.InventoryItem is Reel))
			{
				Debug.Log("You must first move reel to doll");
				GameFactory.Message.ShowReelsMustBeSetup(base.transform.root.gameObject);
				return false;
			}
			Reel reel = (Reel)initRod.Reel.InventoryItem;
			if (reel.MaxLoad < ((Line)dragNDropContent).MaxLoad)
			{
				GameFactory.Message.ShowLineDoesntMatchReel(base.transform.root.gameObject);
			}
			float num = (float)reel.LineCapacity / ((Line)dragNDropContent).Thickness / 100f;
			float num2 = Mathf.Min(num, (float)dragNDropContent.Length.Value);
			if (dragNDropContent.Length.Value < 15.0)
			{
				GameFactory.Message.ShowLengthTooShort();
				return false;
			}
			this.ShowCutPanel(num2, (float)dragNDropContent.Length.Value, dragNDropContent);
		}
		return true;
	}

	private void ShowCutPanel(float capacityReelLength, float lineCapacity, InventoryItem line)
	{
		this._messageBox = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.cutLinesPrefab);
		RectTransform component = this._messageBox.GetComponent<RectTransform>();
		component.anchoredPosition = Vector3.zero;
		component.sizeDelta = Vector2.zero;
		CutLinesController component2 = this._messageBox.GetComponent<CutLinesController>();
		component2.CancelActionCalled += this.DropMeDollLine_CancelActionCalled;
		component2.ConfirmActionCalled += this.DropMeDollLine_ConfirmActionCalled;
		component2.Init(capacityReelLength, lineCapacity, line);
	}

	private void DropMeDollLine_ConfirmActionCalled(float cuttedLength)
	{
		if (!base.MakeSplit(this.rod.InventoryItem, this._dragNDropContentInst, this._dragNDropContentPreviously, cuttedLength))
		{
			return;
		}
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.LineCutClip, SettingsManager.InterfaceVolume);
		base.GetComponent<ChangeHandler>().Refresh();
	}

	private void DropMeDollLine_CancelActionCalled()
	{
		PhotonConnectionFactory.Instance.RaiseInventoryUpdateCancelled();
	}

	public override bool CanEquipNow(InventoryItem itemToEquip)
	{
		if (PhotonConnectionFactory.Instance.Profile.Inventory.StorageExceededInventory.Contains(itemToEquip))
		{
			GameFactory.Message.ShowCanNotMove("Can't move items from exceeded storage", base.transform.root.gameObject);
			return false;
		}
		DropMeDollReel component = base.GetComponent<ChangeHandler>().InitRod.Reel.GetComponent<DropMeDollReel>();
		if (!component.CanEquipNow(itemToEquip))
		{
			return false;
		}
		if ((Reel)base.GetComponent<ChangeHandler>().InitRod.Reel.InventoryItem == null)
		{
			GameFactory.Message.ShowReelsMustBeSetup(base.transform.root.gameObject);
			return false;
		}
		return true;
	}

	private GameObject _messageBox;

	private InventoryItem _dragNDropContentInst;

	private InventoryItem _dragNDropContentPreviously;
}
