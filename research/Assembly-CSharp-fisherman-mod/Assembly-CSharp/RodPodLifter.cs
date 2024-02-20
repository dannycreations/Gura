using System;
using UnityEngine;

public class RodPodLifter : MonoBehaviour
{
	public float RodMovement
	{
		get
		{
			return this._rodMovement;
		}
	}

	private void Awake()
	{
		this._alpha0 = new float[this._slots.Length];
		for (int i = 0; i < this._slots.Length; i++)
		{
			this._alpha0[i] = this._slots[i].eulerAngles.x;
		}
		this._width = Mathf.Abs(this._lift.position.z - this._backTipPod.position.z);
		this._initialLiftPosition = this._lift.localPosition;
	}

	private void Update()
	{
		float num = this._speed;
		if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
		{
			num *= 5f;
		}
		if (ControlsController.ControlsActions.RodStandAddAngle.IsPressed)
		{
			this._curMovement = Mathf.Min(this._curMovement + num * Time.deltaTime, this._maxUpMovement);
			this.UpdateLiftMovement();
		}
		else if (ControlsController.ControlsActions.RodStandDecAngle.IsPressed)
		{
			this._curMovement = Mathf.Max(this._curMovement - num * Time.deltaTime, -this._maxDownMovement);
			this.UpdateLiftMovement();
		}
	}

	private void UpdateLiftMovement()
	{
		this._lift.localPosition = this._initialLiftPosition + this._lift.up * this._curMovement;
		float num = Mathf.Atan(-this._curMovement / this._width) * 57.29578f;
		for (int i = 0; i < this._slots.Length; i++)
		{
			this._slots[i].transform.localEulerAngles = new Vector3(this._alpha0[i] + num, this._slots[i].transform.localEulerAngles.y, 0f);
		}
		float num2 = (this._curMovement + this._maxDownMovement) / (this._maxDownMovement + this._maxUpMovement);
		ShowHudElements.Instance.SetNormalizedAngle(num2);
	}

	public void SetActive(bool flag)
	{
		base.enabled = ShowHudElements.Instance != null && ShowHudElements.Instance.ActivateAngleMeter(flag);
	}

	[SerializeField]
	private Transform _lift;

	[SerializeField]
	private Transform _backTipPod;

	[SerializeField]
	private Transform[] _slots;

	[SerializeField]
	private float _rodMovement = 0.2f;

	[SerializeField]
	private float _maxUpMovement;

	[SerializeField]
	private float _maxDownMovement;

	[SerializeField]
	private float _speed = 0.1f;

	[SerializeField]
	private float _indicatorWidth = 0.5f;

	[SerializeField]
	private float _movementToIndicatorAngle = 10f;

	[SerializeField]
	private Vector3 _inclesIndicatorCenter;

	private float _curMovement;

	private float[] _alpha0;

	private float _width;

	private Vector3 _initialLiftPosition;
}
