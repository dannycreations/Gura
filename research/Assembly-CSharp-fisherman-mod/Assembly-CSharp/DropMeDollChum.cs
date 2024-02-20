using System;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using I2.Loc;
using ObjectModel;

public class DropMeDollChum : DropMeDollTackle
{
	protected override bool TransferItem(InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously)
	{
		Rod rod = this.rod.InventoryItem as Rod;
		if (rod == null)
		{
			return false;
		}
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		ChumIngredient chumIngredient = dragNDropContent as ChumIngredient;
		if (chumIngredient != null)
		{
			if (!(chumIngredient is ChumBase))
			{
				return false;
			}
			if (InventoryHelper.IsBlocked2Equip(rod, dragNDropContent, false))
			{
				return false;
			}
			if (profile.PondId == null)
			{
				UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("OnPondFailedMesssage"), true, null, false);
				return false;
			}
			new MenuHelpers().ChumMixing(chumIngredient, (dragNDropContentPreviously != null) ? null : new Func<InventoryItem, InventoryItem, bool>(this.TransferItem));
			return false;
		}
		else
		{
			Chum chum = dragNDropContent as Chum;
			if (chum == null)
			{
				return false;
			}
			if (InventoryHelper.IsBlocked2EquipChum(rod, chum))
			{
				return false;
			}
			double? weight = chum.Weight;
			if ((double)InventoryHelper.GetFeeder(rod).ChumCapacity > weight && chum.SpecialItem != InventorySpecialItem.Snow)
			{
				MenuHelpers.Instance.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("ChumBaitNotEnoughFailedMesssage"), true, null, false);
				return false;
			}
			return base.TransferItem(dragNDropContent, dragNDropContentPreviously);
		}
	}
}
