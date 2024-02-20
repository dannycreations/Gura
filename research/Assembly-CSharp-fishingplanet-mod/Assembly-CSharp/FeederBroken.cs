using System;
using UnityEngine;

public class FeederBroken : FeederStateBase
{
	protected override void onEnter()
	{
		base.Feeder.EscapeFish();
		this.DisableRenderer(base.Feeder.gameObject);
		if (base.Feeder.HookTransform != null)
		{
			this.DisableRenderer(base.Feeder.HookTransform.gameObject);
		}
		if (base.Feeder.BaitTransform != null)
		{
			this.DisableRenderer(base.Feeder.BaitTransform.gameObject);
		}
		base.Feeder.LeaderLength = 0f;
		Vector3 vector = base.RodSlot.Rod.CurrentTipPosition - base.Feeder.transform.position;
		Vector3 vector2 = base.Feeder.transform.position + vector.normalized * (base.RodSlot.Line.SecuredLineLength - 1f);
		if (vector2.y > 0f)
		{
			vector2..ctor(vector2.x, 0f, vector2.y);
		}
		base.Feeder.transform.position = vector2;
		this.breakTime = 0f;
		if (base.IsInHands)
		{
			base.RodSlot.Sim.TurnLimitsOn(false);
		}
		this._owner.Adapter.SetGameActionFinished();
	}

	private void DisableRenderer(GameObject obj)
	{
		MeshRenderer componentInChildren = obj.GetComponentInChildren<MeshRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = false;
		}
		SkinnedMeshRenderer componentInChildren2 = obj.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.enabled = false;
		}
	}

	protected override Type onUpdate()
	{
		this.breakTime += Time.deltaTime;
		if (base.IsInHands && !GameFactory.Player.IsRodActive && this.breakTime > 0.5f)
		{
			return typeof(FeederHidden);
		}
		return null;
	}

	private const float BreakTimeout = 0.5f;

	private float breakTime;
}
