using System;
using ObjectModel;

public class PlayerEquipmentInit : InitOutfit
{
	protected override void SetHelp()
	{
		if (this.wasInit)
		{
			this.initPlayerRods.Refresh(this._profile);
			base.Setup(this._profile, false);
		}
	}

	protected override void HideHelp()
	{
	}

	protected override void OnInventoryUpdated()
	{
	}

	public void Init(Profile profile)
	{
		this._profile = profile;
		this.initPlayerRods.Refresh(this._profile);
		base.Setup(this._profile, false);
	}

	public InitRods initPlayerRods;

	private Profile _profile;

	private bool wasInit;
}
