using System;
using UnityEngine;

public class FloatBroken : FloatStateBase
{
	protected override void onEnter()
	{
		base.Float.EscapeFish();
		this.DisableRenderer(base.Float.gameObject);
		if (base.Float.HookTransform != null)
		{
			this.DisableRenderer(base.Float.HookTransform.gameObject);
		}
		if (base.Float.BaitTransform != null)
		{
			this.DisableRenderer(base.Float.BaitTransform.gameObject);
		}
		base.Float.LeaderLength = 0f;
		Vector3 vector = base.RodSlot.Rod.CurrentTipPosition - base.Float.transform.position;
		Vector3 vector2 = base.Float.transform.position + vector.normalized * (base.RodSlot.Line.SecuredLineLength - 1f);
		if (vector2.y > 0f)
		{
			vector2..ctor(vector2.x, 0f, vector2.y);
		}
		base.Float.transform.position = vector2;
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
			return typeof(FloatHidden);
		}
		return null;
	}

	private const float BreakTimeout = 0.5f;

	private float breakTime;
}
