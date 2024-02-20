using System;
using UnityEngine;

public class LureBroken : LureStateBase
{
	protected override void onEnter()
	{
		MeshRenderer componentInChildren = base.Lure.gameObject.GetComponentInChildren<MeshRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = false;
		}
		SkinnedMeshRenderer componentInChildren2 = base.Lure.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.enabled = false;
		}
		Vector3 vector = base.RodSlot.Rod.CurrentTipPosition - base.Lure.transform.position;
		Vector3 vector2 = base.Lure.transform.position + vector.normalized * (base.RodSlot.Line.SecuredLineLength - 1f);
		if (vector2.y > 0f)
		{
			vector2..ctor(vector2.x, 0f, vector2.y);
		}
		base.Lure.transform.position = vector2;
		this.breakTime = 0f;
		if (base.IsInHands)
		{
			base.RodSlot.Sim.TurnLimitsOn(true);
		}
		this._owner.Adapter.SetGameActionFinished();
	}

	protected override Type onUpdate()
	{
		this.breakTime += Time.deltaTime;
		if (base.IsInHands && !GameFactory.Player.IsRodActive && this.breakTime > 0.5f)
		{
			return typeof(LureHidden);
		}
		return null;
	}

	private const float BreakTimeout = 0.5f;

	private float breakTime;
}
