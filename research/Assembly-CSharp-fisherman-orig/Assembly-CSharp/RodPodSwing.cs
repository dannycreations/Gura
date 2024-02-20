using System;
using UnityEngine;

public class RodPodSwing : MonoBehaviour
{
	private void Awake()
	{
		base.transform.SetParent(this._swing, true);
	}

	private void Update()
	{
		float num = this._speed;
		if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
		{
			num *= 5f;
		}
		if (ControlsController.ControlsActions.RodStandDecAngle.IsPressed)
		{
			float num2 = Mathf.Max(Math3d.ClampAngleTo180(this._swing.eulerAngles.x) - num * Time.deltaTime, this._minAngle);
			this._swing.localRotation = Quaternion.AngleAxis(num2, Vector3.right);
			float num3 = (num2 - this._minAngle) / (this._maxAngle - this._minAngle);
			ShowHudElements.Instance.SetNormalizedAngle(num3);
		}
		else if (ControlsController.ControlsActions.RodStandAddAngle.IsPressed)
		{
			float num4 = Mathf.Min(Math3d.ClampAngleTo180(this._swing.eulerAngles.x) + num * Time.deltaTime, this._maxAngle);
			this._swing.localRotation = Quaternion.AngleAxis(num4, Vector3.right);
			float num5 = (num4 - this._minAngle) / (this._maxAngle - this._minAngle);
			ShowHudElements.Instance.SetNormalizedAngle(num5);
		}
	}

	public void SetActive(bool flag)
	{
		base.enabled = ShowHudElements.Instance != null && ShowHudElements.Instance.ActivateAngleMeter(flag);
	}

	[SerializeField]
	private Transform _swing;

	[SerializeField]
	private float _minAngle = -20f;

	[SerializeField]
	private float _maxAngle;

	[SerializeField]
	private float _speed = 10f;
}
