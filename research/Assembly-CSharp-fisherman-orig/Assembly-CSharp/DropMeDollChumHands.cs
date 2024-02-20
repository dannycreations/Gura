using System;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using I2.Loc;
using ObjectModel;

public class DropMeDollChumHands : DropMeDoll
{
	protected override bool TransferItem(InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		ChumIngredient chumIngredient = dragNDropContent as ChumIngredient;
		if (chumIngredient != null)
		{
			if (!(chumIngredient is ChumBase))
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
			if (InventoryHelper.IsBlocked2EquipChumHands(chum))
			{
				return false;
			}
			if (chum.IsExpired)
			{
				UIHelper.ShowYesNo(ScriptLocalization.Get("ChumExpired"), delegate
				{
				}, null, "OkButton", delegate
				{
					PhotonConnectionFactory.Instance.DestroyItem(chum);
				}, "RemoveBuoyCaption", null, null, null);
				return false;
			}
			double? weight = chum.Weight;
			if ((double)Inventory.ChumHandCapacity > weight && chum.SpecialItem != InventorySpecialItem.Snow)
			{
				MenuHelpers.Instance.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("ChumBaitNotEnoughFailedMesssage"), true, null, false);
				return false;
			}
			return base.TransferItem(dragNDropContent, dragNDropContentPreviously);
		}
	}
}
