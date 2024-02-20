using System;
using System.Collections.Generic;
using TPM;
using UnityEngine;

public class UnderwaterItem3rdBehaviour : UnderwaterItemBehaviour
{
	public UnderwaterItem3rdBehaviour(UnderwaterItemController controller)
		: base(controller)
	{
		this._renderers = RenderersHelper.GetAllRenderersForObject<Renderer>(controller.transform);
	}

	public void SetVisibility(bool flag)
	{
		for (int i = 0; i < this._renderers.Count; i++)
		{
			this._renderers[i].enabled = flag;
		}
	}

	public void SetOpaque(float prc)
	{
	}

	public void SyncUpdate(TackleBehaviour tackle, float dtPrc)
	{
		base.transform.position = ((this.state != TPMFishState.UnderwaterItemShowing) ? Vector3.Lerp(this._prevPosition, this._targetPosition, dtPrc) : tackle.HookAnchor.position);
	}

	public void Update(ThirdPersonData.FishData fish, float dtPrc)
	{
		if (this._wasInitialized)
		{
			this._prevPosition = Vector3.Lerp(this._prevPosition, this._targetPosition, dtPrc);
		}
		else
		{
			this._wasInitialized = true;
			this._prevPosition = fish.position;
		}
		this._targetPosition = fish.position;
		this.state = fish.state;
	}

	private List<Renderer> _renderers;

	private TPMFishState state;

	private bool _wasInitialized;

	private Vector3 _prevPosition;

	private Vector3 _targetPosition;
}
