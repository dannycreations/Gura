using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStep2 : TutorialStep
{
	protected virtual void Awake()
	{
		this._hintCircle = Object.Instantiate<GameObject>(this._hintCirclePrefab, this.PlaceForCastIllumination.transform.position, Quaternion.identity).GetComponent<ManagedHintObject>();
		this._hintCircle.transform.localScale = this._hintCircleScale;
		this._hintCircle.gameObject.SetActive(false);
		this.PlaceForCastIllumination.GetComponent<MeshRenderer>().enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		if (this.PlaceForCastIllumination.activeSelf && !this._hintCircle.gameObject.activeSelf)
		{
			this._hintCircle.gameObject.SetActive(true);
		}
		else if (!this.PlaceForCastIllumination.activeSelf && this._hintCircle.gameObject.activeSelf)
		{
			this._hintCircle.gameObject.SetActive(false);
		}
	}

	public override void DoStartAction()
	{
		base.DoStartAction();
		this.PlaceForCastIllumination.SetActive(true);
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Fire1", "Move", "RunHotkey" });
		ControlsController.ControlsActions.BlockMouseButtons(false, true, true, true);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		this.PlaceForCastIllumination.SetActive(false);
		ControlsController.ControlsActions.UnBlockInput();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this._hintCircle != null && this._hintCircle.gameObject != null)
		{
			Object.Destroy(this._hintCircle.gameObject);
		}
	}

	[SerializeField]
	protected GameObject _hintCirclePrefab;

	[SerializeField]
	private Vector3 _hintCircleScale = new Vector3(0.4f, 0.4f, 0.4f);

	protected ManagedHintObject _hintCircle;

	public GameObject PlaceForCastIllumination;
}
