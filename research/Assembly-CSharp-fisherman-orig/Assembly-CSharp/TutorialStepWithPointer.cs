using System;
using UnityEngine;

public class TutorialStepWithPointer : TutorialStep
{
	protected override void Update()
	{
		if (this._isPointerActive)
		{
			if (GameFactory.Player.IsLookWithFishMode && this.Pointer.activeSelf)
			{
				this.Pointer.SetActive(false);
			}
			else if (!GameFactory.Player.IsLookWithFishMode && !this.Pointer.activeSelf)
			{
				this.Pointer.SetActive(true);
			}
		}
		base.Update();
	}

	public override void DoStartAction()
	{
		base.DoStartAction();
		this._isPointerActive = true;
		this.Pointer.SetActive(this._isPointerActive);
	}

	public override void DoEndAction()
	{
		this._isPointerActive = false;
		this.Pointer.SetActive(this._isPointerActive);
		base.DoEndAction();
	}

	public GameObject Pointer;

	public RectTransform keepTransform;

	protected bool _isPointerActive;
}
