using System;
using UnityEngine;
using UnityEngine.UI;

public class PhotoModeScreenFOV : PhotoModeScreen
{
	protected override void Awake()
	{
		base.Awake();
		this._actions = new CustomPlayerAction[]
		{
			ControlsController.ControlsActions.PHMFoVIncUI,
			ControlsController.ControlsActions.PHMFoVDecUI,
			ControlsController.ControlsActions.PHMFoVIncByScrollUI,
			ControlsController.ControlsActions.PHMFoVDecByScrollUI
		};
		this._axisActions = new CustomPlayerTwoAxisAction[0];
		this._gameFov = GameFactory.Player.TargetFov;
		this._curFovPrc = 1f - ((float)this._maxFOV - this._gameFov) / (float)(this._maxFOV - this._minFOV);
		this.UpdateFovLabel();
	}

	private void Update()
	{
		bool flag = ControlsController.ControlsActions.PHMFoVIncUI.IsPressed;
		bool flag2 = ControlsController.ControlsActions.PHMFoVDecUI.IsPressed;
		if (flag || flag2)
		{
			this.UpdateFOV(flag, this._fovChangingSpeed);
		}
		else
		{
			flag = ControlsController.ControlsActions.PHMFoVIncByScrollUI.IsPressed;
			flag2 = ControlsController.ControlsActions.PHMFoVDecByScrollUI.IsPressed;
			if (flag || flag2)
			{
				this.UpdateFOV(flag, this._fovChangingByScrollSpeed);
			}
		}
	}

	private void UpdateFOV(bool isInc, float speed)
	{
		if (isInc)
		{
			this._curFovPrc = Mathf.Min(this._curFovPrc + speed * Time.deltaTime, 1f);
		}
		else
		{
			this._curFovPrc = Mathf.Max(0f, this._curFovPrc - speed * Time.deltaTime);
		}
		GameFactory.Player.TargetFov = Mathf.Lerp((float)this._minFOV, (float)this._maxFOV, this._curFovPrc);
		this.UpdateFovLabel();
	}

	private void UpdateFovLabel()
	{
		if (Camera.main != null)
		{
			this._fovValueLabel.text = ((int)((double)GameFactory.Player.TargetFov + 0.5)).ToString();
		}
	}

	private void OnDestroy()
	{
		if (Camera.main != null && GameFactory.Player != null)
		{
			GameFactory.Player.TargetFov = this._gameFov;
		}
	}

	public override string ToString()
	{
		return "PhotoModeScreenFOV";
	}

	[SerializeField]
	private int _minFOV = 50;

	[SerializeField]
	private int _maxFOV = 90;

	[SerializeField]
	private float _fovChangingSpeed = 3f;

	[SerializeField]
	private float _fovChangingByScrollSpeed = 10f;

	[SerializeField]
	private Text _fovValueLabel;

	private float _curFovPrc;

	private float _gameFov;
}
