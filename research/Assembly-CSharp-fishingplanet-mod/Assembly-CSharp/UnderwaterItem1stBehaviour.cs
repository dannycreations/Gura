using System;
using Phy;
using UnityEngine;

public abstract class UnderwaterItem1stBehaviour : UnderwaterItemBehaviour
{
	protected UnderwaterItem1stBehaviour(UnderwaterItemController controller)
		: base(controller)
	{
	}

	public override void LateUpdate()
	{
		if (base.Owner.RodSlot != null && base.Owner.RodSlot.Tackle != null)
		{
			GameFactory.Player.UpdateItem(this._owner.TpmId, this._owner.transform.position, (!base.Owner.RodSlot.Tackle.IsShowing) ? TPMFishState.UnderwaterItem : TPMFishState.UnderwaterItemShowing);
		}
	}

	public Vector3 PositionCorrection(Mass mass)
	{
		Vector3 vector = mass.Position;
		if (base.Owner.RodSlot != null && base.Owner.RodSlot.Tackle != null && base.Owner.RodSlot.Tackle.IsShowing)
		{
			vector = base.Owner.RodSlot.Rod.PositionCorrection(mass, base.Owner.RodSlot.Tackle.IsShowingComplete);
		}
		return vector;
	}

	public override void Start()
	{
		base.Start();
		this._owner.SetTpmId(GameFactory.Player.AddItem(this._owner.ItemId, base.transform.position));
	}

	public override void OnDestroy()
	{
		if (this._owner != null)
		{
			if (GameFactory.Player != null)
			{
				GameFactory.Player.DestroyItem(this._owner.TpmId);
			}
			this._owner.RodSlot.Sim.DestroyItem(base.phyObject);
			base.OnDestroy();
		}
	}
}
